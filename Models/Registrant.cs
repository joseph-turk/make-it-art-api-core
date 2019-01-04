using System;
using System.Collections.Generic;

namespace MakeItArtApi.Models
{
    public class Registrant
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public virtual List<Registration> Registrations { get; set; }
    }
}