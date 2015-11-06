using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Framework.OptionsModel;

namespace Loial
{
    public class HomeController : Controller
    {
        private readonly LoialDb _db;
        private readonly Processor _processor;
        private readonly string _workspacesPath;

        public HomeController(LoialDb db, Processor processor, IOptions<AppSettings> appSettings)
        {
            _db = db;
            _processor = processor;
            _workspacesPath = appSettings.Options.WorkspacesPath;
        }

        public IActionResult Index()
        {
            var projects = _db
                .Projects
                .OrderBy(x => x.Name)
                .ToList();

            return View(projects);
        }

        public IActionResult Builds(int id)
        {
            var project = _db.Projects.Single(x => x.Id == id);
            ViewBag.Id = project.Id;
            ViewBag.Name = project.Name;

            var projectLogsPath = Path.Combine(project.GetFolder(_workspacesPath), "Logs");
            Directory.CreateDirectory(projectLogsPath);

            var files = new DirectoryInfo(projectLogsPath)
                .GetFiles()
                .OrderByDescending(x => x.CreationTime)
                .ToList();

            return View(files);
        }

        public IActionResult Project(int? id)
        {
            var project = _db.Projects.SingleOrDefault(x => x.Id == id)
                           ?? new Project {IsActive = true};
            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> Project(int? id, FormCollection form)
        {
            var project = await _db.Projects.SingleOrDefaultAsync(x => x.Id == id)
                          ?? _db.Projects.Add(new Project()).Entity;

            await TryUpdateModelAsync(project);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var project = await _db.Projects.SingleOrDefaultAsync(x => x.Id == id);
            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public IActionResult Run(int id)
        {
            var project = _db.Projects.Single(x => x.Id == id);
            _processor.Run(project, project.Branch);
            return RedirectToAction("Index");
        }

        public IActionResult Cancel(int id)
        {
            var project = _db.Projects.Single(x => x.Id == id);
            project.IsRunning = false;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Log(int id, int? buildNumber)
        {
            var project = _db.Projects.Single(x => x.Id == id);
            var logfilePath = project.GetLogFilePath(_workspacesPath, buildNumber ?? project.BuildNumber);

            var log = System.IO.File.Exists(logfilePath)
                ? System.IO.File.ReadAllText(logfilePath)
                : "File not found: " + logfilePath;

            return Content(log, "text/plain");
        }

        public dynamic Error()
        {
            return "error";
        }
    };
}
