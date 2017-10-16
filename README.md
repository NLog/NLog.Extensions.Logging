# NLog.Extensions.Logging

[![Build status Windows](https://ci.appveyor.com/api/projects/status/0nrg8cksp4b6tab1/branch/master?svg=true)](https://ci.appveyor.com/project/nlog/nlog-framework-logging/branch/master)
[![Build Status Linux](https://travis-ci.org/NLog/NLog.Extensions.Logging.svg?branch=master)](https://travis-ci.org/NLog/NLog.Extensions.Logging)


[![NuGet Pre Release](https://img.shields.io/nuget/vpre/NLog.Extensions.Logging.svg)](https://www.nuget.org/packages/NLog.Extensions.Logging)

[NLog](https://github.com/NLog/NLog) provider for [Microsoft.Extensions.Logging](https://github.com/aspnet/Logging); .NET Core. 
**ASP.NET Core** users should install  [NLog.Web.AspNetCore](https://www.nuget.org/packages/NLog.web.aspnetcore). 

Routes .NET Core log messages to NLog.


**Note**: Microsoft haven't ported all their classes to .NET standard, so not every target/layout renderer is available. 
Please check [platform support](https://github.com/NLog/NLog/wiki/platform-support)


## Getting started


- [Getting started with ASP.NET Core 2](https://github.com/NLog/NLog.Web/wiki/Getting-started-with-ASP.NET-Core-2)
- [Getting started with .NET Core 2 Console application](https://github.com/NLog/NLog.Extensions.Logging/wiki/Getting-started-with-.NET-Core-2---Console-application)
- [Getting started with ASP.NET Core 1 (csproj - vs2017)](https://github.com/NLog/NLog.Web/wiki/Getting-started-with-ASP.NET-Core-(csproj---vs2017))
- [Getting Started with ASP.NET Core 1 (project.json - vs2015)](https://github.com/NLog/NLog.Web/wiki/Getting-started-with-ASP.NET-Core-(project.json))
- [Multiple blogs to get started with ASP.NET Core and NLog](https://github.com/damienbod/AspNetCoreNlog)




Known issues
---
- Installing the NuGet packages [NLog.config](https://www.nuget.org/packages/NLog.Config/) / [NLog.schema](https://www.nuget.org/packages/NLog.Schema/) won't add to your project. It's recommend to extract (unzip) the NLog.Schema package and place the NLog.XSD in the same folder as NLog.config.
- auto load of NLog extensions won't work yet. Use `<extensions>` (see below)


### How to run the example (aspnet-core-example)
How to run the [aspnet-core-examples](https://github.com/NLog/NLog.Extensions.Logging/tree/master/examples):

1. Install dotnet: http://dot.net 
2. From source: `dotnet run`
3. or, after publish: `dotnet aspnet-core-example.dll`
