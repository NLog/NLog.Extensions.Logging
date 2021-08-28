![NLog](https://nlog-project.org/images/NLog.png)

# NLog.Extensions.Logging & NLog.Extensions.Hosting


[![NuGet Release](https://img.shields.io/nuget/v/NLog.Extensions.Logging.svg?label=NLog.Extensions.Logging)](https://www.nuget.org/packages/NLog.Extensions.Logging)
[![NuGet Pre Release](https://img.shields.io/nuget/vpre/NLog.Extensions.Logging.svg?label=NLog.Extensions.Logging)](https://www.nuget.org/packages/NLog.Extensions.Logging)

[![NuGet Release](https://img.shields.io/nuget/v/NLog.Extensions.Hosting.svg?label=NLog.Extensions.Hosting)](https://www.nuget.org/packages/NLog.Extensions.Hosting)
[![NuGet Pre Release](https://img.shields.io/nuget/vpre/NLog.Extensions.Hosting.svg?label=NLog.Extensions.Hosting)](https://www.nuget.org/packages/NLog.Extensions.Hosting)

[![Build status](https://ci.appveyor.com/api/projects/status/0nrg8cksp4b6tab1/branch/master?svg=true)](https://ci.appveyor.com/project/nlog/nlog-framework-logging/branch/master)
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog.extensions.logging&branch=master&metric=ncloc)](https://sonarcloud.io/dashboard/?id=nlog.extensions.logging&branch=master) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog.extensions.logging&branch=master&metric=bugs)](https://sonarcloud.io/dashboard/?id=nlog.extensions.logging&branch=master) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog.extensions.logging&branch=master&metric=vulnerabilities)](https://sonarcloud.io/dashboard/?id=nlog.extensions.logging&branch=master) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog.extensions.logging&branch=master&metric=code_smells)](https://sonarcloud.io/project/issues?id=nlog.extensions.logging&branch=master&resolved=false&types=CODE_SMELL) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog.extensions.logging&branch=master&metric=duplicated_lines_density)](https://sonarcloud.io/component_measures/domain/Duplications?id=nlog.extensions.logging&branch=master) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog.extensions.logging&branch=master&metric=sqale_debt_ratio)](https://sonarcloud.io/dashboard/?id=nlog.extensions.logging&branch=master) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog.extensions.logging&branch=master&metric=coverage)](https://sonarcloud.io/component_measures?id=nlog.extensions.logging&branch=master&metric=coverage) 

## NLog.Extensions.Logging

[NLog](https://github.com/NLog/NLog) provider for [Microsoft.Extensions.Logging](https://github.com/aspnet/Logging); .NET Core. 
**ASP.NET Core** users should install  [NLog.Web.AspNetCore](https://www.nuget.org/packages/NLog.web.aspnetcore). 

## Getting started

**Note**: Microsoft haven't ported all their classes to .NET standard, so not every target/layout renderer is available. 
Please check [platform support](https://github.com/NLog/NLog/wiki/platform-support)

### ASP.NET Core

- [Getting started for ASP.NET Core 5](https://github.com/NLog/NLog/wiki/Getting-started-with-ASP.NET-Core-5)
- [Getting started for ASP.NET Core 3](https://github.com/NLog/NLog/wiki/Getting-started-with-ASP.NET-Core-3)
- [Getting started for ASP.NET Core 2](https://github.com/NLog/NLog/wiki/Getting-started-with-ASP.NET-Core-2)
- [Getting started for .NET Core 2 Console application](https://github.com/NLog/NLog/wiki/Getting-started-with-.NET-Core-2---Console-application)
- [Getting started for ASP.NET Core 1 (csproj - vs2017)](https://github.com/NLog/NLog.Web/wiki/Getting-started-with-ASP.NET-Core-(csproj---vs2017))
- [How to use structured logging](https://github.com/NLog/NLog/wiki/How-to-use-structured-logging)
- [Multiple blogs to get started with ASP.NET Core and NLog](https://github.com/damienbod/AspNetCoreNlog)

### .NET Core Console application

You can choose whether or not to use Dependency Injection when using NLog in a .NET Core Console application. If you don't want to use DI, you can just add the [NLog NuGet package](https://www.nuget.org/packages/NLog) to your project, manually add an `NLog.config` file, follow this [tutorial](https://github.com/NLog/NLog/wiki/Tutorial#configure-nlog-targets-for-output) to `GetCurrentClassLogger()`, and use that for logging.

To use DI, you can use NLog in conjunction with `Microsoft.Extensions.Logging` by following the 'getting started' tutorial below.

- [Getting started with .NET Core 2 Console application](https://github.com/NLog/NLog.Extensions.Logging/wiki/Getting-started-with-.NET-Core-2---Console-application)


Known issues
---
- auto load of NLog extensions won't work yet. Use `<extensions>` (see [docs](https://github.com/NLog/NLog/wiki/Configuration-file#extensions))
- note: auto loading of extensions is removed in NLog 5. See [NLog 5.0 Preview-1 ready for release testing](https://nlog-project.org/2021/08/25/nlog-5-0-preview1-ready.html)


### How to run the examples
How to run the [dotnet-core-examples](https://github.com/NLog/NLog.Extensions.Logging/tree/master/examples):

1. Install dotnet: http://dot.net 
2. From source: `dotnet run`
