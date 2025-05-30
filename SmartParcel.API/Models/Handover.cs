namespace SmartParcel.API.Models
{
    public class Handover

    {
        public int Id { get; set; }
        public string? TrackingId { get; set; }
        public string? HandledBy { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Location { get; set; }
    }
}

