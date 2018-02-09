![Black Duck Logo](https://cloud.githubusercontent.com/assets/7320197/24118398/06a04b52-0d84-11e7-81ce-9e79adb2532f.png)
# Hub Binary Scanner for .Net

## Overview ##
The Hub .Net Binary Scanner allows you to scan .Net executables and libraries in their production environments for known vulnerabilities. It does not require access to source code, project files, or other metadata. A full installation of Black Duck Hub is required to obtain the vulnerability report.

**Note**: This scanner examines .NET assembly references only. It does not examine package manifests or source code. For best visibility into your open-source risks, use [hub-detect](https://github.com/blackducksoftware/hub-detect) as part of your continuous integration/delivery process.

## Build ##
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0) [![Build status](https://ci.appveyor.com/api/projects/status/fhinl2akrxbchsqw?svg=true)](https://ci.appveyor.com/project/yevster/hub-dotnet-binary-scan) [![Black Duck Security Risk](https://copilot.blackducksoftware.com/github/repos/blackducksoftware/hub-dotnet-binary-scan/branches/master/badge-risk.svg)](https://copilot.blackducksoftware.com/github/repos/blackducksoftware/hub-dotnet-binary-scan/branches/master)

## Where can I get the latest release? ##
Releases published on the [Releases page](https://github.com/blackducksoftware/hub-dotnet-binary-scan/releases). Builds of the latest and, hopefully, greatest code, can be [grabbed from AppVeyor](https://ci.appveyor.com/project/yevster/hub-dotnet-binary-scan/build/artifacts). Alternatively, see the Wiki for [instructions for how to build it yourself](https://github.com/blackducksoftware/hub-dotnet-binary-scan/wiki/Building).

## Documentation ##
All documentation for the Hub .Net Binary Scanner can be found on the [project wiki](https://github.com/blackducksoftware/hub-dotnet-binary-scan/wiki/).
