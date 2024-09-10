using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Policy;
using System.Web.Http.ModelBinding;
using VehicleRentalProject.Models.ViewModels.Vehicle;
using VehicleRentalProject.Models;
using VehicleRentalProject.Repositories.Infrastructure;
using VehicleRentalProject.Web.Models.ViewModels.Vehicle;
using Microsoft.AspNetCore.Authorization;

public class HomeController : Controller
{
    private IVehicleRepository _vehicleRepo;
    private readonly IRentalRepository _rentalRepository;
    private IMapper _mapper;
    private IUserService _userService;

    public HomeController(IVehicleRepository vehicleRepo, IRentalRepository rentalRepository, IMapper mapper, IUserService userService)
    {
        _vehicleRepo = vehicleRepo;
        _rentalRepository = rentalRepository;
        _mapper = mapper;
        _userService = userService;
    }
    //Получения данных из представления
    public async Task<IActionResult> Index(string search, string type, string sortOrder)
    {
        //Получения данных об автомобилях из БД
        var vehicles = _vehicleRepo.GetVehicles().GetAwaiter().GetResult().ToList().Where(x => !x.IsDeleted && x.IsAvailable);

        //Фильтрация по Типу автомобиля
        if (!string.IsNullOrEmpty(type))
        {
            vehicles = vehicles.Where(x => x.VehicleType == type);
        }

        //Поиск по названию автомобиля
        if (!string.IsNullOrEmpty(search))
        {
            vehicles = vehicles.Where(x => x.VehicleName.Contains(search));
        }

        //Сортировка по цене
        switch (sortOrder)
        {
            case "rate_desc":
                vehicles = vehicles.OrderByDescending(x => x.DailyRate);
                break;
            case "rate_asc":
                vehicles = vehicles.OrderBy(x => x.DailyRate);
                break;
            default:
                vehicles = vehicles.OrderBy(x => x.VehicleName);
                break;
        }

        //Формирование и возврат списка автомобилей
        var vm = _mapper.Map<List<VehicleViewModel>>(vehicles);
        return View(vm);
    }

    public async Task<IActionResult> Details(int id, string returnUrl = null)
    {
        var vehicle = await _vehicleRepo.GetVehicleById(id);
        if (vehicle == null)
        {
            return NotFound();
        }
        var vm = _mapper.Map<VehicleDetailsViewModel>(vehicle);
        ViewData["ReturnUrl"] = returnUrl ?? Url.Action("Details", new { id = id });
        return View(vm);
    }

    [HttpPost]
    public IActionResult Summary(VehicleDetailsViewModel vm, string returnUrl = null)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Challenge(new AuthenticationProperties { RedirectUri = returnUrl });
        }

        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        var applicationUser = _userService.GetApplicationUser(claims.Value);

        if (vm.EndDate < vm.StartDate)
        {
            ModelState.AddModelError("EndDate", "Дата окончания аренды не может быть раньше даты начала аренды.");
        }

        if (ModelState.IsValid)
        {
            TimeSpan duration = (TimeSpan)(vm.EndDate - vm.StartDate);
            vm.TotalAmount = vm.DailyRate * duration.Days;
            var viewModel = new SummaryViewModel
            {
                VehicleType = vm.VehicleType,
                EndDate = vm.EndDate,
                StartDate = vm.StartDate,
                VehicleNumber = vm.VehicleNumber,
                TotalAmount = vm.TotalAmount,
                TotalDuration = duration.Days,
                VehicleImage = vm.VehicleImage,
                Id = vm.Id,
                ApplicationUser = applicationUser
            };

            return View(viewModel);
        }

        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CompleteOrder(SummaryViewModel viewModel)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        var applicationUser = _userService.GetApplicationUser(claims.Value);

        if (await _rentalRepository.HasPendingRental(applicationUser.Id))
        {
            return Json(new { success = false, error = "У вас уже есть заявка на рассмотрении." });
        }

        if (!await _rentalRepository.IsVehicleAvailable(viewModel.Id, viewModel.StartDate, viewModel.EndDate))
        {
            return Json(new { success = false, error = "Этот автомобиль уже забронирован на выбранные даты." });
        }

        var rental = new Rental
        {
            StartDate = viewModel.StartDate,
            ReturnDate = viewModel.EndDate,
            TotalPrice = viewModel.TotalAmount,
            IsPaid = false,
            IsApproved = false,
            IsDeleted = false,
            CreatedAt = DateTime.Now,
            VehicleId = viewModel.Id,
            ApplicationUserId = applicationUser.Id,
            RentalStatus = "На рассмотрении"
        };

        _vehicleRepo.AddRental(rental);

        return Json(new { success = true });
    }
}
