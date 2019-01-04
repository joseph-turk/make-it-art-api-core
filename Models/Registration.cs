using System;
using System.Collections.Generic;

namespace MakeItArtApi.Models
{
    public class Registration
    {
        public Guid Id { get; set; }
        public bool IsWaitList { get; set; }

        public virtual Event Event { get; set; }
        public virtual PrimaryContact PrimaryContact { get; set; }
        public virtual Registrant Registrant { get; set; }
    }
}