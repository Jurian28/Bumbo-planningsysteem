using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;
using Bumbo.Models;
using BumboDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        BumboContext context = new BumboContext();
        var dbTemplates = context.Templates.Where(t=> t.ChapterId == GetChapterByUserID()).ToList();

        ViewData["Templates"] = dbTemplates;

        PrognoseViewModel prognoseViewModel = new PrognoseViewModel();

        return View(prognoseViewModel);
    }

    public IActionResult ChangeLanguage(string lang)
    {
        if (!string.IsNullOrEmpty(lang))
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(lang);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
        }
        else
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
        }
        Response.Cookies.Append("Language", lang);
        return Redirect(Request.GetTypedHeaders().Referer.ToString());
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public int GetChapterByUserID()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("User ID: {UserId}", userId);

        BumboContext context = new BumboContext();
        var managerChapter = context.Chapters.FirstOrDefault(c => c.Manager == userId);
        return managerChapter.ChapterId;
    }

    [HttpGet]
    public JsonResult GetAllTemplates()
    {
        BumboContext context = new BumboContext();
        var dbTemplates = context.Templates.ToList();
        Dictionary<int, List<string>> templates = new Dictionary<int, List<string>>();

        foreach (var template in dbTemplates)
        {
            templates.Add(template.TemplateId, new List<string> { template.PredictedCustomers.ToString(), template.PredictedCargo.ToString(), template.Name });
        }
        return Json(templates);
    }

    [HttpPost]
    public IActionResult PrognosisCreated(PrognoseViewModel templates)
    {
        int[] templateDaysArray = templates.SelectedTemplates;

        BumboContext context = new();
        List<DepartmentResult> departments = getDepartments();

        for (int i = 0; i < templateDaysArray.Length; i++)
        {
            var templateId = templateDaysArray[i];
            int[] templateNumbers = getTemplateNumbers(templateId);

            int templateCargo = templateNumbers[1];
            int templateCustomers = templateNumbers[0];

            foreach (var department in departments)
            {
                department.SelectedWeek = templates.Week;
                department.SelectedYear = templates.Year;

                department.TemplateName[i] = context.Templates.FirstOrDefault(dbTemplateId => dbTemplateId.TemplateId == templateId)?.Name;
                department.TemplateCargo[i] = templateCargo;
                department.TemplateCustomers[i] = templateCustomers;

                Norm? dbNorm = context.Norms.OrderByDescending(norm => norm.Date).FirstOrDefault();
                
                if (dbNorm != null)
                {
                    int hoursPrognosis = CalculateHours(dbNorm, department, templateCargo, templateCustomers);
                    department.Results[i] = hoursPrognosis;
                }
                
                
            }
        }

        return View(departments);
    }

    private int CalculateHours(Norm dbNorm, DepartmentResult department, int templateCargo, int templateCustomers)
    {
        BumboContext context = new();
        var chapter = context.Chapters.FirstOrDefault(c => c.ChapterId == dbNorm.ChapterId);
        
        switch (department.Name)
        {
            case "Vers":
                return templateCustomers/dbNorm.FreshCustomersPerHour;
            case "Kassa":
                return templateCustomers/dbNorm.CashierCustomersPerHour;
            case "Vulploeg":
                return ((dbNorm.UnloadingSeconds+ dbNorm.ShelfFillingSeconds)* templateCargo
                        + dbNorm.FacingSecondsPerMeter * chapter.Meters) / 3600;
            default:
                throw new Exception("Unknown department");
        }
    }

    [HttpPost]
    public IActionResult SubmitPrognose(IFormCollection form)
    {
        int week = int.Parse(form["week"]);
        int year = int.Parse(form["year"]);

        BumboContext context = new BumboContext();

        if (!context.PrognosesWeeks.Any(p => p.Week == week && p.Year == year && p.Chapter == 1))
        {
            context.PrognosesWeeks.Add(new PrognosesWeek
            {
                Chapter = 1,
                Week = week,
                Year = year,
                BeginDate = GetDate(year, week, DayOfWeek.Monday),
                EndDate = GetDate(year, week, DayOfWeek.Sunday)
            });
        }

        foreach (var key in form.Keys)
        {
            DateOnly date;

            if (key.Contains("_Monday"))
            {
                date = GetDate(year, week, DayOfWeek.Monday);
                string departmentName = key.Replace("_Monday", "");
                int departmentId = context.Departments.FirstOrDefault(department => department.Name == departmentName && department.Chapter == 1).DepartmentId;
                int value = int.Parse(form[key]);

                var existingPrognosis = context.Prognoses.FirstOrDefault(p => p.Department == departmentId && p.Day == date);

                if (existingPrognosis != null)
                {
                    existingPrognosis.Result = value;
                }
                else
                {
                    var prognosis = new Prognosis
                    {
                        Department = departmentId,
                        Day = date,
                        Result = value
                    };

                    context.Prognoses.Add(prognosis);
                }

                context.SaveChanges();
            }
            else if (key.Contains("_Tuesday"))
            {
                date = GetDate(year, week, DayOfWeek.Tuesday);
                string departmentName = key.Replace("_Tuesday", "");
                int departmentId = context.Departments.FirstOrDefault(department => department.Name == departmentName && department.Chapter == 1).DepartmentId;
                int value = int.Parse(form[key]);

                var existingPrognosis = context.Prognoses.FirstOrDefault(p => p.Department == departmentId && p.Day == date);

                if (existingPrognosis != null)
                {
                    existingPrognosis.Result = value;
                }
                else
                {
                    var prognosis = new Prognosis
                    {
                        Department = departmentId,
                        Day = date,
                        Result = value
                    };

                    context.Prognoses.Add(prognosis);
                }

                context.SaveChanges();
            }
            else if (key.Contains("_Wednesday"))
            {
                date = GetDate(year, week, DayOfWeek.Wednesday);
                string departmentName = key.Replace("_Wednesday", "");
                int departmentId = context.Departments.FirstOrDefault(department => department.Name == departmentName && department.Chapter == 1).DepartmentId;
                int value = int.Parse(form[key]);

                var existingPrognosis = context.Prognoses.FirstOrDefault(p => p.Department == departmentId && p.Day == date);

                if (existingPrognosis != null)
                {
                    existingPrognosis.Result = value;
                }
                else
                {
                    var prognosis = new Prognosis
                    {
                        Department = departmentId,
                        Day = date,
                        Result = value
                    };

                    context.Prognoses.Add(prognosis);
                }

                context.SaveChanges();
            }
            else if (key.Contains("_Thursday"))
            {
                date = GetDate(year, week, DayOfWeek.Thursday);
                string departmentName = key.Replace("_Thursday", "");
                int departmentId = context.Departments.FirstOrDefault(department => department.Name == departmentName && department.Chapter == 1).DepartmentId;
                int value = int.Parse(form[key]);

                var existingPrognosis = context.Prognoses.FirstOrDefault(p => p.Department == departmentId && p.Day == date);

                if (existingPrognosis != null)
                {
                    existingPrognosis.Result = value;
                }
                else
                {
                    var prognosis = new Prognosis
                    {
                        Department = departmentId,
                        Day = date,
                        Result = value
                    };

                    context.Prognoses.Add(prognosis);
                }

                context.SaveChanges();
            }
            else if (key.Contains("_Friday"))
            {
                date = GetDate(year, week, DayOfWeek.Friday);
                string departmentName = key.Replace("_Friday", "");
                int departmentId = context.Departments.FirstOrDefault(department => department.Name == departmentName && department.Chapter == 1).DepartmentId;
                int value = int.Parse(form[key]);

                var existingPrognosis = context.Prognoses.FirstOrDefault(p => p.Department == departmentId && p.Day == date);

                if (existingPrognosis != null)
                {
                    existingPrognosis.Result = value;
                }
                else
                {
                    var prognosis = new Prognosis
                    {
                        Department = departmentId,
                        Day = date,
                        Result = value
                    };

                    context.Prognoses.Add(prognosis);
                }

                context.SaveChanges();
            }
            else if (key.Contains("_Saturday"))
            {
                date = GetDate(year, week, DayOfWeek.Saturday);
                string departmentName = key.Replace("_Saturday", "");
                int departmentId = context.Departments.FirstOrDefault(department => department.Name == departmentName && department.Chapter == 1).DepartmentId;
                int value = int.Parse(form[key]);

                var existingPrognosis = context.Prognoses.FirstOrDefault(p => p.Department == departmentId && p.Day == date);

                if (existingPrognosis != null)
                {
                    existingPrognosis.Result = value;
                }
                else
                {
                    var prognosis = new Prognosis
                    {
                        Department = departmentId,
                        Day = date,
                        Result = value
                    };

                    context.Prognoses.Add(prognosis);
                }

                context.SaveChanges();
            }
            else if (key.Contains("_Sunday"))
            {
                date = GetDate(year, week, DayOfWeek.Sunday);
                string departmentName = key.Replace("_Sunday", "");
                int departmentId = context.Departments.FirstOrDefault(department => department.Name == departmentName && department.Chapter == 1).DepartmentId;
                int value = int.Parse(form[key]);

                var existingPrognosis = context.Prognoses.FirstOrDefault(p => p.Department == departmentId && p.Day == date);

                if (existingPrognosis != null)
                {
                    existingPrognosis.Result = value;
                }
                else
                {
                    var prognosis = new Prognosis
                    {
                        Department = departmentId,
                        Day = date,
                        Result = value
                    };

                    context.Prognoses.Add(prognosis);
                }

                context.SaveChanges();
            }
        }

        // HIER EEN ALERT

        return RedirectToAction("Prognoses");
    }

    public static DateOnly GetDate(int year, int week, DayOfWeek dayOfWeek)
    {
        return DateOnly.FromDateTime(ISOWeek.ToDateTime(year, week, dayOfWeek));
    }

    private List<DepartmentResult> getDepartments()
    {
        BumboContext context = new BumboContext();
        var dbDepartments = context.Departments.Where(d => d.Chapter == GetChapterByUserID()).ToList();
        List<DepartmentResult> depRes = new List<DepartmentResult>();

        foreach (var department in dbDepartments)
        {
            depRes.Add(new DepartmentResult { Id = department.DepartmentId, Name = department.Name, Description = department.Description });
        }

        return depRes;
    }

    private int[] getTemplateNumbers(int templateId)
    {
        BumboContext context = new BumboContext();
        var dbTemplates = context.Templates.ToList();

        int[] templateNumbers = new int[2]
        {
            dbTemplates.FirstOrDefault(template => template.TemplateId == templateId)?.PredictedCustomers ?? 0,
            dbTemplates.FirstOrDefault(template => template.TemplateId == templateId)?.PredictedCargo ?? 0
        };

        return templateNumbers;
    }

    public IActionResult Prognoses()
    {
        BumboContext context = new BumboContext();
        var prognosesWeeks = context.PrognosesWeeks.Where(p => p.Chapter == GetChapterByUserID()).OrderByDescending(y => y.Year).ThenByDescending(w => w.Week).ToList();

        return View(prognosesWeeks);
    }

    public IActionResult ViewPrognosis(int year, int week)
    {
        List<DepartmentResult> departments = getDepartments();

        BumboContext context = new BumboContext();

        foreach (var department in departments)
        {
            department.SelectedWeek = week;
            department.SelectedYear = year;

            


            department.Results[0] = context.Prognoses.FirstOrDefault(p => p.Department == department.Id && p.Day == GetDate(year, week, DayOfWeek.Monday))?.Result ?? 0;
            department.Results[1] = context.Prognoses.FirstOrDefault(p => p.Department == department.Id && p.Day == GetDate(year, week, DayOfWeek.Tuesday))?.Result ?? 0;
            department.Results[2] = context.Prognoses.FirstOrDefault(p => p.Department == department.Id && p.Day == GetDate(year, week, DayOfWeek.Wednesday))?.Result ?? 0;
            department.Results[3] = context.Prognoses.FirstOrDefault(p => p.Department == department.Id && p.Day == GetDate(year, week, DayOfWeek.Thursday))?.Result ?? 0;
            department.Results[4] = context.Prognoses.FirstOrDefault(p => p.Department == department.Id && p.Day == GetDate(year, week, DayOfWeek.Friday))?.Result ?? 0;
            department.Results[5] = context.Prognoses.FirstOrDefault(p => p.Department == department.Id && p.Day == GetDate(year, week, DayOfWeek.Saturday))?.Result ?? 0;
            department.Results[6] = context.Prognoses.FirstOrDefault(p => p.Department == department.Id && p.Day == GetDate(year, week, DayOfWeek.Sunday))?.Result ?? 0;
        }

        return View(departments);
    }

   
}