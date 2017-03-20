![Black Duck Logo](https://cloud.githubusercontent.com/assets/7320197/24118398/06a04b52-0d84-11e7-81ce-9e79adb2532f.png)
# Hub Binary Scanner for .Net
## Overview
The Hub .Net Binary Scanner allows you to scan .Net executables and libraries in their production environments for known vulnerabilities. It does not require access to source code, project files, or other metadata. A full installation of [Black Duck Hub](https://www.blackducksoftware.com/products/hub) is required to obtain the vulnerability report.

## How it works
Unlike binary files on many platforms, .NET assemblies (.exe and .dll) files can be examined to determine what other assemblies they reference. Those in turn can be examined to see what assemblies they reference, all the way down to the class libraries of .Net itself. Which is important, because the .Net runtime itself could be open-source, and it will be even more important in the future, as Microsoft and the .Net foundation work to create a single, standardized, open-source library across all the runtimes. Thus, every .Net application will have open-source dependencies that can have vulnerabilities, and those dependencies will not be in the metadata scanned by the current Nuget/msbuild integration.
 
Additionally, when .Net code invokes native libraries, these libraries can be installed into a shared location on the target system without appearing in source code metadata. This scanner finds calls to such native libraries (known as "platform invocations" or "P-Invokes") and includes those libraries into its scan result.

![DotNetReferenceTreeDiagram](https://cloud.githubusercontent.com/assets/7320197/24118422/19b15d3a-0d84-11e7-8722-763218959b64.jpg)

## Supported Platforms
We have tested the scanner on Windows on .Net 4.5.2 and on Mono version 4.8 on OSX.

## Building
The executables can be built with Visual Studio 2017 on Windows and Mac and with `msbuild` from .Net SDK 4.6.2 or later.

## Running
### Scan to Hub
To scan directly to a hub instance, you'll need to create a file named scanner.ini in the same directory as scanner.exe with the following content:
```
url=http://myhubserver
username=myusername
password=mypassword
```

Then, just run 
scanner.exe "C:\Program Files\MyApp\App.exe"

The use of a configuration file allows for easy deploymen of the scanner to various machines where .Net applications may be running without having to configure the hub location on each one.

__Caution:__ Storing the password in cleartext, as above, is an obvious security risk, and something [we plan to remedy](https://github.com/blackducksoftware/hub-dotnet-binary-scan/issues/2). Until then, use a sufficiently low-priveleged Hub acount for this feature or follow the "Scan to File" instructions below to use the scanner without storing a Hub password. 

### Scan to File
If the machine containing the application to be scanned cannot reach the hub due to network configuration or other issues, the scanner can produce a standard scanner JSON file that can be uploaded to the Hub via the UI. To produce a scanner file, run
```
scanner.exe "C:\Program Files\MyApp\App.exe" scanResult.json
```


