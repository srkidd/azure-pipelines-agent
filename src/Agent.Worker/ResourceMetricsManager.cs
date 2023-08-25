// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Services.Agent.Util;
using Microsoft.VisualStudio.Services.Agent.Worker;
using Microsoft.VisualStudio.Services.Agent.Worker.ResourceMetrics;

namespace Microsoft.VisualStudio.Services.Agent.Listener
{
    [ServiceLocator(Default = typeof(ResourceMetricsManager))]
    public interface IResourceMetricsManager : IAgentService
    {
        Task Run();
        void Setup(IExecutionContext context, ITerminal terminal);
    }

    public sealed class ResourceMetricsManager : AgentService, IResourceMetricsManager
    {
        const int ACTIVE_MODE_INTERVAL = 5000;
        IExecutionContext _context;
        List<IResourceDataCollector> _resourceDataCollectors = new List<IResourceDataCollector>();
        private ITerminal _terminal;

        public void Setup(IExecutionContext context, ITerminal terminal)
        {
            ArgUtil.NotNull(context, nameof(context));
            ArgUtil.NotNull(terminal, nameof(terminal));
            _context = context;

            _resourceDataCollectors.Add(new DiskInfoCollector());
            _resourceDataCollectors.Add(new MemoryInfoCollector());
            _resourceDataCollectors.Add(new CpuInfoCollector());

            _terminal = terminal;
        }
        public async Task Run()
        {
            while (!_context.CancellationToken.IsCancellationRequested)
            {
                _resourceDataCollectors.ForEach(f => _context.Debug(f.GetCurrentData(_terminal)));
                await Task.Delay(ACTIVE_MODE_INTERVAL, _context.CancellationToken);
            }
        }
    }
}
