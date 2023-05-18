## Features
 - Upgrade vss api netcore version, has BuildXL changes (#4220)
 - Pass Process handler args through env variables (#4259)

## Bugs
 - Fix httpclient timeout on AzureMetada retrieving  (#4249)
 - Fix that plugins ignores proxy configuration (#4260)
 - Revert PR 4220 "Upgrade vss api netcore version, has BuildXL changes" (#4278)
 - Fix PH telemetry debug formatting (#4282)

## Misc
 - Add force of _tasks creation by knob (#4236)
 - update node from `16.17.1` to `16.20.0` (#4238)
 - Update link to new NodeTaskRunnerInstaller@0 article in task reference (#4264)
 - Update dotnet install scripts (#4265)
 - Bump default agent version to 3.999.999 (#4269)


## Agent Downloads

|             | Package | SHA-256 |
| ----------- | ------- | ------- |
| Windows x64 | [vsts-agent-win-x64-<AGENT_VERSION>.zip](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/vsts-agent-win-x64-<AGENT_VERSION>.zip) | <HASH> |
| Windows x86 | [vsts-agent-win-x86-<AGENT_VERSION>.zip](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/vsts-agent-win-x86-<AGENT_VERSION>.zip) | <HASH> |
| macOS x64   | [vsts-agent-osx-x64-<AGENT_VERSION>.tar.gz](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/vsts-agent-osx-x64-<AGENT_VERSION>.tar.gz) | <HASH> |
| macOS arm64 | [vsts-agent-osx-arm64-<AGENT_VERSION>.tar.gz](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/vsts-agent-osx-arm64-<AGENT_VERSION>.tar.gz) | <HASH> |
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
| macOS x64   | [pipelines-agent-osx-x64-<AGENT_VERSION>.tar.gz](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/pipelines-agent-osx-x64-<AGENT_VERSION>.tar.gz) | <HASH> |
| macOS arm64 | [pipelines-agent-osx-arm64-<AGENT_VERSION>.tar.gz](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/pipelines-agent-osx-x64-<AGENT_VERSION>.tar.gz) | <HASH> |
| Linux x64   | [pipelines-agent-linux-x64-<AGENT_VERSION>.tar.gz](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/pipelines-agent-linux-x64-<AGENT_VERSION>.tar.gz) | <HASH> |
| Linux ARM   | [pipelines-agent-linux-arm-<AGENT_VERSION>.tar.gz](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/pipelines-agent-linux-arm-<AGENT_VERSION>.tar.gz) | <HASH> |
| Linux ARM64 | [pipelines-agent-linux-arm64-<AGENT_VERSION>.tar.gz](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/pipelines-agent-linux-arm64-<AGENT_VERSION>.tar.gz) | <HASH> |
| RHEL 6 x64  | [pipelines-agent-rhel.6-x64-<AGENT_VERSION>.tar.gz](https://vstsagentpackage.azureedge.net/agent/<AGENT_VERSION>/pipelines-agent-rhel.6-x64-<AGENT_VERSION>.tar.gz) | <HASH> |
