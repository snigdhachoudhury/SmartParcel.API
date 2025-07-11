using SmartParcel.API.Models;

namespace SmartParcel.API.Services.Interfaces
{
    public interface IPricingService
    {
        Task<decimal> CalculateShippingCostAsync(decimal weight, int pricingTierId, string pickupLocation, string deliveryLocation);
        Task<List<PricingTier>> GetActivePricingTiersAsync();
    }
}