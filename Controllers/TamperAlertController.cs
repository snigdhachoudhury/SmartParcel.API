using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParcel.API.Data;
using SmartParcel.API.DTOs;
using SmartParcel.API.Services.Interfaces;


[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Handler,Admin")]
public class TamperAlertController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITamperHandler _tamperHandler;
    private readonly ILogger<TamperAlertController> _logger;

    public TamperAlertController(
        AppDbContext context,
        ITamperHandler tamperHandler,
        ILogger<TamperAlertController> logger)
    {
        _context = context;
        _tamperHandler = tamperHandler;
        _logger = logger;
    }

    [HttpPost("report")]
    public async Task<IActionResult> ReportTampering([FromBody] TamperReportRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrEmpty(request.Reason))
            return BadRequest(new { Message = "Reason is required when reporting tampering." });

        bool success = await _tamperHandler.ReportTamperingAsync(
            request.TrackingId,
            request.Reason,
            request.Location
        );

        if (!success)
            return BadRequest(new { Message = "Failed to report tampering. Parcel may not exist or is already delivered." });

        return Ok(new { Message = "Tampering reported successfully." });
    }

    [HttpPost("resolve")]
    public async Task<IActionResult> ResolveTampering([FromBody] TamperResolveRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrEmpty(request.Resolution))
            return BadRequest(new { Message = "Resolution notes are required." });

        if (!_tamperHandler.IsValidTamperResolutionStatus(request.NextStatus))
            return BadRequest(new
            {
                Message = "Invalid next status. Please use TamperResolved, ReturnedDueToDamage, or Lost."
            });

        bool success = await _tamperHandler.ResolveTamperingAsync(
            request.TrackingId,
            request.Resolution,
            request.NextStatus,
            request.Location
        );

        if (!success)
            return BadRequest(new { Message = "Failed to resolve tampering. Parcel may not exist or isn't in tampered state." });

        return Ok(new { Message = "Tampering resolved successfully." });
    }

    [HttpGet("history/{trackingId}")]
    public async Task<IActionResult> GetTamperHistory(string trackingId)
    {
        var tamperAlerts = await _context.TamperAlerts
            .Where(t => t.TrackingId == trackingId)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync();

        return Ok(tamperAlerts);
    }
}