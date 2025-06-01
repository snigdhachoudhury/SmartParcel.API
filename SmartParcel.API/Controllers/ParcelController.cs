using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParcel.API.Data;
using SmartParcel.API.DTOs;
using SmartParcel.API.Models;
using System.Security.Claims; // Required for ClaimTypes
using Microsoft.EntityFrameworkCore; // Required for ToListAsync and FirstOrDefaultAsync

namespace SmartParcel.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParcelController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ParcelController(AppDbContext context)
        {
            _context = context;
        }

        // SENDER: Create a new parcel using a DTO
        [Authorize(Roles = "Sender")]
        [HttpPost("create")]
        public IActionResult CreateParcel([FromBody] CreateParcelRequest request)
        {
            var trackingId = $"PCL-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
            if (request.PickupDate.Kind == DateTimeKind.Unspecified)
            {
                request.PickupDate = DateTime.SpecifyKind(request.PickupDate, DateTimeKind.Utc);
            }

            if (request.DeliveryDate.Kind == DateTimeKind.Unspecified)
            {
                request.DeliveryDate = DateTime.SpecifyKind(request.DeliveryDate, DateTimeKind.Utc);
            }

            var parcel = new Parcel
            {
                TrackingId = trackingId,
                SenderEmail = request.SenderEmail,
                RecipientEmail = request.RecipientEmail,
                Description = request.Description,
                Weight = request.Weight.ToString(), // Fix: Convert decimal to string
                PickupLocation = request.PickupLocation,
                DeliveryLocation = request.DeliveryLocation,
                PickupDate = request.PickupDate,
                DeliveryDate = request.DeliveryDate,
                Status = "Created",
                CreatedAt = DateTime.UtcNow // Ensure CreatedAt is set
            };

            _context.Set<Parcel>().Add(parcel);
            _context.SaveChanges(); // Synchronous save

            return Ok(new
            {
                parcel.TrackingId,
                Message = "Parcel created successfully."
            });
        }

        // SENDER: Create a new parcel using the Parcel model directly (less common for client-side)
        [Authorize(Roles = "Sender")]
        [HttpPost("create-with-parcel")]
        public IActionResult CreateParcel(Parcel parcel)
        {
            parcel.TrackingId = Guid.NewGuid().ToString().Substring(0, 8); // Generate tracking ID
            parcel.CreatedAt = DateTime.UtcNow; // Ensure CreatedAt is set

            _context.Parcels.Add(parcel);
            _context.SaveChanges(); // Synchronous save

            return Ok(new { TrackingId = parcel.TrackingId, Message = "Parcel created successfully." });
        }

        // SENDER: Track a single parcel by tracking ID (returns partial data)
        [Authorize(Roles = "Sender")]
        [HttpGet("track/{trackingId}")]
        public async Task<IActionResult> TrackParcel(string trackingId)
        {
            var parcel = await _context.Parcels.FirstOrDefaultAsync(p => p.TrackingId == trackingId);
            if (parcel == null)
                return NotFound(new { Message = "Parcel not found." });

            return Ok(new
            {
                parcel.TrackingId,
                parcel.Status,
                parcel.DeliveryLocation,
                parcel.DeliveryDate
            });
        }

        // HANDLER: Scan a parcel (update status to "Scanned")
        [Authorize(Roles = "Handler")]
        [HttpPost("scan/{trackingId}")]
        public async Task<IActionResult> ScanParcel(string trackingId)
        {
            var parcel = await _context.Set<Parcel>().FirstOrDefaultAsync(p => p.TrackingId == trackingId);
            if (parcel == null)
            {
                return NotFound(new { Message = "Parcel not found." });
            }

            parcel.Status = "Scanned";
            await _context.SaveChangesAsync(); // Asynchronous save

            return Ok(new
            {
                parcel.TrackingId,
                parcel.Status,
                Message = "Parcel scanned successfully."
            });
        }

        // HANDLER: Hand over a parcel (update status to "HandedOver")
        [Authorize(Roles = "Handler")]
        [HttpPost("handover/{trackingId}")]
        public async Task<IActionResult> HandoverParcel(string trackingId)
        {
            var parcel = await _context.Set<Parcel>().FirstOrDefaultAsync(p => p.TrackingId == trackingId);
            if (parcel == null)
            {
                return NotFound(new { Message = "Parcel not found." });
            }

            parcel.Status = "HandedOver";
            await _context.SaveChangesAsync(); // Asynchronous save

            return Ok(new
            {
                parcel.TrackingId,
                parcel.Status,
                Message = "Parcel handed over successfully."
            });
        }

        // ADMIN: View all parcels
        [Authorize(Roles = "Admin")]
        [HttpGet("all-parcels")]
        public async Task<IActionResult> GetAllParcels()
        {
            var parcels = await _context.Set<Parcel>().ToListAsync();
            return Ok(parcels);
        }

        // ADMIN: View a single parcel by tracking ID
        [Authorize(Roles = "Admin")]
        [HttpGet("{trackingId}")]
        public async Task<IActionResult> GetParcelByTrackingId(string trackingId)
        {
            var parcel = await _context.Set<Parcel>().FirstOrDefaultAsync(p => p.TrackingId == trackingId);
            if (parcel == null)
            {
                return NotFound(new { Message = "Parcel not found." });
            }

            return Ok(parcel);
        }

        // ADMIN: Get parcels by status
        [Authorize(Roles = "Admin")]
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetParcelsByStatus(string status)
        {
            var parcels = await _context.Set<Parcel>()
                .Where(p => p.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                .ToListAsync();

            return Ok(parcels);
        }

        // --- NEW ENDPOINT FOR SENDER'S PARCELS ---
        // SENDER: View only their own parcels
        [Authorize(Roles = "Sender")]
        [HttpGet("my-parcels")] // New route: /api/Parcel/my-parcels
        public async Task<IActionResult> GetMyParcels()
        {
            // Get the current user's email from the JWT claims.
            // This assumes your authentication token contains a claim for the user's email.
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User email claim not found in token.");
            }

            // Fetch parcels where SenderEmail matches the authenticated user's email
            var parcels = await _context.Parcels
                                        .Where(p => p.SenderEmail == userEmail)
                                        .ToListAsync();

            if (parcels == null || !parcels.Any())
            {
                // It's usually better to return an empty array (Ok) than NotFound for an empty list
                return Ok(new List<Parcel>()); // Return an empty list if no parcels found
            }

            return Ok(parcels);
        }
    }
}