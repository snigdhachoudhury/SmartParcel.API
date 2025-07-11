using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartParcel.API.Models
{
    public class Parcel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string TrackingId { get; set; }
        public required string SenderEmail { get; set; }
        public required string RecipientEmail { get; set; }
        public required string Description { get; set; }
        public required string Weight { get; set; }
        public required string PickupLocation { get; set; }
        public required string DeliveryLocation { get; set; }
        public DateTime ExpectedPickupDate { get; set; }
        public DateTime ExpectedDeliveryDate { get; set; }
        public DateTime? ActualPickupDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // OTP properties
        public string? DeliveryOTP { get; set; }
        public DateTime? OTPGeneratedAt { get; set; }
        public bool IsOTPVerified { get; set; }

        // Shipping cost and pricing tier properties
        public decimal ShippingCost { get; set; }
        public int? PricingTierId { get; set; }
        public PricingTier? PricingTier { get; set; } // Navigation property
    }
}