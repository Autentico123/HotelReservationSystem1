using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem1.Models
{
    public class Room
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string RoomNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string RoomType { get; set; } = string.Empty; // Single, Double, Suite, Deluxe
        
        [Range(1, 10)]
        public int Capacity { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal PricePerNight { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public string? ImageUrl { get; set; }
        
        // Navigation property
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
