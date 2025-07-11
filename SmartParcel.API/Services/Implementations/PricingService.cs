using Microsoft.EntityFrameworkCore;
using SmartParcel.API.Data;
using SmartParcel.API.Models;
using SmartParcel.API.Services.Interfaces;

namespace SmartParcel.API.Services.Implementations
{
    public class PricingService : IPricingService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PricingService> _logger;

        public PricingService(AppDbContext context, ILogger<PricingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<decimal> CalculateShippingCostAsync(decimal weight, int pricingTierId, string pickupLocation, string deliveryLocation)
        {
            var pricingTier = await _context.PricingTiers.FindAsync(pricingTierId);
            if (pricingTier == null)
            {
                _logger.LogWarning("Invalid pricing tier ID: {PricingTierId}", pricingTierId);
                throw new ArgumentException("Invalid pricing tier");
            }

            // Base calculation
            decimal cost = pricingTier.BasePrice;
            
            // Add weight-based calculation
            cost += weight * pricingTier.PricePerKg;
            
            // You could extend this with distance calculations between locations
            // using a mapping service API in a production environment
            
            return Math.Round(cost, 2);
        }

        public async Task<List<PricingTier>> GetActivePricingTiersAsync()
        {
            return await _context.PricingTiers
                .Where(pt => pt.IsActive)
                .OrderBy(pt => pt.BasePrice)
                .ToListAsync();
        }
    }
}