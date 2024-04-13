// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.DistributedTask.WebApi;

namespace Microsoft.VisualStudio.Services.Agent.Worker;

public interface IJobExecutionContext : IExecutionContext
{
    public IExecutionContext CreateTaskExecutionContext(
        Guid recordId,
        string displayName,
        string refName,
        Variables taskVariables = null,
        bool outputForward = false,
        List<TaskRestrictions> taskRestrictions = null);
}

public sealed class JobExecutionContext : ExecutionContext, IJobExecutionContext
{
    public IExecutionContext CreateTaskExecutionContext(
        Guid recordId,
        string displayName,
        string refName,
        Variables taskVariables = null,
        bool outputForward = false,
        List<TaskRestrictions> taskRestrictions = null)
    {
        return CreateChild(recordId, displayName, refName, taskVariables, outputForward, taskRestrictions);
    }
}
