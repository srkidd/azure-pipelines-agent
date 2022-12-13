// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Agent.Sdk;

namespace Microsoft.VisualStudio.Services.Agent.Util
{
    public static class PipelineTasksUtil
    {
        public static WellKnownScriptShell GetShellByTaskName(string taskName)
        {
            if (taskName == "CmdLine")
            {
                if (PlatformUtil.RunningOnWindows)
                {
                    return WellKnownScriptShell.Cmd;
                }
                else
                {
                    return WellKnownScriptShell.Bash;
                }
            }
            else
            {
                var isTaskForShell = Constants.Variables.ScriptShellsPerTasks.TryGetValue(taskName, out var shellName);
                if (isTaskForShell)
                {
                    return shellName;
                }
            }

            return WellKnownScriptShell.Unknown;
        }
    }
}
