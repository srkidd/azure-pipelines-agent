using Agent.Worker.Handlers.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Test.L0.Worker.Handlers
{
    public sealed class ProcessHandlerHelperL0
    {
        [Fact]
        public void EmptyLineTest()
        {
            string argsLine = "";
            string expectedArgs = "";

            var (actualArgs, _) = ProcessHandlerHelper.ProcessInputArguments(argsLine, new());

            Assert.Equal(expectedArgs, actualArgs);
        }

        [Fact]
        public void BasicTest()
        {
            string argsLine = "%VAR1% 2";
            string expectedArgs = "value1 2";
            var testEnv = new Dictionary<string, string>()
            {
                { "VAR1", "value1"}
            };

            var (actualArgs, _) = ProcessHandlerHelper.ProcessInputArguments(argsLine, testEnv);

            Assert.Equal(expectedArgs, actualArgs);
        }

        [Fact]
        public void TestWithMultipleVars()
        {
            string argsLine = "1 %VAR1% %VAR2%";
            string expectedArgs = "1 value1 value2";
            var testEnv = new Dictionary<string, string>()
            {
                { "VAR1", "value1" },
                { "VAR2", "value2" }
            };

            var (actualArgs, _) = ProcessHandlerHelper.ProcessInputArguments(argsLine, testEnv);

            Assert.Equal(expectedArgs, actualArgs);
        }

        [Theory]
        [InlineData("%VAR1% %VAR2%%VAR3%", "1 23")]
        [InlineData("%VAR1% %VAR2%_%VAR3%", "1 2_3")]
        [InlineData("%VAR1%%VAR2%%VAR3%", "123")]
        public void TestWithCloseVars(string inputArgs, string expectedArgs)
        {
            var testEnv = new Dictionary<string, string>()
            {
                { "VAR1", "1" },
                { "VAR2", "2" },
                { "VAR3", "3" }
            };

            var (actualArgs, _) = ProcessHandlerHelper.ProcessInputArguments(inputArgs, testEnv);

            Assert.Equal(expectedArgs, actualArgs);
        }

        [Fact]
        public void NestedVariablesNotExpands()
        {
            string argsLine = "%VAR1% %VAR2%";
            string expectedArgs = "%NESTED% 2";
            var testEnv = new Dictionary<string, string>()
            {
                { "VAR1", "%NESTED%" },
                { "VAR2", "2"},
                { "NESTED", "nested" }
            };

            var (actualArgs, _) = ProcessHandlerHelper.ProcessInputArguments(argsLine, testEnv);

            Assert.Equal(expectedArgs, actualArgs);
        }
    }
}
