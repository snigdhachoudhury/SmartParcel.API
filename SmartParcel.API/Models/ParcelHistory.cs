namespace SmartParcel.API.Models
{
    public class ParcelHistory
    {
        public int Id { get; set; }
        public required string TrackingId { get; set; }
        public required string Status { get; set; }
        public string? Location { get; set; }
        public string? Notes { get; set; }
        public string? HandledBy { get; set; }

        public DateTime Timestamp { get; set; }

        //navigation properties
        public Parcel? Parcel { get; set; }
    }
}
