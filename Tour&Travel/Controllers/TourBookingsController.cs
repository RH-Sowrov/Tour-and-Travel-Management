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
    public class TourBookingsController : ControllerBase
    {
        private readonly TourDbContext _context;

        public TourBookingsController(TourDbContext context)
        {
            _context = context;
        }

        // GET: api/TourBookings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TourBooking>>> GetTourBookings()
        {
            return await _context.TourBookings.ToListAsync();
        }

        // GET: api/TourBookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TourBooking>> GetTourBooking(int id)
        {
            var tourBooking = await _context.TourBookings.FindAsync(id);

            if (tourBooking == null)
            {
                return NotFound();
            }

            return tourBooking;
        }

        // PUT: api/TourBookings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTourBooking(int id, TourBooking tourBooking)
        {
            if (id != tourBooking.TourBookingId)
            {
                return BadRequest();
            }

            var existingBooking = await _context.TourBookings.FindAsync(id);
            if (existingBooking == null)
            {
                return NotFound();
            }

            // If the tour or booking date is being changed, adjust slots accordingly
            if (existingBooking.TourId != tourBooking.TourId || existingBooking.BookingDate.Date != tourBooking.BookingDate.Date)
            {
                var selectedTour = await _context.Tours.Include(t => t.TourPackage).FirstOrDefaultAsync(t => t.TourId == tourBooking.TourId);
                if (selectedTour == null)
                {
                    return BadRequest("Invalid tour selected.");
                }

                // Adjust slots for the existing booking
                var existingAvailability = await _context.TourAvailabilities.FirstOrDefaultAsync(avail => avail.TourId == existingBooking.TourId);
                if (existingAvailability != null)
                {
                    existingAvailability.AvailableSlots += existingBooking.NumberOfPersons;
                }

                // Check availability for the new booking
                var newAvailability = await _context.TourAvailabilities.FirstOrDefaultAsync(avail => avail.TourId == tourBooking.TourId);
                if (newAvailability != null)
                {
                    if (newAvailability.AvailableSlots >= tourBooking.NumberOfPersons)
                    {
                        newAvailability.AvailableSlots -= tourBooking.NumberOfPersons;
                    }
                    else
                    {
                        return BadRequest("Not enough available slots for the selected tour.");
                    }
                }
                else
                {
                    return BadRequest("Tour availability information not found.");
                }
            }

            // Update the existing booking
            existingBooking.TourId = tourBooking.TourId;
            existingBooking.BookingDate = tourBooking.BookingDate;
            existingBooking.NumberOfPersons = tourBooking.NumberOfPersons;
            existingBooking.PromotionId = tourBooking.PromotionId;
            existingBooking.TotalAmount = tourBooking.TotalAmount;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TourBookingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(existingBooking);
        }

        // POST: api/TourBookings
        [HttpPost]
        public async Task<ActionResult<TourBooking>> PostTourBooking(TourBooking tourBooking)
        {
            var userExists = await _context.Users.AnyAsync(u => u.UserId == tourBooking.UserId);
            if (!userExists)
            {
                return BadRequest("Invalid user Id provided.");
            }
            var selectedTour = await _context.Tours.Include(t => t.TourPackage).FirstOrDefaultAsync(t => t.TourId == tourBooking.TourId);
            if (selectedTour == null)
            {
                return BadRequest("Invalid tour selected.");
            }
            decimal totalAmount = selectedTour.TourPackage.TotalAmount * tourBooking.NumberOfPersons;
            if (tourBooking.PromotionId != 0)
            {
                var promotion = await _context.Promotions.FindAsync(tourBooking.PromotionId);
                if (promotion != null)
                {
                    totalAmount -= (totalAmount * promotion.Discount / 100);
                }
                else
                {
                    return BadRequest("Invalid promotion Id provided.");
                }
            }

            tourBooking.TotalAmount = totalAmount;

            // Save the booking
            _context.TourBookings.Add(tourBooking);
            await _context.SaveChangesAsync();

            // Adjust availability
            var tourAvailability = await _context.TourAvailabilities.FirstOrDefaultAsync(avail => avail.TourId == tourBooking.TourId);
            if (tourAvailability != null)
            {
                tourAvailability.AvailableSlots -= tourBooking.NumberOfPersons;
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction("GetTourBooking", new { id = tourBooking.TourBookingId }, tourBooking);
        }

        // DELETE: api/TourBookings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTourBooking(int id)
        {
            var tourBooking = await _context.TourBookings.FindAsync(id);
            if (tourBooking == null)
            {
                return NotFound();
            }

            // Increase available slots in the database
            var tourAvailability = await _context.TourAvailabilities.FirstOrDefaultAsync(avail => avail.TourId == tourBooking.TourId);
            if (tourAvailability != null)
            {
                tourAvailability.AvailableSlots += tourBooking.NumberOfPersons;
                await _context.SaveChangesAsync();
            }

            _context.TourBookings.Remove(tourBooking);
            await _context.SaveChangesAsync();

            return Ok("Successfully Deleted");
        }

        private bool TourBookingExists(int id)
        {
            return _context.TourBookings.Any(e => e.TourBookingId == id);
        }
    }
}
