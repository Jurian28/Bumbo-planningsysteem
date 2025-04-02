using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BumboDB.Models;

namespace Bumbo.Models
{
    public class WeekAvailabilityViewModel
    {
        [Range(1, 9999, ErrorMessage = "Year must be between 1 and 9999.")]
        public int Year { get; set; }

        [Range(1, 52, ErrorMessage = "Week must be between 1 and 52.")]
        public int Week { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        [Required] public List<Availability> Availabilities { get; set; } = new List<Availability>();

        public void InitializeDefaultAvailabilities()
        {
            if (Availabilities == null || !Availabilities.Any())
            {
                for (int i = 0; i < 7; i++)
                {
                    var date = StartDate.AddDays(i);
                    Availabilities.Add(new Availability
                    {
                        Day = date,
                        IsAvailable = true // Default value
                    });
                }
            }
        }
    }
}
    
