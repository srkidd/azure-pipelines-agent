// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.VisualStudio.Services.Agent.Worker;

namespace Microsoft.VisualStudio.Services.Agent.Tests.Worker
{
    internal class FakeAgentPluginManagerL0 : AgentPluginManager
    {
        public override void Initialize(IHostContext hostContext)
        {
            // do nothing
        }
    }
}
