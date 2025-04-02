using System;
using System.ComponentModel.DataAnnotations;

namespace BumboDB.Models
{
    public class Norm
    {
        [Key]
        public int NormId { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; } 
        public int ChapterId { get; set; } 
        public int FreshCustomersPerHour { get; set; }
        public int CashierCustomersPerHour { get; set; }
        
        public int ShelfFillingSeconds { get; set; } 
        public int UnloadingSeconds { get; set; } 
        public int FacingSecondsPerMeter { get; set; } 
    }
}