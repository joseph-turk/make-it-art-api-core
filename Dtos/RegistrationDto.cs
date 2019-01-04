using System.Collections.Generic;
using MakeItArtApi.Models;

namespace MakeItArtApi.Dtos
{
    public class RegistrationDto
    {
        public Event Event { get; set; }
        public PrimaryContact PrimaryContact { get; set; }
        public List<Registrant> Registrants { get; set; }
    }
}