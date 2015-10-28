using System;
using System.Diagnostics;
using System.IO;
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
                    var logfile = Path.Combine(_appEnviroment.ApplicationBasePath, "Projects", project.Name, "Logs", project.BuildNumber + ".log");
                    ExecuteProject(project, logfile);
                }
                catch (Exception ex)
                {
                    File.AppendAllText(@"C:\Temp\LoialLog.txt", ex.ToString());
                }
            });

            return true;
        }

        private void ExecuteProject(Project project, string logfile)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logfile));
                Exec(project.Command, msg => File.AppendAllText(logfile, msg));

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
            log($"EXEC: {cmd}");
            var process = new Process();
            try
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true; // Redirect the output stream of the child process.
                //process.StartInfo.Verb = "runas";
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/C " + cmd;
                //process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();

                while (!process.HasExited)
                {
                    log(process.StandardOutput.ReadToEnd());
                    log(process.StandardError.ReadToEnd());
                }
                process.WaitForExit(); //5 * 60 * 1000
                log(process.StandardOutput.ReadToEnd());
                log(process.StandardError.ReadToEnd());

                //Console.WriteLine("Process.ExitCode: " + process.ExitCode);
                if (process.ExitCode != 0)
                {
                    if (throws)
                    {
                        throw new ApplicationException("Process failed, ExitCode: " + process.ExitCode);
                    }
                    else
                    {
                        log($"Process failed, ExitCode: {process.ExitCode}");
                        return false;
                    }
                }
                return true;
            }
            finally
            {
                process.Close();
            }
        }
    };
}
