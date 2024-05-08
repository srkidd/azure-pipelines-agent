using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

namespace Agent.Sdk.Util;

public class PsModulePathUtil
{
    [SupportedOSPlatform("windows")]
    public static string GetPsModulePathWithoutPowershellCoreLocations(string currentPsModulePath)
    {
        string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        string programFilesPath86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

        string wellKnownPsHomeModuleLocation = Path.Combine(programFilesPath, "PowerShell", "Modules");
        string wellKnownPsHomeModuleLocation86 = Path.Combine(programFilesPath86, "PowerShell", "Modules");

        string wellKnownProgramFilesModuleLocation = Path.Combine(programFilesPath.ToLower(), "powershell", "7", "Modules");
        string wellKnownProgramFilesModuleLocation86 = Path.Combine(programFilesPath86.ToLower(), "powershell", "7", "Modules");

        string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string wellKnownDocumentsModuleLocation = Path.Combine("Documents", "PowerShell", "Modules");

        char delimiter = ';';
        IEnumerable<string> splitModules = currentPsModulePath.Split(delimiter).Where(path =>
        {
            return path != wellKnownPsHomeModuleLocation
                && path != wellKnownPsHomeModuleLocation86
                && path != wellKnownProgramFilesModuleLocation
                && path != wellKnownProgramFilesModuleLocation86
                && !path.Contains(wellKnownDocumentsModuleLocation);
        });

        string newPsModulePath = string.Join(delimiter, splitModules);

        return newPsModulePath;
    }
}
