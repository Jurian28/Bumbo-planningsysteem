using Microsoft.AspNetCore.Mvc;
using BumboDB;
using BumboDB.Models;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Bumbo.Controllers
{
    [Authorize]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class TemplateController : Controller
    {
        private const int PageSize = 10; 
        private readonly BumboContext context;
        private readonly ILogger<TemplateController> logger;

        public TemplateController(BumboContext context, ILogger<TemplateController> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public IActionResult Index(int currentPage = 1)
        {
            if (currentPage < 1)
            {
                logger.LogWarning("Invalid currentPage value: {CurrentPage}", currentPage);
                return RedirectToAction("Index", new { currentPage = 1 });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            logger.LogInformation("User ID: {UserId}", userId);

            var managerChapter = context.Chapters.FirstOrDefault(c => c.Manager == userId);

            if (managerChapter == null)
            {
                logger.LogWarning("Manager chapter not found for user ID: {UserId}", userId);
                return View(new Tuple<IEnumerable<Norm>, IEnumerable<Template>>(new List<Norm>(), new List<Template>()));
            }

            var templates = context.Templates
                .Where(t => t.ChapterId == managerChapter.ChapterId)
                .Skip((currentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var norms = new List<Norm>();

            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPages = (int)Math.Ceiling((double)context.Templates.Count(t => t.ChapterId == managerChapter.ChapterId) / PageSize);

            var model = new Tuple<IEnumerable<Norm>, IEnumerable<Template>>(norms, templates);
            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateTemplate(Template template)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                logger.LogInformation("User ID: {UserId}", userId);

                var managerChapter = context.Chapters.FirstOrDefault(c => c.Manager == userId);

                if (managerChapter != null)
                {
                    template.ChapterId = managerChapter.ChapterId;

                    var maxTemplateId = context.Templates.Max(t => (int?)t.TemplateId) ?? 0;
                    template.TemplateId = maxTemplateId + 1;

                    context.Templates.Add(template);
                    context.SaveChanges();
                    logger.LogInformation("Template created successfully for ChapterId {ChapterId}", managerChapter.ChapterId);
                    TempData["SuccessMessage"] = "Je template is gemaakt.";
                    return RedirectToAction("Index");
                }
                else
                {
                    logger.LogWarning("Manager chapter not found for user ID: {UserId}", userId);
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    logger.LogWarning("Model state error: {ErrorMessage}", error.ErrorMessage);
                }
            }

            return RedirectToAction("Index");
        }
        
        [HttpPost]
        public IActionResult DeleteTemplate(int templateId, int chapterId)
        {
            var template = context.Templates.Find(templateId, chapterId);
            if (template != null)
            {
                context.Templates.Remove(template);
                context.SaveChanges();
                logger.LogInformation("Template deleted successfully with TemplateId {TemplateId} and ChapterId {ChapterId}", templateId, chapterId);
                TempData["SuccessMessage"] = "Je template is verwijderd.";
                return Json(new { success = true });
            }
            else
            {
                logger.LogWarning("Template not found with TemplateId {TemplateId} and ChapterId {ChapterId}", templateId, chapterId);
                TempData["ErrorMessage"] = "Template niet gevonden.";
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public IActionResult EditTemplate(Template template)
        {
            if (ModelState.IsValid)
            {
                var existingTemplate = context.Templates.Find(template.TemplateId, template.ChapterId);
                if (existingTemplate != null)
                {
                    existingTemplate.Name = template.Name;
                    existingTemplate.Description = template.Description;
                    existingTemplate.PredictedCustomers = template.PredictedCustomers;
                    existingTemplate.PredictedCargo = template.PredictedCargo;

                    context.Entry(existingTemplate).State = EntityState.Modified;
                    context.SaveChanges();
                    logger.LogInformation("Template edited successfully with TemplateId {TemplateId} and ChapterId {ChapterId}", template.TemplateId, template.ChapterId);
                    return Json(new { success = true });
                }
                else
                {
                    logger.LogWarning("Template not found with TemplateId {TemplateId} and ChapterId {ChapterId}", template.TemplateId, template.ChapterId);
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    logger.LogWarning("Model state error: {ErrorMessage}", error.ErrorMessage);
                }
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public IActionResult NextPage(int currentPage)
        {
            currentPage++;
            logger.LogInformation("Navigating to next page: {CurrentPage}", currentPage);
            return RedirectToAction("Index", new { currentPage });
        }

        [HttpPost]
        public IActionResult PreviousPage(int currentPage)
        {
            if (currentPage > 1)
            {
                currentPage--;
                logger.LogInformation("Navigating to previous page: {CurrentPage}", currentPage);
            }
            return RedirectToAction("Index", new { currentPage });
        }
    }
}