namespace SmartLibrary.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "User"; // Admin, Teacher, Student
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; } // 院系
    public string? Position { get; set; }   // 职务
    public string? RealName { get; set; }   // 姓名
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class BorrowRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int BookId { get; set; }
    public Book? Book { get; set; }
    public DateTime BorrowDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; } = "Borrowed"; // Borrowed, Returned, Overdue
}

public class SmartDevice
{
    public int Id { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty; // RFID, SelfCheckout, Kiosk
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = "Online"; // Online, Offline, Maintenance
    public DateTime LastMaintenance { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Reservation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int BookId { get; set; }
    public Book? Book { get; set; }
    public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Fulfilled, Cancelled, Expired
}
