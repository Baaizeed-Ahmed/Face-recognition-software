using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
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
                return BadRequest("No image provided.");

            byte[] uploadedImageData;
            try
            {
                uploadedImageData = await ConvertImageToByteArrayAsync(image);
            }
            catch
            {
                return BadRequest("Invalid image format or processing error.");
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
                    return Ok(new { message = "Employee recognized" });
                }
            }

            return Ok(new { message = "Unrecognized person" });
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
