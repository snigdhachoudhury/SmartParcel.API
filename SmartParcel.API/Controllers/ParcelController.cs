using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Required for ToListAsync and FirstOrDefaultAsync
using QRCoder; // You'll need to install QRCoder NuGet package
using SmartParcel.API.Data;
using SmartParcel.API.DTOs;
using SmartParcel.API.Models;
using SmartParcel.API.Services.Implementations;
using SmartParcel.API.Services.Interfaces;
using System.Drawing;
using System.IO;
using System.Security.Claims; // Required for ClaimTypes
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;


namespace SmartParcel.API.Controllers

{

    [ApiController]

    [Route("api/[controller]")]

    public class ParcelController : ControllerBase

    {

        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<ParcelController> _logger;
        private readonly Random _random = new Random();

        public ParcelController(AppDbContext context, IEmailService emailService, ILogger<ParcelController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
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
            var qrCodeBase64 = GenerateQRCode(trackingId);

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

            return Ok(new { parcel.TrackingId, QRCode = qrCodeBase64, Message = "Parcel created successfully." });
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

            parcel.Status = "Scanned";  // Changed from "Delivered" to "Scanned"
            parcel.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

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
        [Authorize(Roles = "Handler")]
        private string DecodeQRCode(Bitmap bitmap)
        {
            // Convert Bitmap to byte array
            byte[] bitmapBytes;
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                bitmapBytes = memoryStream.ToArray();
            }

            // Convert byte array to LuminanceSource using RGBLuminanceSource
            var luminanceSource = new RGBLuminanceSource(bitmapBytes, bitmap.Width, bitmap.Height, RGBLuminanceSource.BitmapFormat.BGR32);
            var binarizer = new ZXing.Common.HybridBinarizer(luminanceSource);
            var binaryBitmap = new ZXing.BinaryBitmap(binarizer);

            var reader = new ZXing.BarcodeReaderGeneric
            {
                Options = new ZXing.Common.DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new[] { ZXing.BarcodeFormat.QR_CODE }
                }
            };

            // Fix: Pass LuminanceSource instead of BinaryBitmap
            var result = reader.Decode(luminanceSource);
            return result?.Text ?? string.Empty; // Return an empty string if result is null
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

        [Authorize(Roles = "Handler")]
        [HttpPost("initiate-delivery/{trackingId}")]
        public async Task<IActionResult> InitiateDelivery(string trackingId)
        {
            var parcel = await _context.Parcels
                .FirstOrDefaultAsync(p => p.TrackingId == trackingId);

            if (parcel == null)
                return NotFound(new { Message = "Parcel not found." });

            if (parcel.Status == "Delivered")
                return BadRequest(new { Message = "Parcel is already delivered." });

            // Generate 6-digit OTP
            string otp = GenerateOTP();
            parcel.DeliveryOTP = otp;
            parcel.OTPGeneratedAt = DateTime.UtcNow;
            parcel.IsOTPVerified = false;
            parcel.Status = "Out for Delivery";

            await _context.SaveChangesAsync();

            try
            {
                string subject = "SmartParcel Delivery OTP";
                string message = $@"
            <h2>SmartParcel Delivery Verification</h2>
            <p>Your SmartParcel delivery OTP is: <strong>{otp}</strong></p>
            <p>Please share this with the delivery handler to confirm receipt of your parcel.</p>
            <p>Tracking ID: {trackingId}</p>
            <p>This OTP will expire in 30 minutes.</p>";

                await _emailService.SendEmailAsync(parcel.RecipientEmail, subject, message);

                return Ok(new { Message = "OTP has been sent to recipient's email." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP");
                return StatusCode(500, new { Message = "Failed to send OTP. Please try again." });
            }
        }

        [Authorize(Roles = "Handler")]
        [HttpPost("verify-delivery")]
        public async Task<IActionResult> VerifyDelivery([FromBody] VerifyDeliveryRequest request)
        {
            var parcel = await _context.Parcels
                .FirstOrDefaultAsync(p => p.TrackingId == request.TrackingId);

            if (parcel == null)
                return NotFound(new { Message = "Parcel not found." });

            if (parcel.Status == "Delivered")
                return BadRequest(new { Message = "Parcel is already delivered." });

            // Verify OTP
            if (parcel.DeliveryOTP != request.EmailOTP)
                return BadRequest(new { Message = "Invalid OTP." });

            // Check if OTP is expired (30 minutes validity)
            if (parcel.OTPGeneratedAt?.AddMinutes(30) < DateTime.UtcNow)
                return BadRequest(new { Message = "OTP has expired. Please request a new one." });

            // Update parcel status
            parcel.Status = "Delivered";
            parcel.ActualDeliveryDate = DateTime.UtcNow;
            parcel.IsOTPVerified = true;
            parcel.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Delivery confirmed successfully." });
        }
        private string GenerateOTP()
        {
            // Generate a random 6-digit OTP
            return _random.Next(100000, 999999).ToString();
        }
        // Add this method to ParcelController
        private string GenerateQRCode(string trackingId)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(trackingId, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrCodeBytes);
        }

        [Authorize(Roles = "Handler")]
        [HttpPost("scan-qr")]
        public async Task<IActionResult> ScanQRCode([FromBody] string qrCodeImage)
        {
            try
            {
                _logger.LogInformation("Starting QR code scan");

                if (string.IsNullOrEmpty(qrCodeImage))
                {
                    _logger.LogWarning("Received empty QR code image");
                    return BadRequest(new { Message = "No image data received" });
                }

                // Remove data URL prefix if present
                qrCodeImage = qrCodeImage.Replace("data:image/png;base64,", "")
                                        .Replace("data:image/jpeg;base64,", "")
                                        .Replace("data:image/jpg;base64,", "");

                byte[] imageBytes = Convert.FromBase64String(qrCodeImage);

                using var stream = new MemoryStream(imageBytes);
                using var bitmap = new Bitmap(stream);

                string trackingId = DecodeQRCode(bitmap);

                if (string.IsNullOrEmpty(trackingId))
                {
                    _logger.LogWarning("Could not decode QR code");
                    return BadRequest(new { Message = "Could not read QR code from image." });
                }

                _logger.LogInformation($"Successfully decoded QR code with tracking ID: {trackingId}");

                // Use the existing scan endpoint logic
                return await ScanParcel(trackingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing QR code");
                return StatusCode(500, new { Message = $"Error processing QR code: {ex.Message}" });
            }
        }
    }
}