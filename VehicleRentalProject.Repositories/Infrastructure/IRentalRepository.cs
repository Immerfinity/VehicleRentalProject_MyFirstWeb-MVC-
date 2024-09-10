using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleRentalProject.Models;

namespace VehicleRentalProject.Repositories.Infrastructure
{
    public interface IRentalRepository
    {
        Task<IEnumerable<Rental>> GetAllRentals();
        Task<Rental> GetRentalById(int id);
        Task UpdateRental(Rental rental);
        Task<bool> HasPendingRental(string userId);
        Task<bool> IsVehicleAvailable(int vehicleId, DateTime startDate, DateTime? endDate);
        Task DeleteRentalAsync(int id);
        Task<IEnumerable<Rental>> GetFinedRentalsAsync();
    }
}
