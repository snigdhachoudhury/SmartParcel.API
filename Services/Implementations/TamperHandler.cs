using Microsoft.EntityFrameworkCore;
using SmartParcel.API.Data;
using SmartParcel.API.Models;
using SmartParcel.API.Services.Interfaces;

namespace SmartParcel.API.Services.Implementations
{
    public class TamperHandler : ITamperHandler
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<TamperHandler> _logger;

        // Valid next statuses after tampering
        private readonly string[] _validTamperNextStatuses = new[] 
        { 
            ParcelStatus.TamperResolved, 
            ParcelStatus.ReturnedDueToDamage, 
            ParcelStatus.Lost 
        };

        public TamperHandler(AppDbContext context, IEmailService emailService, ILogger<TamperHandler> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<bool> ReportTamperingAsync(string trackingId, string reason, string? location)
        {
            var parcel = await _context.Parcels
                .FirstOrDefaultAsync(p => p.TrackingId == trackingId);

            if (parcel == null || parcel.Status == ParcelStatus.Delivered)
                return false;

            // Create tamper alert
            var tamperAlert = new TamperAlert
            {
                TrackingId = trackingId,
                Reason = reason,
                Timestamp = DateTime.UtcNow
            };

            // Update parcel status
            parcel.Status = ParcelStatus.Tampered;
            parcel.UpdatedAt = DateTime.UtcNow;
            
            _context.TamperAlerts.Add(tamperAlert);
            
            // Add history entry
            var history = new ParcelHistory
            {
                TrackingId = trackingId,
                Status = ParcelStatus.Tampered,
                Location = location,
                Notes = $"TAMPERING DETECTED: {reason}",
                Timestamp = DateTime.UtcNow
            };
            
            _context.ParcelHistory.Add(history);
            await _context.SaveChangesAsync();

            // Send notification
            try
            {
                string subject = $"ALERT: Package Tampering Detected - {trackingId}";
                string message = $@"
                <h2 style='color: #d00;'>⚠️ TAMPERING ALERT ⚠️</h2>
                <p>Tampering has been detected with your parcel:</p>
                <p><strong>Tracking ID:</strong> {trackingId}</p>
                <p><strong>Reason:</strong> {reason}</p>
                <p>This issue has been flagged for investigation.</p>";

                await _emailService.SendEmailAsync(parcel.SenderEmail, subject, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send tamper notification for {trackingId}");
            }

            return true;
        }

        public async Task<bool> ResolveTamperingAsync(string trackingId, string resolution, string nextStatus, string? location)
        {
            var parcel = await _context.Parcels
                .FirstOrDefaultAsync(p => p.TrackingId == trackingId);

            if (parcel == null || parcel.Status != ParcelStatus.Tampered)
                return false;

            if (!IsValidTamperResolutionStatus(nextStatus))
                return false;

            // Update parcel status
            parcel.Status = nextStatus;
            parcel.UpdatedAt = DateTime.UtcNow;
            
            // Add history entry
            var history = new ParcelHistory
            {
                TrackingId = trackingId,
                Status = nextStatus,
                Location = location,
                Notes = $"TAMPER RESOLVED: {resolution}",
                Timestamp = DateTime.UtcNow
            };
            
            _context.ParcelHistory.Add(history);
            await _context.SaveChangesAsync();

            // Send notification
            try
            {
                string subject = $"Update: Package Tampering Resolution - {trackingId}";
                string message = $@"
                <h2>Parcel Tampering Update</h2>
                <p>The tampering issue with your parcel has been addressed:</p>
                <p><strong>Tracking ID:</strong> {trackingId}</p>
                <p><strong>Resolution:</strong> {resolution}</p>
                <p><strong>New Status:</strong> {nextStatus}</p>";

                await _emailService.SendEmailAsync(parcel.SenderEmail, subject, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send tamper resolution notification for {trackingId}");
            }

            return true;
        }

        public bool IsValidTamperResolutionStatus(string status)
        {
            return _validTamperNextStatuses.Contains(status);
        }

        public bool IsValidTamperResolutionStatus(object nextStatus)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ResolveTamperingAsync(string trackingId, string resolution, string nextStatus, object location)
        {
            throw new NotImplementedException();
        }
    }
}
