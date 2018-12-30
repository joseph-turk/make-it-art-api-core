using System;

namespace MakeItArtApi.Models
{
    public class Artwork
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public Guid ImageId { get; set; }
        public string ImageExtension { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SoldOn { get; set; }
        public bool IsSold { get; set; }
    }
}