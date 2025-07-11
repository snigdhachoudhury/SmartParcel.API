using System.ComponentModel.DataAnnotations;

namespace SmartParcel.API.DTOs
{
    public class TamperResolveRequest
    {
        [Required]
        public string TrackingId { get; set; } = string.Empty;
        
        [Required]
        public string Resolution { get; set; } = string.Empty;
        
        [Required]
        public string NextStatus { get; set; } = string.Empty;
        
        public string? Location { get; set; }
    }
}
