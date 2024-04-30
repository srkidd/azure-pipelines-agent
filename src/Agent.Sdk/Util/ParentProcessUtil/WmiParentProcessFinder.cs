// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace Agent.Sdk.Util.ParentProcessUtil
{
    internal static class WmiParentProcessFinder
    {
        private const string ParentProcessState = nameof(ParentProcessState);

        internal static (Process process, Dictionary<string, string> telemetry) GetParentProcess(Process currentProcess)
        {
            using var query = new ManagementObjectSearcher($"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId={currentProcess.Id}");

            using ManagementObject foundProcess = query.Get().OfType<ManagementObject>().FirstOrDefault();

            if (foundProcess == null)
            {
                return (null, null);
            }

            try
            {
                var parentProcessId = (int)(uint)foundProcess["ParentProcessId"];

                return (Process.GetProcessById(parentProcessId), null);
            }
            catch
            {
                return (null, new() { [ParentProcessState] = "Error occurred while trying to get parent process id via WMI" });
            }
        }
    }
}
