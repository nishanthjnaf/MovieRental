using System.Reflection.Metadata.Ecma335;

namespace MovieRentalAPI.Models
{
    public class User : IComparable<User>, IEquatable<User>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; 
        public string Status { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public byte[] Password { get; set; } = Array.Empty<byte>();
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

        public ICollection<Rental>? Rentals { get; set; }
        public ICollection<Watchlist>? Watchlists { get; set; }
        public List<Review?> Reviews { get; internal set; }

        public int CompareTo(User? other)
        {
            return other != null ? Id.CompareTo(other.Id) : 1;
        }

        public bool Equals(User? other)
        {
            return other != null && Id == other.Id;
        }

        public override string ToString()
        {
            return $"Id: {Id}, Name: {Name}, Email: {Email}, Role: {Role}";
        }
    }
}
