// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace Microsoft.VisualStudio.Services.Agent.Util
{
    public static class ProcessUtil
    {
        public static Process GetParentProcess(int processId)
        {
            try
            {
                using var query = new ManagementObjectSearcher($"SELECT * FROM Win32_Process WHERE ProcessId={processId}");
                var process = query.Get().OfType<ManagementObject>().First();
                var parentProcessId = (int)(uint)process["ParentProcessId"];
                return Process.GetProcessById(parentProcessId);
            }
            catch
            {
                return null;
            }
        }

        public static List<Process> GetProcessList(Process process)
        {
            var processList = new List<Process>(){ process };

            while (true)
            {
                Process parentProcess = GetParentProcess(processList.Last().Id);

                if (parentProcess == null)
                {
                    break;
                }

                processList.Add(parentProcess);
            }

            return processList;
        }

        public static bool ProcessIsRunningInPowerShell(Process process)
        {
            return GetProcessList(process).Exists(process => process.ProcessName.ToLower() == "pwsh" || process.ProcessName.ToLower() == "powershell");
        }

        public static bool AgentIsRunningInPowerShell()
        {
            return ProcessIsRunningInPowerShell(Process.GetCurrentProcess());
        }
    }
}
