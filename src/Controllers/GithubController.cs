using System.Collections.Generic;
using System.Linq;
using Common;
using Microsoft.AspNet.Mvc;

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
        public dynamic PushEvent([FromBody] Github.PushEvent pushEvent)
        {
            var repo = pushEvent?.Repository?.Full_Name;
            var branch = pushEvent?.Ref?.Replace("refs/heads/", "");

            var projects = FindProjects(repo, branch);
            if (projects.Count == 0)
            {
                //System.IO.File.AppendAllText(@"C:\Temp\Loial_GithubController_PushEvent_Errors.log",
                //    JsonConvert.SerializeObject(pushEvent, Formatting.Indented) + Environment.NewLine);

                return new
                {
                    error = "Project not found",
                    repo,
                    branch,
                    pushEvent
                };
            }

            var results = new List<dynamic>();
            foreach (var project in projects)
            {
                var started = _processor.Run(project, branch);
                results.Add(new
                {
                    started,
                    project
                });
            }

            return results;
        }

        private List<Project> FindProjects(string repo, string branch)
        {
            if (string.IsNullOrWhiteSpace(repo) || string.IsNullOrWhiteSpace(branch))
                return new List<Project>();

            return _db
                .Projects
                .Where(x => x.Repository == repo && x.IsActive)
                .ToArray()
                .Where(x => Ext.LikeString(x.Branch, branch))
                .ToList();
        }
    };
}
