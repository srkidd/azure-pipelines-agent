using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Agent.Sdk.Util.ParentProcessUtil
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

    internal static class InteropParentProcessFinder
    {
        private const string ParentProcessState = nameof(ParentProcessState);

        private static int? GetParentProcessId(IntPtr handle)
        {
            Interop.PROCESS_BASIC_INFORMATION pbi;
            int returnLength;
            int status = Interop.NtQueryInformationProcess(handle, 0, out pbi, Marshal.SizeOf<Interop.PROCESS_BASIC_INFORMATION>(), out returnLength);

            if (status != 0)
            {
                return null;
            }

            int parentProcessId = pbi.InheritedFromUniqueProcessId.ToInt32();

            return parentProcessId;
        }

        internal static (Process process, Dictionary<string, string> telemetry) GetParentProcess(Process process)
        {
            var telemetry = new Dictionary<string, string>();

            try
            {
                int? parentProcessId = GetParentProcessId(process.Handle);

                if (parentProcessId == null)
                {
                    return (null, telemetry);
                }

                Process parentProcess = Process.GetProcessById(parentProcessId.Value);

                if (parentProcess == null
                    || parentProcess.StartTime > process.StartTime
                    || parentProcess.HasExited)
                {
                    return (null, telemetry);
                }

                return (parentProcess, telemetry);
            }
            catch (ArgumentException)
            {
                telemetry.Add(ParentProcessState, "Invalid argument passed for GetProcessById when using interop");
                return (null, telemetry);
            }
            catch (Win32Exception)
            {
                telemetry.Add(ParentProcessState, "Win32 exception: could not retrieve parent process using interop");
                return (null, telemetry);
            }
        }
    }
}
