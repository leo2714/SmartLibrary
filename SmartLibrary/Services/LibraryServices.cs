using SmartLibrary.Data;
using SmartLibrary.DTOs;
using SmartLibrary.Middlewares;
using SmartLibrary.Models;

namespace SmartLibrary.Services;

public interface IBookService
{
    Task<List<BookDto>> GetAllBooksAsync(string? category = null, string? search = null);
    Task<Book?> GetBookByIdAsync(int id);
    Task<Book> CreateBookAsync(Book book);
    Task UpdateBookAsync(Book book);
    Task DeleteBookAsync(int id);
}

public class BookService : IBookService
{
    private readonly LibraryDbContext _context;

    public BookService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<List<BookDto>> GetAllBooksAsync(string? category = null, string? search = null)
    {
        var query = _context.Books.AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(b => b.Category == category);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(b => b.Title.Contains(search) || b.Author.Contains(search));
        }

        return await query.Select(b => new BookDto
        {
            Id = b.Id,
            Title = b.Title,
            Author = b.Author,
            ISBN = b.ISBN,
            Category = b.Category,
            TotalCopies = b.TotalCopies,
            AvailableCopies = b.AvailableCopies,
            Location = b.Location,
            PublishedDate = b.PublishedDate,
            Price = b.Price
        }).ToListAsync();
    }

    public async Task<Book?> GetBookByIdAsync(int id)
    {
        return await _context.Books.FindAsync(id);
    }

    public async Task<Book> CreateBookAsync(Book book)
    {
        if (await _context.Books.AnyAsync(b => b.ISBN == book.ISBN))
        {
            throw new ApiException(ErrorCodes.Conflict, "ISBN 已存在");
        }

        book.AvailableCopies = book.TotalCopies;
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task UpdateBookAsync(Book book)
    {
        var existing = await _context.Books.FindAsync(book.Id);
        if (existing == null)
        {
            throw new ApiException(ErrorCodes.NotFound, "图书不存在");
        }

        existing.Title = book.Title;
        existing.Author = book.Author;
        existing.ISBN = book.ISBN;
        existing.Category = book.Category;
        existing.TotalCopies = book.TotalCopies;
        existing.AvailableCopies = book.AvailableCopies;
        existing.Location = book.Location;
        existing.PublishedDate = book.PublishedDate;
        existing.Price = book.Price;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteBookAsync(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            throw new ApiException(ErrorCodes.NotFound, "图书不存在");
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
    }
}

public interface IBorrowService
{
    Task<List<BorrowRecordDto>> GetAllBorrowRecordsAsync(int? userId = null, string? status = null);
    Task<BorrowRecord?> GetBorrowRecordByIdAsync(int id);
    Task<BorrowRecord> BorrowBookAsync(int userId, int bookId, int days = 30);
    Task ReturnBookAsync(int id);
    Task DeleteBorrowRecordAsync(int id);
}

public class BorrowService : IBorrowService
{
    private readonly LibraryDbContext _context;

    public BorrowService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<List<BorrowRecordDto>> GetAllBorrowRecordsAsync(int? userId = null, string? status = null)
    {
        var query = _context.BorrowRecords
            .Include(b => b.User)
            .Include(b => b.Book)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(b => b.UserId == userId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(b => b.Status == status);
        }

        return await query.Select(b => new BorrowRecordDto
        {
            Id = b.Id,
            UserId = b.UserId,
            UserName = b.User != null ? b.User.Username : null,
            BookId = b.BookId,
            BookTitle = b.Book != null ? b.Book.Title : null,
            BorrowDate = b.BorrowDate,
            DueDate = b.DueDate,
            ReturnDate = b.ReturnDate,
            Status = b.Status
        }).ToListAsync();
    }

    public async Task<BorrowRecord?> GetBorrowRecordByIdAsync(int id)
    {
        return await _context.BorrowRecords
            .Include(b => b.User)
            .Include(b => b.Book)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<BorrowRecord> BorrowBookAsync(int userId, int bookId, int days = 30)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new ApiException(ErrorCodes.NotFound, "用户不存在");
        }

        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
        {
            throw new ApiException(ErrorCodes.NotFound, "图书不存在");
        }

        if (book.AvailableCopies <= 0)
        {
            throw new ApiException(ErrorCodes.Conflict, "图书已全部借出");
        }

        var record = new BorrowRecord
        {
            UserId = userId,
            BookId = bookId,
            BorrowDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(days),
            Status = "Borrowed"
        };

        book.AvailableCopies--;

        _context.BorrowRecords.Add(record);
        await _context.SaveChangesAsync();

        return record;
    }

    public async Task ReturnBookAsync(int id)
    {
        var record = await _context.BorrowRecords.FindAsync(id);
        if (record == null)
        {
            throw new ApiException(ErrorCodes.NotFound, "借阅记录不存在");
        }

        if (record.Status == "Returned")
        {
            throw new ApiException(ErrorCodes.Conflict, "图书已归还");
        }

        var book = await _context.Books.FindAsync(record.BookId);
        if (book != null)
        {
            book.AvailableCopies++;
        }

        record.ReturnDate = DateTime.UtcNow;
        record.Status = "Returned";

        await _context.SaveChangesAsync();
    }

    public async Task DeleteBorrowRecordAsync(int id)
    {
        var record = await _context.BorrowRecords.FindAsync(id);
        if (record == null)
        {
            throw new ApiException(ErrorCodes.NotFound, "借阅记录不存在");
        }

        _context.BorrowRecords.Remove(record);
        await _context.SaveChangesAsync();
    }
}

public interface IDeviceService
{
    Task<List<SmartDeviceDto>> GetAllDevicesAsync(string? status = null);
    Task<SmartDevice?> GetDeviceByIdAsync(int id);
    Task<SmartDevice> CreateDeviceAsync(SmartDevice device);
    Task UpdateDeviceAsync(SmartDevice device);
    Task DeleteDeviceAsync(int id);
}

public class DeviceService : IDeviceService
{
    private readonly LibraryDbContext _context;

    public DeviceService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<List<SmartDeviceDto>> GetAllDevicesAsync(string? status = null)
    {
        var query = _context.SmartDevices.AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(d => d.Status == status);
        }

        return await query.Select(d => new SmartDeviceDto
        {
            Id = d.Id,
            DeviceName = d.DeviceName,
            DeviceType = d.DeviceType,
            Location = d.Location,
            Status = d.Status,
            LastMaintenance = d.LastMaintenance
        }).ToListAsync();
    }

    public async Task<SmartDevice?> GetDeviceByIdAsync(int id)
    {
        return await _context.SmartDevices.FindAsync(id);
    }

    public async Task<SmartDevice> CreateDeviceAsync(SmartDevice device)
    {
        _context.SmartDevices.Add(device);
        await _context.SaveChangesAsync();
        return device;
    }

    public async Task UpdateDeviceAsync(SmartDevice device)
    {
        var existing = await _context.SmartDevices.FindAsync(device.Id);
        if (existing == null)
        {
            throw new ApiException(ErrorCodes.NotFound, "设备不存在");
        }

        existing.DeviceName = device.DeviceName;
        existing.DeviceType = device.DeviceType;
        existing.Location = device.Location;
        existing.Status = device.Status;
        existing.LastMaintenance = device.LastMaintenance;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteDeviceAsync(int id)
    {
        var device = await _context.SmartDevices.FindAsync(id);
        if (device == null)
        {
            throw new ApiException(ErrorCodes.NotFound, "设备不存在");
        }

        _context.SmartDevices.Remove(device);
        await _context.SaveChangesAsync();
    }
}

public interface IReservationService
{
    Task<List<ReservationDto>> GetAllReservationsAsync(int? userId = null, string? status = null);
    Task<Reservation?> GetReservationByIdAsync(int id);
    Task<Reservation> CreateReservationAsync(int userId, int bookId);
    Task UpdateReservationStatusAsync(int id, string status);
    Task DeleteReservationAsync(int id);
}

public class ReservationService : IReservationService
{
    private readonly LibraryDbContext _context;

    public ReservationService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReservationDto>> GetAllReservationsAsync(int? userId = null, string? status = null)
    {
        var query = _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Book)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(r => r.UserId == userId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(r => r.Status == status);
        }

        return await query.Select(r => new ReservationDto
        {
            Id = r.Id,
            UserId = r.UserId,
            UserName = r.User != null ? r.User.Username : null,
            BookId = r.BookId,
            BookTitle = r.Book != null ? r.Book.Title : null,
            ReservationDate = r.ReservationDate,
            ExpiryDate = r.ExpiryDate,
            Status = r.Status
        }).ToListAsync();
    }

    public async Task<Reservation?> GetReservationByIdAsync(int id)
    {
        return await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Book)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Reservation> CreateReservationAsync(int userId, int bookId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new ApiException(ErrorCodes.NotFound, "用户不存在");
        }

        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
        {
            throw new ApiException(ErrorCodes.NotFound, "图书不存在");
        }

        var reservation = new Reservation
        {
            UserId = userId,
            BookId = bookId,
            ReservationDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            Status = "Pending"
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return reservation;
    }

    public async Task UpdateReservationStatusAsync(int id, string status)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
        {
            throw new ApiException(ErrorCodes.NotFound, "预约记录不存在");
        }

        reservation.Status = status;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteReservationAsync(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
        {
            throw new ApiException(ErrorCodes.NotFound, "预约记录不存在");
        }

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();
    }
}
