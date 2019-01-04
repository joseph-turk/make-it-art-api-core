using System;
using System.Collections.Generic;

namespace MakeItArtApi.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int Capacity { get; set; }
        public Guid ImageId { get; set; }
        public string ImageExtension { get; set; }

        public virtual List<Registration> Registrations { get; set; }
    }
}