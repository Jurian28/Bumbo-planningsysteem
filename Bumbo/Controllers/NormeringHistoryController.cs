using BumboDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;

namespace Bumbo.Controllers
{
    public class NormeringHistoryController : Controller
    {
        private readonly BumboContext _context;
        private readonly ILogger<NormeringHistoryController> _logger;

        public NormeringHistoryController(BumboContext context, ILogger<NormeringHistoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var managerChapter = _context.Chapters.FirstOrDefault(c => c.Manager == userId);

            if (managerChapter == null)
            {
                _logger.LogWarning("Manager chapter not found for user {UserId}", userId);
                return View(new List<Norm>());
            }

            var norms = _context.Norms
                .Where(n => n.ChapterId == managerChapter.ChapterId)
                .OrderByDescending(n => n.Date)
                .ToList();

            return View("Index", norms);
        }

        [HttpGet]
        public IActionResult EditNorm(int? NormId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var managerChapter = _context.Chapters.FirstOrDefault(c => c.Manager == userId);

            if (managerChapter == null)
            {
                _logger.LogWarning("Manager chapter not found for user {UserId}", userId);
                return NotFound();
            }

            Norm norm = null;
            if (NormId.HasValue)
            {
                norm = _context.Norms.FirstOrDefault(n => n.NormId == NormId && n.ChapterId == managerChapter.ChapterId);
            }
            else
            {
                norm = _context.Norms.Where(n => n.ChapterId == managerChapter.ChapterId)
                    .OrderByDescending(n => n.Date)
                    .FirstOrDefault();
            }

            if (norm == null)
            {
                _logger.LogWarning("Norm not found for NormId {NormId} and ChapterId {ChapterId}", NormId, managerChapter.ChapterId);
                return NotFound();
            }

            return View(norm);
        }

        [HttpPost]
        public IActionResult UpdateNorm(Norm norm)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var managerChapter = _context.Chapters.FirstOrDefault(c => c.Manager == userId);

                if (managerChapter == null)
                {
                    _logger.LogWarning("Manager chapter not found for user {UserId}", userId);
                    return NotFound();
                }

                norm.ChapterId = managerChapter.ChapterId;

                var existingNorm = _context.Norms.FirstOrDefault(n => n.NormId == norm.NormId && n.ChapterId == norm.ChapterId);
                if (existingNorm != null)
                {
                    _context.Entry(existingNorm).CurrentValues.SetValues(norm);
                    _context.Entry(existingNorm).State = EntityState.Modified;
                    _context.SaveChanges();
                    _logger.LogInformation("Norm updated successfully for NormId {NormId}", norm.NormId);
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.LogWarning("Existing norm not found for NormId {NormId} and ChapterId {ChapterId}", norm.NormId, norm.ChapterId);
                }
            }
            else
            {
                _logger.LogWarning("Model state is invalid for NormId {NormId}", norm.NormId);
            }
            return View("EditNorm", norm);
        }

        [HttpGet]
        public IActionResult CreateNorm()
        {
            return View(new Norm());
        }

        [HttpPost]
        public IActionResult CreateNorm(Norm norm)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var managerChapter = _context.Chapters.FirstOrDefault(c => c.Manager == userId);

                if (managerChapter != null)
                {
                    norm.ChapterId = managerChapter.ChapterId;
                    _context.Norms.Add(norm);
                    _context.SaveChanges();
                    _logger.LogInformation("Norm created successfully for ChapterId {ChapterId}", managerChapter.ChapterId);
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.LogWarning("Manager chapter not found for user {UserId}", userId);
                }
            }
            else
            {
                _logger.LogWarning("Model state is invalid for new norm");
            }
            return View(norm);
        }

        [HttpPost]
        public IActionResult DeleteNorm(int NormId)
        {
            var norm = _context.Norms.Find(NormId);
            if (norm != null)
            {
                _context.Norms.Remove(norm);
                _context.SaveChanges();
                _logger.LogInformation("Norm deleted successfully for NormId {NormId}", NormId);
            }
            else
            {
                _logger.LogWarning("Norm not found for NormId {NormId}", NormId);
            }
            return RedirectToAction("Index");
        }
    }
}