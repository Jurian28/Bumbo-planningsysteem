using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using BumboDB.Models;

namespace Bumbo.Controllers
{
    public class OpeningstijdenController : Controller
    {
        private static List<OpeningHour> _openingHours = new List<OpeningHour>
        {
            new OpeningHour { Id = 1, Day = "Monday", OpenTime = "08:00", CloseTime = "22:00" },
            new OpeningHour { Id = 2, Day = "Tuesday", OpenTime = "08:00", CloseTime = "22:00" },
            new OpeningHour { Id = 3, Day = "Wednesday", OpenTime = "08:00", CloseTime = "22:00" },
            new OpeningHour { Id = 4, Day = "Thursday", OpenTime = "08:00", CloseTime = "22:00" },
            new OpeningHour { Id = 5, Day = "Friday", OpenTime = "08:00", CloseTime = "22:00" },
            new OpeningHour { Id = 6, Day = "Saturday", OpenTime = "08:00", CloseTime = "22:00" },
            new OpeningHour { Id = 7, Day = "Sunday", OpenTime = "08:00", CloseTime = "22:00" }
        };

        public IActionResult Index()
        {
            return View(_openingHours);
        }

        [HttpPost]
        public IActionResult Save(List<OpeningHour> openingHours)
        {
            _openingHours = openingHours;
            return RedirectToAction("Index");
        }
    }
}