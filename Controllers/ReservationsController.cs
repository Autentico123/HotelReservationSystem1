using HotelReservationSystem1.Data;
using HotelReservationSystem1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationSystem1.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var reservations = isAdmin
                ? await _context.Reservations
                    .Include(r => r.Room)
                    .Include(r => r.User)
                    .Include(r => r.Payments)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync()
                : await _context.Reservations
                    .Include(r => r.Room)
                    .Include(r => r.Payments)
                    .Where(r => r.UserId == user!.Id)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

            return View(reservations);
        }

        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && reservation.UserId != user!.Id)
            {
                return Forbid();
            }

            return View(reservation);
        }

        // GET: Reservations/Create
        public async Task<IActionResult> Create(int? roomId)
        {
            var availableRooms = await _context.Rooms
                .Where(r => r.IsAvailable)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();
            
            ViewData["RoomId"] = new SelectList(availableRooms, "Id", "RoomNumber", roomId);
            ViewBag.AvailableRooms = availableRooms; // Pass full room data for JavaScript
            
            var reservation = new Reservation
            {
                CheckInDate = DateTime.Now.Date,
                CheckOutDate = DateTime.Now.Date.AddDays(1),
                NumberOfGuests = 1
            };
            
            if (roomId.HasValue)
            {
                reservation.RoomId = roomId.Value;
                var selectedRoom = availableRooms.FirstOrDefault(r => r.Id == roomId.Value);
                if (selectedRoom != null)
                {
                    ViewBag.SelectedRoom = selectedRoom;
                }
            }
            
            return View(reservation);
        }

        // POST: Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId,CheckInDate,CheckOutDate,NumberOfGuests,SpecialRequests")] Reservation reservation)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Set UserId BEFORE validation
            reservation.UserId = user.Id;
            
            // Remove UserId from ModelState since we're setting it programmatically
            ModelState.Remove("UserId");

            // Validate dates
            if (reservation.CheckInDate < DateTime.Now.Date)
            {
                ModelState.AddModelError("CheckInDate", "Check-in date cannot be in the past.");
            }

            if (reservation.CheckOutDate <= reservation.CheckInDate)
            {
                ModelState.AddModelError("CheckOutDate", "Check-out date must be after check-in date.");
            }

            // Validate room
            var room = await _context.Rooms.FindAsync(reservation.RoomId);
            if (room == null)
            {
                ModelState.AddModelError("RoomId", "Selected room not found.");
            }
            else if (!room.IsAvailable)
            {
                ModelState.AddModelError("RoomId", "This room is not available.");
            }
            else
            {
                // Validate guest capacity
                if (reservation.NumberOfGuests > room.Capacity)
                {
                    ModelState.AddModelError("NumberOfGuests", $"This room can accommodate maximum {room.Capacity} guest(s).");
                }
                
                // Check for overlapping reservations
                var hasOverlap = await _context.Reservations
                    .Where(r => r.RoomId == reservation.RoomId 
                           && r.Status != "Cancelled"
                           && ((r.CheckInDate <= reservation.CheckInDate && r.CheckOutDate > reservation.CheckInDate)
                           || (r.CheckInDate < reservation.CheckOutDate && r.CheckOutDate >= reservation.CheckOutDate)
                           || (r.CheckInDate >= reservation.CheckInDate && r.CheckOutDate <= reservation.CheckOutDate)))
                    .AnyAsync();
                
                if (hasOverlap)
                {
                    ModelState.AddModelError("", "This room is already booked for the selected dates. Please choose different dates.");
                }
                else
                {
                    // Calculate total amount
                    var nights = (reservation.CheckOutDate - reservation.CheckInDate).Days;
                    reservation.TotalAmount = room.PricePerNight * nights;
                    reservation.Status = "Confirmed";
                    reservation.CreatedAt = DateTime.Now;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(reservation);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"Booking confirmed! Room {room!.RoomNumber} reserved from {reservation.CheckInDate:MMM dd, yyyy} to {reservation.CheckOutDate:MMM dd, yyyy}. Total: ?{reservation.TotalAmount:N2}";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while creating the reservation. Please try again.");
                    // Log the exception here if you have logging configured
                }
            }

            // If we got here, something failed, redisplay form
            var availableRooms = await _context.Rooms
                .Where(r => r.IsAvailable)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();
            
            ViewData["RoomId"] = new SelectList(availableRooms, "Id", "RoomNumber", reservation.RoomId);
            ViewBag.AvailableRooms = availableRooms;
            
            if (reservation.RoomId > 0)
            {
                var selectedRoom = availableRooms.FirstOrDefault(r => r.Id == reservation.RoomId);
                if (selectedRoom != null)
                {
                    ViewBag.SelectedRoom = selectedRoom;
                }
            }
            
            return View(reservation);
        }

        // GET: Reservations/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
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

            // Get all rooms for dropdown
            var allRooms = await _context.Rooms.OrderBy(r => r.RoomNumber).ToListAsync();
            
            ViewData["RoomId"] = new SelectList(allRooms, "Id", "RoomNumber", reservation.RoomId);
            
            // Pass room data for JavaScript
            ViewBag.AllRooms = allRooms.Select(r => new
            {
                id = r.Id,
                roomNumber = r.RoomNumber,
                roomType = r.RoomType,
                pricePerNight = r.PricePerNight,
                capacity = r.Capacity
            }).ToList();
            
            return View(reservation);
        }

        // POST: Reservations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,RoomId,CheckInDate,CheckOutDate,NumberOfGuests,TotalAmount,Status,SpecialRequests,CreatedAt")] Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return NotFound();
            }

            // Validate dates
            if (reservation.CheckOutDate <= reservation.CheckInDate)
            {
                ModelState.AddModelError("CheckOutDate", "Check-out date must be after check-in date.");
            }

            // Validate room
            var room = await _context.Rooms.FindAsync(reservation.RoomId);
            if (room == null)
            {
                ModelState.AddModelError("RoomId", "Selected room not found.");
            }
            else
            {
                // Validate guest capacity
                if (reservation.NumberOfGuests > room.Capacity)
                {
                    ModelState.AddModelError("NumberOfGuests", $"This room can accommodate maximum {room.Capacity} guest(s).");
                }
                
                // Check for overlapping reservations (excluding current)
                var hasOverlap = await _context.Reservations
                    .Where(r => r.RoomId == reservation.RoomId 
                           && r.Id != reservation.Id
                           && r.Status != "Cancelled"
                           && ((r.CheckInDate <= reservation.CheckInDate && r.CheckOutDate > reservation.CheckInDate)
                           || (r.CheckInDate < reservation.CheckOutDate && r.CheckOutDate >= reservation.CheckOutDate)
                           || (r.CheckInDate >= reservation.CheckInDate && r.CheckOutDate <= reservation.CheckOutDate)))
                    .AnyAsync();
                
                if (hasOverlap)
                {
                    ModelState.AddModelError("", "This room is already booked for the selected dates.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservation);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"Reservation #{reservation.Id} updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Reload data on failure
            var allRooms = await _context.Rooms.OrderBy(r => r.RoomNumber).ToListAsync();
            ViewData["RoomId"] = new SelectList(allRooms, "Id", "RoomNumber", reservation.RoomId);
            ViewBag.AllRooms = allRooms.Select(r => new
            {
                id = r.Id,
                roomNumber = r.RoomNumber,
                roomType = r.RoomType,
                pricePerNight = r.PricePerNight,
                capacity = r.Capacity
            }).ToList();
            
            reservation.User = await _context.Users.FindAsync(reservation.UserId);
            reservation.Room = await _context.Rooms.FindAsync(reservation.RoomId);
            
            return View(reservation);
        }

        // POST: Reservations/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && reservation.UserId != user!.Id)
            {
                return Forbid();
            }

            reservation.Status = "Cancelled";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reservation cancelled successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Reservations/EditBooking/5 (For regular users)
        public async Task<IActionResult> EditBooking(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == id);
    
            if (reservation == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            
            // Only allow users to edit their own reservations (admins use different Edit action)
            if (!User.IsInRole("Admin") && reservation.UserId != user!.Id)
            {
                return Forbid();
            }

            // Check if reservation can be edited
            if (reservation.Status == "Cancelled" || reservation.Status == "Completed")
            {
                TempData["ErrorMessage"] = "Cannot edit a cancelled or completed reservation.";
                return RedirectToAction(nameof(Index));
            }

            if (reservation.CheckInDate < DateTime.Now.Date)
            {
                TempData["ErrorMessage"] = "Cannot edit a reservation that has already started or passed.";
                return RedirectToAction(nameof(Index));
            }

            // Get available rooms for the current date range (excluding overlapping bookings except this one)
            var availableRooms = await _context.Rooms
                .AsNoTracking()
                .Where(r => r.IsAvailable || r.Id == reservation.RoomId)
                .OrderBy(r => r.RoomNumber)
                .Select(r => new
                {
                    r.Id,
                    r.RoomNumber,
                    r.RoomType,
                    r.PricePerNight,
                    r.Capacity
                })
                .ToListAsync();
            
            ViewData["RoomId"] = new SelectList(availableRooms, "Id", "RoomNumber", reservation.RoomId);
            ViewBag.AvailableRooms = availableRooms;
            
            return View(reservation);
        }

    // POST: Reservations/EditBooking/5
        [HttpPost]
     [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBooking(int id, [Bind("Id,UserId,RoomId,CheckInDate,CheckOutDate,NumberOfGuests,SpecialRequests,Status,TotalAmount,CreatedAt")] Reservation reservation)
        {
      if (id != reservation.Id)
            {
          return NotFound();
            }

       var user = await _userManager.GetUserAsync(User);
  
   // Verify ownership
            if (!User.IsInRole("Admin") && reservation.UserId != user!.Id)
         {
              return Forbid();
            }

            // Check if reservation can be edited
  if (reservation.Status == "Cancelled" || reservation.Status == "Completed")
            {
   TempData["ErrorMessage"] = "Cannot edit a cancelled or completed reservation.";
       return RedirectToAction(nameof(Index));
       }

            // Validate dates
            if (reservation.CheckInDate < DateTime.Now.Date)
        {
        ModelState.AddModelError("CheckInDate", "Check-in date cannot be in the past.");
   }

            if (reservation.CheckOutDate <= reservation.CheckInDate)
            {
         ModelState.AddModelError("CheckOutDate", "Check-out date must be after check-in date.");
 }

         // Validate room
            var room = await _context.Rooms.FindAsync(reservation.RoomId);
 if (room == null)
         {
    ModelState.AddModelError("RoomId", "Selected room not found.");
     }
            else
   {
// Validate guest capacity
      if (reservation.NumberOfGuests > room.Capacity)
       {
              ModelState.AddModelError("NumberOfGuests", $"This room can accommodate maximum {room.Capacity} guest(s).");
         }
         
    // Check for overlapping reservations (excluding current reservation)
         var hasOverlap = await _context.Reservations
             .Where(r => r.RoomId == reservation.RoomId 
    && r.Id != reservation.Id
  && r.Status != "Cancelled"
   && ((r.CheckInDate <= reservation.CheckInDate && r.CheckOutDate > reservation.CheckInDate)
 || (r.CheckInDate < reservation.CheckOutDate && r.CheckOutDate >= reservation.CheckOutDate)
                || (r.CheckInDate >= reservation.CheckInDate && r.CheckOutDate <= reservation.CheckOutDate)))
         .AnyAsync();
                
   if (hasOverlap)
          {
      ModelState.AddModelError("", "This room is already booked for the selected dates. Please choose different dates.");
}
         else
   {
          // Recalculate total amount
          var nights = (reservation.CheckOutDate - reservation.CheckInDate).Days;
          reservation.TotalAmount = room.PricePerNight * nights;
   }
            }

            if (ModelState.IsValid)
      {
         try
             {
         _context.Update(reservation);
        await _context.SaveChangesAsync();
 
  TempData["SuccessMessage"] = $"Your booking has been updated successfully! Room {room!.RoomNumber} from {reservation.CheckInDate:MMM dd, yyyy} to {reservation.CheckOutDate:MMM dd, yyyy}. New Total: ?{reservation.TotalAmount:N2}";
           return RedirectToAction(nameof(Index));
        }
     catch (DbUpdateConcurrencyException)
        {
         if (!ReservationExists(reservation.Id))
     {
       return NotFound();
         }
     else
                    {
   throw;
    }
   }
       }

       // Reload data on failure
       var availableRooms = await _context.Rooms
       .AsNoTracking()
       .Where(r => r.IsAvailable || r.Id == reservation.RoomId)
       .OrderBy(r => r.RoomNumber)
       .Select(r => new
                {
                    r.Id,
                    r.RoomNumber,
                    r.RoomType,
                    r.PricePerNight,
                    r.Capacity
                })
                .ToListAsync();
            
            ViewData["RoomId"] = new SelectList(availableRooms, "Id", "RoomNumber", reservation.RoomId);
            ViewBag.AvailableRooms = availableRooms;
            
            // Reload related entities for display
            var roomForDisplay = await _context.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == reservation.RoomId);
            reservation.Room = roomForDisplay;

            return View(reservation);
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
}
    }
}
