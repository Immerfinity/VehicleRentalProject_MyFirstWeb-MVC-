using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleRentalProject.Models;
using VehicleRentalProject.Repositories.Infrastructure;

namespace VehicleRentalProject.Repositories.Implementation
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly CarContext _context;

        public VehicleRepository(CarContext context)
        {
            _context = context;
        }

        public async Task DeleteVehicle(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                _context.SaveChanges();
            }
        }

        public async Task<Vehicle> GetVehicleById(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                return vehicle;
            }
            throw new Exception($"Машина с ID: {id} не найдена");
        }

        public async Task<IEnumerable<Vehicle>> GetVehicles()
        {
            var vehicles = await _context.Vehicles.ToListAsync();
            return vehicles;
        }

        public async Task InsertVehicle(Vehicle vehicle)
        {
            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateVehicle(Vehicle vehicle)
        {
            var vehicleFromDb = await _context.Vehicles.FindAsync(vehicle.Id);
            if (vehicleFromDb != null)
            {
                vehicleFromDb.VehicleName = vehicle.VehicleName;
                vehicleFromDb.VehicleType = vehicle.VehicleType;
                vehicleFromDb.VehicleModel = vehicle.VehicleModel;
                vehicleFromDb.VehicleNumber = vehicle.VehicleNumber;
                vehicleFromDb.VehicleColor = vehicle.VehicleColor;
                vehicleFromDb.DailyRate = vehicle.DailyRate;
                vehicleFromDb.VehicleDescription = vehicle.VehicleDescription;
                if (vehicle.VehicleImage != null)
                {
                    vehicleFromDb.VehicleImage = vehicle.VehicleImage;
                }
                vehicleFromDb.VehiclePrice = vehicle.VehiclePrice;
                vehicleFromDb.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
            }
        }

        public void AddRental(Rental rental)
        {
            _context.Rentals.Add(rental);
            _context.SaveChanges();
        }

        public async Task<List<Rental>> GetRentalsByUserIdAsync(string userId)
        {
            return await _context.Rentals
                .Where(r => r.ApplicationUserId == userId && !r.IsDeleted)
                .Include(r => r.Vehicle)
                .ToListAsync();
        }
    }
}
