using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParcel.API.Data;
using SmartParcel.API.DTOs;
using SmartParcel.API.Models;

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
                Status = "Created"
            };

            _context.Set<Parcel>().Add(parcel);
            _context.SaveChanges();

            return Ok(new
            {
                parcel.TrackingId,
                Message = "Parcel created successfully."
            });
        }

        [Authorize(Roles = "Sender")]
        [HttpPost("create-with-parcel")]
        public IActionResult CreateParcel(Parcel parcel)
        {
            parcel.TrackingId = Guid.NewGuid().ToString().Substring(0, 8); // Generate tracking ID
            parcel.CreatedAt = DateTime.UtcNow;

            _context.Parcels.Add(parcel);
            _context.SaveChanges();

            return Ok(new { TrackingId = parcel.TrackingId, Message = "Parcel created successfully." });
        }

        [Authorize(Roles = "Sender")]
        [HttpGet("track/{trackingId}")]
        public IActionResult TrackParcel(string trackingId)
        {
            var parcel = _context.Parcels.FirstOrDefault(p => p.TrackingId == trackingId);
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

        [Authorize(Roles = "Handler")]
        [HttpPost("scan/{trackingId}")]
        public IActionResult ScanParcel(string trackingId)
        {
            var parcel = _context.Set<Parcel>().FirstOrDefault(p => p.TrackingId == trackingId);
            if (parcel == null)
            {
                return NotFound(new { Message = "Parcel not found." });
            }

            parcel.Status = "Scanned";
            _context.SaveChanges();

            return Ok(new
            {
                parcel.TrackingId,
                parcel.Status,
                Message = "Parcel scanned successfully."
            });
        }

        [Authorize(Roles = "Handler")]
        [HttpPost("handover/{trackingId}")]
        public IActionResult HandoverParcel(string trackingId)
        {
            var parcel = _context.Set<Parcel>().FirstOrDefault(p => p.TrackingId == trackingId);
            if (parcel == null)
            {
                return NotFound(new { Message = "Parcel not found." });
            }

            parcel.Status = "HandedOver";
            _context.SaveChanges();

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
        public IActionResult GetAllParcels()
        {
            var parcels = _context.Set<Parcel>().ToList();
            return Ok(parcels);
        }

        // ADMIN: View a parcel by tracking ID
        [Authorize(Roles = "Admin")]
        [HttpGet("{trackingId}")]
        public IActionResult GetParcelByTrackingId(string trackingId)
        {
            var parcel = _context.Set<Parcel>().FirstOrDefault(p => p.TrackingId == trackingId);
            if (parcel == null)
            {
                return NotFound(new { Message = "Parcel not found." });
            }

            return Ok(parcel);
        }

        // ADMIN: Get parcels by status (optional but useful)
        [Authorize(Roles = "Admin")]
        [HttpGet("status/{status}")]
        public IActionResult GetParcelsByStatus(string status)
        {
            var parcels = _context.Set<Parcel>()
                .Where(p => p.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(parcels);
        }
    }
}
