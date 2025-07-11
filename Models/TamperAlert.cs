using System.ComponentModel.DataAnnotations;

namespace SmartParcel.API.Models
{
    public class TamperAlert
    {
        public int Id { get; set; }
        
        [Required]
        public string TrackingId { get; set; } = string.Empty;  // Required - every alert needs a parcel
        
        [Required]
        public string Reason { get; set; } = string.Empty;      // Required - document why tampering is suspected
        
        public DateTime Timestamp { get; set; }
        
        public string? ReportedBy { get; set; }                 // Track who reported the tampering
        
        public string? Location { get; set; }                   // Where tampering was detected
        
        // Resolution tracking
        public bool IsResolved { get; set; } = false;
        public string? Resolution { get; set; }                 // How the issue was resolved
        public DateTime? ResolvedAt { get; set; }               // When it was resolved
        public string? ResolvedBy { get; set; }                 // Who resolved it
    }
}
