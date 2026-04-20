using Microsoft.AspNetCore.Mvc;
using SmartLibrary.DTOs;
using SmartLibrary.Models;
using SmartLibrary.Services;

namespace SmartLibrary.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;

    public DevicesController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SmartDeviceDto>>>> GetAllDevices([FromQuery] string? status)
    {
        var devices = await _deviceService.GetAllDevicesAsync(status);
        return Ok(ApiResponse<List<SmartDeviceDto>>.Success(devices));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SmartDeviceDto>>> GetDeviceById(int id)
    {
        var device = await _deviceService.GetDeviceByIdAsync(id);
        if (device == null)
        {
            return NotFound(ApiResponse<SmartDeviceDto>.Error(404, "设备不存在"));
        }
        var result = new SmartDeviceDto
        {
            Id = device.Id,
            DeviceName = device.DeviceName,
            DeviceType = device.DeviceType,
            Location = device.Location,
            Status = device.Status,
            LastMaintenance = device.LastMaintenance
        };
        return Ok(ApiResponse<SmartDeviceDto>.Success(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SmartDeviceDto>>> CreateDevice([FromBody] SmartDeviceDto dto)
    {
        var device = new SmartDevice
        {
            DeviceName = dto.DeviceName,
            DeviceType = dto.DeviceType,
            Location = dto.Location,
            Status = dto.Status,
            LastMaintenance = dto.LastMaintenance
        };

        var created = await _deviceService.CreateDeviceAsync(device);
        var result = new SmartDeviceDto
        {
            Id = created.Id,
            DeviceName = created.DeviceName,
            DeviceType = created.DeviceType,
            Location = created.Location,
            Status = created.Status,
            LastMaintenance = created.LastMaintenance
        };
        return CreatedAtAction(nameof(GetDeviceById), new { id = created.Id }, ApiResponse<SmartDeviceDto>.Success(result, "创建成功"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<SmartDeviceDto>>> UpdateDevice(int id, [FromBody] SmartDeviceDto dto)
    {
        var device = await _deviceService.GetDeviceByIdAsync(id);
        if (device == null)
        {
            return NotFound(ApiResponse<SmartDeviceDto>.Error(404, "设备不存在"));
        }

        device.DeviceName = dto.DeviceName;
        device.DeviceType = dto.DeviceType;
        device.Location = dto.Location;
        device.Status = dto.Status;
        device.LastMaintenance = dto.LastMaintenance;

        await _deviceService.UpdateDeviceAsync(device);
        var result = new SmartDeviceDto
        {
            Id = device.Id,
            DeviceName = device.DeviceName,
            DeviceType = device.DeviceType,
            Location = device.Location,
            Status = device.Status,
            LastMaintenance = device.LastMaintenance
        };
        return Ok(ApiResponse<SmartDeviceDto>.Success(result, "更新成功"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDevice(int id)
    {
        await _deviceService.DeleteDeviceAsync(id);
        return Ok(ApiResponse<object>.Success(null, "删除成功"));
    }
}

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ReservationDto>>>> GetAllReservations([FromQuery] int? userId, [FromQuery] string? status)
    {
        var reservations = await _reservationService.GetAllReservationsAsync(userId, status);
        return Ok(ApiResponse<List<ReservationDto>>.Success(reservations));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ReservationDto>>> GetReservationById(int id)
    {
        var reservation = await _reservationService.GetReservationByIdAsync(id);
        if (reservation == null)
        {
            return NotFound(ApiResponse<ReservationDto>.Error(404, "预约记录不存在"));
        }
        var result = new ReservationDto
        {
            Id = reservation.Id,
            UserId = reservation.UserId,
            UserName = reservation.UserName,
            BookId = reservation.BookId,
            BookTitle = reservation.BookTitle,
            ReservationDate = reservation.ReservationDate,
            ExpiryDate = reservation.ExpiryDate,
            Status = reservation.Status
        };
        return Ok(ApiResponse<ReservationDto>.Success(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReservationDto>>> CreateReservation([FromBody] ReservationRequest request)
    {
        var reservation = await _reservationService.CreateReservationAsync(request.UserId, request.BookId);
        var result = new ReservationDto
        {
            Id = reservation.Id,
            UserId = reservation.UserId,
            BookId = reservation.BookId,
            ReservationDate = reservation.ReservationDate,
            ExpiryDate = reservation.ExpiryDate,
            Status = reservation.Status
        };
        return Ok(ApiResponse<ReservationDto>.Success(result, "预约成功"));
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateReservationStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        await _reservationService.UpdateReservationStatusAsync(id, request.Status);
        return Ok(ApiResponse<object>.Success(null, "更新成功"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteReservation(int id)
    {
        await _reservationService.DeleteReservationAsync(id);
        return Ok(ApiResponse<object>.Success(null, "删除成功"));
    }
}

public class ReservationRequest
{
    public int UserId { get; set; }
    public int BookId { get; set; }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
