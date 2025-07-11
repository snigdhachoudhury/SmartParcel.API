using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartParcel.API.DTOs
{
    public class CreateParcelRequest : IValidatableObject
    {
        [Required(ErrorMessage = "Sender email is required.")]
        [EmailAddress(ErrorMessage = "Invalid sender email format.")]
        public string? SenderEmail { get; set; }

        [Required(ErrorMessage = "Recipient email is required.")]
        [EmailAddress(ErrorMessage = "Invalid recipient email format.")]
        public string? RecipientEmail { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Weight is required.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Weight must be greater than zero.")]
        public decimal Weight { get; set; }

        [Required(ErrorMessage = "Pickup location is required.")]
        public string? PickupLocation { get; set; }

        [Required(ErrorMessage = "Delivery location is required.")]
        public string? DeliveryLocation { get; set; }

        [Required(ErrorMessage = "Expected pickup date is required.")]
        public DateTime ExpectedPickupDate { get; set; }

        [Required(ErrorMessage = "Expected delivery date is required.")]
        public DateTime ExpectedDeliveryDate { get; set; }

        [Required(ErrorMessage = "Pricing tier is required.")]
        public int PricingTierId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ExpectedDeliveryDate <= ExpectedPickupDate)
            {
                yield return new ValidationResult(
                    "Expected delivery date must be after expected pickup date.",
                    new[] { nameof(ExpectedDeliveryDate) });
            }
        }
    }
}