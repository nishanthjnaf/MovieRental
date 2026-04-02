using MovieRentalAPI.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovieRentalAPI.Models
{
    public class Payment : IComparable<Payment>, IEquatable<Payment>
    {
        public int Id { get; set; }
        public int RentalId { get; set; }
        public int UserId { get; set; }
        public Rental Rental { get; set; } = null!;
        public float Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentStatus Status { get; set; }
        public PaymentType PaymentType { get; set; }
        public string PaymentId { get; set; } = string.Empty;
        public User User { get; set; } = null!;

        // Kept for backward compat on refund records (populated only when PaymentType == Refund)
        public double? RefundAmount { get; set; }
        public DateTime? RefundedAt { get; set; }

        public int CompareTo(Payment? other)
        {
            return other != null ? Id.CompareTo(other.Id) : 1;
        }

        public bool Equals(Payment? other)
        {
            return other != null && Id == other.Id;
        }

        public override string ToString()
        {
            return $"Payment Id: {Id}, Type: {PaymentType}, Amount: {Amount}, Status: {Status}";
        }
    }
}
