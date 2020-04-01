[![Build Status](https://dev.azure.com/rmja/Cloudspool/_apis/build/status/Build?branchName=master)](https://dev.azure.com/rmja/Cloudspool/_build/latest?definitionId=11&branchName=master)

Cloudspool provides convenient printing capabilities to pos printers


Build Instructions
==================
Run `.\build.ps1 -all` from the `k8s` folders, and deploy the resulting images to the local dev kubernetes cluster with `.\deploy.ps1 -all`.

Release Instructions
====================

Create a github release with the tag `vX.Y.Z` where the version matches the one produced by the [CI server](https://dev.azure.com/rmja/Cloudspool).
The name of the release must also contain the version number.
Do not include assets, they are added afterwards, triggered by the tag
