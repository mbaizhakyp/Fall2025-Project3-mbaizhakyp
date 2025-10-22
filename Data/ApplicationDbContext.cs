using Fall2025_Project3_mbaizhakyp.Models; // <-- Add this
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fall2025_Project3_mbaizhakyp.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<ActorMovie> ActorMovies { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Actor> Actors { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure the composite primary key for the ActorMovie join table
            builder.Entity<ActorMovie>()
                .HasKey(am => new { am.MovieId, am.ActorId });
        }
}
