namespace MovieRentalAPI.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        /// <summary>UserId of the actor (0 = system)</summary>
        public int UserId { get; set; }

        /// <summary>Display name / username of the actor</summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>Customer | Admin | System</summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>e.g. Payment, Rental, Movie, User, Promo …</summary>
        public string Entity { get; set; } = string.Empty;

        /// <summary>e.g. MakePayment, CreateRental, Login, AddMovie …</summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>Human-readable description of what happened</summary>
        public string Details { get; set; } = string.Empty;

        /// <summary>Success | Failure</summary>
        public string Status { get; set; } = "Success";

        public DateTime PerformedAt { get; set; }
    }
}
