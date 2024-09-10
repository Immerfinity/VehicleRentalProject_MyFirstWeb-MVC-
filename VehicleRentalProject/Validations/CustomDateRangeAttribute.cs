using System;
using System.ComponentModel.DataAnnotations;

namespace VehicleRentalProject.Web.Validations
{
    public class CustomDateRangeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var startDate = (DateTime?)validationContext.ObjectType.GetProperty("StartDate")?.GetValue(validationContext.ObjectInstance, null);
            var endDate = (DateTime?)value;

            var currentDate = DateTime.Today;

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate > endDate)
                {
                    return new ValidationResult("Дата окончания должна быть больше или равна дате начала");
                }
                if (startDate < currentDate || endDate < currentDate)
                {
                    return new ValidationResult("Дата начала и окончания должны быть больше или равны текущей дате");
                }
            }

            return ValidationResult.Success;
        }
    }
}
