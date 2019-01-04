using System.Threading.Tasks;
using System.Linq;
using MakeItArtApi.Models;
using MakeItArtApi.Services;
using MakeItArtApi.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MakeItArtApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly ModelContext _context;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ImageService imageService;

        public EventsController(ModelContext context, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            imageService = new ImageService();
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            List<EventDto> events = await _context.Events.Select(e => new EventDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Start = e.Start,
                End = e.End,
                Capacity = e.Capacity,
                ImageId = e.ImageId,
                ImageExtension = e.ImageExtension,
                IsFull = e.Registrations.Where(r => !r.IsWaitList).Count() >= e.Capacity,
                RegistrationCount = e.Registrations.Where(r => !r.IsWaitList).Count(),
                WaitListCount = e.Registrations.Where(r => r.IsWaitList).Count()
            }).ToListAsync();

            return Ok(events);
        }

        [HttpGet("{id}", Name = "GetEvent")]
        public async Task<ActionResult> GetById(int id)
        {
            var singleEvent = await _context.Events
                .Where(e => e.Id.Equals(id))
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    Start = e.Start,
                    End = e.End,
                    Capacity = e.Capacity,
                    ImageId = e.ImageId,
                    ImageExtension = e.ImageExtension,
                    IsFull = e.Registrations.Where(r => !r.IsWaitList).Count() >= e.Capacity,
                    RegistrationCount = e.Registrations.Where(r => !r.IsWaitList).Count(),
                    WaitListCount = e.Registrations.Where(r => r.IsWaitList).Count()
                })
                .FirstAsync();

            if (singleEvent == null) return NotFound();

            return Ok(singleEvent);
        }

        [HttpGet("{id}/with-registrations")]
        public async Task<ActionResult> GetByIdWithRegistrations(int id)
        {
            var singleEvent = await _context.Events
                .Where(e => e.Id.Equals(id))
                .Include(e => e.Registrations)
                .ThenInclude(r => r.Registrant)
                .Include(e => e.Registrations)
                .ThenInclude(r => r.PrimaryContact)
                .FirstAsync();

            if (singleEvent == null) return NotFound();

            return Ok(singleEvent);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create()
        {
            if (!Request.HasFormContentType) return BadRequest();

            Guid imageId = Guid.NewGuid();
            IFormCollection form = Request.Form;

            // Form Fields
            string name = form["name"];
            string description = form["description"];
            DateTime start = Convert.ToDateTime(form["start"]).ToUniversalTime();
            DateTime end = Convert.ToDateTime(form["end"]).ToUniversalTime();
            int capacity = int.Parse(form["capacity"]);

            // Form Upload
            IFormFile image = form.Files.FirstOrDefault();
            string fileExtension = String.Empty;

            if (image == null) return BadRequest();

            // Set upload directories
            string uploadsDir = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
            string eventsDir = Path.Combine(uploadsDir, "events");
            string imageDir = Path.Combine(eventsDir, imageId.ToString());

            // Create directories if needed
            if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
            if (!Directory.Exists(eventsDir)) Directory.CreateDirectory(eventsDir);
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

            // Create event and save to DB
            Event newEvent = new Event
            {
                Name = name,
                Description = description,
                Start = start,
                End = end,
                Capacity = capacity,
                ImageId = imageId,
                ImageExtension = fileExtension
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtRoute("GetEvent", new Event { Id = newEvent.Id }, newEvent);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> Update(int id)
        {
            if (!Request.HasFormContentType) return BadRequest();

            var existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null) return NotFound();

            IFormCollection form = Request.Form;

            if (!existingEvent.Id.Equals(int.Parse(form["id"]))) return BadRequest();

            // Update Fields
            existingEvent.Name = form["name"];
            existingEvent.Description = form["description"];
            existingEvent.Start = Convert.ToDateTime(form["start"]).ToUniversalTime();
            existingEvent.End = Convert.ToDateTime(form["end"]).ToUniversalTime();
            existingEvent.Capacity = int.Parse(form["capacity"]);

            // Update Image
            IFormFile image = form.Files.FirstOrDefault();
            if (image != null)
            {
                Guid imageId = Guid.NewGuid();

                // Set upload directories
                string uploadsDir = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                string eventsDir = Path.Combine(uploadsDir, "events");
                string imageDir = Path.Combine(eventsDir, imageId.ToString());

                // Create directories if needed
                if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
                if (!Directory.Exists(eventsDir)) Directory.CreateDirectory(eventsDir);
                Directory.CreateDirectory(imageDir);

                // Get file extension
                string fileExtension = Path.GetExtension(image.FileName);

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

                existingEvent.ImageId = imageId;
                existingEvent.ImageExtension = fileExtension;
            }

            _context.Events.Update(existingEvent);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}