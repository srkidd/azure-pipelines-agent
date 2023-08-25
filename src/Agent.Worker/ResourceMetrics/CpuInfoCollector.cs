using System;
using System.Diagnostics;
using System.Text;

namespace Microsoft.VisualStudio.Services.Agent.Worker.ResourceMetrics
{
    class CpuInfoCollector : IResourceDataCollector
    {
        private Process _currentProcess;

        public CpuInfoCollector()
        {
            _currentProcess = Process.GetCurrentProcess();
        }
        public string GetCurrentData(ITerminal terminal)
        {
            StringBuilder sb = new();

            TimeSpan totalCpuTime = _currentProcess.TotalProcessorTime;
            TimeSpan elapsedTime = DateTime.Now - _currentProcess.StartTime;
            double cpuUsage = (totalCpuTime.TotalMilliseconds / elapsedTime.TotalMilliseconds) * 100.0;
            sb.Append($"Agent uses {cpuUsage}% CPU");

            return sb.ToString();
        }
    }
}