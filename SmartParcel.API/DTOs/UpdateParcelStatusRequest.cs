namespace SmartParcel.API.DTOs
{
    public class UpdateParcelStatusRequest
    {
        public required string Status { get; set; }
        public required string Location { get; set; } // Marked as required to ensure non-null value
        public string? Notes { get; set; } // Optional property already referenced in the controller
    }
}
