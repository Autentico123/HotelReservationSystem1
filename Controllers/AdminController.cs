using HotelReservationSystem1.Data;
using HotelReservationSystem1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationSystem1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var totalRooms = await _context.Rooms.CountAsync();
            var availableRooms = await _context.Rooms.CountAsync(r => r.IsAvailable);
            var totalReservations = await _context.Reservations.CountAsync();
            var activeReservations = await _context.Reservations
                .CountAsync(r => r.Status == "Confirmed" && r.CheckOutDate >= DateTime.Today);
            var totalUsers = await _userManager.Users.CountAsync();
            var todayCheckIns = await _context.Reservations
                .CountAsync(r => r.CheckInDate.Date == DateTime.Today && r.Status == "Confirmed");
            var todayCheckOuts = await _context.Reservations
                .CountAsync(r => r.CheckOutDate.Date == DateTime.Today && r.Status == "Confirmed");
            var totalRevenue = await _context.Reservations
                .Where(r => r.Status == "Confirmed")
                .SumAsync(r => r.TotalAmount);

            // Recent reservations
            var recentReservations = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalRooms = totalRooms;
            ViewBag.AvailableRooms = availableRooms;
            ViewBag.TotalReservations = totalReservations;
            ViewBag.ActiveReservations = activeReservations;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TodayCheckIns = todayCheckIns;
            ViewBag.TodayCheckOuts = todayCheckOuts;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.RecentReservations = recentReservations;

            return View();
        }

        // GET: Admin/Rooms
        public async Task<IActionResult> Rooms()
        {
            var rooms = await _context.Rooms
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();
            return View(rooms);
        }

        // GET: Admin/Reservations
        public async Task<IActionResult> Reservations(string status = "all")
        {
            IQueryable<Reservation> query = _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User);

            if (status != "all")
            {
                query = query.Where(r => r.Status.ToLower() == status.ToLower());
            }

            var reservations = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.CurrentStatus = status;
            return View(reservations);
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            var userViewModels = new List<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var reservationCount = await _context.Reservations
                    .CountAsync(r => r.UserId == user.Id);

                userViewModels.Add(new UserViewModel
                {
                    User = user,
                    Roles = roles.ToList(),
                    ReservationCount = reservationCount
                });
            }

            return View(userViewModels);
        }

        // POST: Admin/UpdateReservationStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateReservationStatus(int id, string status)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            reservation.Status = status;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Reservation status updated to {status}";
            return RedirectToAction(nameof(Reservations));
        }

        // POST: Admin/ToggleUserRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var isInRole = await _userManager.IsInRoleAsync(user, role);
            if (isInRole)
            {
                await _userManager.RemoveFromRoleAsync(user, role);
                TempData["SuccessMessage"] = $"Removed {role} role from {user.Email}";
            }
            else
            {
                await _userManager.AddToRoleAsync(user, role);
                TempData["SuccessMessage"] = $"Added {role} role to {user.Email}";
            }

            return RedirectToAction(nameof(Users));
        }

        // GET: Admin/Reports
        public async Task<IActionResult> Reports()
        {
            var startDate = DateTime.Today.AddMonths(-1);
            var endDate = DateTime.Today;

            var reservationsByStatus = await _context.Reservations
                .Where(r => r.CreatedAt >= startDate)
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var reservationsByRoomType = await _context.Reservations
                .Include(r => r.Room)
                .Where(r => r.CreatedAt >= startDate)
                .GroupBy(r => r.Room.RoomType)
                .Select(g => new { RoomType = g.Key, Count = g.Count() })
                .ToListAsync();

            var monthlyRevenue = await _context.Reservations
                .Where(r => r.Status == "Confirmed" && r.CreatedAt >= startDate)
                .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
                .Select(g => new 
                { 
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}", 
                    Revenue = g.Sum(r => r.TotalAmount) 
                })
                .ToListAsync();

            ViewBag.ReservationsByStatus = reservationsByStatus;
            ViewBag.ReservationsByRoomType = reservationsByRoomType;
            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View();
        }
    }

    // Helper ViewModel for Users
    public class UserViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public List<string> Roles { get; set; } = new List<string>();
        public int ReservationCount { get; set; }
    }
}
