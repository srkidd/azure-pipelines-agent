using System;
using System.Text;

namespace Microsoft.VisualStudio.Services.Agent.Worker.ResourceMetrics
{
    class MemoryInfoCollector : IResourceDataCollector
    {
        public string GetCurrentData(ITerminal terminal)
        {
            StringBuilder sb = new();

            var gcMemoryInfo = GC.GetGCMemoryInfo();
            var installedMemory = (int)(gcMemoryInfo.TotalAvailableMemoryBytes / 1048576.0);
            var usedMemory = (int)(gcMemoryInfo.HeapSizeBytes / 1048576.0);

            sb.Append($"Agent uses {usedMemory}MB out of {installedMemory}MB");

            return sb.ToString();
        }
    }
}