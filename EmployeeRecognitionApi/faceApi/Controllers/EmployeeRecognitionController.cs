using Microsoft.AspNetCore.Mvc;
using faceApi.Services;
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
        private readonly string _imagesDir;
        private readonly ImageProcessingService _imageProcessingService;

        public ImageRecognitionController(ImageProcessingService imageProcessingService)
        {
            _imagesDir = Path.Combine(Directory.GetCurrentDirectory(), "Images");
            _imageProcessingService = imageProcessingService;

            if (!Directory.Exists(_imagesDir))
            {
                Directory.CreateDirectory(_imagesDir);
            }
        }

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
                return BadRequest(new { message = "No image provided.", time = currentTime });
            }

            byte[] uploadedImageData;
            try
            {
                uploadedImageData = await _imageProcessingService.ConvertImageToByteArrayAsync(image);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return BadRequest(new { message = "Invalid image format or processing error.", time = currentTime });
            }

            try
            {
                var recognizedEmployee = await RecognizeEmployeeAsync(uploadedImageData, currentTime);
                if (recognizedEmployee != null)
                {
                    return Ok(recognizedEmployee);
                }

                return Ok(new
                {
                    message = "Unrecognized person.\nSecurity has been alerted!",
                    time = currentTime
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during image recognition.", time = currentTime });
            }
        }

        private async Task<object> RecognizeEmployeeAsync(byte[] uploadedImageData, string currentTime)
        {
            foreach (var filePath in Directory.GetFiles(_imagesDir))
            {
                byte[] storedImageData;
                try
                {
                    storedImageData = await _imageProcessingService.ConvertImageToByteArrayAsync(filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file '{filePath}': {ex.Message}");
                    continue;
                }

                if (uploadedImageData.SequenceEqual(storedImageData))
                {
                    string employeeName = Path.GetFileNameWithoutExtension(filePath);
                    return new
                    {
                        message = $"Access granted to {employeeName}",
                        time = currentTime,
                        name = employeeName
                    };
                }
            }

            return null;
        }
    }
}
