// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Agent.Sdk;
using Microsoft.VisualStudio.Services.Agent.Util;
using Microsoft.VisualStudio.Services.Agent.Worker.Telemetry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.Services.Agent.Worker
{
    [ServiceLocator(Default = typeof(ResourceMetricsManager))]
    public interface IResourceMetricsManager : IAgentService, IDisposable
    {
        Task RunDebugResourceMonitor();
        Task RunMemoryUtilizationMonitor();
        Task RunDiskSpaceUtilizationMonitor();
        Task RunCpuUtilizationMonitor(string taskId);
        void Setup(IExecutionContext context);
        void SetContext(IExecutionContext context);
    }

    public sealed class ResourceMetricsManager : AgentService, IResourceMetricsManager
    {
        private const int METRICS_UPDATE_INTERVAL = 5000;
        private const int ACTIVE_MODE_INTERVAL = 5000;
        private const int WARNING_MESSAGE_INTERVAL = 5000;
        private const int AVAILABLE_DISK_SPACE_PERCENTAGE_THRESHOLD = 5;
        private const int AVAILABLE_MEMORY_PERCENTAGE_THRESHOLD = 5;
        private const int CPU_UTILIZATION_PERCENTAGE_THRESHOLD = 95;

        private IExecutionContext _context;

        private Process _currentProcess;

        private static CpuInfo _cpuInfo;
        private static DiskInfo _diskInfo;
        private static MemoryInfo _memoryInfo;

        private static readonly object _cpuLock = new object();
        private static readonly object _diskLock = new object();
        private static readonly object _memoryLock = new object();

        private struct CpuInfo
        {
            public DateTime Updated;
            public double Usage;
        }

        private struct DiskInfo
        {
            public DateTime Updated;
            public long TotalDiskSpaceMB;
            public long FreeDiskSpaceMB;
            public string VolumeRoot;
        }

        public struct MemoryInfo
        {
            public DateTime Updated;
            public long TotalMemoryMB;
            public long UsedMemoryMB;
        }

        public void Setup(IExecutionContext context)
        {
            //initial context
            ArgUtil.NotNull(context, nameof(context));

            _context = context;

            _currentProcess = Process.GetCurrentProcess();
        }

        public void SetContext(IExecutionContext context)
        {
            ArgUtil.NotNull(context, nameof(context));
            _context = context;
        }

        public void Dispose()
        {
            _currentProcess?.Dispose();
        }

        private void PublishTelemetry(string message, string taskId)
        {
            try
            {
                Dictionary<string, string> telemetryData = new Dictionary<string, string>
                        {
                            { "TaskId", taskId },
                            { "JobId", _context.Variables.System_JobId.ToString() },
                            { "PlanId", _context.Variables.Get(Constants.Variables.System.PlanId) },
                            { "Warning", message }
                        };

                var cmd = new Command("telemetry", "publish")
                {
                    Data = JsonConvert.SerializeObject(telemetryData, Formatting.None)
                };

                cmd.Properties.Add("area", "AzurePipelinesAgent");
                cmd.Properties.Add("feature", "ResourceUtilization");

                var publishTelemetryCmd = new TelemetryCommandExtension();
                publishTelemetryCmd.Initialize(HostContext);
                publishTelemetryCmd.ProcessCommand(_context, cmd);
            }
            catch (Exception ex)
            {
                Trace.Warning($"Unable to publish resource utilization telemetry data. Exception: {ex.Message}");
            }
        }

        private CpuInfo GetCpuInfo()
        {
            lock (_cpuLock)
            {
                if (_cpuInfo.Updated == DateTime.Now - TimeSpan.FromMilliseconds(METRICS_UPDATE_INTERVAL))
                {
                    return _cpuInfo;
                }

                TimeSpan totalCpuTime = _currentProcess.TotalProcessorTime;
                TimeSpan elapsedTime = DateTime.Now - _currentProcess.StartTime;

                _cpuInfo.Updated = DateTime.Now;
                _cpuInfo.Usage = (totalCpuTime.TotalMilliseconds / elapsedTime.TotalMilliseconds) * 100.0;

                return _cpuInfo;
            }
        }

        private DiskInfo GetDiskInfo()
        {
            lock (_diskLock)
            {
                if (_diskInfo.Updated == DateTime.Now - TimeSpan.FromMilliseconds(METRICS_UPDATE_INTERVAL))
                {
                    return _diskInfo;
                }

                string root = Path.GetPathRoot(_context.GetVariableValueOrDefault(Constants.Variables.Agent.WorkFolder));
                var driveInfo = new DriveInfo(root);

                _diskInfo.Updated = DateTime.Now;
                _diskInfo.TotalDiskSpaceMB = driveInfo.TotalSize / 1048576;
                _diskInfo.FreeDiskSpaceMB = driveInfo.AvailableFreeSpace / 1048576;
                _diskInfo.VolumeRoot = root;

                return _diskInfo;
            }
        }

        private MemoryInfo GetMemoryInfo()
        {
            lock (_memoryLock)
            {
                if (_memoryInfo.Updated == DateTime.Now - TimeSpan.FromMilliseconds(METRICS_UPDATE_INTERVAL))
                {
                    return _memoryInfo;
                }

                if (PlatformUtil.RunningOnWindows)
                {
                    using var query = new ManagementObjectSearcher("SELECT FreePhysicalMemory, TotalVisibleMemorySize FROM CIM_OperatingSystem");

                    ManagementObject memoryInfo = query.Get().OfType<ManagementObject>().FirstOrDefault();

                    var freeMemory = Convert.ToInt64(memoryInfo["FreePhysicalMemory"]);
                    var totalMemory = Convert.ToInt64(memoryInfo["TotalVisibleMemorySize"]);

                    _memoryInfo.Updated = DateTime.Now;
                    _memoryInfo.TotalMemoryMB = totalMemory / 1024;
                    _memoryInfo.UsedMemoryMB = (totalMemory - freeMemory) / 1024;
                }

                if (PlatformUtil.RunningOnLinux)
                {
                    // Some compact Linux distributions like UBI may not have "free" utility installed, or it may have a custom output
                    // We don't want to break currently existing pipelines with ADO warnings
                    // so related errors thrown here will be sent to the trace or debug logs by caller methods

                    try
                    {
                        ProcessStartInfo processStartInfo = new ProcessStartInfo();

                        processStartInfo.FileName = "free";
                        processStartInfo.Arguments = "-m";
                        processStartInfo.RedirectStandardOutput = true;

                        var processStartInfoOutput = "";
                        using (var process = Process.Start(processStartInfo))
                        {
                            processStartInfoOutput = process.StandardOutput.ReadToEnd();
                        }

                        var processStartInfoOutputString = processStartInfoOutput.Split("\n");
                        var memoryInfoString = processStartInfoOutputString[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

                        if (memoryInfoString.Length != 7)
                        {
                            throw new Exception("\"free\" utility has non-default output");
                        }

                        _memoryInfo.Updated = DateTime.Now;
                        _memoryInfo.TotalMemoryMB = Int32.Parse(memoryInfoString[1]);
                        _memoryInfo.UsedMemoryMB = Int32.Parse(memoryInfoString[2]);
                    }
                    catch (Win32Exception ex)
                    {
                        throw new Exception($"\"free\" utility is unavailable. Exception: {ex.Message}");
                    }
                }

                if (PlatformUtil.RunningOnMacOS)
                {
                    // vm_stat allows to get the most detailed information about memory usage on MacOS
                    // but unfortunately it returns values in pages and has no built-in arguments for custom output
                    // so we need to parse and cast the output manually

                    ProcessStartInfo processStartInfo = new ProcessStartInfo();

                    processStartInfo.FileName = "vm_stat";
                    processStartInfo.RedirectStandardOutput = true;

                    var processStartInfoOutput = "";
                    using (var process = Process.Start(processStartInfo))
                    {
                        processStartInfoOutput = process.StandardOutput.ReadToEnd();
                    }

                    var processStartInfoOutputString = processStartInfoOutput.Split("\n");

                    var pageSize = Int32.Parse(processStartInfoOutputString[0].Split(" ", StringSplitOptions.RemoveEmptyEntries)[7]);

                    var pagesFree = Int64.Parse(processStartInfoOutputString[1].Split(" ", StringSplitOptions.RemoveEmptyEntries)[2].Trim('.'));
                    var pagesActive = Int64.Parse(processStartInfoOutputString[2].Split(" ", StringSplitOptions.RemoveEmptyEntries)[2].Trim('.'));
                    var pagesInactive = Int64.Parse(processStartInfoOutputString[3].Split(" ", StringSplitOptions.RemoveEmptyEntries)[2].Trim('.'));
                    var pagesSpeculative = Int64.Parse(processStartInfoOutputString[4].Split(" ", StringSplitOptions.RemoveEmptyEntries)[2].Trim('.'));
                    var pagesWiredDown = Int64.Parse(processStartInfoOutputString[6].Split(" ", StringSplitOptions.RemoveEmptyEntries)[3].Trim('.'));
                    var pagesOccupied = Int64.Parse(processStartInfoOutputString[16].Split(" ", StringSplitOptions.RemoveEmptyEntries)[4].Trim('.'));

                    var freeMemory = (pagesFree + pagesInactive) * pageSize;
                    var usedMemory = (pagesActive + pagesSpeculative + pagesWiredDown + pagesOccupied) * pageSize;

                    _memoryInfo.Updated = DateTime.Now;
                    _memoryInfo.TotalMemoryMB = (freeMemory + usedMemory) / 1048576;
                    _memoryInfo.UsedMemoryMB = usedMemory / 1048576;
                }

                return _memoryInfo;
            }
        }

        private string GetCpuInfoString()
        {
            try
            {
                GetCpuInfo();

                return StringUtil.Loc("ResourceMonitorCPUInfo", $"{_cpuInfo.Usage:0.00}");
            }
            catch (Exception ex)
            {
                return StringUtil.Loc("ResourceMonitorCPUInfoError", ex.Message);
            }
        }

        private string GetDiskInfoString()
        {
            try
            {
                GetDiskInfo();

                return StringUtil.Loc("ResourceMonitorDiskInfo", _diskInfo.VolumeRoot, $"{_diskInfo.FreeDiskSpaceMB:0.00}", $"{_diskInfo.TotalDiskSpaceMB:0.00}");

            }
            catch (Exception ex)
            {
                return StringUtil.Loc("ResourceMonitorDiskInfoError", ex.Message);
            }
        }

        private string GetMemoryInfoString()
        {
            try
            {
                GetMemoryInfo();

                return StringUtil.Loc("ResourceMonitorMemoryInfo", $"{_memoryInfo.UsedMemoryMB:0.00}", $"{_memoryInfo.TotalMemoryMB:0.00}");
            }
            catch (Exception ex)
            {
                return StringUtil.Loc("ResourceMonitorMemoryInfoError", ex.Message);
            }
        }

        public async Task RunDebugResourceMonitor()
        {
            while (!_context.CancellationToken.IsCancellationRequested)
            {
                _context.Debug(StringUtil.Loc("ResourceMonitorAgentEnvironmentResource", GetDiskInfoString(), GetMemoryInfoString(), GetCpuInfoString()));

                await Task.Delay(ACTIVE_MODE_INTERVAL, _context.CancellationToken);
            }
        }

        public async Task RunDiskSpaceUtilizationMonitor()
        {
            while (!_context.CancellationToken.IsCancellationRequested)
            {
                try
                {
                    GetDiskInfo();

                    var freeDiskSpacePercentage = Math.Round(((_diskInfo.FreeDiskSpaceMB / (double)_diskInfo.TotalDiskSpaceMB) * 100.0), 2);
                    var usedDiskSpacePercentage = 100.0 - freeDiskSpacePercentage;

                    if (freeDiskSpacePercentage <= AVAILABLE_DISK_SPACE_PERCENTAGE_THRESHOLD)
                    {
                        _context.Warning(StringUtil.Loc("ResourceMonitorFreeDiskSpaceIsLowerThanThreshold", _diskInfo.VolumeRoot, AVAILABLE_DISK_SPACE_PERCENTAGE_THRESHOLD, $"{usedDiskSpacePercentage:0.00}"));
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Trace.Warning($"Unable to get disk info. Exception: {ex.Message}");

                    break;
                }

                await Task.Delay(WARNING_MESSAGE_INTERVAL, _context.CancellationToken);
            }
        }

        public async Task RunMemoryUtilizationMonitor()
        {
            while (!_context.CancellationToken.IsCancellationRequested)
            {
                try
                {
                    GetMemoryInfo();

                    var usedMemoryPercentage = Math.Round(((_memoryInfo.UsedMemoryMB / (double)_memoryInfo.TotalMemoryMB) * 100.0), 2);

                    if (100.0 - usedMemoryPercentage <= AVAILABLE_MEMORY_PERCENTAGE_THRESHOLD)
                    {
                        _context.Warning(StringUtil.Loc("ResourceMonitorMemorySpaceIsLowerThanThreshold", AVAILABLE_MEMORY_PERCENTAGE_THRESHOLD, $"{usedMemoryPercentage:0.00}"));

                        break;
                    }
                }
                catch (Exception ex)
                {
                    Trace.Warning($"Unable to get memory info. Exception: {ex.Message}");

                    break;
                }

                await Task.Delay(WARNING_MESSAGE_INTERVAL, _context.CancellationToken);
            }
        }

        public async Task RunCpuUtilizationMonitor(string taskId)
        {
            while (!_context.CancellationToken.IsCancellationRequested)
            {
                try
                {
                    GetCpuInfo();

                    if (_cpuInfo.Usage >= CPU_UTILIZATION_PERCENTAGE_THRESHOLD)
                    {
                        string message = $"CPU utilization is higher than {CPU_UTILIZATION_PERCENTAGE_THRESHOLD}%; currently used: {_cpuInfo.Usage:0.00}%";

                        PublishTelemetry(message, taskId);

                        break;
                    }

                }
                catch (Exception ex)
                {
                    Trace.Warning($"Unable to get CPU info. Exception: {ex.Message}");

                    break;
                }

                await Task.Delay(WARNING_MESSAGE_INTERVAL, _context.CancellationToken);
            }
        }
    }
}
