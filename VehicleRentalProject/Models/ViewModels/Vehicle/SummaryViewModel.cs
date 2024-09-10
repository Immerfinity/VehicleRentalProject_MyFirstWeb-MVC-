using System.ComponentModel.DataAnnotations;
using VehicleRentalProject.Models;
using VehicleRentalProject.Web.Validations;

namespace VehicleRentalProject.Web.Models.ViewModels.Vehicle
{
    public class SummaryViewModel
    {
        [Required]
        public string VehicleType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime? EndDate { get; set; }

        [Required]
        public string VehicleNumber { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Общая сумма должна быть положительным числом.")]
        public int TotalAmount { get; set; }

        public int TotalDuration { get; set; }
        public string VehicleImage { get; set; }

        [Required]
        public int Id { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }

}
