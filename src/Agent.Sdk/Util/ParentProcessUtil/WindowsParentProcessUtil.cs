using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Agent.Sdk.Util.ParentProcessUtil
{
    public enum ParentProcessNames { Pwsh, Powershell };

    public static class WindowsParentProcessUtil
    {
        public static (bool, Dictionary<string, string>) IsParentProcess(bool useInteropToFindParentProcess, params ParentProcessNames[] processNames)
        {
            var processList = new List<Process>();

            try
            {
                (processList, var telemetry) = GetProcessList(Process.GetCurrentProcess(), useInteropToFindParentProcess);

                bool isProcessRunningInPowerShell = processList.Exists(process => IsProcessPowershell(process, processNames));

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

        private static bool IsProcessPowershell(Process process, params ParentProcessNames[] processNamesForSearch)
        {
            try
            {
                // Getting process name can throw.
                string name = process.ProcessName.ToLower();
                
                return Array.Exists(processNamesForSearch, enumValue => enumValue.ToString().ToLower() == name);
            }
            catch
            {
                return false;
            }
        }
    }
}
