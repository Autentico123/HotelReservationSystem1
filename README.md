# Carmen Grand Hotel - Reservation System

A comprehensive hotel reservation and management system built with ASP.NET Core MVC, designed specifically for Carmen Grand Hotel in Carmen, Bohol.

## ?? About Carmen Grand Hotel

Carmen Grand Hotel offers luxury accommodations and exceptional service in the heart of Carmen, Bohol. Our online reservation system makes it easy for guests to book rooms and manage their stays.

## ? Features

- **User Management**
  - User registration and authentication
  - Profile management
  - Role-based access control (Admin/Staff/User)

- **Room Management**
  - Browse available rooms
  - View room details and amenities
  - Real-time availability checking
  - Image gallery for rooms

- **Reservation System**
  - Easy booking process
  - View and manage reservations
  - Edit booking details
  - Cancellation support

- **Payment Processing**
  - Multiple payment methods
  - GCash integration with QR code
  - Payment tracking and history
  - Automated payment status updates

- **Admin Dashboard**
  - Comprehensive dashboard
  - Room management
  - Reservation oversight
  - User management (Admin only)
  - Reports and analytics
  - **Print reports functionality** for all admin pages

- **Staff Access**
  - Dashboard and reports access
  - Room and reservation management
  - Limited administrative privileges

- **Print Reports**
  - Print dashboard summary reports
  - Print reservations reports with filters
  - Print rooms inventory reports
  - Print user management reports
  - Print analytics and statistics reports
  - Professional print layout with hotel branding
  - Export to PDF via browser print dialog

## ??? Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: MySQL with Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Bootstrap 5, Bootstrap Icons
- **Languages**: C#, JavaScript, HTML, CSS

## ?? Installation

### Prerequisites
- .NET 8.0 SDK or later
- MySQL Server
- Visual Studio 2022 or VS Code

### Setup Instructions

1. Clone the repository:
   ```bash
   git clone https://github.com/Autentico123/HotelReservationSystem1.git
   cd HotelReservationSystem1
   ```

2. Update the connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=hotelreservationdb;User=root;Password=your_password;"
     }
   }
   ```

3. Run database migrations:
   ```bash
   dotnet ef database update
   ```
   Or run the SQL script manually:
   ```bash
   mysql -u root -p < CreatePaymentsTable.sql
   ```

4. Run the application:
   ```bash
   dotnet run
   ```

5. Access the application at `https://localhost:5001`

## ?? Default Accounts

After initial setup, the following accounts are available:

- **Admin Account**
  - Email: admin@carmengrandhotel.com
  - Password: Admin@123
  - Full access to all features including user management

- **Test User Account**
  - Email: user@carmengrandhotel.com
  - Password: User@123
  - Standard user access for making reservations

## ?? User Roles

The system supports three user roles with different permission levels:

### Admin
- Full system access
- User role management
- Can assign/remove Admin and Staff roles
- Access to all dashboard features

### Staff
- Access to admin dashboard
- Can manage rooms and reservations
- Can view reports and analytics
- Cannot manage users or assign roles

### User
- Standard customer access
- Can make and manage their own reservations
- Profile management
- Payment processing

## ?? Contact Information

**Carmen Grand Hotel**
- **Phone**: +639123478567
- **Email**: info@carmengrandhotel.com
- **Address**: Carmen, Bohol
- **Website**: Coming Soon

## ?? License

© 2025 Carmen Grand Hotel. All Rights Reserved.

## ?? Support

For technical support or inquiries, please contact the development team or open an issue on GitHub.
