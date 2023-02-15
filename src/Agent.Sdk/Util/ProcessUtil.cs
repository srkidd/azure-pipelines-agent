// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace Microsoft.VisualStudio.Services.Agent.Util
{
    public static class WindowsProcessUtil
    {
        public static Process GetParentProcess(int processId)
        {
            using var query = new ManagementObjectSearcher($"SELECT * FROM Win32_Process WHERE ProcessId={processId}");
            var process = query.Get().OfType<ManagementObject>().First();
            var parentProcessId = (int)(uint)process["ParentProcessId"];

            // Getting process by id can throw.
            return Process.GetProcessById(parentProcessId);
        }

        public static List<Process> GetProcessList(Process process)
        {
            var processList = new List<Process>(){ process };

            while (true)
            {
                try
                {
                    int lastProcessId = processList.Last().Id;

                    // Getting parent process can throw.
                    Process parentProcess = GetParentProcess(lastProcessId);

                    processList.Add(parentProcess);
                }
                catch
                {
                    return processList;
                }
            }
        }

        public static bool ProcessIsPowerShell(Process process)
        {
            try
            {
                // Getting process name can throw.
                string name = process.ProcessName.ToLower();

                return name == "pwsh" || name == "powershell";
            }
            catch
            {
                return false;
            }
        }

        public static bool ProcessIsRunningInPowerShell(Process process)
        {
            return GetProcessList(process).Exists(ProcessIsPowerShell);
        }

        public static bool AgentIsRunningInPowerShell()
        {
            return ProcessIsRunningInPowerShell(Process.GetCurrentProcess());
        }
    }
}
