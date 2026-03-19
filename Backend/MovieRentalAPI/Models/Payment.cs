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
        public Rental Rental { get; set; }
        public float Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; } 
        public DateTime PaymentDate { get; set; }
        public PaymentStatus Status { get; set; }
        public string PaymentId { get; set; }
        public User User { get; set; }


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
            return $"Payment Id: {Id}, Amount: {Amount}, Method: {PaymentMethod}";
        }
    }
}
