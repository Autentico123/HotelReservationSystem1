using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem1.Models
{
 public class Payment
    {
  public int Id { get; set; }
    
  [Required]
        public int ReservationId { get; set; }
        public Reservation? Reservation { get; set; }
 
        [Required]
        [StringLength(20)]
        public string PaymentMethod { get; set; } = "GCash"; // GCash, Cash, Card
        
[Required]
        [StringLength(20)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed, Refunded
        
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }
     
   // GCash specific fields
 [Phone]
        [StringLength(15)]
        public string? GCashNumber { get; set; }
  
     [StringLength(50)]
        public string? GCashReferenceNumber { get; set; }
 
 public DateTime CreatedAt { get; set; } = DateTime.Now;
      
    public DateTime? PaidAt { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
