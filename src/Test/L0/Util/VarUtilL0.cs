// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.VisualStudio.Services.Agent.Util;
using Xunit;

namespace Microsoft.VisualStudio.Services.Agent.Tests.Util
{
    public class VarUtilL0
    {
        public const string VariableVulnerableToExecWarnLocKey = "VariableVulnerableToExecWarn";

        [Theory]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        public void ExpandValues_Prevents_InvalidValues_In_Target(string targetValue, string extectedValue)
        {
            using TestHostContext hc = new TestHostContext(this);
            var source = new Dictionary<string, string>();
            var target = new Dictionary<string, string>()
            {
                ["targetVar"] = targetValue
            };

            VarUtil.ExpandValues(hc, source, target);

            Assert.Equal(extectedValue, target["targetVar"]);
        }

        [Theory]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        [InlineData("testVar", "source = 'testVar'")]
        [InlineData(null, "source = ''")]
        [InlineData("", "source = ''")]
        [InlineData(" ", "source = ' '")]
        [InlineData("_", "source = '_'")]
        public void ExpandValues_Replaces_Variable_To_SourceValue_In_Target(string sourceValue, string expectedValue)
        {
            using TestHostContext hc = new TestHostContext(this);
            var source = new Dictionary<string, string>()
            {
                ["sourceVar"] = sourceValue,
            };
            var target = new Dictionary<string, string>()
            {
                ["targetVar"] = "source = '$(sourceVar)'",
            };

            VarUtil.ExpandValues(hc, source, target);

            Assert.Equal(expectedValue, target["targetVar"]);
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void ExpandValues_RecursiveExpanding_NotHappening()
        {
            using TestHostContext hc = new TestHostContext(this);
            var source = new Dictionary<string, string>
            {
                ["sourceVar1"] = "sourceValue1",
                ["sourceVar2"] = "sourceValue1 $(sourceVar1)",
            };
            var target = new Dictionary<string, string>
            {
                ["targetVar"] = "targetValue $(sourceVar2)",
            };

            VarUtil.ExpandValues(hc, source, target);

            Assert.Equal("targetValue sourceValue1 $(sourceVar1)", target["targetVar"]);
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void ExpandValues_ExpandNestedVariableTest()
        {
            using TestHostContext hc = new TestHostContext(this);
            var source = new Dictionary<string, string>
            {
                ["sourceVar"] = "sourceValue",
            };
            var target = new Dictionary<string, string>
            {
                ["targetVar"] = "targetValue $(sourceVar)",
            };

            VarUtil.ExpandValues(hc, source, target);

            Assert.Equal("targetValue sourceValue", target["targetVar"]);
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void ExpandValues_Keeping_Same_Value_If_No_Match_With_Target()
        {
            using TestHostContext hc = new TestHostContext(this);
            var source = new Dictionary<string, string>
            {
                ["sourceVar1"] = "source value"
            };
            var target = new Dictionary<string, string>
            {
                ["targetVar"] = "targetValue $(sourceVar2)",
            };

            VarUtil.ExpandValues(hc, source, target);

            Assert.Equal("targetValue $(sourceVar2)", target["targetVar"]);
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void ExpandValues_Get_Warnings_Per_ShellTask()
        {
            using TestHostContext hc = new TestHostContext(this);
            var source = new Dictionary<string, string>();
            var target = GetTargetValuesWithVulnerableVariables();

            VarUtil.ExpandValues(hc, source, target, WellKnownScriptShell.Bash);

            Assert.Equal(target["system.DefinitionName var"], target["system.DefinitionName var"]);
            Assert.Equal(target["build.SourceVersionMessage var"], target["build.SourceVersionMessage var"]);
        }

        // Not working because loc strings not present for now.
        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void ExpandValues_Warns_VulnerableVariables_Ignoring_LetterCase()
        {
            using TestHostContext hc = new TestHostContext(this);
            var source = new Dictionary<string, string>();
            var target = new Dictionary<string, string>()
            {
                ["targetVar"] = $"$(systeM.DeFiNiTioNname)",
            };
            var expectedWarning = StringUtil.Loc(VariableVulnerableToExecWarnLocKey, "system.definitionName", "$SYSTEM_DEFINITIONNAME");

            VarUtil.ExpandValues(hc, source, target, out var resultWarnings, WellKnownScriptShell.Bash);
            var resultWarning = resultWarnings[0];

            Assert.Equal(expectedWarning, resultWarning);
        }

        // Not working because loc strings not present for now.
        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void ExpandValues_Warns_About_Multiple_VulnerableVariables_In_Target()
        {
            using TestHostContext hc = new TestHostContext(this);
            var source = new Dictionary<string, string>();
            var target = new Dictionary<string, string>()
            {
                ["targetVar"] = "variable1 = $(system.stageDisplayName); variable2 = $(system.phaseDisplayName); variable3 = $(release.environmentName)",
            };

            VarUtil.ExpandValues(hc, source, target, out var resultWarnings, WellKnownScriptShell.Bash);
            var expectedWarning1 = StringUtil.Loc(VariableVulnerableToExecWarnLocKey, "system.stageDisplayName", "$SYSTEM_STAGEDISPLAYNAME");
            var expectedWarning2 = StringUtil.Loc(VariableVulnerableToExecWarnLocKey, "system.phaseDisplayName", "$SYSTEM_PHASEDISPLAYNAME");
            var expectedWarning3 = StringUtil.Loc(VariableVulnerableToExecWarnLocKey, "system.environmentName", "$SYSTEM_ENVIRONMENTNAME");

            Assert.Equal(expectedWarning1, resultWarnings[0]);
            Assert.Equal(expectedWarning2, resultWarnings[1]);
            Assert.Equal(expectedWarning3, resultWarnings[2]);
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void ExpandValues_Keeping_Same_If_No_Task_Specified()
        {
            using TestHostContext hc = new TestHostContext(this);
            var source = new Dictionary<string, string>();
            var target = GetTargetValuesWithVulnerableVariables();

            VarUtil.ExpandValues(hc, source, target);

            Assert.Equal(target["system.DefinitionName var"], target["system.DefinitionName var"]);
            Assert.Equal(target["build.DefinitionName var"], target["build.DefinitionName var"]);
            Assert.Equal(target["build.SourceVersionMessage var"], target["build.SourceVersionMessage var"]);
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void ExpandValues_No_Warn_If_Wrong_Shell_Specified()
        {
            using TestHostContext hc = new TestHostContext(this);
            var source = new Dictionary<string, string>();
            var target = new Dictionary<string, string>()
            {
                ["targetVar"] = $"test $(system.definitionName)",
            };
            var wrongShell = (WellKnownScriptShell)255;

            VarUtil.ExpandValues(hc, source, target, out var outputWarnings, wrongShell);

            Assert.Empty(outputWarnings);
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void ExpandValues_CommandShell_InputExpanding()
        {
            using TestHostContext hc = new TestHostContext(this);
            var source = new Dictionary<string, string>
            {
                ["build.sourceVersionMessage"] = "& | > <",
            };
            var target = new Dictionary<string, string>
            {
                ["targetVar"] = "echo $(build.sourceVersionMessage)",
            };

            VarUtil.ExpandValues(hc, source, target, WellKnownScriptShell.Cmd);

            Assert.Equal("echo ^& ^| ^> ^<", target["targetVar"]);
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void ExpandValues_CommandShell_Not_Expands_NonVulnerableVariables()
        {
            using TestHostContext hc = new TestHostContext(this);
            var source = new Dictionary<string, string>
            {
                ["sourceVar"] = "1 & echo 2 &&>|<"
            };
            var target = new Dictionary<string, string>
            {
                ["targetVar"] = "echo $(sourceVar)"
            };

            VarUtil.ExpandValues(hc, source, target, WellKnownScriptShell.Cmd);

            Assert.Equal("echo 1 & echo 2 &&>|<", target["targetVar"]);
        }

        [Theory]
        [InlineData("test.value1", "TEST_VALUE1")]
        [InlineData("test value2", "TEST_VALUE2")]
        [InlineData("tesT vaLue.3", "TEST_VALUE_3")]
        [InlineData(".tesT vaLue 4", "_TEST_VALUE_4")]
        [InlineData("TEST_VALUE_5", "TEST_VALUE_5")]
        [InlineData(".. TEST   VALUE. 6", "___TEST___VALUE__6")]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData(" ", "_")]
        [InlineData(".", "_")]
        public void TestConverterToEnvVariableFormat(string input, string expected)
        {
            var result = VarUtil.ConvertToEnvVariableFormat(input);

            Assert.Equal(expected, result);
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
