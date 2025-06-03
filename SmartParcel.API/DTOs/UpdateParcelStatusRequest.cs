using System.ComponentModel.DataAnnotations;

namespace SmartParcel.API.DTOs
{
    public class UpdateParcelStatusRequest
    {
        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(Created|In Transit to Handler|In transit to recipient|In Warehouse|Delivered|Out for Delivery|Returned to Sender|Received by Handler)$",
            ErrorMessage = "Invalid status value")]
        public string Status { get; set; } = null!;

        [Required(ErrorMessage = "Current location is required")]
        public string Location { get; set; } = null!;

        public string? Notes { get; set; }

        // Optional actual dates that handlers can set
        [DataType(DataType.DateTime)]
        public DateTime? ActualPickupDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ActualDeliveryDate { get; set; }

        // Basic validation can be added if needed
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ActualDeliveryDate.HasValue && ActualPickupDate.HasValue)
            {
                if (ActualDeliveryDate.Value < ActualPickupDate.Value)
                {
                    yield return new ValidationResult(
                        "Actual delivery date cannot be earlier than actual pickup date",
                        new[] { nameof(ActualDeliveryDate) }
                    );
                }
            }
        }
    }
}
