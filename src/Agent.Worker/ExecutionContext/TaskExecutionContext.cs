// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.VisualStudio.Services.Agent.Worker;

public interface ITaskExecutionContext : IExecutionContext
{
    public IJobExecutionContext JobContext { get; }
}

public sealed class TaskExecutionContext : ExecutionContext, ITaskExecutionContext
{
    public IJobExecutionContext JobContext { get; private set; }
}
