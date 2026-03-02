using Microsoft.EntityFrameworkCore;
using MovieRentalAPI.Models;
namespace MovieRentalModels
{
    public class MovieRentalContext : DbContext
    {
        public MovieRentalContext(DbContextOptions<MovieRentalContext> options)
            : base(options)
        {
        }
        // Tables
        public DbSet<User> Users => Set<User>();
        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<Genre> Genres => Set<Genre>();
        public DbSet<Inventory> Inventories => Set<Inventory>();
        public DbSet<Rental> Rentals => Set<Rental>();
        public DbSet<RentalItem> RentalItems => Set<RentalItem>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Watchlist> Watchlists => Set<Watchlist>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<Movie>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<Genre>()
                .HasKey(g => g.Id);

            modelBuilder.Entity<Movie>()
                .HasMany(m => m.Genres)
                .WithMany(g => g.Movies);

            modelBuilder.Entity<Inventory>()
                .HasKey(i => i.Id);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Movie)
                .WithMany(m => m.Inventories)
                .HasForeignKey(i => i.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Rental>()
                .HasKey(r => r.Id);
            modelBuilder.Entity<Rental>()
                .HasOne(r => r.User)
                .WithMany(u => u.Rentals)
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<RentalItem>()
                .HasKey(ri => ri.Id);
            modelBuilder.Entity<RentalItem>()
                .HasOne(ri => ri.Rental)
                .WithMany(r => r.RentalItems)
                .HasForeignKey(ri => ri.RentalId);



            modelBuilder.Entity<Payment>()
                .HasKey(p => p.Id);
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Rental)
                .WithOne(r => r.Payment)
                .HasForeignKey<Payment>(p => p.RentalId);

            modelBuilder.Entity<Review>()
                .HasKey(rv => rv.Id);
            modelBuilder.Entity<Review>()
                .HasOne(rv => rv.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(rv => rv.UserId);
            modelBuilder.Entity<Review>()
                .HasOne(rv => rv.Movie)
                .WithMany(m => m.Reviews)
                .HasForeignKey(rv => rv.MovieId);

            modelBuilder.Entity<Watchlist>()
                .HasKey(w => w.Id);
            modelBuilder.Entity<Watchlist>()
                .HasOne(w => w.User)
                .WithMany(u => u.Watchlists)
                .HasForeignKey(w => w.UserId);
            modelBuilder.Entity<Watchlist>()
                .HasOne(w => w.Movie)
                .WithMany(m => m.Watchlists)
                .HasForeignKey(w => w.MovieId);
        }
    }
}