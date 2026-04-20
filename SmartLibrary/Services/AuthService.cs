using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SmartLibrary.Data;
using SmartLibrary.DTOs;
using SmartLibrary.Middlewares;
using SmartLibrary.Models;

namespace SmartLibrary.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<User> RegisterAsync(string username, string password, string email, string role);
    Task<User?> GetUserByIdAsync(int id);
    Task<List<UserDto>> GetAllUsersAsync();
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int id);
}

public class AuthService : IAuthService
{
    private readonly LibraryDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(LibraryDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FindAsync(request.Username);
        if (user == null || user.Password != HashPassword(request.Password))
        {
            throw new ApiException(ErrorCodes.Unauthorized, "用户名或密码错误");
        }

        var token = GenerateJwtToken(user);

        return new LoginResponse
        {
            Token = token,
            Username = user.Username,
            Role = user.Role,
            UserId = user.Id
        };
    }

    public async Task<User> RegisterAsync(string username, string password, string email, string role)
    {
        if (await _context.Users.FindAsync(username) != null)
        {
            throw new ApiException(ErrorCodes.Conflict, "用户名已存在");
        }

        var user = new User
        {
            Username = username,
            Password = HashPassword(password),
            Email = email,
            Role = role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        var existing = await _context.Users.FindAsync(user.Username);
        if (existing == null)
        {
            throw new ApiException(ErrorCodes.NotFound, "用户不存在");
        }

        existing.Email = user.Email;
        existing.Role = user.Role;
        
        if (!string.IsNullOrEmpty(user.Password))
        {
            existing.Password = HashPassword(user.Password);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            throw new ApiException(ErrorCodes.NotFound, "用户不存在");
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? "DefaultSecretKey12345678901234567890"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("UserId", user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "SmartLibrary",
            audience: _configuration["Jwt:Audience"] ?? "SmartLibrary",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
