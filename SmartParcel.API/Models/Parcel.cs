namespace SmartParcel.API.Models
{
    public class Parcel
    {
        public int Id { get; set; }
        public string? TrackingId { get; set; }
        public string? SenderEmail { get; set; }
        public string? RecipientEmail { get; set; }
        public string? Description { get; set; }
        public string? Weight { get; set; }
        //public string? Dimensions { get; set; }
        public string? PickupLocation { get; set; }
        public string? DeliveryLocation { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string Status { get; set; } = "Created";
        public DateTime CreatedAt { get; internal set; } = DateTime.UtcNow; // Fix: Initialize with a default value
    }
}

 