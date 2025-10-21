using Microsoft.AspNetCore.Identity;

namespace HotelReservationSystem1.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation property
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
