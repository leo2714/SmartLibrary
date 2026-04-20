namespace SmartLibrary.Middlewares;

public static class ErrorCodes
{
    public const int Success = 200;
    public const int BadRequest = 400;
    public const int Unauthorized = 401;
    public const int Forbidden = 403;
    public const int NotFound = 404;
    public const int Conflict = 409;
    public const int InternalError = 500;
}

public class ApiException : Exception
{
    public int Code { get; set; }

    public ApiException(int code, string message) : base(message)
    {
        Code = code;
    }
}

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            _logger.LogWarning(ex, "API Exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex.Code, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled Exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ErrorCodes.InternalError, "服务器内部错误");
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, int code, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = code == ErrorCodes.Unauthorized ? 401 : 
                                       code == ErrorCodes.Forbidden ? 403 :
                                       code == ErrorCodes.NotFound ? 404 :
                                       code == ErrorCodes.Conflict ? 409 :
                                       code == ErrorCodes.BadRequest ? 400 : 500;

        var response = new
        {
            code = code,
            message = message,
            data = (object?)null
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
