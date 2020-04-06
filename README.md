[![Build Status](https://dev.azure.com/rmja/Cloudspool/_apis/build/status/Build?branchName=master)](https://dev.azure.com/rmja/Cloudspool/_build/latest?definitionId=11&branchName=master)

Cloudspool provides convenient printing capabilities to pos printers


Build Instructions
==================
Run `.\build.ps1 -All` from the `k8s` folders, and deploy the resulting images to the local dev kubernetes cluster with `.\deploy.ps1 -All`.

Deploy Instructions
===================
The deploy script can be run with with multiple switches, for example:
```
.\deploy.ps1 -All -Environment Production -Configs C:\MyConfigs\Production -Context MyClusterContext
```
The production configs are [here](https://github.com/rmja/Cloudspool) (private repo).

Release Instructions
====================

Create a github release with the tag `vX.Y.Z` where the version matches the one produced by the [CI server](https://dev.azure.com/rmja/Cloudspool).
The name of the release must also contain the version number.
Do not include assets, they are added afterwards, triggered by the tag