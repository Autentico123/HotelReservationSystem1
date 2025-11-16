using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem1.Models.ViewModels
{
    public class GCashPaymentViewModel
    {
        public int ReservationId { get; set; }
        public decimal Amount { get; set; }
        
   [Required(ErrorMessage = "GCash number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
    [RegularExpression(@"^(09|\+639)\d{9}$", ErrorMessage = "Please enter a valid Philippine mobile number (09XXXXXXXXX or +639XXXXXXXXX)")]
    [Display(Name = "GCash Mobile Number")]
        public string GCashNumber { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Reference number is required")]
    [StringLength(50, MinimumLength = 10, ErrorMessage = "Reference number must be between 10 and 50 characters")]
     [Display(Name = "GCash Reference Number")]
        public string GCashReferenceNumber { get; set; } = string.Empty;
  
  [Display(Name = "Payment Notes (Optional)")]
        [StringLength(500)]
     public string? Notes { get; set; }
        
        // Read-only properties for display
        public string? RoomNumber { get; set; }
        public string? RoomType { get; set; }
   public DateTime CheckInDate { get; set; }
 public DateTime CheckOutDate { get; set; }
        public int NumberOfNights { get; set; }
    }
}
