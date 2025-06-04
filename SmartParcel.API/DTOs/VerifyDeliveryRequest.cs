using System.ComponentModel.DataAnnotations;

namespace SmartParcel.API.DTOs
{
    public class VerifyDeliveryRequest
    {
        [Required]
        public string TrackingId { get; set; } = string.Empty;

        [Required]
        public string EmailOTP { get; set; } = string.Empty;
    }
}
