using VehicleRentalProject.Models;

namespace VehicleRentalProject.Web.Models.ViewModels.Rental
{
    public class RentalHistoryViewModel
    {
        public int RentalId { get; set; }
        public string VehicleNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int TotalPrice { get; set; }
        public string Status { get; set; }
        public int PenaltyAmount { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }

}
