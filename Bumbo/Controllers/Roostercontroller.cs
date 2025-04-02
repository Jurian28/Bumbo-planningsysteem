using Bumbo.Models;
using BumboDB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Bumbo.Controllers
{
    public class RoosterController : Controller
    {
        private ILogger<RoosterController> _logger;

        public RoosterController(ILogger<RoosterController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(int year, int weekNumber)
        {
            RoosterWeekViewModel weekModel = GetWeekModel(year, weekNumber);

            return View(weekModel);
        }

        public RoosterWeekViewModel GetWeekModel(int year, int weekNumber)
        {
            BumboContext context = new BumboContext();

            RoosterWeekViewModel weekModel = new RoosterWeekViewModel();

            if (weekNumber <= 0)
            {
                weekNumber = GetCurrentWeekNumber();
                year = DateTime.Now.Year;
                weekModel.Week = GetCurrentWeekNumber();
                weekModel.Year = DateTime.Now.Year;
            }
            weekModel.BeginDate = GetDate(year, weekNumber, DayOfWeek.Monday);
            weekModel.EndDate = GetDate(year, weekNumber, DayOfWeek.Sunday);
            weekModel.Week = weekNumber;
            weekModel.Year = year;

            for (int i = 0; i < 7; i++)
            {
                RoosterDayViewModel dayModel = new RoosterDayViewModel();

                //dayModel.Day = weekModel.BeginDate.AddDays(i).DayOfWeek.ToString();
                dayModel.WeekNumber = weekNumber;
                dayModel.Year = weekModel.BeginDate.Year;
                dayModel.Date = weekModel.BeginDate.AddDays(i);

                int chapter = 1; // TODO: GET CHAPTER FROM USER

                var departments = context.Departments.Where(d => d.Chapter == chapter).ToList();
                foreach (var department in departments)
                {
                    var prognoses = context.Prognoses.FirstOrDefault(p => p.Department == department.DepartmentId && p.Day == dayModel.Date);
                    double currentHours = context.Shifts
                        .Where(s => s.DepartmentId == department.DepartmentId && s.Date == dayModel.Date)
                        .AsEnumerable() // Forces client-side evaluation
                        .Sum(s => (s.EndTime - s.StartTime).TotalHours);
                    if (prognoses != null)
                    {
                        dayModel.DepartmentHours.Add(department.Name, (prognoses.Result, currentHours));
                    }
                    else
                    {
                        dayModel.DepartmentHours.Add(department.Name, (0, currentHours));
                    }
                }

                weekModel.Days[i] = dayModel;
            }

            return weekModel;
        }

        public static DateOnly GetDate(int year, int week, DayOfWeek dayOfWeek)
        {
            return DateOnly.FromDateTime(ISOWeek.ToDateTime(year, week, dayOfWeek));
        }

        private int GetCurrentWeekNumber()
        {
            DateTime time = DateTime.Now;

            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static int GetWeekNumberFromDate(DateOnly date)
        {
            DateTime dateTime = date.ToDateTime(TimeOnly.MinValue);

            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(dateTime);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                dateTime = dateTime.AddDays(3);
            }

            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }


        public IActionResult PrevWeek(int currentYear, int currentWeek)
        {
            int newWeek = currentWeek;
            int newYear = currentYear;

            newWeek--;
            if (newWeek < 1)
            {
                newWeek = 52;
                newYear--;
            }

            return RedirectToAction("Index", new { weekNumber = newWeek, year = newYear });
        }

        public IActionResult NextWeek(int currentYear, int currentWeek)
        {
            int newWeek = currentWeek;
            int newYear = currentYear;


            newWeek++;
            if (newWeek > 52)
            {
                newWeek = 1;
                newYear++;
            }

            return RedirectToAction("Index", new { weekNumber = newWeek, year = newYear });
        }

        public IActionResult DayOverview(int year, int weekNumber, DateOnly date)
        {
            Console.WriteLine("DayOverview "+year+"/"+weekNumber);

            BumboContext context = new BumboContext();

            RoosterWeekViewModel weekModel = GetWeekModel(year, weekNumber);

            RoosterDayViewModel selectedDay = weekModel.Days.FirstOrDefault(d => d.Date == date);

            foreach (var department in selectedDay.DepartmentHours)
            {
                var shifts = context.Shifts.Where(s => s.DepartmentId == context.Departments.FirstOrDefault(d => d.Name == department.Key && d.Chapter == 1).DepartmentId && s.Date == date && s.ChapterId == 1).ToList();


                List<ShiftViewModel> temp = new List<ShiftViewModel>();

                foreach(var shift in shifts){
                    ShiftViewModel shiftViewModel = new ShiftViewModel();

                    shiftViewModel.ShiftId = shift.ShiftId;
                    shiftViewModel.StartTime = shift.StartTime;
                    shiftViewModel.EndTime = shift.EndTime;

                    string employeeName = context.Users.FirstOrDefault(e => e.Id == shift.EmployeeId)?.UserName;
                    shiftViewModel.EmployeeName = employeeName;
                    

                    temp.Add(shiftViewModel);
                }

                selectedDay.DepartmentShifts.Add(department.Key, temp);
            }

            return View(selectedDay);
        }

        public IActionResult RemoveShift(int shiftID, int year, int weekNumber, DateOnly date)
        {
            BumboContext context = new BumboContext();

            Shift shiftToRemove = context.Shifts.FirstOrDefault(s => s.ShiftId == shiftID);

            if (shiftToRemove != null)
            {

                context.Remove(shiftToRemove);
                context.SaveChanges();
            }


            return RedirectToAction("DayOverview", new { weekNumber = weekNumber, year = year, date = date });

        }


        public IActionResult AddShift(DateOnly date, string afdeling, bool otherChapters, TimeOnly beginTime, TimeOnly endTime, string empName)
        {
            BumboContext context = new BumboContext();

            AddShiftViewModel shiftModel = new AddShiftViewModel();
            shiftModel.Date = date;
            shiftModel.OtherChapters = otherChapters;
            shiftModel.StartTime = beginTime;
            shiftModel.EndTime = endTime;

            shiftModel.SelectedDepartment = context.Departments.FirstOrDefault(d => d.Name == afdeling && d.Chapter == 1); // TODO CHAPTER
            shiftModel.AvailableDepartments = context.Departments.Where(d => d.Chapter == 1).ToList(); // TODO CHAPTER

            var hoursPlanned = context.Shifts
                        .AsEnumerable()
                        .Where(s => s.Date == date && s.ChapterId == 1 && s.DepartmentId == shiftModel.SelectedDepartment.DepartmentId) // TODO CHAPTER
                        .ToList();
            double totalHoursPlanned = hoursPlanned.Sum(s => (s.EndTime - s.StartTime).TotalHours);

            var hoursPrognosis = context.Prognoses.FirstOrDefault(p => p.Day == date && p.Department == shiftModel.SelectedDepartment.DepartmentId);

            if (hoursPrognosis == null)
            {
                shiftModel.HoursLeft = 0;
            }
            else
            {
                shiftModel.HoursLeft = (int)(hoursPrognosis.Result - totalHoursPlanned);
            }

            if (shiftModel.HoursLeft < 0)
            {
                shiftModel.HoursLeft = 0;
            }

            if (endTime == TimeOnly.MinValue)
            {
                return View(shiftModel);
            }
            else if (beginTime > endTime || beginTime == endTime)
            {
                return View(shiftModel);
            }

            List<Employee> employeesInitial;

            if (otherChapters == true)
            {
                employeesInitial = context.Employees.ToList();
            }
            else
            {
                employeesInitial = context.Employees.Where(e => e.Chapter == 1).ToList(); // TODO CHAPTER
            }

            shiftModel.EmpName = empName;
            if (empName != null)
            {
                employeesInitial = employeesInitial.Where(e => e.Name.ToLower().Contains(empName.ToLower())).ToList();
            }

            List<Employee> employeesAvailibilityFilter = new List<Employee>();

            foreach (Employee employee in employeesInitial)
            {
                var availability = context.Availabilities.FirstOrDefault(a => a.Employee == employee.Id && a.Day == date);
                if (availability != null && availability.IsAvailable && availability.StartTime <= beginTime && availability.EndTime >= endTime)
                {
                    Console.WriteLine("Avail. " + employee.Name);
                    employeesAvailibilityFilter.Add(employee);
                }
            }

            List<Employee> employeesTimeOffFilter = new List<Employee>();

            foreach (Employee employee in employeesAvailibilityFilter)
            {
                DateTime beginDateTime = new DateTime(date.Year, date.Month, date.Day, beginTime.Hour, beginTime.Minute, beginTime.Second);
                DateTime endDateTime = new DateTime(date.Year, date.Month, date.Day, endTime.Hour, endTime.Minute, endTime.Second);
                var timeOff = context.TimeOffRequests.FirstOrDefault(t => t.EmployeeId == employee.Id && ((t.StartDate <= beginDateTime && t.EndDate >= beginDateTime) || (t.StartDate <= endDateTime && t.EndDate >= endDateTime)));
                if (timeOff == null)
                {
                    Console.WriteLine("TimeOff. " + employee.Name);
                    employeesTimeOffFilter.Add(employee);
                }
            }

            List<Employee> employeesShiftFilter = new List<Employee>();

            foreach (Employee employee in employeesTimeOffFilter)
            {
                var shifts = context.Shifts.FirstOrDefault(s => s.EmployeeId == employee.Id && s.Date == date);
                if (shifts == null)
                {
                    Console.WriteLine("Shift. " + employee.Name);
                    employeesShiftFilter.Add(employee);
                }
            }

            List<Employee> employeesCAOFilter = new List<Employee>();

            foreach (Employee employee in employeesShiftFilter)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var sixteenYearsAgo = today.AddYears(-16);
                var eighteenYearsAgo = today.AddYears(-18);

                switch (employee.Birthday)
                {
                    case var birthday when birthday > sixteenYearsAgo:
                        // Under 16 years old
                        var shiftsThisWeekUnder16 = context.Shifts
                            .AsEnumerable()
                            .Where(s => s.EmployeeId == employee.Id && GetWeekNumberFromDate(s.Date) == GetWeekNumberFromDate(date))
                            .ToList();
                        var availibilityEightHours = context.Availabilities.FirstOrDefault(a => a.Employee == employee.Id && a.Day == date);
                        double totalHoursThisWeekUnder16 = shiftsThisWeekUnder16.Sum(s => (s.EndTime - s.StartTime).TotalHours);

                        if ((totalHoursThisWeekUnder16 + (endTime - beginTime).TotalHours <= 40) &&
                            (beginTime < TimeOnly.Parse("19:00:00") && endTime <= TimeOnly.Parse("19:00:00")) &&
                            (shiftsThisWeekUnder16.Count < 5) && (availibilityEightHours.HoursWorkedSchool + (endTime - beginTime).TotalHours <= 8) &&
                            ((endTime - beginTime).TotalHours <= 12))
                        {
                            employeesCAOFilter.Add(employee);
                        }
                        break;

                    case var birthday when birthday <= sixteenYearsAgo && birthday > eighteenYearsAgo:
                        // 16 or 17 years old
                        var shiftsLast4Weeks = context.Shifts
                            .AsEnumerable()
                            .Where(s => s.EmployeeId == employee.Id &&
                                        GetWeekNumberFromDate(s.Date) >= GetWeekNumberFromDate(date) - 3 &&
                                        GetWeekNumberFromDate(s.Date) <= GetWeekNumberFromDate(date))
                            .ToList();
                        double totalHoursLast4Weeks = shiftsLast4Weeks.Sum(s => (s.EndTime - s.StartTime).TotalHours);
                        double averageHours = totalHoursLast4Weeks / 4;
                        var availibilityNineHours = context.Availabilities.FirstOrDefault(a => a.Employee == employee.Id && a.Day == date);

                        if ((averageHours <= 40 && (endTime - beginTime).TotalHours <= 9) && (availibilityNineHours.HoursWorkedSchool + (endTime - beginTime).TotalHours <= 9) &&
                            ((endTime - beginTime).TotalHours <= 12))
                        {
                            employeesCAOFilter.Add(employee);
                        }
                        break;

                    case var birthday when birthday <= eighteenYearsAgo:
                        // 18 years or older
                        var shiftsThisWeekAdult = context.Shifts
                            .AsEnumerable()
                            .Where(s => s.EmployeeId == employee.Id && GetWeekNumberFromDate(s.Date) == GetWeekNumberFromDate(date))
                            .ToList();
                        double totalHoursThisWeekAdult = shiftsThisWeekAdult.Sum(s => (s.EndTime - s.StartTime).TotalHours);

                        if ((totalHoursThisWeekAdult + (endTime - beginTime).TotalHours <= 60) && ((endTime - beginTime).TotalHours <= 12))
                        {
                            employeesCAOFilter.Add(employee);
                        }
                        break;

                    default:
                        break;
                }
            }

            // TODO: ZIEKTE FILTER & CHECKEN OF HET EEN SCHOOLWEEK IS VOOR <16

            foreach (Employee employee in employeesCAOFilter)
            {
                var manager = context.UserRoles.FirstOrDefault(u => u.UserId == employee.Id && u.RoleId == "2");
                if (manager != null)
                {
                    Console.WriteLine("Manager. " + employee.Name);
                    shiftModel.AvailableEmployees.Add(employee);
                }
            }

            shiftModel.AvailableEmployees = shiftModel.AvailableEmployees
                .OrderBy(employee =>
                {
                    var shiftsThisWeek = context.Shifts
                        .AsEnumerable()
                        .Where(s => s.EmployeeId == employee.Id && GetWeekNumberFromDate(s.Date) == GetWeekNumberFromDate(date))
                        .ToList();

                    double totalHoursThisWeek = shiftsThisWeek.Sum(s => (s.EndTime - s.StartTime).TotalHours);

                    return totalHoursThisWeek;
                })
                .ToList();


            foreach (Employee employee in shiftModel.AvailableEmployees)
            {
                Console.WriteLine(employee.Id);
            }
            

            return View(shiftModel);
        }

        [HttpPost]
        public IActionResult PlanShift(int SelectedEmployee, DateOnly date, TimeOnly beginTime, TimeOnly endTime, string afdeling)
        {
            if (SelectedEmployee == 0)
            {
                return RedirectToAction("AddShift", new { date = date, afdeling = afdeling, otherChapters = false, beginTime = beginTime, endTime = endTime });
            }

            BumboContext context = new BumboContext();

            var employee = context.Employees.FirstOrDefault(e => e.Id == SelectedEmployee.ToString());

            var newShift = new Shift
            {
                StartTime = beginTime,
                EndTime = endTime,
                Date = date,
                EmployeeId = SelectedEmployee.ToString(),
                ChapterId = GetChapterByUserID(),
                DepartmentId = context.Departments.FirstOrDefault(d => d.Name == afdeling && d.Chapter == 1).DepartmentId,
                IsPublished = false
            };

            context.Shifts.Add(newShift);
            context.SaveChanges();

            return RedirectToAction("DayOverview", new { weekNumber = GetWeekNumberFromDate(date), year = date.Year, date = date });
        }

        public int GetChapterByUserID()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User ID: {UserId}", userId);

            BumboContext context = new BumboContext();
            var managerChapter = context.Chapters.FirstOrDefault(c => c.Manager == userId);
            return managerChapter.ChapterId;
        }

        public IActionResult PostSchedule(int year, int week)
        {
           
            BumboContext context = new BumboContext();

            var shifts = context.Shifts.AsEnumerable()
                .Where(s => s.ChapterId == GetChapterByUserID() && s.Date.Year == year && GetWeekNumberFromDate(s.Date) == week);

            foreach (var shift in shifts)
            {
                shift.IsPublished = true;
            }

            context.SaveChanges();

            return RedirectToAction("Index", new { year = year, weekNumber = week });
        }


        public bool IsSomeoneScheduled(DateOnly date, TimeOnly time, int departmentID)
        {
            BumboContext context = new BumboContext();

            // Controleer of er een shift bestaat voor de opgegeven datum, tijd en afdeling
            bool isScheduled = context.Shifts.Any(s => s.Date == date && s.StartTime <= time && s.EndTime > time && s.DepartmentId == departmentID);

            return isScheduled;
        }

        [Authorize(Roles = "Manager")]
        public IActionResult EditShift(int shiftID)
        {
            BumboContext context = new BumboContext();

            var shift = context.Shifts
                .Include(s => s.Department)
                .Include(s => s.Employee)
                .FirstOrDefault(s => s.ShiftId == shiftID);

            if (shift == null)
            {
                return NotFound();
            }

            return View(shift);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public IActionResult SaveEditedShift(int ShiftId, TimeOnly startTime, TimeOnly endTime, DateOnly date)
        {
            BumboContext context = new BumboContext();

            var shift = context.Shifts.FirstOrDefault(s => s.ShiftId == ShiftId);
            if (shift != null)
            {
                shift.StartTime = startTime;
                shift.EndTime = endTime;
                context.Update(shift);
                context.SaveChanges();
                return RedirectToAction("DayOverview", new { weekNumber = GetWeekNumberFromDate(date), year = date.Year, date = date });
            }
            return NotFound();
        }
    }
}