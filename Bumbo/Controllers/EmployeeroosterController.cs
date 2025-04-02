using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Bumbo.Models;
using BumboDB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Bumbo.Controllers
{
    [Authorize]
    public class EmployeeRoosterController : Controller
    {
        private readonly BumboContext _context;
        private readonly UserManager<Employee> _userManager;
        private readonly ILogger<EmployeeRoosterController> _logger;

        public EmployeeRoosterController(BumboContext context, UserManager<Employee> userManager,
            ILogger<EmployeeRoosterController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("User not found.");
                return Challenge();
            }

            _logger.LogInformation("Fetching user shifts for user {UserId}", user.Id);
            var userShifts = await _context.Shifts
                .Include(s => s.Department)
                .Include(s => s.Chapter)
                .Where(s => s.EmployeeId == user.Id)
                .ToListAsync();

            _logger.LogInformation("Fetching available shifts for user {UserId}", user.Id);
            var availableShifts = await _context.Shifts
                .Include(s => s.Department)
                .Include(s => s.Chapter)
                .Where(s => s.IsAvailableForSwap && s.ChapterId == user.Chapter)
                .ToListAsync();

            var viewModel = new EmployeeRoosterViewModel
            {
                UserShifts = userShifts,
                AvailableShifts = availableShifts
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RequestSwap(int shiftId)
        {
            _logger.LogInformation("Requesting swap for shift {ShiftId}", shiftId);
            var shift = await _context.Shifts.FindAsync(shiftId);
            if (shift != null)
            {
                shift.IsAvailableForSwap = true;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Shift {ShiftId} marked as available for swap", shiftId);
            }
            else
            {
                _logger.LogWarning("Shift {ShiftId} not found", shiftId);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AvailableShifts()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("User not found.");
                return Challenge();
            }

            _logger.LogInformation("Fetching available shifts for user {UserId}", user.Id);
            var availableShifts = await _context.Shifts
                .Include(s => s.Department)
                .Include(s => s.Chapter)
                .Where(s => s.IsAvailableForSwap && s.ChapterId == user.Chapter && s.EmployeeId != user.Id)
                .ToListAsync();

            return View(availableShifts);
        }

        [HttpPost]
        public async Task<IActionResult> TakeOverShift(int shiftId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Gebruiker niet gevonden" });
            }

            var shift = await _context.Shifts.FindAsync(shiftId);
            if (shift == null)
            {
                return Json(new { success = false, message = "Dienst niet gevonden" });
            }

            if (shift.EmployeeId == user.Id)
            {
                return Json(new { success = false, message = "Je kan je eigen dienst niet overnemen" });
            }

            shift.EmployeeId = user.Id;
            _context.Update(shift);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Je hebt de dienst overgenomen", shift });
        }
    }

}