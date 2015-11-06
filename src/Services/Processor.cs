using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.Dnx.Runtime;

namespace Loial
{
    public class Processor
    {
        private readonly LoialDb _db;
        private readonly IApplicationLifetime _lifetime;
        private readonly IApplicationEnvironment _appEnviroment;

        public Processor(LoialDb db, IApplicationLifetime lifetime, IApplicationEnvironment appEnviroment)
        {
            _db = db;
            _lifetime = lifetime;
            _appEnviroment = appEnviroment;
        }

        public bool Run(Project project)
        {
            if (project == null)
                return false;

            if (project.IsRunning)
                return project.IsRunning;

            project.IsRunning = true;
            project.BuildNumber += 1;
            _db.SaveChanges();

            QueueBackgroundWorkItem(cancellationToken =>
            {
                try
                {
                    var logfile = project.GetLogFilePath(_appEnviroment.ApplicationBasePath, project.BuildNumber);
                    ExecuteProject(project, logfile);
                }
                catch (Exception ex)
                {
                    File.AppendAllText(@"C:\Temp\LoialLog.txt", ex.ToString());
                }
            });

            return true;
        }

        private string GetBuildBatContents(Project project, string globalBuild)
        {
            var branch = project.Branch;
            var sb = new StringBuilder();
            sb.AppendLine(":: Project Settings :::::::::::::::::::::::");
            sb.AppendLine($"SET JOB_NAME={project.Name}");
            sb.AppendLine($"SET GIT_BRANCH={branch}");
            sb.AppendLine($"SET BUILD_NUMBER={project.BuildNumber}");
            sb.AppendLine($"SET CLONE_URL={project.CloneUrl}");
            sb.AppendLine();
            sb.AppendLine(":: Global Build Script :::::::::::::::::::::");
            sb.AppendLine(File.ReadAllText(globalBuild));
            sb.AppendLine();
            sb.AppendLine(":: Project Build Script :::::::::::::::::::::");
            sb.AppendLine(project.Command);
            return sb.ToString();
        }

        private void ExecuteProject(Project project, string logfile)
        {
            try
            {
                var globalBuild = Path.GetFullPath(Path.Combine(project.GetFolder(_appEnviroment.ApplicationBasePath), @"..\GlobalBuild.bat"));
                var buildfile = Path.Combine(project.GetFolder(_appEnviroment.ApplicationBasePath), "build.bat");
                File.WriteAllText(buildfile, GetBuildBatContents(project, globalBuild));

                Exec(buildfile, msg => File.AppendAllText(logfile, msg));

                project.IsRunning = false;
                _db.SaveChanges();

                File.AppendAllText(logfile, "[Completed]");
            }
            catch (Exception ex)
            {
                File.AppendAllText(logfile, $"[Error] {ex}");
            }
        }

        //REF: http://stackoverflow.com/questions/27676140/what-is-the-equivalent-of-registerobject-queuebackgroundworkitem-in-asp-net-5
        private void QueueBackgroundWorkItem(Action<CancellationToken> service)
        {
            var stoppingToken = _lifetime.ApplicationStopping;
            var stoppedToken = _lifetime.ApplicationStopped;

            // This, in particular, would need to be properly thought out,
            // preferably including an execution context to minimise threadpool use
            // for async-heavy background services
            var serviceTask = Task.Run(() => service(stoppingToken));

            stoppedToken.Register(() =>
            {
                try
                {
                    // Block (with timeout) to allow graceful shutdown
                    if (!serviceTask.Wait(TimeSpan.FromSeconds(30)))
                    {
                        // Log: Background service didn't gracefully shutdown.
                        //      It will be terminated with the host process
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText(@"C:\Temp\LoialLog.txt", ex.ToString());
                }
            });
        }

        private static bool Exec(string cmd, Action<string> log, bool throws = false)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"EXEC: {cmd}");
            var process = new Process();
            try
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/C " + cmd;
                process.StartInfo.WorkingDirectory = Path.GetDirectoryName(cmd);
                process.Start();

                process.OutputDataReceived += (sender, e) => sb.AppendLine(e.Data);
                process.BeginOutputReadLine();
                process.ErrorDataReceived += (sender, e) => sb.AppendLine(e.Data);
                process.BeginErrorReadLine();
                process.WaitForExit(); //5 * 60 * 1000

                if (process.ExitCode != 0)
                {
                    if (throws)
                    {
                        throw new ApplicationException("Process failed, ExitCode: " + process.ExitCode);
                    }
                    else
                    {
                        sb.AppendLine($"Process failed, ExitCode: {process.ExitCode}");
                        return false;
                    }
                }
                return true;
            }
            finally
            {
                log(sb.ToString());
                process.Close();
            }
        }
    };
}
