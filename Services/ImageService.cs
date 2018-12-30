using Microsoft.AspNetCore.Http;
using SixLabors.Primitives;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace MakeItArtApi.Services
{
    public class ImageService
    {
        /// <summary>
        /// Resize an image and crop it to a square aspect ratio
        /// </summary>
        public Image<Rgba32> ResizeImage(IFormFile file, int size)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                file.CopyTo(ms);
                Image<Rgba32> image = Image.Load(ms.ToArray());

                return image.Clone(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(size),
                    Mode = ResizeMode.Crop
                }));
            }
        }

        /// <summary>
        /// Resize an image and crop it to a custom aspect ratio
        /// </summary>
        public Image<Rgba32> ResizeImage(IFormFile file, int width, int height)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                file.CopyTo(ms);
                Image<Rgba32> image = Image.Load(ms.ToArray());

                return image.Clone(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(width, height),
                    Mode = ResizeMode.Crop
                }));
            }
        }
    }
}