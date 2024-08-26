using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;

namespace faceApi.Services
{
    public class ImageProcessingService
    {
        public async Task<byte[]> ConvertImageToByteArrayAsync(IFormFile image)
        {
            using var stream = new MemoryStream();
            await image.CopyToAsync(stream);
            return await ResizeImageAsync(stream.ToArray());
        }

        public async Task<byte[]> ConvertImageToByteArrayAsync(string filePath)
        {
            var image = await File.ReadAllBytesAsync(filePath);
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
