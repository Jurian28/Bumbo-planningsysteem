using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Bumbo.ViewModels;
using BumboDB.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bumbo.Controllers
{
    [Authorize]
    public class VerlofController : Controller
    {
        private readonly BumboContext _context;
        private readonly UserManager<Employee> _userManager;
        private readonly ILogger<VerlofController> _logger;

        public VerlofController(BumboContext context, UserManager<Employee> userManager, ILogger<VerlofController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Fetching all time off requests.");
                var timeOffRequests = await _context.TimeOffRequests.Include(t => t.Employee).ToListAsync();
                return View(timeOffRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching time off requests.");
                TempData["Error"] = "An error occurred while fetching time off requests. Please try again later.";
                return RedirectToAction("Error", "Home"); // Redirect to an error page
            }
        }

        public IActionResult Create()
        {
            _logger.LogInformation("Loading create time off request page.");
            return View(new TimeOffRequestViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(TimeOffRequestViewModel model)
        {
            _logger.LogInformation("Create TimeOffRequest invoked.");
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid.");
                LogModelStateErrors();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Unable to find the logged-in user.");
                ModelState.AddModelError("", "Unable to find the logged-in user.");
                return View(model);
            }

            var request = new TimeOffRequest
            {
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Reason = model.Reason,
                Ziek = model.Ziek,
                EmployeeId = user.Id,
                IsApproved = model.Ziek ? true : (bool?)null
            };

            try
            {
                _logger.LogInformation("Saving new time off request for user {UserId}.", user.Id);
                _context.TimeOffRequests.Add(request);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Time off request created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving TimeOffRequest to database.");
                ModelState.AddModelError("", "An error occurred while saving your request. Please try again later.");
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var request = await _context.TimeOffRequests.FindAsync(id);
                if (request == null || request.IsApproved != null)
                {
                    _logger.LogWarning("Time off request with ID {Id} not found or already processed.", id);
                    return NotFound();
                }

                var model = new TimeOffRequestViewModel
                {
                    TimeOffRequestId = request.TimeOffRequestId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Reason = request.Reason,
                    Ziek = request.Ziek
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching time off request for edit.");
                TempData["Error"] = "An error occurred while loading the edit page. Please try again later.";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TimeOffRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid during edit.");
                LogModelStateErrors();
                return View(model);
            }

            try
            {
                var request = await _context.TimeOffRequests.FindAsync(model.TimeOffRequestId);
                if (request == null || request.IsApproved != null)
                {
                    _logger.LogWarning("Time off request with ID {Id} not found or already processed.", model.TimeOffRequestId);
                    return NotFound();
                }

                request.StartDate = model.StartDate;
                request.EndDate = model.EndDate;
                request.Reason = model.Reason;
                request.Ziek = model.Ziek;
                request.IsApproved = model.Ziek ? true : (bool?)null;

                _context.Update(request);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Time off request updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating TimeOffRequest with ID {Id}.", model.TimeOffRequestId);
                ModelState.AddModelError("", "An error occurred while updating the request. Please try again later.");
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var request = await _context.TimeOffRequests.FindAsync(id);
                if (request == null)
                {
                    _logger.LogWarning("Time off request with ID {Id} not found.", id);
                    return NotFound();
                }

                _context.TimeOffRequests.Remove(request);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Time off request cancelled successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling TimeOffRequest with ID {Id}.", id);
                TempData["Error"] = "An error occurred while cancelling the request. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        private void LogModelStateErrors()
        {
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    _logger.LogError("ModelState Error: {ErrorMessage}", error.ErrorMessage);
                }
            }
        }
    }
}