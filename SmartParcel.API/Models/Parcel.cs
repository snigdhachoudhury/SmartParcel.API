using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // For [NotMapped] if needed

namespace SmartParcel.API.Models
{
    public class Parcel
    {
        [Key]
        public Guid Id { get; set; } // Primary key
        public required string TrackingId { get; set; }
        public required string SenderEmail { get; set; }
        public required string RecipientEmail { get; set; }
        public required string Description { get; set; }
        public required string Weight { get; set; } // Assuming you'll keep this as string for now
        public required string PickupLocation { get; set; }
        public required string DeliveryLocation { get; set; }

        // Dates
        public DateTime ExpectedPickupDate { get; set; } // Expected date set by sender
        public DateTime ExpectedDeliveryDate { get; set; } // Expected date set by sender
        public DateTime? ActualPickupDate { get; set; } // Actual date set by handler (nullable)
        public DateTime? ActualDeliveryDate { get; set; } // Actual date set by handler (nullable)

        // Status and timestamps
        public string Status { get; set; } = "Created"; // Current status of the parcel
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } // Last update timestamp for the main parcel record
    }
}