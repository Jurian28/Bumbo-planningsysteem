using System;
using System.ComponentModel.DataAnnotations;

namespace BumboDB.Models
{
    public class TimeOffRequest
    {
        public int TimeOffRequestId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(TimeOffRequest), nameof(ValidateStartDate))]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(TimeOffRequest), nameof(ValidateEndDate))]
        public DateTime EndDate { get; set; }

        public string Reason { get; set; }
        public bool? IsApproved { get; set; } 
        public bool Ziek { get; set; }    

        public string? EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }

        public static ValidationResult ValidateStartDate(DateTime startDate, ValidationContext context)
        {
            if (startDate.Date < DateTime.Today)
            {
                return new ValidationResult("Startdatum moet vanaf vandaag zijn.");
            }
            return ValidationResult.Success;
        }

        
        public static ValidationResult ValidateEndDate(DateTime endDate, ValidationContext context)
        {
            var instance = context.ObjectInstance as TimeOffRequest;
            if (instance != null)
            {
                if (endDate.Date < instance.StartDate.Date)
                {
                    return new ValidationResult("Einddatum mag niet eerder zijn dan de startdatum.");
                }
                if (endDate.Date > instance.StartDate.AddDays(28))
                {
                    return new ValidationResult("Einddatum moet binnen 4 weken na de startdatum zijn.");
                }
            }
            return ValidationResult.Success;
        }
    }
}