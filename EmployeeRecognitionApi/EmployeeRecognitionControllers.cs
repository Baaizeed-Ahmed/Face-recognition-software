using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeRecognitionApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageRecognitionController : ControllerBase
    {
        private readonly string _imagesDir = Path.Combine(Directory.GetCurrentDirectory(), "Images");

        public ImageRecognitionController()
        {
            if (!Directory.Exists(_imagesDir))
            {
                Directory.CreateDirectory(_imagesDir);
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                var response = new { message = "No image provided.", time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(response));  // Log the response
                return BadRequest(response);
            }

            byte[] uploadedImageData;
            try
            {
                uploadedImageData = await ConvertImageToByteArrayAsync(image);
            }
            catch
            {
                var response = new { message = "Invalid image format or processing error.", time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(response));  // Log the response
                return BadRequest(response);
            }

            foreach (var filePath in Directory.GetFiles(_imagesDir))
            {
                byte[] storedImageData;
                try
                {
                    storedImageData = await ConvertImageToByteArrayAsync(filePath);
                }
                catch
                {
                    continue; // Skip this file if there's an issue processing it.
                }

                if (uploadedImageData.SequenceEqual(storedImageData))
                {
                    string employeeName = Path.GetFileNameWithoutExtension(filePath);
                    var response = new
                    {
                        message = $"Access granted to {employeeName}",
                        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        name = employeeName
                    };
                    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(response));  // Log the response
                    return Ok(response);
                }
            }

            var unrecognizedResponse = new
            {
                message = "Unrecognized person\nSecurity has been alerted!"
                time = DateTime.Now.ToLongDateString()
            };
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(unrecognizedResponse));  // Log the response
            return Ok(unrecognizedResponse);
        }

        private async Task<byte[]> ConvertImageToByteArrayAsync(IFormFile image)
        {
            using var stream = new MemoryStream();
            await image.CopyToAsync(stream);
            return await ResizeImageAsync(stream.ToArray());
        }

        private async Task<byte[]> ConvertImageToByteArrayAsync(string filePath)
        {
            var image = await System.IO.File.ReadAllBytesAsync(filePath);
            return await ResizeImageAsync(image);
        }

        private async Task<byte[]> ResizeImageAsync(byte[] imageBytes)
        {
            using var image = await Image.LoadAsync<Rgba32>(imageBytes);
            image.Mutate(x => x.Resize(200, 200));
            using var ms = new MemoryStream();
            await image.SaveAsJpegAsync(ms);
            return ms.ToArray();
        }
    }
}
