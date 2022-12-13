// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Agent.Sdk;
using System.Collections.Generic;
using System;

namespace Microsoft.VisualStudio.Services.Agent.Util
{
    public static class PipelineTasksUtil
    {
        public static readonly Dictionary<string, WellKnownScriptShell> ScriptShellsPerTasks = new Dictionary<string, WellKnownScriptShell>(StringComparer.OrdinalIgnoreCase)
        {
            ["PowerShell"] = WellKnownScriptShell.PowerShell,
            ["Bash"] = WellKnownScriptShell.Bash
        };

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
                var isTaskForShell = ScriptShellsPerTasks.TryGetValue(taskName, out var shellName);
                if (isTaskForShell)
                {
                    return shellName;
                }
            }

            return WellKnownScriptShell.Unknown;
        }
    }
}
