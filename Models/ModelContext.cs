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
        public DbSet<Event> Events { get; set; }
        public DbSet<PrimaryContact> PrimaryContacts { get; set; }
        public DbSet<Registrant> Registrants { get; set; }
        public DbSet<Registration> Registrations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>()
                .HasMany(e => e.Registrations)
                .WithOne(r => r.Event)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PrimaryContact>()
                .HasMany(pc => pc.Registrations)
                .WithOne(r => r.PrimaryContact)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Registrant>()
                .HasMany(r => r.Registrations)
                .WithOne(reg => reg.Registrant)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}