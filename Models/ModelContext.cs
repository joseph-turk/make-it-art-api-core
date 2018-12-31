using Microsoft.EntityFrameworkCore;

namespace MakeItArtApi.Models
{
    public class ModelContext : DbContext
    {
        public ModelContext(DbContextOptions<ModelContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Artwork> Artworks { get; set; }

        // protected override void OnModelCreating(ModelBuilder modelBuilder)
        // {
        //     // TODO: Add relationships for entities
        // }
    }
}