using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using BumboDB.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Bumbo.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<Employee> _userManager;

        public ProfileController(UserManager<Employee> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(
            [Required, EmailAddress] string email,
            [Required] string currentPassword,
            [Required, MinLength(6)] string newPassword)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid input" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(user, currentPassword);
            if (!passwordCheck)
            {
                return Json(new { success = false, message = "Current password is incorrect" });
            }

            user.Email = email;
            user.UserName = email;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return Json(new { success = false, message = "Failed to update email" });
            }

            var passwordResult = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!passwordResult.Succeeded)
            {
                return Json(new { success = false, message = "Failed to update password" });
            }

            return Json(new { success = true, message = "Profile updated successfully" });
        }
    }
}