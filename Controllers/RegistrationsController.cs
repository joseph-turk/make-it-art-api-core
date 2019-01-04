using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MakeItArtApi.Models;
using MakeItArtApi.Dtos;

namespace MakeItArtApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationsController : ControllerBase
    {
        private readonly ModelContext _context;

        public RegistrationsController(ModelContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult> GetById(Guid id)
        {
            Registration registration = await _context.Registrations
                .Where(r => r.Id.Equals(id))
                .Include(r => r.Registrant)
                .Include(r => r.Event)
                .FirstAsync();

            if (registration == null) return NotFound();

            return Ok(registration);
        }

        [HttpPost]
        public async Task<ActionResult> Create(RegistrationDto registrationDto)
        {
            PrimaryContact primaryContact = registrationDto.PrimaryContact;
            Event regEvent = _context.Events
                .Where(e => e.Id.Equals(registrationDto.Event.Id))
                .Include(e => e.Registrations)
                .ThenInclude(r => r.Registrant)
                .First();

            // Add primary contact if necessary
            if (!_context.PrimaryContacts.Any(pc => pc.Name.Equals(primaryContact.Name)
                && pc.EmailAddress.Equals(primaryContact.EmailAddress)
                && pc.PhoneNumber.Equals(primaryContact.PhoneNumber)))
            {
                _context.PrimaryContacts.Add(primaryContact);
                await _context.SaveChangesAsync();
            }
            else
            {
                primaryContact = _context.PrimaryContacts
                    .Where(pc => pc.Name.Equals(registrationDto.PrimaryContact.Name))
                    .First();
            }

            // Iterate over registrants
            registrationDto.Registrants.ForEach(registrant =>
            {
                bool isWaitList = false;

                // Add registrant if necessary
                if (!_context.Registrants.Any(r => r.Name.Equals(registrant.Name)))
                {
                    _context.Registrants.Add(registrant);
                    _context.SaveChanges();
                }
                else
                {
                    registrant = _context.Registrants
                        .Where(r => r.Name.Equals(registrant.Name))
                        .First();
                }

                // Set wait list
                if (regEvent.Registrations.Where(r => !r.IsWaitList).Count()
                    >= regEvent.Capacity)
                {
                    isWaitList = true;
                }

                // Create registration
                Registration registration = new Registration
                {
                    Event = regEvent,
                    PrimaryContact = primaryContact,
                    Registrant = registrant,
                    IsWaitList = isWaitList
                };

                // Add registration if it doesn't already exist
                if (regEvent.Registrations == null
                    || regEvent.Registrations.Count == 0
                    || !regEvent.Registrations.Any(r => r.Registrant.Name.Equals(registration.Registrant.Name)))
                {
                    _context.Registrations.Add(registration);
                    _context.SaveChanges();
                }
            });

            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> Update(Guid id, Registration registration)
        {
            Registration existingRegistration = await _context.Registrations.FindAsync(id);
            if (existingRegistration == null) return NotFound();

            existingRegistration.IsWaitList = registration.IsWaitList;

            _context.Registrations.Update(existingRegistration);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}