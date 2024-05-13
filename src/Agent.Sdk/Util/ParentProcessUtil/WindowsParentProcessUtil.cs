using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Agent.Sdk.Util.ParentProcessUtil
{
    public static class WindowsParentProcessUtil
    {
        public static (bool, Dictionary<string, string>) IsAgentRunningInPowerShellOrCmd(bool useInteropToFindParentProcess)
        {
            var processList = new List<Process>();

            try
            {
                (processList, var telemetry) = GetProcessList(Process.GetCurrentProcess(), useInteropToFindParentProcess);

                var isProcessRunningInPowerShell = processList.Exists(IsProcessPowershellOrCmd);

                return (isProcessRunningInPowerShell, telemetry);
            }
            finally
            {
                foreach (var process in processList)
                {
                    process.Dispose();
                }
            }
        }

        internal static (List<Process>, Dictionary<string, string>) GetProcessList(Process process, bool useInterop)
        {
            var telemetry = new Dictionary<string, string>();
            var processList = new List<Process>() { process };
            const int maxSearchDepthForProcess = 10;

            while (processList.Count < maxSearchDepthForProcess)
            {
                Process lastProcess = processList.Last();

                (Process parentProcess, telemetry) = useInterop
                    ? InteropParentProcessFinder.GetParentProcess(lastProcess)
                    : WmiParentProcessFinder.GetParentProcess(lastProcess);

                if (parentProcess == null)
                {
                    break;
                }

                processList.Add(parentProcess);
            }

            return (processList, telemetry);
        }

        private static bool IsProcessPowershellOrCmd(Process process)
        {
            try
            {
                // Getting process name can throw.
                string name = process.ProcessName.ToLower();

                return name == "pwsh" || name == "powershell" || name == "cmd";
            }
            catch
            {
                return false;
            }
        }
    }
}
