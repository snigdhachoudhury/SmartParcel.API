using System.ComponentModel.DataAnnotations;

namespace SmartParcel.API.DTOs
{
    public class TamperReportRequest
    {
        [Required]
        public string TrackingId { get; set; } = string.Empty;
        
        [Required]
        public string Reason { get; set; } = string.Empty;
        
        public string? Location { get; set; }
    }
}
