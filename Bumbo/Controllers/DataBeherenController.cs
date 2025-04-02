using Bumbo.Models;
using BumboDB;
using BumboDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Bumbo.Controllers {
    
    [Authorize]
    public class DataBeherenController : Controller {

        private readonly BumboContext dbContext;
        private static DataBeherenWeekSelectorViewModel weekSelectorViewModel;

        public DataBeherenController(BumboContext context) {
            dbContext = context;
        }

        static DataBeherenController() {
            weekSelectorViewModel = new DataBeherenWeekSelectorViewModel();
        }

        private Chapter? getManagerChapter() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var managerChapter = dbContext.Chapters.FirstOrDefault(c => c.Manager == userId);
            return managerChapter;
        }

        public IActionResult Index(int weekNumber, int yearNumber) {
            if (weekNumber > 0) {
                weekSelectorViewModel.WeekNumber = weekNumber;
                weekSelectorViewModel.YearNumber = yearNumber;
            } else {
                weekSelectorViewModel.WeekNumber = GetCurrentWeekNumber();
                weekNumber = GetCurrentWeekNumber();
                weekSelectorViewModel.YearNumber = DateTime.Now.Year;
                yearNumber = DateTime.Now.Year;
            }
            DateOnly date = GetDateFromISOWeekYearAndDay(weekSelectorViewModel.YearNumber, weekSelectorViewModel.WeekNumber, DayOfWeek.Monday);
            var managerChapter = getManagerChapter();
            List <History> histories_ = new List<History>();

            for (int i = 0; i < 7; i++) {
                histories_.Add(dbContext.Histories.FirstOrDefault(h => h.Day.Equals(date) && h.Chapter == managerChapter.ChapterId));
                date = date.AddDays(1);
            }

            IEnumerable<History> historyList = histories_;
            if(historyList.Count() > 0) {
                historyList.OrderBy(d => d.Day);
            }

            List<int> ints = new List<int>();
            ints.Add(weekNumber);
            ints.Add(yearNumber);
            IEnumerable<int> numbers = ints;

            DataBeherenCombinedViewModel dataBeherenCombinedViewModel = new DataBeherenCombinedViewModel();
            dataBeherenCombinedViewModel.Histories_ = histories_;
            dataBeherenCombinedViewModel.Numbers = numbers;

            return View(dataBeherenCombinedViewModel);
        }

        private int GetCurrentWeekNumber() {
            DateTime time = DateTime.Now;

            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday) {
                time = time.AddDays(3);
            }
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public IActionResult PrevWeek() {
            weekSelectorViewModel.WeekNumber--;
            if (weekSelectorViewModel.WeekNumber < 1) {
                weekSelectorViewModel.WeekNumber = 52; 
                weekSelectorViewModel.YearNumber--;  
            }

            return RedirectToAction("Index", new {weekNumber = weekSelectorViewModel.WeekNumber, yearNumber = weekSelectorViewModel.YearNumber});
        }

        public IActionResult NextWeek() {
            if (weekSelectorViewModel.YearNumber == DateTime.Now.Year && weekSelectorViewModel.WeekNumber == GetCurrentWeekNumber()) {
            } else {
                weekSelectorViewModel.WeekNumber++;
            }
            if (weekSelectorViewModel.WeekNumber > 52) {
                weekSelectorViewModel.WeekNumber = 1;  
                weekSelectorViewModel.YearNumber++; 
            }

            return RedirectToAction("Index", new {weekNumber = weekSelectorViewModel.WeekNumber, yearNumber = weekSelectorViewModel.YearNumber });
        }

        public static DateOnly GetDateFromISOWeekYearAndDay(int year, int week, DayOfWeek dayOfWeek) {
            if (week < 1 || week > 53) {
                throw new ArgumentOutOfRangeException(nameof(week), "Week number must be between 1 and 53.");
            }

            DateTime jan1 = new DateTime(year, 1, 1);

            int daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;

            if (daysOffset > 0) {
                daysOffset -= 7;
            }

            DateTime firstMonday = jan1.AddDays(daysOffset);

            DateTime firstWeekDate = firstMonday.AddDays((week - 1) * 7);

            DateTime finalDate = firstWeekDate.AddDays((int)dayOfWeek - (int)DayOfWeek.Monday);

            return DateOnly.FromDateTime(finalDate);
        }

        private void DeleteOldHistory() {
            DateOnly date = GetDateFromISOWeekYearAndDay(weekSelectorViewModel.YearNumber, weekSelectorViewModel.WeekNumber, DayOfWeek.Monday);
            var managerChapter = getManagerChapter();
            for (int i = 0; i < 7; i++) {
                if (dbContext.Histories.FirstOrDefault(h => h.Day.Equals(date) && h.Chapter == managerChapter.ChapterId) != null) {
                    dbContext.Histories.Remove(dbContext.Histories.FirstOrDefault(h => h.Day.Equals(date) && h.Chapter == managerChapter.ChapterId));
                    date = date.AddDays(1);
                }
            }
        }

        [HttpPost]
        public IActionResult SubmitForm() {
            DateOnly date = GetDateFromISOWeekYearAndDay(weekSelectorViewModel.YearNumber, weekSelectorViewModel.WeekNumber, DayOfWeek.Monday);

            this.DeleteOldHistory();

            var managerChapter = getManagerChapter();

            History datumMonday = new History();
            int val = 0;
            datumMonday.Day = date;
            datumMonday.Chapter = managerChapter.ChapterId;
            Int32.TryParse(HttpContext.Request.Form["A1"], out val);
            datumMonday.Customers = val;
            Int32.TryParse(HttpContext.Request.Form["A2"], out val);
            datumMonday.CargoCrates = val;
            datumMonday.Holiday = HttpContext.Request.Form["A3"];
            History datumTuesday = new History();
            datumTuesday.Day = date.AddDays(1);
            datumTuesday.Chapter = managerChapter.ChapterId;
            Int32.TryParse(HttpContext.Request.Form["B1"], out val);
            datumTuesday.Customers = val;
            Int32.TryParse(HttpContext.Request.Form["B2"], out val);
            datumTuesday.CargoCrates = val;
            datumTuesday.Holiday = HttpContext.Request.Form["B3"];
            History datumWednesday = new History();
            datumWednesday.Day = date.AddDays(2);
            datumWednesday.Chapter = managerChapter.ChapterId;
            Int32.TryParse(HttpContext.Request.Form["C1"], out val);
            datumWednesday.Customers = val;
            Int32.TryParse(HttpContext.Request.Form["C2"], out val);
            datumWednesday.CargoCrates = val;
            datumWednesday.Holiday = HttpContext.Request.Form["C3"];
            History datumThursday = new History();
            datumThursday.Day = date.AddDays(3);
            datumThursday.Chapter = managerChapter.ChapterId;
            Int32.TryParse(HttpContext.Request.Form["D1"], out val);
            datumThursday.Customers = val;
            Int32.TryParse(HttpContext.Request.Form["D2"], out val);
            datumThursday.CargoCrates = val;
            datumThursday.Holiday = HttpContext.Request.Form["D3"];
            History datumFriday = new History();
            datumFriday.Day = date.AddDays(4);
            datumFriday.Chapter = managerChapter.ChapterId;
            Int32.TryParse(HttpContext.Request.Form["E1"], out val);
            datumFriday.Customers = val;
            Int32.TryParse(HttpContext.Request.Form["E2"], out val);
            datumFriday.CargoCrates = val;
            datumFriday.Holiday = HttpContext.Request.Form["E3"];
            History datumSaturday = new History();
            datumSaturday.Day = date.AddDays(5);
            datumSaturday.Chapter = managerChapter.ChapterId;
            Int32.TryParse(HttpContext.Request.Form["F1"], out val);
            datumSaturday.Customers = val;
            Int32.TryParse(HttpContext.Request.Form["F2"], out val);
            datumSaturday.CargoCrates = val;
            datumSaturday.Holiday = HttpContext.Request.Form["F3"];
            History datumSunday = new History();
            datumSunday.Day = date.AddDays(6);
            datumSunday.Chapter = managerChapter.ChapterId;
            Int32.TryParse(HttpContext.Request.Form["G1"], out val);
            datumSunday.Customers = val;
            Int32.TryParse(HttpContext.Request.Form["G2"], out val);
            datumSunday.CargoCrates = val;
            datumSunday.Holiday = HttpContext.Request.Form["G3"];

            dbContext.Add(datumMonday);
            dbContext.Add(datumTuesday);
            dbContext.Add(datumWednesday);
            dbContext.Add(datumThursday);
            dbContext.Add(datumFriday);
            dbContext.Add(datumSaturday);
            dbContext.Add(datumSunday);

            dbContext.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
