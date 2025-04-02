using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BumboDB.Models
{
    public class PrognosesWeek
    {
        [Key]
        public int Chapter { get; set; }

        [Key]
        public int Week { get; set; }

        [Key]
        public int Year { get; set; }

        public DateOnly BeginDate { get; set; }

        public DateOnly EndDate { get; set; }
    }
}