using HotelReservationSystem1.Data;
using HotelReservationSystem1.Models;
using HotelReservationSystem1.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationSystem1.Controllers
{
 [Authorize]
    public class PaymentsController : Controller
    {
     private readonly ApplicationDbContext _context;
   private readonly UserManager<ApplicationUser> _userManager;

 public PaymentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
   {
      _context = context;
       _userManager = userManager;
   }

  // GET: Payments/GCashPayment/5
        public async Task<IActionResult> GCashPayment(int? id)
   {
     if (id == null)
      {
   return NotFound();
       }

   var reservation = await _context.Reservations
          .Include(r => r.Room)
       .Include(r => r.User)
     .FirstOrDefaultAsync(r => r.Id == id);

      if (reservation == null)
            {
        return NotFound();
    }

    var user = await _userManager.GetUserAsync(User);
     if (!User.IsInRole("Admin") && reservation.UserId != user!.Id)
      {
       return Forbid();
      }

      // Check if already paid
 var existingPayment = await _context.Payments
          .FirstOrDefaultAsync(p => p.ReservationId == id && p.PaymentStatus == "Paid");
            
  if (existingPayment != null)
            {
                TempData["ErrorMessage"] = "This reservation has already been paid.";
    return RedirectToAction("Details", "Reservations", new { id });
      }

    var model = new GCashPaymentViewModel
{
       ReservationId = reservation.Id,
  Amount = reservation.TotalAmount,
   RoomNumber = reservation.Room?.RoomNumber,
    RoomType = reservation.Room?.RoomType,
       CheckInDate = reservation.CheckInDate,
   CheckOutDate = reservation.CheckOutDate,
  NumberOfNights = (reservation.CheckOutDate - reservation.CheckInDate).Days
      };

     return View(model);
 }

   // POST: Payments/GCashPayment
   [HttpPost]
 [ValidateAntiForgeryToken]
   public async Task<IActionResult> GCashPayment(GCashPaymentViewModel model)
  {
     if (!ModelState.IsValid)
      {
   // Reload reservation details for display
      var res = await _context.Reservations
  .Include(r => r.Room)
      .FirstOrDefaultAsync(r => r.Id == model.ReservationId);
                
      if (res != null)
    {
    model.RoomNumber = res.Room?.RoomNumber;
   model.RoomType = res.Room?.RoomType;
      model.CheckInDate = res.CheckInDate;
      model.CheckOutDate = res.CheckOutDate;
      model.NumberOfNights = (res.CheckOutDate - res.CheckInDate).Days;
           }
     
return View(model);
   }

      var reservation = await _context.Reservations.FindAsync(model.ReservationId);
      if (reservation == null)
   {
   return NotFound();
     }

   var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && reservation.UserId != user!.Id)
  {
  return Forbid();
      }

     // Check if reference number already exists
  var duplicateRef = await _context.Payments
    .AnyAsync(p => p.GCashReferenceNumber == model.GCashReferenceNumber && p.PaymentStatus != "Failed");
  
 if (duplicateRef)
        {
   ModelState.AddModelError("GCashReferenceNumber", "This reference number has already been used. Please check your transaction.");
        
    // Reload reservation details
         var res = await _context.Reservations
     .Include(r => r.Room)
          .FirstOrDefaultAsync(r => r.Id == model.ReservationId);
      
         if (res != null)
        {
    model.RoomNumber = res.Room?.RoomNumber;
     model.RoomType = res.Room?.RoomType;
    model.CheckInDate = res.CheckInDate;
 model.CheckOutDate = res.CheckOutDate;
      model.NumberOfNights = (res.CheckOutDate - res.CheckInDate).Days;
        }
  
        return View(model);
  }

   // Create payment record
     var payment = new Payment
            {
     ReservationId = model.ReservationId,
     PaymentMethod = "GCash",
     PaymentStatus = "Pending", // Admin will verify and approve
    Amount = model.Amount,
    GCashNumber = model.GCashNumber,
     GCashReferenceNumber = model.GCashReferenceNumber,
     Notes = model.Notes,
  CreatedAt = DateTime.Now
     };

   _context.Payments.Add(payment);
      await _context.SaveChangesAsync();

   TempData["SuccessMessage"] = "GCash payment submitted successfully! Your payment is pending verification. We'll notify you once confirmed.";
      return RedirectToAction("Details", "Reservations", new { id = model.ReservationId });
        }

  // GET: Payments/Index
 [Authorize(Roles = "Admin")]
   public async Task<IActionResult> Index(string status = "all")
   {
  IQueryable<Payment> query = _context.Payments
       .Include(p => p.Reservation)
      .ThenInclude(r => r!.Room)
    .Include(p => p.Reservation)
       .ThenInclude(r => r!.User);

   if (status != "all")
        {
      query = query.Where(p => p.PaymentStatus.ToLower() == status.ToLower());
     }

   var payments = await query
     .OrderByDescending(p => p.CreatedAt)
     .ToListAsync();

    ViewBag.CurrentStatus = status;
            return View(payments);
  }

        // POST: Payments/VerifyPayment
   [HttpPost]
        [ValidateAntiForgeryToken]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> VerifyPayment(int id)
 {
   var payment = await _context.Payments
     .Include(p => p.Reservation)
         .FirstOrDefaultAsync(p => p.Id == id);
        
  if (payment == null)
     {
     return NotFound();
            }

payment.PaymentStatus = "Paid";
            payment.PaidAt = DateTime.Now;
    
            // Update reservation status to Confirmed if it was Pending
 if (payment.Reservation != null && payment.Reservation.Status == "Pending")
       {
    payment.Reservation.Status = "Confirmed";
            }

  await _context.SaveChangesAsync();

      TempData["SuccessMessage"] = $"Payment #{payment.Id} verified and marked as paid!";
   return RedirectToAction(nameof(Index));
        }

   // POST: Payments/RejectPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
   [Authorize(Roles = "Admin")]
   public async Task<IActionResult> RejectPayment(int id, string reason)
   {
      var payment = await _context.Payments.FindAsync(id);
     if (payment == null)
     {
     return NotFound();
      }

       payment.PaymentStatus = "Failed";
   payment.Notes = $"Rejected: {reason}";

     await _context.SaveChangesAsync();

   TempData["SuccessMessage"] = $"Payment #{payment.Id} has been rejected.";
   return RedirectToAction(nameof(Index));
 }
    }
}
