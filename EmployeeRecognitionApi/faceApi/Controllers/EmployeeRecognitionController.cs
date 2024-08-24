using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace faceApi.Controllers
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

        // Handle the preflight OPTIONS request for CORS
        [HttpOptions("upload")]
        public IActionResult PreflightUpload()
        {
            Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
            Response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
            Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
            return Ok();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile image)
        {
            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (image == null || image.Length == 0)
            {
                var response = new { message = "No image provided.", time = currentTime };
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(response));  // Log the response
                return BadRequest(response);
            }

            byte[] uploadedImageData;
            try
            {
                uploadedImageData = await ConvertImageToByteArrayAsync(image);
            }
            catch (Exception ex)
            {
                var response = new { message = "Invalid image format or processing error.", time = currentTime };
                Console.WriteLine($"Error: {ex.Message}\n{Newtonsoft.Json.JsonConvert.SerializeObject(response)}");  // Log the response and error
                return BadRequest(response);
            }

            try
            {
                foreach (var filePath in Directory.GetFiles(_imagesDir))
                {
                    byte[] storedImageData;
                    try
                    {
                        storedImageData = await ConvertImageToByteArrayAsync(filePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing file '{filePath}': {ex.Message}"); // Log the error and continue
                        continue; // Skip this file if there's an issue processing it.
                    }

                    if (uploadedImageData.SequenceEqual(storedImageData))
                    {
                        string employeeName = Path.GetFileNameWithoutExtension(filePath);
                        var response = new
                        {
                            message = $"Access granted to {employeeName}",
                            time = currentTime,
                            name = employeeName
                        };
                        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(response));  // Log the response
                        return Ok(response);
                    }
                }

                var unrecognizedResponse = new
                {
                    message = "Unrecognized person.\nSecurity has been alerted!",
                    time = currentTime
                };
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(unrecognizedResponse));  // Log the response
                return Ok(unrecognizedResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new { message = "An error occurred during image recognition.", time = currentTime };
                Console.WriteLine($"Unhandled error: {ex.Message}\n{Newtonsoft.Json.JsonConvert.SerializeObject(errorResponse)}");  // Log the error and response
                return StatusCode(500, errorResponse); // Return internal server error
            }
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
            using var ms = new MemoryStream(imageBytes);
            using var image = await Image.LoadAsync<Rgba32>(ms);
            image.Mutate(x => x.Resize(200, 200));
            using var output = new MemoryStream();
            await image.SaveAsJpegAsync(output);
            return output.ToArray();
        }
    }
}
