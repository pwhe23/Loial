using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Newtonsoft.Json;

namespace Loial
{
    [Route("github")]
    public class GithubController : Controller
    {
        private readonly LoialDb _db;
        private readonly Processor _processor;

        public GithubController(LoialDb db, Processor processor)
        {
            _db = db;
            _processor = processor;
        }

        [HttpPost, Route("PushEvent")]
        public async Task<dynamic> PushEvent([FromBody] Github.PushEvent pushEvent)
        {
            var repo = pushEvent.Repository.Full_Name;
            var branch = pushEvent.Ref.Replace("refs/heads/", "");

            var project = await _db
                .Projects
                .SingleOrDefaultAsync(x => x.Repository == repo
                                           && x.Branch == branch);

            if (project == null)
            {
                System.IO.File.AppendAllText(@"C:\Temp\Loial_GithubController_PushEvent_Errors.log",
                    JsonConvert.SerializeObject(pushEvent, Formatting.Indented));
                return new
                {
                    error = "Project not found",
                    repo,
                    branch,
                    pushEvent
                };
            }

            var started = _processor.Run(project);
            return new
            {
                started,
                project
            };
        }
    };
}
