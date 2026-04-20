using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartLibrary.Data;
using SmartLibrary.Middlewares;
using SmartLibrary.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=120.55.234.65;Database=smartlibrary;User=root;Password=123456;";
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "DefaultSecretKey12345678901234567890";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "SmartLibrary",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "SmartLibrary",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBorrowService, BorrowService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Serve static files from wwwroot
app.UseStaticFiles();

// Fallback to index.html for SPA
app.MapFallbackToFile("index.html");

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    
    // Ensure database is created
    await context.Database.EnsureCreatedAsync();
    
    // Seed data if empty
    await DbInitializer.SeedDataAsync(context, config);
}

app.Run();

public static class DbInitializer
{
    public static async Task SeedDataAsync(LibraryDbContext context, IConfiguration config)
    {
        if (!context.Users.Any())
        {
            // Create admin user from teacher.json concept
            var adminUser = new User
            {
                Username = "admin",
                Password = HashPassword("123456"),
                Email = "admin@library.com",
                Role = "Admin"
            };
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();

            // Add sample teachers
            var teacher1 = new User
            {
                Username = "teacher1",
                Password = HashPassword("123456"),
                Email = "teacher1@school.com",
                Role = "Teacher"
            };
            context.Users.Add(teacher1);
            await context.SaveChangesAsync();

            // Add sample students
            var student1 = new User
            {
                Username = "student1",
                Password = HashPassword("123456"),
                Email = "student1@school.com",
                Role = "Student"
            };
            context.Users.Add(student1);
            await context.SaveChangesAsync();
        }

        if (!context.Books.Any())
        {
            var books = new[]
            {
                new Book { Title = "C# Programming", Author = "John Smith", ISBN = "978-0-123456-01", Category = "Technology", TotalCopies = 5, AvailableCopies = 5, Location = "A1-01", PublishedDate = DateTime.UtcNow.AddYears(-2), Price = 59.99m },
                new Book { Title = "Database Design", Author = "Jane Doe", ISBN = "978-0-123456-02", Category = "Technology", TotalCopies = 3, AvailableCopies = 3, Location = "A1-02", PublishedDate = DateTime.UtcNow.AddYears(-1), Price = 49.99m },
                new Book { Title = "Web Development", Author = "Bob Wilson", ISBN = "978-0-123456-03", Category = "Technology", TotalCopies = 4, AvailableCopies = 4, Location = "A1-03", PublishedDate = DateTime.UtcNow, Price = 69.99m },
                new Book { Title = "Data Structures", Author = "Alice Brown", ISBN = "978-0-123456-04", Category = "Computer Science", TotalCopies = 6, AvailableCopies = 6, Location = "B2-01", PublishedDate = DateTime.UtcNow.AddYears(-3), Price = 55.00m },
                new Book { Title = "Algorithms", Author = "Charlie Davis", ISBN = "978-0-123456-05", Category = "Computer Science", TotalCopies = 4, AvailableCopies = 4, Location = "B2-02", PublishedDate = DateTime.UtcNow.AddYears(-1), Price = 65.00m },
                new Book { Title = "Machine Learning", Author = "Eva Green", ISBN = "978-0-123456-06", Category = "AI", TotalCopies = 3, AvailableCopies = 3, Location = "C3-01", PublishedDate = DateTime.UtcNow, Price = 79.99m },
                new Book { Title = "Deep Learning", Author = "Frank Miller", ISBN = "978-0-123456-07", Category = "AI", TotalCopies = 2, AvailableCopies = 2, Location = "C3-02", PublishedDate = DateTime.UtcNow.AddMonths(-6), Price = 89.99m },
                new Book { Title = "Python Basics", Author = "Grace Lee", ISBN = "978-0-123456-08", Category = "Programming", TotalCopies = 8, AvailableCopies = 8, Location = "D4-01", PublishedDate = DateTime.UtcNow.AddYears(-2), Price = 45.00m },
                new Book { Title = "JavaScript Guide", Author = "Henry Wang", ISBN = "978-0-123456-09", Category = "Programming", TotalCopies = 5, AvailableCopies = 5, Location = "D4-02", PublishedDate = DateTime.UtcNow.AddMonths(-3), Price = 52.00m },
                new Book { Title = "Cloud Computing", Author = "Ivy Chen", ISBN = "978-0-123456-10", Category = "Technology", TotalCopies = 4, AvailableCopies = 4, Location = "E5-01", PublishedDate = DateTime.UtcNow, Price = 72.00m }
            };
            context.Books.AddRange(books);
            await context.SaveChangesAsync();
        }

        if (!context.SmartDevices.Any())
        {
            var devices = new[]
            {
                new SmartDevice { DeviceName = "Main Entrance RFID", DeviceType = "RFID", Location = "Entrance", Status = "Online", LastMaintenance = DateTime.UtcNow.AddMonths(-1) },
                new SmartDevice { DeviceName = "Self Checkout Kiosk 1", DeviceType = "SelfCheckout", Location = "Floor 1", Status = "Online", LastMaintenance = DateTime.UtcNow.AddMonths(-2) },
                new SmartDevice { DeviceName = "Self Checkout Kiosk 2", DeviceType = "SelfCheckout", Location = "Floor 1", Status = "Online", LastMaintenance = DateTime.UtcNow.AddMonths(-2) },
                new SmartDevice { DeviceName = "Information Kiosk", DeviceType = "Kiosk", Location = "Lobby", Status = "Online", LastMaintenance = DateTime.UtcNow.AddMonths(-3) },
                new SmartDevice { DeviceName = "Book Return Slot", DeviceType = "RFID", Location = "Exit", Status = "Online", LastMaintenance = DateTime.UtcNow.AddMonths(-1) }
            };
            context.SmartDevices.AddRange(devices);
            await context.SaveChangesAsync();
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
