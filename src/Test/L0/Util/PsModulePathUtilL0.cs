using Microsoft.VisualStudio.Services.Agent.Tests;
using Microsoft.VisualStudio.Services.Agent;
using Xunit;
using Agent.Sdk.Util;
using System;
using System.Reflection;
using System.Diagnostics;
using System.IO;

namespace Test.L0.Util
{
    public sealed class PsModulePathUtilL0
    {
        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        [Trait("SkipOn", "darwin")]
        [Trait("SkipOn", "linux")]
        public void RemovesPowershellCoreLocations_AndPreservesCustomPaths()
        {
            using TestHostContext hc = new TestHostContext(this);
            using Tracing trace = hc.GetTrace();

            // Arrange
            string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string programFilesPath86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            string wellKnownPsHomeModuleLocation = Path.Combine(programFilesPath, "PowerShell", "Modules");
            string wellKnownProgramFilesModuleLocation = Path.Combine(programFilesPath.ToLower(), "powershell", "7", "Modules");

            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string wellKnownDocumentsModuleLocation = Path.Combine(userFolderPath, "OneDrive", "Documents", "PowerShell", "Modules");

            string customPath = "C:/Custom/Module/Path";

            string currentPsModulePath = string.Join(';', 
                wellKnownPsHomeModuleLocation, 
                wellKnownProgramFilesModuleLocation, 
                wellKnownDocumentsModuleLocation, 
                customPath);

            // Act
            string newPsModulePath = PsModulePathUtil.GetPsModulePathWithoutPowershellCoreLocations(currentPsModulePath);

            // Assert
            Assert.Equal(customPath, newPsModulePath);
        }
    }
}
