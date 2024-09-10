using AutoMapper;
using VehicleRentalProject.Models;
using VehicleRentalProject.Models.ViewModels.Vehicle;
using VehicleRentalProject.Utility;
using VehicleRentalProject.Web.Models.ViewModels.Rental;
using VehicleRentalProject.Web.Models.ViewModels.Vehicle;

namespace VehicleRentalProject.Mapper
{
    public class VehicleProfile : Profile
    {
        private IWebHostEnvironment _WebHostEnvironment;
        public VehicleProfile(IWebHostEnvironment webHostEnvironment)
        {
            _WebHostEnvironment = webHostEnvironment;
            CreateMap<Vehicle, VehicleViewModel>();

            CreateMap<CreateVehicleViewModel, Vehicle>()
                .ForMember(dest => dest.VehicleImage,
                opt => opt.MapFrom(src => new ImageUpload(_WebHostEnvironment).SaveImageFile(src.VehicleImageUrl)));

            CreateMap<Vehicle, EditVehicleViewModel>()
                .ForMember(dest => dest.VehicleImageUrl, opt => opt.Ignore());

            CreateMap<Vehicle, VehicleDetailsViewModel>()
                .ForMember(dest => dest.StartDate, opt => opt.Ignore())
                .ForMember(dest => dest.EndDate, opt => opt.Ignore());

            CreateMap<EditVehicleViewModel, Vehicle>()
                .ForMember(dest => dest.VehicleImage,
                opt => opt.MapFrom(src => new ImageUpload(_WebHostEnvironment).SaveImageFile(src.VehicleImageUrl)));

            CreateMap<Rental, RentalHistoryViewModel>()
           .ForMember(dest => dest.RentalId, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.VehicleNumber, opt => opt.MapFrom(src => src.Vehicle.VehicleNumber))
           .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.RentalStatus));

            CreateMap<Rental, UpdateRentalStatusViewModel>()
                .ForMember(dest => dest.VehicleNumber, opt => opt.MapFrom(src => src.Vehicle.VehicleNumber));

            CreateMap<UpdateRentalStatusViewModel, Rental>();
        }

    }
}
