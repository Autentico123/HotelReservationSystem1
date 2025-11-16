using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem1.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        
        [Required]
        public int RoomId { get; set; }
        public Room? Room { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }
        
        public int NumberOfGuests { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed
        
        [StringLength(500)]
        public string? SpecialRequests { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
 
        // Navigation property for payments
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
