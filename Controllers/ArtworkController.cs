using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MakeItArtApi.Models;
using MakeItArtApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace MakeItArtApi.Controllers
{
    [Route("api/[controller]")]
    public class ArtworkController : ControllerBase
    {
        private readonly ModelContext _context;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ImageService imageService;

        public ArtworkController(ModelContext context, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            imageService = new ImageService();
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var artworks = await _context.Artworks.ToListAsync();

            return Ok(artworks);
        }

        [HttpGet("{id}", Name = "GetArtwork")]
        public async Task<ActionResult> GetById(int id)
        {
            var artwork = await _context.Artworks.FindAsync(id);

            if (artwork == null) return NotFound();

            return Ok(artwork);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create()
        {
            if (!Request.HasFormContentType) return BadRequest();

            Guid imageId = Guid.NewGuid();
            IFormCollection form = Request.Form;

            // Form Fields
            string title = form["title"];
            string description = form["description"];
            decimal price = Convert.ToDecimal(form["price"]);

            // Form Upload
            IFormFile image = form.Files.FirstOrDefault();
            string fileExtension = String.Empty;

            if (image == null) return BadRequest();

            // Set upload directories
            string uploadsDir = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
            string artworkDir = Path.Combine(uploadsDir, "artwork");
            string imageDir = Path.Combine(artworkDir, imageId.ToString());

            // Create directories if needed
            if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
            if (!Directory.Exists(artworkDir)) Directory.CreateDirectory(artworkDir);
            Directory.CreateDirectory(imageDir);

            // Get file extension
            fileExtension = Path.GetExtension(image.FileName);

            // Create resized images
            Image<Rgba32> thumbnailImage = imageService.ResizeImage(image, 400);
            Image<Rgba32> heroImage = imageService.ResizeImage(image, 1920, 1080);

            // Set paths for saving files
            string fullFileName = String.Concat("full", fileExtension);
            string thumbnailFileName = String.Concat("thumbnail", fileExtension);
            string heroFileName = String.Concat("hero", fileExtension);
            string fullPath = Path.Combine(imageDir, fullFileName);
            string thumbnailPath = Path.Combine(imageDir, thumbnailFileName);
            string heroPath = Path.Combine(imageDir, heroFileName);

            // Save full-size image
            using (FileStream fileStream = new FileStream(fullPath, FileMode.Create))
            {
                image.CopyTo(fileStream);
                fileStream.Flush();
            }

            // Save resized images
            thumbnailImage.Save(thumbnailPath);
            heroImage.Save(heroPath);

            // Create artwork and save to DB
            Artwork artwork = new Artwork
            {
                Title = title,
                Description = description,
                Price = price,
                ImageId = imageId,
                ImageExtension = fileExtension,
                CreatedAt = DateTime.Now,
                SoldOn = null,
                IsSold = false
            };

            _context.Artworks.Add(artwork);
            await _context.SaveChangesAsync();

            return CreatedAtRoute("GetArtwork", new Artwork { Id = artwork.Id }, artwork);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> Update(int id, [FromBody] Artwork artwork)
        {
            var existingArtwork = await _context.Artworks.FindAsync(id);

            if (existingArtwork == null) return NotFound();

            existingArtwork.Title = artwork.Title;
            existingArtwork.Description = artwork.Description;
            existingArtwork.Price = artwork.Price;
            existingArtwork.IsSold = artwork.IsSold;

            if (existingArtwork.IsSold) existingArtwork.SoldOn = DateTime.Now;

            _context.Artworks.Update(existingArtwork);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            var existingArtwork = await _context.Artworks.FindAsync(id);

            if (existingArtwork == null) return NotFound();

            string imagePath = Path.Combine(
                _hostingEnvironment.WebRootPath,
                "uploads",
                "artwork",
                existingArtwork.ImageId.ToString()
            );

            _context.Artworks.Remove(existingArtwork);
            await _context.SaveChangesAsync();

            Directory.Delete(imagePath, true);

            return Ok();
        }
    }
}