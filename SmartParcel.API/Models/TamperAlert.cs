namespace SmartParcel.API.Models
{
    public class TamperAlert
    {
        public int Id { get; set; }
        public string? TrackingId { get; set; }
        public string? Reason { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
