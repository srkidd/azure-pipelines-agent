using Microsoft.VisualStudio.Services.Agent.Util;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.VisualStudio.Services.Agent.Tests.Util
{
    public class VarUtilL0
    {
        public static TheoryData<string, string[]> InputsForVariablesReplacementTest => new TheoryData<string, string[]>()
        {
            { "Bash", new string[]{ "$SYSTEM_DEFINITIONNAME", "$BUILD_DEFINITIONNAME", "$BUILD_SOURCEVERSIONMESSAGE" } },
            { "PowerShell", new string[]{ "$env:SYSTEM_DEFINITIONNAME", "$env:BUILD_DEFINITIONNAME", "$env:BUILD_SOURCEVERSIONMESSAGE" }},
            { "CmdLine", new string[]{ "%SYSTEM_DEFINITIONNAME%", "%BUILD_DEFINITIONNAME%", "%BUILD_SOURCEVERSIONMESSAGE%" }},
        };

        [Theory]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        [MemberData(nameof(InputsForVariablesReplacementTest))]
        public void ReplacingToEnvVariablesForTask(string taskName, string[] expectedVariables)
        {
            using TestHostContext hc = new TestHostContext(this);

            var source = new Dictionary<string, string>();

            var target = GetTargetValuesWithVulnerableVariables();

            VarUtil.ExpandValues(hc, source, target, taskName);

            Assert.Equal($"test {expectedVariables[0]}", target["system.DefinitionName var"]);
            Assert.Equal($"test {expectedVariables[1]}", target["build.DefinitionName var"]);
            Assert.Equal($"test {expectedVariables[2]}", target["build.SourceVersionMessage var"]);
        }

        [Fact]
        public void KeepSameIfNoTaskSpecified()
        {
            using TestHostContext hc = new TestHostContext(this);

            var source = new Dictionary<string, string>();

            var target = GetTargetValuesWithVulnerableVariables();

            VarUtil.ExpandValues(hc, source, target);

            Assert.Equal(target["system.DefinitionName var"], target["system.DefinitionName var"]);
            Assert.Equal(target["build.DefinitionName var"], target["build.DefinitionName var"]);
            Assert.Equal(target["build.SourceVersionMessage var"], target["build.SourceVersionMessage var"]);
        }

        private Dictionary<string, string> GetTargetValuesWithVulnerableVariables()
        {
            return new Dictionary<string, string>()
            {
                ["system.DefinitionName var"] = $"test $({Constants.Variables.System.DefinitionName})",
                ["build.DefinitionName var"] = $"test $({Constants.Variables.Build.DefinitionName})",
                ["build.SourceVersionMessage var"] = $"test $({Constants.Variables.Build.SourceVersionMessage})",
            };
        }
    }
}
