using BumboDB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Bumbo.Controllers {
    [Authorize(Roles = "Manager")] 
    public class MedewerkersBeherenController : Controller {
        private readonly BumboContext context;

        public MedewerkersBeherenController(BumboContext context) {
            this.context = context;
        }

        [HttpGet]
        public IActionResult Index(string searchQuery = "", string newEmployeePassword = "") {
            var employees = context.Employees.ToList();
            Chapter? managerChapter = GetManagerChapter();

            if (managerChapter == null) {
                return View();
            }

            var chapterEmployees = context.Employees
                .Where(e => e.Chapter == managerChapter.ChapterId)
                .ToList();

            chapterEmployees = chapterEmployees
                .Where(chapterEmployee => {
                    var employee = context.UserRoles.FirstOrDefault(e => e.UserId == chapterEmployee.Id);
                    return employee == null || employee.RoleId != "3";
                })
                .ToList();

            foreach (var employee in chapterEmployees) {
                var nowPlusThirty = DateTime.Now.AddDays(30);
                if (employee.FiredDate >= DateOnly.FromDateTime(nowPlusThirty)) {
                    Delete(employee.Id);
                }
            }

            var result = Search(searchQuery, chapterEmployees);
            ViewData["Query"] = searchQuery;
            ViewData["newEmployeePassword"] = newEmployeePassword;

            return View(result);
        }

        private Chapter? GetManagerChapter() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return context.Chapters.FirstOrDefault(c => c.Manager == userId);
        }

        public List<Employee> Search(string query, List<Employee> chapterEmployees) {
            if (string.IsNullOrWhiteSpace(query)) {
                return chapterEmployees;
            }

            return chapterEmployees
                .Where(e => e.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        [HttpGet]
        public IActionResult Create(int _) {
            var newEmployee = new Employee();
            try {
                int maxIntId = context.Employees
                    .Select(e => e.Id)
                    .Where(id => int.TryParse(id, out _))
                    .Select(id => int.Parse(id))
                    .DefaultIfEmpty(0)
                    .Max();

                int newIntId = maxIntId + 1;
                newEmployee.Id = newIntId.ToString();

                newEmployee.PasswordHash = GeneratePassword();
            } catch {
                newEmployee.Id = "1";
            }
            return View(newEmployee);
        }

        [HttpPost]
        public IActionResult Create(Employee newEmployee) {
            if (string.IsNullOrWhiteSpace(newEmployee.Email) || !new EmailAddressAttribute().IsValid(newEmployee.Email)) {
                ModelState.AddModelError("Email", "Enter a valid email address.");
            }

            if (string.IsNullOrWhiteSpace(newEmployee.PhoneNumber) || !Regex.IsMatch(newEmployee.PhoneNumber, @"^\d{10}$")) {
                ModelState.AddModelError("PhoneNumber", "Phone number must be exactly 10 digits.");
            }

            if (ModelState.IsValid) {
                try {
                    newEmployee.UserName = newEmployee.Name;
                    newEmployee.EmployedSince = DateOnly.FromDateTime(DateTime.Now);

                    newEmployee.EmailConfirmed = true;
                    newEmployee.NormalizedEmail = newEmployee.Email?.ToUpper();
                    newEmployee.NormalizedUserName = newEmployee.UserName?.ToUpper();
                    newEmployee.PhoneNumberConfirmed = true;
                    newEmployee.LockoutEnabled = false;

                    SetEmployeeChapter(newEmployee);
                    SetEmployeeIdentityUserRole(newEmployee);
                    SetHashedStandardPassword(newEmployee, newEmployee.PasswordHash);

                    context.Employees.Add(newEmployee);
                    context.SaveChanges();
                    return RedirectToAction("Index", new { newEmployeePassword = newEmployee.PasswordHash });
                } catch {
                    return View(newEmployee);
                }
            }
            return View(newEmployee);
        }

        private string GeneratePassword() {
            var randomNumber = new byte[6];
            using (var rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }

        private void SetEmployeeChapter(Employee newEmployee) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var managerChapter = context.Chapters.FirstOrDefault(m => m.Manager == userId);
            if (managerChapter != null) {
                newEmployee.Chapter = managerChapter.ChapterId;
            }
        }

        private void SetEmployeeIdentityUserRole(Employee newEmployee) {
            var identityRole = new IdentityUserRole<string> {
                UserId = newEmployee.Id,
                RoleId = "2" // Example role ID for "Employee"
            };
            context.Add(identityRole);
        }

        private void SetHashedStandardPassword(Employee newEmployee, string unhashedPassword) {
            var hasher = new PasswordHasher<Employee>();
            newEmployee.PasswordHash = hasher.HashPassword(newEmployee, unhashedPassword);
        }

        private void Delete(string id) {
            var toDelete = context.Employees.FirstOrDefault(e => e.Id == id);
            if (toDelete != null) {
                context.Employees.Remove(toDelete);
                context.SaveChanges();
            }
        }

        public IActionResult Fire(string id) {
            var toDelete = context.Employees.FirstOrDefault(e => e.Id == id);
            if (toDelete != null) {
                if (toDelete.Status != "Fired") {
                    toDelete.Status = "Fired";
                    toDelete.FiredDate = DateOnly.FromDateTime(DateTime.Now);
                } else {
                    toDelete.Status = "Active";
                    toDelete.FiredDate = null;
                }
                context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(string id) {
            var employee = context.Employees.FirstOrDefault(e => e.Id == id);
            if (employee == null) {
                return NotFound();
            }
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Employee employee) {
            if (string.IsNullOrWhiteSpace(employee.Email) || !new EmailAddressAttribute().IsValid(employee.Email)) {
                ModelState.AddModelError("Email", "Enter a valid email address.");
            }
            if (string.IsNullOrWhiteSpace(employee.PhoneNumber) || !Regex.IsMatch(employee.PhoneNumber, @"^\d{10}$")) {
                ModelState.AddModelError("PhoneNumber", "Phone number must be exactly 10 digits.");
            }

            if (ModelState.IsValid) {
                var existingEmployee = context.Employees.FirstOrDefault(e => e.Id == employee.Id);
                if (existingEmployee == null) {
                    return NotFound();
                }

                existingEmployee.Name = employee.Name;
                existingEmployee.UserName = employee.Name;
                existingEmployee.Birthday = employee.Birthday;
                existingEmployee.Zipcode = employee.Zipcode;
                existingEmployee.HouseNumber = employee.HouseNumber;
                existingEmployee.Email = employee.Email;
                existingEmployee.PhoneNumber = employee.PhoneNumber;
                existingEmployee.PayScale = employee.PayScale;

                context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(employee);
        }
    }
}
