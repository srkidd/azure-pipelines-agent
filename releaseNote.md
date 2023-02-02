## Features
 - Allow env.sh to accept additional env var names (#3339)
 - Fix SonarQube's "'Any()' should be used to test for emptiness" / Code… (#3404)
 - Make agent update list of systems supporting .NET 6 periodically (#4081)
 - Add script to predict v2->v3 upgrade compatibility (#4092)
 - DockerLogin retries (#4100)
 - Added strict boolean knob values (#4103)
 - Add the exact operation system version and environment to the telemetry (#4104)
 - Add option to skip rhel6 in pipeline and ci (#4121)

## Bugs
 - Fixed name of DockerAdditionalNetworkOptions Knob (#3888)
 - Revert "Fix execution of scripts in variables (#4016)" (#4088)
 - fix dotnet host search (#4124)

## Misc
 - Retries to lock Services database (#4085)
 - Format agent source code (#4101)
 - Fix agent lint errors (#4117)
 - Mingit version upgraded to 2.39.1 (#4126)
 - Revert "Allow env.sh to accept additional env var names (#3339)" (#4143)


## Agent Downloads

|             | Package | SHA-256 |
| ----------- | ------- | ------- |
| Windows x64 | [vsts-agent-win-x64-<AGENT_VERSION>.zip](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/vsts-agent-win-x64-<AGENT_VERSION>.zip) | <HASH> |
| Windows x86 | [vsts-agent-win-x86-<AGENT_VERSION>.zip](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/vsts-agent-win-x86-<AGENT_VERSION>.zip) | <HASH> |
| macOS       | [vsts-agent-osx-x64-<AGENT_VERSION>.tar.gz](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/vsts-agent-osx-x64-<AGENT_VERSION>.tar.gz) | <HASH> |
| Linux x64   | [vsts-agent-linux-x64-<AGENT_VERSION>.tar.gz](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/vsts-agent-linux-x64-<AGENT_VERSION>.tar.gz) | <HASH> |
| Linux ARM   | [vsts-agent-linux-arm-<AGENT_VERSION>.tar.gz](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/vsts-agent-linux-arm-<AGENT_VERSION>.tar.gz) | <HASH> |
| Linux ARM64 | [vsts-agent-linux-arm64-<AGENT_VERSION>.tar.gz](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/vsts-agent-linux-arm64-<AGENT_VERSION>.tar.gz) | <HASH> |
| RHEL 6 x64  | [vsts-agent-rhel.6-x64-<AGENT_VERSION>.tar.gz](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/vsts-agent-rhel.6-x64-<AGENT_VERSION>.tar.gz) | <HASH> |

After Download:

## Windows x64

``` bash
C:\> mkdir myagent && cd myagent
C:\myagent> Add-Type -AssemblyName System.IO.Compression.FileSystem ; [System.IO.Compression.ZipFile]::ExtractToDirectory("$HOME\Downloads\vsts-agent-win-x64-<AGENT_VERSION>.zip", "$PWD")
```

## Windows x86

``` bash
C:\> mkdir myagent && cd myagent
C:\myagent> Add-Type -AssemblyName System.IO.Compression.FileSystem ; [System.IO.Compression.ZipFile]::ExtractToDirectory("$HOME\Downloads\vsts-agent-win-x86-<AGENT_VERSION>.zip", "$PWD")
```

## macOS

``` bash
~/$ mkdir myagent && cd myagent
~/myagent$ tar xzf ~/Downloads/vsts-agent-osx-x64-<AGENT_VERSION>.tar.gz
```

## Linux x64

``` bash
~/$ mkdir myagent && cd myagent
~/myagent$ tar xzf ~/Downloads/vsts-agent-linux-x64-<AGENT_VERSION>.tar.gz
```

## Linux ARM

``` bash
~/$ mkdir myagent && cd myagent
~/myagent$ tar xzf ~/Downloads/vsts-agent-linux-arm-<AGENT_VERSION>.tar.gz
```

## Linux ARM64

``` bash
~/$ mkdir myagent && cd myagent
~/myagent$ tar xzf ~/Downloads/vsts-agent-linux-arm64-<AGENT_VERSION>.tar.gz
```

## RHEL 6 x64

``` bash
~/$ mkdir myagent && cd myagent
~/myagent$ tar xzf ~/Downloads/vsts-agent-rhel.6-x64-<AGENT_VERSION>.tar.gz
```

## Alternate Agent Downloads

This following alternate packages do not include Node 6 and are only suitable for users who do not use Node 6 dependent tasks. 
See [notes](docs/node6.md) on Node version support for more details.

|             | Package | SHA-256 |
| ----------- | ------- | ------- |
| Windows x64 | [pipelines-agent-win-x64-<AGENT_VERSION>.zip](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/pipelines-agent-win-x64-<AGENT_VERSION>.zip) | <HASH> |
| Windows x86 | [pipelines-agent-win-x86-<AGENT_VERSION>.zip](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/pipelines-agent-win-x86-<AGENT_VERSION>.zip) | <HASH> |
| macOS       | [pipelines-agent-osx-x64-<AGENT_VERSION>.tar.gz](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/pipelines-agent-osx-x64-<AGENT_VERSION>.tar.gz) | <HASH> |
| Linux x64   | [pipelines-agent-linux-x64-<AGENT_VERSION>.tar.gz](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/pipelines-agent-linux-x64-<AGENT_VERSION>.tar.gz) | <HASH> |
| Linux ARM   | [pipelines-agent-linux-arm-<AGENT_VERSION>.tar.gz](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/pipelines-agent-linux-arm-<AGENT_VERSION>.tar.gz) | <HASH> |
| Linux ARM64 | [pipelines-agent-linux-arm64-<AGENT_VERSION>.tar.gz](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/pipelines-agent-linux-arm64-<AGENT_VERSION>.tar.gz) | <HASH> |
| RHEL 6 x64  | [pipelines-agent-rhel.6-x64-<AGENT_VERSION>.tar.gz](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/pipelines-agent-rhel.6-x64-<AGENT_VERSION>.tar.gz) | <HASH> |
