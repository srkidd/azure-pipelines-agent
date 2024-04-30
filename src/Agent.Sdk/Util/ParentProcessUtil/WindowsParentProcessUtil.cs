using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Agent.Sdk.Util.ParentProcessUtil
{
    public enum ParentProcessType { Other, PwshCore, WindowsPowershell };
    public static class WindowsParentProcessUtil
    {
        public static bool IsAgentRunningInPowerShell(bool useInteropToFindParentProcess)
        {
            var processList = GetProcessList(Process.GetCurrentProcess(), useInteropToFindParentProcess);

            var isProcessRunningInPowerShell = processList.Exists(process =>
                GetParentProcessType(process) == ParentProcessType.PwshCore
                    || GetParentProcessType(process) == ParentProcessType.WindowsPowershell);

            return isProcessRunningInPowerShell;
        }

        private static List<Process> GetProcessList(Process process, bool useInterop)
        {
            var processList = new List<Process>() { process };
            int maxSearchDepthForProcess = 10;

            while (processList.Count < maxSearchDepthForProcess)
            {
                Process lastProcess = processList.Last();

                (Process parentProcess, Dictionary<string, string> telemetry) = useInterop
                    ? InteropParentProcessFinder.GetParentProcess(lastProcess)
                    : WmiParentProcessFinder.GetParentProcess(lastProcess);

                if (parentProcess == null)
                {
                    break;
                }

                processList.Add(parentProcess);
            }

            return processList;
        }

        private static ParentProcessType GetParentProcessType(Process process)
        {
            try
            {
                // Getting process name can throw.
                string name = process.ProcessName.ToLower();

                if (name == "pwsh")
                {
                    return ParentProcessType.PwshCore;
                }
                if (name == "powershell")
                {
                    return ParentProcessType.WindowsPowershell;
                }
                else
                {
                    return ParentProcessType.Other;
                }
            }
            catch
            {
                return ParentProcessType.Other;
            }
        }
    }
}
