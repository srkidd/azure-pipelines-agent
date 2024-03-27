// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Agent.Sdk;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.Services.Agent.Util
{
    internal class Interop
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_BASIC_INFORMATION
        {
            internal IntPtr ExitStatus;
            internal IntPtr PebBaseAddress;
            internal IntPtr AffinityMask;
            internal IntPtr BasePriority;
            internal IntPtr UniqueProcessId;
            internal IntPtr InheritedFromUniqueProcessId;
        }

        [DllImport("ntdll.dll", SetLastError = true)]
        internal static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, out PROCESS_BASIC_INFORMATION processInformation, int processInformationLength, out int returnLength);
    }

    public static class WindowsProcessUtil
    {
        internal static int? GetParentProcessId(IntPtr handle)
        {
            Interop.PROCESS_BASIC_INFORMATION pbi;
            int returnLength;
            int status = Interop.NtQueryInformationProcess(handle, 0, out pbi, Marshal.SizeOf<Interop.PROCESS_BASIC_INFORMATION>(), out returnLength);

            if (status != 0) return null;

            int parentProcessId = pbi.InheritedFromUniqueProcessId.ToInt32();

            return parentProcessId;
        }

        internal static Process GetParentProcess(Process process)
        {
            try
            {
                int? parentProcessId = GetParentProcessId(process.Handle);

                if (parentProcessId == null) return null;

                Process parentProcess = Process.GetProcessById(parentProcessId.Value);

                if (parentProcess == null
                    || parentProcess.StartTime > process.StartTime
                    || parentProcess.HasExited)
                {
                    return null;
                }

                return parentProcess;
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (Win32Exception)
            {
                return null;
            }
        }

        public static Process GetParentProcess(int processId)
        {
            using var query = new ManagementObjectSearcher($"SELECT * FROM Win32_Process WHERE ProcessId={processId}");

            ManagementObject process = query.Get().OfType<ManagementObject>().FirstOrDefault();

            if (process == null)
            {
                return null;
            }

            var parentProcessId = (int)(uint)process["ParentProcessId"];

            try
            {
                return Process.GetProcessById(parentProcessId);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public static List<Process> GetProcessList(Process process, bool useInteropToFindParentProcess, ITraceWriter trace = null)
        {
            var processList = new List<Process>() { process };

            while (true)
            {
                Process lastProcess = processList.Last();
                trace?.Info($"testing lastProcess: {lastProcess}");
                Process parentProcess = useInteropToFindParentProcess
                    ? GetParentProcess(lastProcess)
                    : GetParentProcess(lastProcess.Id);

                if (parentProcess == null)
                {
                    return processList;
                }

                processList.Add(parentProcess);
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

        public static bool ProcessIsRunningInPowerShell(Process process, bool useInteropKnob, ITraceWriter trace = null)
        {
            return GetProcessList(process, useInteropKnob, trace).Exists(ProcessIsPowerShell);
        }

        public static bool AgentIsRunningInPowerShell(bool useInteropKnob, ITraceWriter trace = null)
        {
            return ProcessIsRunningInPowerShell(Process.GetCurrentProcess(), useInteropKnob, trace);
        }
    }
}
