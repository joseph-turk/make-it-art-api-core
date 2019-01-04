using System;
using System.Collections.Generic;

namespace MakeItArtApi.Models
{
    public class PrimaryContact
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }

        public virtual List<Registration> Registrations { get; set; }
    }
}