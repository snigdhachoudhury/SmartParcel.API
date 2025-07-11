namespace SmartParcel.API.DTOs
{
    public class UpdateParcelDatesRequest
    {
        public DateTime PickupDate { get; set; }
        public DateTime DeliveryDate { get; set; }
    }
}
