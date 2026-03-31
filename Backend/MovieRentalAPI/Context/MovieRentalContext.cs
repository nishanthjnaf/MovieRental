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
        public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<RentalItemRefund> RentalItemRefunds => Set<RentalItemRefund>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<BroadcastMessage> BroadcastMessages => Set<BroadcastMessage>();
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
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
                .WithMany(r => r.Payments)
                .HasForeignKey(p => p.RentalId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

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

            modelBuilder.Entity<UserPreference>()
                .HasKey(p => p.Id);
            modelBuilder.Entity<UserPreference>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<UserPreference>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Movie)
                .WithMany()
                .HasForeignKey(c => c.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RentalItemRefund>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<Notification>()
                .HasKey(n => n.Id);

            modelBuilder.Entity<BroadcastMessage>()
                .HasKey(b => b.Id);

            modelBuilder.Entity<ActivityLog>()
                .HasKey(a => a.Id);
        }
    }
}