namespace Microsoft.VisualStudio.Services.Agent.Worker.ResourceMetrics
{
    public interface IResourceDataCollector
    {
        string GetCurrentData(ITerminal terminal);
    }
}