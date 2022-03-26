using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Taskkiller
{
    public class Taskkiller
    {
        public static void Main(string[] args)
        {
            if (!(Environment.OSVersion.Platform == PlatformID.Win32NT))
            {
                Console.WriteLine("Only supported on Windows");
                return;
            }
            if (args.Length == 0 || new[] { "-h", "--help", "-?" }.Contains(args[0]))
            {

                Console.WriteLine($"{nameof(Taskkiller)} will find the Process with the supplied name and attempt to kill it.");
                Console.WriteLine($"Usage:");
                Console.WriteLine($"\t{nameof(Taskkiller)} <Process Name or part of it> <-c>");
                Console.WriteLine($"\tIf multiple Processes fit the supplied name, a list with possible matches is printed.");
                Console.WriteLine($"\tIf -c is set, {nameof(Taskkiller)} will ask you to confirm the killing of the Process.");
                return;
            }
            var taskName = args[0];
            var confirmKill = false;
            if (args.Length > 1 && new[] { "-c", "--confirm" }.Contains(args[1]))
            {
                confirmKill = true;
            }
            var tasklistProcessStartInfo = new ProcessStartInfo("tasklist")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = System.Text.Encoding.ASCII,
                CreateNoWindow = false
            };

            var taskListProcess = new Process
            {
                StartInfo = tasklistProcessStartInfo
            };
            taskListProcess.Start();

            var possibleCanidates = new List<(string ProcessName, string Pid)>();

            var regex = new Regex(@"^(?<ProcessName>([A-Za-z0-9 ]+(\.exe)?))( {3,})(?<PID>\d+) (Services|Console)", RegexOptions.Compiled);

            while (!taskListProcess.StandardOutput.EndOfStream)
            {
                var line = taskListProcess.StandardOutput.ReadLine();
                if (line?.Contains(taskName, StringComparison.InvariantCultureIgnoreCase) ?? false)
                {
                    var match = regex.Match(line);
                    possibleCanidates.Add((match.Groups["ProcessName"].Value, match.Groups["PID"].Value));
                }

            }
            taskListProcess.WaitForExit();

            if (possibleCanidates.Count == 0)
            {
                Console.WriteLine($"No match found for [{taskName}]");
                return;
            }

            int index = 0;

            if (possibleCanidates.Count > 1)
            {
                int i = 1;
                Console.WriteLine("Found the following Processes matching the supplied name");
                foreach (var possibleCanidate in possibleCanidates)
                {
                    Console.WriteLine($"{i}: {possibleCanidate.ProcessName}");
                    i++;
                }
                int read;
                Console.WriteLine("Write number of Process to kill or 0 to abort");
                do
                {
                    var input = Console.ReadLine();
                    if (!int.TryParse(input, out read))
                    {
                        //Invalid
                        read = -1;
                    }
                    if (read == 0)
                    {
                        Console.WriteLine("Aborting...");
                        return;
                    }
                } while (read < 0 || read > i);
                index = read - 1;
            }

            if (confirmKill)
            {
                Console.WriteLine($"Kill Process {possibleCanidates[index].ProcessName}? Y/N");
                string? read;
                do
                {
                    read = Console.ReadLine();
                } while (!(read?.Equals("y", StringComparison.InvariantCultureIgnoreCase) ?? false || (read?.Equals("n", StringComparison.InvariantCultureIgnoreCase) ?? false)));
                if (read.Equals("n", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine("Aborted on users wishes");
                    return;
                }
            }
            KillProcess(possibleCanidates[index].Pid);
        }

        private static void KillProcess(string pid)
        {
            var taskkillProcessStartInfo = new ProcessStartInfo("taskkill", $"/PID {pid} /F")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = System.Text.Encoding.ASCII,
                CreateNoWindow = false
            };

            var taskKillProcess = new Process
            {
                StartInfo = taskkillProcessStartInfo
            };
            taskKillProcess.Start();
            Console.WriteLine("Instructed taskkill to kill the process");
        }
    }
}