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

        public string? Description { get; set; } // e.g., books, glass, fragile
        [Required(ErrorMessage = "Weight is required.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Weight must be greater than zero.")]
        public decimal Weight { get; set; }

       // [Required(ErrorMessage = "Weight unit is required.")]
       // [RegularExpression("^(g|kg|lb)$", ErrorMessage = "Weight unit must be 'g', 'kg', or 'lb'.")]
        //public string? WeightUnit { get; set; }



        [Required(ErrorMessage = "Pickup location is required.")]
        public string? PickupLocation { get; set; }

        [Required(ErrorMessage = "Delivery location is required.")]
        public string? DeliveryLocation { get; set; }

        [Required(ErrorMessage = "Pickup date is required.")]
        [DataType(DataType.Date)]
        public DateTime PickupDate { get; set; }

        [Required(ErrorMessage = "Delivery date is required.")]
        [DataType(DataType.Date)]
        public DateTime DeliveryDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DeliveryDate <= PickupDate)
            {
                yield return new ValidationResult(
                    "Delivery date must be after pickup date.",
                    new[] { nameof(DeliveryDate) });
            }
        }
    }
}


