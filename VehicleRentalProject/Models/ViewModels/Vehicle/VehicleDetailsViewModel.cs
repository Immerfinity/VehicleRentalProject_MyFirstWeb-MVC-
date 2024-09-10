using System.ComponentModel.DataAnnotations;
using VehicleRentalProject.Web.Validations;

namespace VehicleRentalProject.Web.Models.ViewModels.Vehicle
{
    public class VehicleDetailsViewModel
    {
        public int Id { get; set; }
        public string VehicleName { get; set; }
        public string VehicleType { get; set; }
        public string VehicleModel { get; set; }
        public string VehicleNumber { get; set; }
        public string VehicleColor { get; set; }
        public string VehicleDescription { get; set; }
        public string VehicleImage { get; set; }
        public int DailyRate { get; set; }

        [Display(Name ="Start Date")]
        [Required(ErrorMessage ="Пожалуйста выберите дату.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [Required(ErrorMessage = "Пожалуйста выберите Конечную дату.")]
        [DataType(DataType.Date)]
        [CustomDateRange(ErrorMessage = "Дата окончания должна быть больше или равна дате начала")]
        public DateTime? EndDate { get; set; }

        public int TotalAmount { get; set; }
    }
}
