namespace SmartParcel.API.DTOs
{
    public class UpdateStatusRequest
    {
        public string TrackingId { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
    }
}
