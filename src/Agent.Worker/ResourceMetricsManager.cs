// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Services.Agent.Util;
using Microsoft.VisualStudio.Services.Agent.Worker;

namespace Microsoft.VisualStudio.Services.Agent.Listener
{
    [ServiceLocator(Default = typeof(ResourceMetricsManager))]
    public interface IResourceMetricsManager : IAgentService, IDisposable
    {
        Task Run();
        void Setup(IExecutionContext context);
    }

    public sealed class ResourceMetricsManager : AgentService, IResourceMetricsManager
    {
        const int ACTIVE_MODE_INTERVAL = 5000;
        IExecutionContext _context;
        private ITerminal _terminal;

        public void Setup(IExecutionContext context)
        {
            ArgUtil.NotNull(context, nameof(context));
            _context = context;

            try
            {
                _currentProcess = Process.GetCurrentProcess();
            }
            catch (Exception ex)
            {
                _context.Warning($"Unable to get current process, ex:{ex.Message}");
            }
        }
        public async Task Run()
        {
            while (!_context.CancellationToken.IsCancellationRequested)
            {
                _context.Debug($"Agent running environment resource - {GetDiskInfo()}, {GetMemoryInfo(_terminal)}, {GetCpuInfo()}");
                await Task.Delay(ACTIVE_MODE_INTERVAL, _context.CancellationToken);
            }
        }
        public string GetDiskInfo()
        {
            try
            {
                string root = Path.GetPathRoot(System.Reflection.Assembly.GetEntryAssembly().Location);

                var s = new DriveInfo(root);
                return $"Disk: {root}, label:{s.VolumeLabel}, available:{s.AvailableFreeSpace / c_kb}KB out of {s.TotalSize / c_kb}KB";

            }
            catch (Exception ex)
            {
                return $"Unable to get Disk info, ex:{ex.Message}";
            }
        }

        private const int c_kb = 1024;

        private Process _currentProcess;

        public string GetCpuInfo()
        {
            if (_currentProcess == null)
                return $"Unable to get CPU info";
            try
            {
                TimeSpan totalCpuTime = _currentProcess.TotalProcessorTime;
                TimeSpan elapsedTime = DateTime.Now - _currentProcess.StartTime;
                double cpuUsage = (totalCpuTime.TotalMilliseconds / elapsedTime.TotalMilliseconds) * 100.0;

                return $"CPU: {cpuUsage:0.00}";
            }
            catch (Exception ex)
            {
                return $"Unable to get CPU info, ex:{ex.Message}";
            }
        }

        public string GetMemoryInfo(ITerminal terminal)
        {
            try
            {
                var gcMemoryInfo = GC.GetGCMemoryInfo();
                var installedMemory = (int)(gcMemoryInfo.TotalAvailableMemoryBytes / 1048576.0);
                var usedMemory = (int)(gcMemoryInfo.HeapSizeBytes / 1048576.0);

                return $"Memory: {usedMemory}MB out of {installedMemory}MB";
            }
            catch (Exception ex)
            {
                return $"Unable to get Memory info, ex:{ex.Message}";
            }
        }

        public void Dispose()
        {
            _currentProcess?.Dispose();
        }
    }
}
