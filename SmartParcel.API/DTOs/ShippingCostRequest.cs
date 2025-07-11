using System.ComponentModel.DataAnnotations;

namespace SmartParcel.API.DTOs
{
    public class ShippingCostRequest
    {
        [Required]
        [Range(0.001, double.MaxValue, ErrorMessage = "Weight must be greater than zero.")]
        public decimal Weight { get; set; }
        
        [Required]
        public int PricingTierId { get; set; }
        
        [Required]
        public string PickupLocation { get; set; } = string.Empty;
        
        [Required]
        public string DeliveryLocation { get; set; } = string.Empty;
    }
}