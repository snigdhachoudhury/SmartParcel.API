namespace SmartParcel.API.Models
{
    public class PricingTier
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // "Standard", "Express", "Premium"
        public decimal BasePrice { get; set; }
        public decimal PricePerKg { get; set; }
        public int EstimatedDeliveryDays { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
    }
}