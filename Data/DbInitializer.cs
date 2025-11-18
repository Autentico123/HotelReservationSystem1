using HotelReservationSystem1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationSystem1.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Apply any pending migrations (use migrations instead of EnsureCreated)
            await context.Database.MigrateAsync();

            // Create roles if they don't exist
            string[] roleNames = { "Admin", "Staff", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create admin user if doesn't exist
            var adminEmail = "admin@carmengrandhotel.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed rooms if none exist
            if (!context.Rooms.Any())
            {
                var rooms = new Room[]
                {
                    new Room
                    {
                        RoomNumber = "101",
                        RoomType = "Single",
                        Capacity = 1,
                        PricePerNight = 2500.00m,  // ?2,500 per night
                        IsAvailable = true,
                        Description = "Cozy single room with a comfortable bed and modern amenities.",
                        ImageUrl = "https://images.unsplash.com/photo-1611892440504-42a792e24d32?w=800&q=80"
                    },
                    new Room
                    {
                        RoomNumber = "102",
                        RoomType = "Double",
                        Capacity = 2,
                        PricePerNight = 4000.00m,  // ?4,000 per night
                        IsAvailable = true,
                        Description = "Spacious double room perfect for couples.",
                        ImageUrl = "https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=800&q=80"
                    },
                    new Room
                    {
                        RoomNumber = "103",
                        RoomType = "Suite",
                        Capacity = 4,
                        PricePerNight = 8000.00m,  // ?8,000 per night
                        IsAvailable = true,
                        Description = "Luxurious suite with separate living area and stunning views.",
                        ImageUrl = "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=800&q=80"
                    },
                    new Room
                    {
                        RoomNumber = "201",
                        RoomType = "Deluxe",
                        Capacity = 2,
                        PricePerNight = 6000.00m,  // ?6,000 per night
                        IsAvailable = true,
                        Description = "Premium deluxe room with king-size bed and premium facilities.",
                        ImageUrl = "https://images.unsplash.com/photo-1566665797739-1674de7a421a?w=800&q=80"
                    },
                    new Room
                    {
                        RoomNumber = "202",
                        RoomType = "Double",
                        Capacity = 2,
                        PricePerNight = 4000.00m,  // ?4,000 per night
                        IsAvailable = true,
                        Description = "Comfortable double room with city view.",
                        ImageUrl = "https://images.unsplash.com/photo-1590490360182-c33d57733427?w=800&q=80"
                    },
                    new Room
                    {
                        RoomNumber = "301",
                        RoomType = "Suite",
                        Capacity = 6,
                        PricePerNight = 10000.00m,  // ?10,000 per night
                        IsAvailable = true,
                        Description = "Executive suite with multiple bedrooms and luxury amenities.",
                        ImageUrl = "https://images.unsplash.com/photo-1591088398332-8a7791972843?w=800&q=80"
                    }
                };

                context.Rooms.AddRange(rooms);
                await context.SaveChangesAsync();
            }
        }
    }
}
