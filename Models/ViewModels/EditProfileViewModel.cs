using System.ComponentModel.DataAnnotations;

namespace HotelReservationSystem1.Models.ViewModels
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
  public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
   [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
    }
}
