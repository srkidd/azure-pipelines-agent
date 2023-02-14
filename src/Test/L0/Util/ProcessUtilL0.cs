// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Agent.Sdk;
using Microsoft.VisualStudio.Services.Agent.Util;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace Microsoft.VisualStudio.Services.Agent.Tests.Util
{
    public sealed class WindowsProcessUtilL0
    {
        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void Test_GetProcessList()
        {
            if (PlatformUtil.RunningOnWindows)
            {
                using TestHostContext hc = new TestHostContext(this);
                Tracing trace = hc.GetTrace();

                // This test is based on the current process.
                Process currentProcess = Process.GetCurrentProcess();

                // The first three processes in the list.
                // We do not take other processes since they may differ.
                string[] expectedProcessNames = { currentProcess.ProcessName, "dotnet", "dotnet" };
                int count = expectedProcessNames.Length;

                // Act.
                List<Process> processList = WindowsProcessUtil.GetProcessList(currentProcess);
                processList.RemoveRange(count, processList.Count - count);
                string[] actualProcessNames = processList.ConvertAll<string>(process => process.ProcessName).ToArray();

                // Assert.
                Assert.Equal(expectedProcessNames, actualProcessNames);
            }
            else
            {
                Assert.True(true, "Passively pass this test since it is designed only for Windows.");
            }
        }
    }
}
