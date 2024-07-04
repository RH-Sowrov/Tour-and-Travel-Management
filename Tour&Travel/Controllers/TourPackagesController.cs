using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tour_Travel.Data;
using Tour_Travel.Models;

namespace Tour_Travel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourPackagesController : ControllerBase
    {
        private readonly TourDbContext _context;

        public TourPackagesController(TourDbContext context)
        {
            _context = context;
        }

        // GET: api/TourPackages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TourPackage>>> GetTourPackages()
        {
            
            return await _context.TourPackages.ToListAsync();
        }

        // GET: api/TourPackages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TourPackage>> GetTourPackage(int id)
        {
            var tourPackage = await _context.TourPackages.FindAsync(id);

            if (tourPackage == null)
            {
                return NotFound();
            }

            return tourPackage;
        }

        // PUT: api/TourPackages/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTourPackage(int id, TourPackage tourPackage)
        {
            if (id != tourPackage.TourPackageId)
            {
                return BadRequest();
            }

            _context.Entry(tourPackage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TourPackageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(tourPackage);
        }

        // POST: api/TourPackages
        [HttpPost]
        public async Task<ActionResult<TourPackage>> PostTourPackage(TourPackage tourPackage)
        {
            var totalPrice = _context.TourTransports
                .Where(t => t.TourTransportId == tourPackage.TourTransportId)
                .Select(t => t.RentPerPerson)
                .FirstOrDefault();

            totalPrice += _context.TourFoods
                .Where(f => f.TourFoodId == tourPackage.TourFoodId)
                .Select(f => f.Price)
                .FirstOrDefault();

            totalPrice += _context.TourGuides
                .Where(g => g.TourGuideId == tourPackage.TourGuideId)
                .Select(g => g.GuideFee)
                .FirstOrDefault();

            totalPrice += _context.TourHotels
                .Where(h => h.TourHotelId == tourPackage.TourHotelId)
                .Select(h => h.PricePerNight)
                .FirstOrDefault();

            tourPackage.TotalAmount = totalPrice;

            _context.TourPackages.Add(tourPackage);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTourPackage", new { id = tourPackage.TourPackageId }, tourPackage);
        }

        // DELETE: api/TourPackages/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTourPackage(int id)
        {
            var tourPackage = await _context.TourPackages.FindAsync(id);
            if (tourPackage == null)
            {
                return NotFound();
            }

            _context.TourPackages.Remove(tourPackage);
            await _context.SaveChangesAsync();

            return Ok("Successfully Deleted");
        }

        private bool TourPackageExists(int id)
        {
            return _context.TourPackages.Any(e => e.TourPackageId == id);
        }
    }
}
