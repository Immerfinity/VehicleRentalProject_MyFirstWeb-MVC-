using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleRentalProject.Models;

namespace VehicleRentalProject.Repositories.Infrastructure
{
    public class RentalRepository : IRentalRepository
    {
        private readonly CarContext _context;

        public RentalRepository(CarContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Rental>> GetAllRentals()
        {
            return await _context.Rentals.Include(r => r.Vehicle).Include(r => r.ApplicationUser).ToListAsync();
        }

        public async Task<Rental> GetRentalById(int id)
        {
            return await _context.Rentals.Include(r => r.Vehicle).FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task UpdateRental(Rental rental)
        {
            var rentalFromDb = await _context.Rentals.FindAsync(rental.Id);
            if (rentalFromDb != null)
            {
                rentalFromDb.Vehicle.VehicleNumber = rental.Vehicle.VehicleNumber;
                rentalFromDb.StartDate = rental.StartDate;
                rentalFromDb.ReturnDate = rental.ReturnDate;
                rentalFromDb.TotalPrice = rental.TotalPrice;
                rentalFromDb.RentalStatus = rental.RentalStatus;

                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasPendingRental(string userId)
        {
            return await _context.Rentals.AnyAsync(r => r.ApplicationUserId == userId && r.RentalStatus == "На рассмотрении");
        }

        public async Task<bool> IsVehicleAvailable(int vehicleId, DateTime startDate, DateTime? endDate)
        {
            return !await _context.Rentals.AnyAsync(r => r.VehicleId == vehicleId &&
                                                         r.RentalStatus != "Не одобрено" &&
                                                         ((r.StartDate <= endDate && r.ReturnDate >= startDate) ||
                                                         (r.StartDate <= startDate && r.ReturnDate >= endDate) ||
                                                         (r.StartDate >= startDate && r.ReturnDate <= endDate)));
        }

        public async Task DeleteRentalAsync(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null)
            {
                _context.Rentals.Remove(rental);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Rental>> GetFinedRentalsAsync()
        {
            return await _context.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.ApplicationUser)
                .Where(r => r.PenaltyAmount > 0)
                .ToListAsync();
        }
    }
}
