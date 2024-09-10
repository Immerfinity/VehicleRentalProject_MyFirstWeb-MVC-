using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleRentalProject.Models
{
    public class Rental
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int PenaltyAmount { get; set; }
        public int TotalPrice { get; set; }
        public bool IsPaid { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string RentalStatus { get; set; }
        public int VehicleId { get; set; }
        public string ApplicationUserId { get; set; }

        //Navigation Properties
        public virtual Vehicle Vehicle { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

    }
}
