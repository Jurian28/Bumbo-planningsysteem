using BumboDB.Models;
using Bumbo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Bumbo.Controllers {
    public class VerlofBeherenController(BumboContext context) : Controller {

        public IActionResult Index() {
            var timeOffRequests = context.TimeOffRequests.ToList();
            var employeesWithRequest = new List<TimeOffRequest>();
            var employees = new List<Employee>();
            Chapter? managerChapter = getManagerChapter();

            if (managerChapter == null) {
                return View();
            }

            foreach (var timeOffRequest in timeOffRequests) {
                var employeeId = timeOffRequest.EmployeeId;
                Employee? employee = context.Employees.FirstOrDefault(e => e.Id == employeeId);
                if (employee.Chapter == managerChapter.ChapterId) {
                    employeesWithRequest.Add(timeOffRequest);
                    employees.Add(employee);
                }
            }

            VerlofBeherenViewModel verlofBeherenVM = new VerlofBeherenViewModel();
            verlofBeherenVM.timeOffRequests = employeesWithRequest;
            verlofBeherenVM.employees = employees;
            return View(verlofBeherenVM);
        }

        private Chapter? getManagerChapter() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var managerChapter = context.Chapters.FirstOrDefault(c => c.Manager == userId);
            return managerChapter;
        }

        private TimeOffRequest? getTimeOffRequest(int id) {
            var timeOffRequest = context.TimeOffRequests.FirstOrDefault(i => i.TimeOffRequestId == id);
            return timeOffRequest;
        }

        public IActionResult Accept(int id) {
            var timeOffRequest = getTimeOffRequest(id);
            if (timeOffRequest != null && timeOffRequest.Ziek == false) {
                timeOffRequest.IsApproved = true;
                context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Decline(int id) {
            var timeOffRequest = getTimeOffRequest(id);
            if (timeOffRequest != null && timeOffRequest.Ziek == false) {
                timeOffRequest.IsApproved = false;
                context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
