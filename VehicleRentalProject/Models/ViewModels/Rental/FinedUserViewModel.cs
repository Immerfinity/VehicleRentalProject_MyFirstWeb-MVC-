namespace VehicleRentalProject.Web.Models.ViewModels.Rental
{
    public class FinedUserViewModel
    {
        public int RentalId { get; set; }
        public string VehicleNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public decimal PenaltyAmount { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
    }
}
