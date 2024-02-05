using Microsoft.VisualStudio.Services.Agent.Worker;
using Microsoft.VisualStudio.Services.Agent.Worker.Build;
using Moq;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Microsoft.VisualStudio.Services.Agent.Tests.Worker.Build;

public class TestGitCommandManagerL0
{
    private readonly string gitPath = Path.Combine("agenthomedirectory", "externals", "git", "cmd", "git.exe");
    private readonly string ffGitPath = Path.Combine("agenthomedirectory", "externals", "ff_git", "cmd", "git.exe");

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
        using var hostContext = new TestHostContext(this, $"GitFeatureFlagStatus_{gitFeatureFlagStatus}");
        var executionContext = new Mock<IExecutionContext>();

        GitCommandManager gitCliManager = new();
        var paths = gitCliManager.GetInternalGitPaths(executionContext.Object, gitFeatureFlagStatus);

        if (gitFeatureFlagStatus)
        {
            Assert.Equal(paths.Item1, ffGitPath);
        }
        else
        {
            Assert.Equal(paths.Item1, gitPath);
        }
    }
}
