# Self-service installation of outdated Node runner

Due to the end of the Node 6 cycle of life, we planning to remove the outdated Node version from the agent shipment.
However, since the customers may rely on the tasks that use Node 6, we provide self-service methods to install the designated Node runner. 

## Install Node runner manually

To manually install the required Node runner on your agent, please, use the following steps:

1. Download the latest binaries of required Node version from https://releases.nodejs.org/ for your operating system.
1. Extract downloaded binaries into the `node` folder under the `agent/externals` directory.

Windows:
```powershell
    Invoke-WebRequest -Uri "https://nodejs.org/dist/v${version}/win-${osArch}/node.exe" -OutFile "${agent_folder}/externals/node/node.exe"
    Invoke-WebRequest -Uri "https://nodejs.org/dist/v${version}/win-${osArch}/node.lib" -OutFile "${agent_folder}/externals/node/node.lib"
```

Linux/Unix:
```bash
    wget -O "/tmp/node-v${version}-${osInfo.osPlatform}-${targetOsArch}.tar.gz" "https://nodejs.org/dist/v${version}/node-v${version}-${osInfo.osPlatform}-${targetOsArch}.tar.gz"
    tar -xvf "/tmp/node-v${version}-${osInfo.osPlatform}-${targetOsArch}.tar.gz" -C "${agent_folder}/externals/node/"
```

The list of supported OS:
- Windows-x64
- Windows-x86
- Linux-x64
- Linux-ARM
- Linux-ARM64
- Darwin-x64

## Install Node runner via NodeTaskRunnerInstaller

You can install the required runner version using the Azure DevOps task [NodeTaskRunnerInstaller](https://github.com/microsoft/azure-pipelines-tasks/tree/master/Tasks/NodeTaskRunnerInstallerV0).
Please, use the following pipeline template to install the latest version of Node 6 runner:

```yaml
  steps:
  - task: NodeTaskRunnerInstaller@0
    inputs:
      runnerVersion: 6
```

Please, check the details in [documentation](TODO)
