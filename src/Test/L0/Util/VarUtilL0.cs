using Microsoft.VisualStudio.Services.Agent.Util;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.VisualStudio.Services.Agent.Tests.Util
{
    public class VarUtilL0
    {
        [Theory]
        [InlineData("test.value1", "TEST_VALUE1")]
        [InlineData("test value2", "TEST_VALUE2")]
        [InlineData("tesT vaLue.3", "TEST_VALUE_3")]
        [InlineData(".tesT vaLue 4", "_TEST_VALUE_4")]
        [InlineData("TEST_VALUE_5", "TEST_VALUE_5")]
        [InlineData(".. TEST   VALUE. 6", "___TEST___VALUE__6")]
        public void TestConverterToEnvVariableFormat(string input, string expected)
        {
            var result = VarUtil.ConvertToEnvVariableFormat(input);

            Assert.Equal(expected, result);
        }
    }
}
