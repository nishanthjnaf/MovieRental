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

        // Series
        public DbSet<Series> Series => Set<Series>();
        public DbSet<Season> Seasons => Set<Season>();
        public DbSet<Episode> Episodes => Set<Episode>();
        public DbSet<SeasonReview> SeasonReviews => Set<SeasonReview>();
        public DbSet<SeriesWatchlist> SeriesWatchlists => Set<SeriesWatchlist>();
        public DbSet<SeriesRentalItem> SeriesRentalItems => Set<SeriesRentalItem>();
        public DbSet<SeriesCartItem> SeriesCartItems => Set<SeriesCartItem>();
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

            // Series
            modelBuilder.Entity<Series>()
                .HasKey(s => s.Id);
            modelBuilder.Entity<Series>()
                .HasMany(s => s.Genres)
                .WithMany()
                .UsingEntity(j => j.ToTable("SeriesGenres"));

            modelBuilder.Entity<Season>()
                .HasKey(s => s.Id);
            modelBuilder.Entity<Season>()
                .HasOne(s => s.Series)
                .WithMany(sr => sr.Seasons)
                .HasForeignKey(s => s.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Episode>()
                .HasKey(e => e.Id);
            modelBuilder.Entity<Episode>()
                .HasOne(e => e.Season)
                .WithMany(s => s.Episodes)
                .HasForeignKey(e => e.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SeasonReview>()
                .HasKey(r => r.Id);
            modelBuilder.Entity<SeasonReview>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<SeasonReview>()
                .HasOne(r => r.Season)
                .WithMany(s => s.Reviews)
                .HasForeignKey(r => r.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SeriesWatchlist>()
                .HasKey(w => w.Id);
            modelBuilder.Entity<SeriesWatchlist>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SeriesWatchlist>()
                .HasOne(w => w.Series)
                .WithMany(s => s.Watchlists)
                .HasForeignKey(w => w.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SeriesRentalItem>()
                .HasKey(r => r.Id);
            modelBuilder.Entity<SeriesRentalItem>()
                .HasOne(r => r.Rental)
                .WithMany()
                .HasForeignKey(r => r.RentalId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SeriesRentalItem>()
                .HasOne(r => r.Series)
                .WithMany()
                .HasForeignKey(r => r.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SeriesCartItem>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<SeriesCartItem>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SeriesCartItem>()
                .HasOne(c => c.Series)
                .WithMany()
                .HasForeignKey(c => c.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}