using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolutionRenamer.Win.Logic.Logging;

namespace SolutionRenamer.Win.Logic.Shell
{
    // Or "ProcessHelper".. ugh.
    public static class ProcessUtility
    {
        public static ProcessResult RunCommand(string name, string command, ILogger logger)
        {
            // TODO: wrap in a provider instead of doing this directly
            try
            {
                logger.WriteInfo("Executing: " + name);
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };

                var process = new Process { StartInfo = startInfo };

                process.Start();
                process.StandardInput.WriteLine(command);
                process.StandardInput.WriteLine("exit");

                process.WaitForExit();

                logger.WriteInfo("Finished: " + name);
                return new ProcessResult($"{name} ran successfully", true);
            }
            catch (Exception ex)
            {
                logger.WriteWarning("Failed: " + name);
                return new ProcessResult($"{name} failed: {ex.ToString()}", false);
            }
        }
    }
}