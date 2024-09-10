using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleRentalProject.Models;
using VehicleRentalProject.Repositories.Infrastructure;

namespace VehicleRentalProject.Repositories.Implementation
{
    public class UserService : IUserService
    {
        private CarContext _context;
        public UserService(CarContext context)
        {
            _context = context;
        }
        public ApplicationUser GetApplicationUser(string userId)
        {
            var applicationUser = _context.ApplicationUsers.Where(x => x.Id == userId).FirstOrDefault();
            return applicationUser;
        }
    }
}
