using Microsoft.VisualStudio.Services.Agent.Worker;
using Microsoft.VisualStudio.Services.Agent.Worker.Build;
using Moq;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Microsoft.VisualStudio.Services.Agent.Tests.Worker.Build;

public class TestGitCommandManagerL0
{
    public static IEnumerable<object[]> UseNewGitVersionFeatureFlagsData => new List<object[]>
    {
        new object[] { true },
        new object[] { false },
    };
    
    [Theory]
    [Trait("Level", "L0")]
    [Trait("Category", "Plugin")]
    [MemberData(nameof(UseNewGitVersionFeatureFlagsData))]
    public void TestGetInternalGitPaths(bool gitFeatureFlagStatus)
    {
        using var tc = new TestHostContext(this, $"GitFeatureFlagStatus_{gitFeatureFlagStatus}");
        var trace = tc.GetTrace();
        var executionContext = new Mock<IExecutionContext>();

        GitCommandManager gitCliManager = new();
        gitCliManager.Initialize(tc);
        var paths = gitCliManager.GetInternalGitPaths(executionContext.Object, gitFeatureFlagStatus);

        string gitPath;

        if (gitFeatureFlagStatus)
        {
            gitPath = Path.Combine(tc.GetDirectory(WellKnownDirectory.Externals), "externals", "ff_git", "cmd", "git.exe");
        }
        else
        {
            gitPath = Path.Combine(tc.GetDirectory(WellKnownDirectory.Externals), "externals", "git", "cmd", "git.exe");
        }

        Assert.Equal(paths.Item1, gitPath);
    }
}
