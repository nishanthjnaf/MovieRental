using MovieRentalAPI.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovieRentalAPI.Models

{
    public class Rental : IComparable<Rental>, IEquatable<Rental>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime RentalDate { get; set; }
        public RentalStatus Status { get; set; }
        public string? PaymentId { get; set; }
        public float TotalAmount { get; set; }

        public User? User { get; set; }
        public ICollection<RentalItem>? RentalItems { get; set; }
        public ICollection<Payment>? Payments { get; set; }

        public int CompareTo(Rental? other)
        {
            return other != null ? Id.CompareTo(other.Id) : 1;
        }

        public bool Equals(Rental? other)
        {
            return other != null && Id == other.Id;
        }

        public override string ToString()
        {
            return $"Rental Id: {Id}, UserId: {UserId}, Status: {Status}";
        }
    }
}
