using VehicleRentalProject.Models;

namespace VehicleRentalProject.Web.Models.ViewModels.Rental
{
    public class UpdateRentalStatusViewModel
    {
        public int Id { get; set; }
        public string VehicleNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int TotalPrice { get; set; }
        public string Status { get; set; }
        public int PenaltyAmount { get; set; }
        public string ApplicationUser { get; set; }
    }
}
