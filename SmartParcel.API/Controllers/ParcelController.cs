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

        public async Task<IActionResult> CreateParcel([FromBody] CreateParcelRequest request)

        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var trackingId = $"PCL-{DateTime.UtcNow:yyyyMMddHHmmssfff}";



            // Convert to UTC for expected dates

            if (request.ExpectedPickupDate.Kind == DateTimeKind.Unspecified)

            {

                request.ExpectedPickupDate = DateTime.SpecifyKind(request.ExpectedPickupDate, DateTimeKind.Utc);

            }

            if (request.ExpectedDeliveryDate.Kind == DateTimeKind.Unspecified)

            {

                request.ExpectedDeliveryDate = DateTime.SpecifyKind(request.ExpectedDeliveryDate, DateTimeKind.Utc);

            }



            var parcel = new Parcel

            {

                TrackingId = trackingId,

                SenderEmail = request.SenderEmail ?? throw new ArgumentNullException(nameof(request.SenderEmail)),
                RecipientEmail = request.RecipientEmail ?? throw new ArgumentNullException(nameof(request.RecipientEmail)),
                Description = request.Description ?? throw new ArgumentNullException(nameof(request.Description)),
                Weight = request.Weight.ToString(),
                PickupLocation = request.PickupLocation ?? throw new ArgumentNullException(nameof(request.PickupLocation)),
                DeliveryLocation = request.DeliveryLocation ?? throw new ArgumentNullException(nameof(request.DeliveryLocation)),
                ExpectedPickupDate = request.ExpectedPickupDate,
                ExpectedDeliveryDate = request.ExpectedDeliveryDate,
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            };



            await _context.Parcels.AddAsync(parcel);

            await _context.SaveChangesAsync();



            return Ok(new { parcel.TrackingId, Message = "Parcel created successfully." });

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

                ExpectedPickup = parcel.ExpectedPickupDate,

                ExpectedDelivery = parcel.ExpectedDeliveryDate,

                ActualPickup = parcel.ActualPickupDate,

                ActualDelivery = parcel.ActualDeliveryDate

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



            parcel.Status = "Delivered";
            parcel.ActualDeliveryDate = DateTime.UtcNow; // Optionally set delivery date

            await _context.SaveChangesAsync();

            return Ok(new
            {
                parcel.TrackingId,
                parcel.Status,
                Message = "Parcel scanned and marked as Delivered."
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



        // HANDLER: Update parcel status and actual dates

        [Authorize(Roles = "Handler")]

        [HttpPut("{trackingId}/status")]

        public async Task<IActionResult> UpdateParcelStatus(string trackingId, [FromBody] UpdateParcelStatusRequest request)

        {

            var parcel = await _context.Parcels.FirstOrDefaultAsync(p => p.TrackingId == trackingId);

            if (parcel == null)

            {

                return NotFound(new { Message = "Parcel not found." });

            }



            parcel.Status = request.Status;



            // Update actual dates if provided by handler

            if (request.ActualPickupDate.HasValue)

            {

                parcel.ActualPickupDate = request.ActualPickupDate.Value;

            }

            if (request.ActualDeliveryDate.HasValue)

            {

                parcel.ActualDeliveryDate = request.ActualDeliveryDate.Value;

            }



            await _context.SaveChangesAsync();



            return Ok(new

            {

                parcel.TrackingId,

                parcel.Status,

                ExpectedPickup = parcel.ExpectedPickupDate,

                ExpectedDelivery = parcel.ExpectedDeliveryDate,

                ActualPickup = parcel.ActualPickupDate,

                ActualDelivery = parcel.ActualDeliveryDate,

                Message = "Parcel status and actual dates updated successfully."

            });

        }



        // ADMIN: View all parcels

        [Authorize(Roles = "Admin, Handler")]

        [HttpGet("all-parcels")]

        public async Task<IActionResult> GetAllParcels()

        {

            var parcels = await _context.Set<Parcel>().ToListAsync();

            return Ok(parcels);

        }



        // ADMIN: View a single parcel by tracking ID

        [Authorize(Roles = "Admin, Handler")] // <--- ADDED "Handler" role here

        [HttpGet("{trackingId}")] // This is the /api/Parcel/{trackingId} endpoint

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

