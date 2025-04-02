using Microsoft.AspNetCore.Mvc;
using BumboDB.Models;
using System.Linq;
using System.Collections.Generic;
using System;
using Bumbo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bumbo.Controllers
{
    public class BeschikbaarheidController : Controller
    {
        private readonly BumboContext _context;
        private readonly ILogger<BeschikbaarheidController> _logger;

        public BeschikbaarheidController(BumboContext context, ILogger<BeschikbaarheidController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IActionResult Index(int year = 2024, int week = 49)
        {
            _logger.LogInformation("Received request: Year={Year}, Week={Week}", year, week);

            if (!ValidateYearAndWeek(year, week))
            {
                _logger.LogWarning("Validation failed for Year={Year}, Week={Week}", year, week);
                return View(); // View with validation errors
            }

            var userId = GetAuthenticatedUserId();
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User not authenticated.");
                return RedirectToAction("Login", "Account");
            }

            var model = GetWeekAvailabilityViewModel(year, week, userId);
            _logger.LogInformation("Returning view with model: {@Model}", model);
            return View(model);
        }

 [HttpPost]
public IActionResult UpdateAvailability(WeekAvailabilityViewModel model)
{
    _logger.LogInformation("UpdateAvailability called with model: {@Model}", model);

    if (!ModelState.IsValid)
    {
        _logger.LogError("ModelState errors: {Errors}",
            ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        return View("Index", model);
    }

    if (model.Availabilities == null || !model.Availabilities.Any())
    {
        _logger.LogWarning("No availabilities provided.");
        ModelState.AddModelError(string.Empty, "No availabilities provided.");
        return View("Index", model);
    }

    var userId = GetAuthenticatedUserId();
    if (string.IsNullOrEmpty(userId))
    {
        _logger.LogWarning("User not authenticated.");
        return RedirectToAction("Login", "Account");
    }

    try
    {
        foreach (var availability in model.Availabilities)
        {
            availability.Employee = userId; // Set the Employee ID to the authenticated user's ID
            _logger.LogInformation("Processing availability for day: {Day}, IsAvailable: {IsAvailable}", availability.Day, availability.IsAvailable);

            var existing = _context.Availabilities
                .FirstOrDefault(a => a.Day == availability.Day && a.Employee == availability.Employee);

            if (existing != null)
            {
                _logger.LogInformation("Updating existing availability: {@ExistingAvailability}", existing);
                existing.IsAvailable = availability.IsAvailable;
                existing.HoursWorkedSchool = availability.HoursWorkedSchool;
                existing.StartTime = availability.StartTime;
                existing.EndTime = availability.EndTime;

                _context.Availabilities.Update(existing);
            }
            else
            {
                var newAvailability = new Availability
                {
                    Day = availability.Day,
                    Employee = availability.Employee,
                    IsAvailable = availability.IsAvailable,
                    HoursWorkedSchool = availability.HoursWorkedSchool,
                    StartTime = availability.StartTime,
                    EndTime = availability.EndTime
                };
                _logger.LogInformation("Adding new availability: {@NewAvailability}", newAvailability);
                _context.Availabilities.Add(newAvailability);
            }
        }

        _context.SaveChanges();
        _logger.LogInformation("Availabilities updated successfully.");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating availability");
        ModelState.AddModelError(string.Empty, "An error occurred while updating availability.");
        return View("Index", model);
    }

    var updatedModel = GetWeekAvailabilityViewModel(model.Year, model.Week, userId);
    return View("Index", updatedModel);
}

private bool ValidateYearAndWeek(int year, int week)
{
    _logger.LogInformation("Validating Year={Year}, Week={Week}", year, week);

    if (year < 1 || year > 9999)
    {
        ModelState.AddModelError(nameof(year), "Year must be between 1 and 9999 PLASNEGER.");
        _logger.LogWarning("Invalid year: {Year}", year);
    }

    if (week < 1 || week > 52)
    {
        ModelState.AddModelError(nameof(week), "Week number must be between 1 and 52 PLASNEGER.");
        _logger.LogWarning("Invalid week: {Week}", week);
    }

    if (!ModelState.IsValid)
    {
        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        {
            _logger.LogError("Validation Error: {ErrorMessage}", error.ErrorMessage);
        }
        return false;
    }
    return true;
}

        private string GetAuthenticatedUserId()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Authenticated user ID: {UserId}", userId);
            return userId;
        }

        private WeekAvailabilityViewModel GetWeekAvailabilityViewModel(int year, int week, string userId)
        {
            var startDate = FirstDateOfWeekIso8601(year, week);
            var endDate = startDate.AddDays(6);

            _logger.LogInformation("Calculated start date: {StartDate}, end date: {EndDate}", startDate, endDate);

            var availability = _context.Availabilities
                .Where(a => a.Employee == userId && a.Day >= startDate && a.Day <= endDate)
                .ToList();

            _logger.LogInformation("Retrieved {Count} availability records.", availability.Count);

            var model = new WeekAvailabilityViewModel
            {
                Year = year,
                Week = week,
                StartDate = startDate,
                EndDate = endDate,
                Availabilities = availability ?? new List<Availability>()
            };

            model.InitializeDefaultAvailabilities();
            _logger.LogInformation("Initialized default availabilities for model: {@Model}", model);
            return model;
        }

        private static DateOnly FirstDateOfWeekIso8601(int year, int weekOfYear)
        {
            var jan1 = new DateTime(year, 1, 1);
            var daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;
            var firstThursday = jan1.AddDays(daysOffset);
            var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            var firstWeek = cal.GetWeekOfYear(firstThursday, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }

            var result = firstThursday.AddDays(weekNum * 7 - 3);
            return DateOnly.FromDateTime(result);
        }
    }
}