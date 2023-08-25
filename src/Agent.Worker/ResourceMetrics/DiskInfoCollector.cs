using System.IO;
using System.Text;

namespace Microsoft.VisualStudio.Services.Agent.Worker.ResourceMetrics
{
    class DiskInfoCollector : IResourceDataCollector
    {
        public string GetCurrentData(ITerminal terminal)
        {
            StringBuilder sb = new();

            string root = Path.GetPathRoot(System.Reflection.Assembly.GetEntryAssembly().Location);
            sb.Append($"Agent running on Drive {root}; ");

            if (!string.IsNullOrEmpty(root))
            {
                var s = new DriveInfo(root); sb.Append($" - type:{s.DriveType}, label:{s.VolumeLabel}, total:{s.TotalSize / c_kb}KB, available:{s.AvailableFreeSpace / c_kb}KB");
            }

            return sb.ToString();
        }

        private const int c_kb = 1024;
    }
}