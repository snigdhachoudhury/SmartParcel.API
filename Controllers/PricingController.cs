using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParcel.API.Data;
using SmartParcel.API.DTOs;
using SmartParcel.API.Models;
using SmartParcel.API.Services.Interfaces;

namespace SmartParcel.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PricingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPricingService _pricingService;
        private readonly ILogger<PricingController> _logger;

        public PricingController(AppDbContext context, IPricingService pricingService, ILogger<PricingController> logger)
        {
            _context = context;
            _pricingService = pricingService;
            _logger = logger;
        }

        [HttpGet("tiers")]
        public async Task<IActionResult> GetPricingTiers()
        {
            var tiers = await _pricingService.GetActivePricingTiersAsync();
            return Ok(tiers);
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateCost([FromBody] ShippingCostRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            try
            {
                decimal cost = await _pricingService.CalculateShippingCostAsync(
                    request.Weight,
                    request.PricingTierId,
                    request.PickupLocation,
                    request.DeliveryLocation
                );
                
                return Ok(new { Cost = cost });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating shipping cost");
                return BadRequest(new { Message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("tier")]
        public async Task<IActionResult> CreatePricingTier([FromBody] PricingTier tier)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            _context.PricingTiers.Add(tier);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetPricingTierById), new { id = tier.Id }, tier);
        }

        [HttpGet("tier/{id}")]
        public async Task<IActionResult> GetPricingTierById(int id)
        {
            var tier = await _context.PricingTiers.FindAsync(id);
            if (tier == null)
                return NotFound();
                
            return Ok(tier);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("tier/{id}")]
        public async Task<IActionResult> UpdatePricingTier(int id, [FromBody] PricingTier tier)
        {
            if (id != tier.Id)
                return BadRequest();
                
            _context.Entry(tier).State = EntityState.Modified;
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.PricingTiers.AnyAsync(e => e.Id == id))
                    return NotFound();
                throw;
            }
            
            return NoContent();
        }
    }
}