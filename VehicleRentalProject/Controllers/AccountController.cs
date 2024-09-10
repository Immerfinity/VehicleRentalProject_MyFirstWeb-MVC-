using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using VehicleRentalProject.Models;
using VehicleRentalProject.Repositories.Infrastructure;
using VehicleRentalProject.Web.Models.ViewModels.Rental;


namespace VehicleRentalProject.Controllers
{
    [Authorize(Roles = "Customer")]
    public class AccountController : Controller
    {
        private readonly IVehicleRepository _vehicleRepo;
        private readonly IRentalRepository _rentalRepository;
        private readonly IUserService _userService;

        public AccountController(IVehicleRepository vehicleRepo, IUserService userService, IRentalRepository rentalRepository)
        {
            _vehicleRepo = vehicleRepo;
            _userService = userService;
            _rentalRepository = rentalRepository;
        }

        public async Task<IActionResult> History()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var applicationUser = _userService.GetApplicationUser(claims.Value);

            var rentals = await _vehicleRepo.GetRentalsByUserIdAsync(claims.Value);
            var vm = rentals.Select(r => new RentalHistoryViewModel
            {
                RentalId = r.Id,
                VehicleNumber = r.Vehicle.VehicleNumber,
                StartDate = r.StartDate,
                ReturnDate = r.ReturnDate,
                TotalPrice = r.TotalPrice,
                Status = r.RentalStatus,
                ApplicationUser = applicationUser,
                PenaltyAmount = r.PenaltyAmount
            }).ToList();

            return View(vm);
        }



        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var rental = await _rentalRepository.GetRentalById(id);
            if (rental != null && rental.RentalStatus == "На рассмотрении")
            {
                await _rentalRepository.DeleteRentalAsync(id);
                return RedirectToAction(nameof(History));
            }

            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
