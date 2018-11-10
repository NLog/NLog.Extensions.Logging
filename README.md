# NLog.Extensions.Logging



[![NuGet Pre Release](https://img.shields.io/nuget/vpre/NLog.Extensions.Logging.svg)](https://www.nuget.org/packages/NLog.Extensions.Logging)
[![Build status Windows](https://ci.appveyor.com/api/projects/status/0nrg8cksp4b6tab1/branch/master?svg=true)](https://ci.appveyor.com/project/nlog/nlog-framework-logging/branch/master)
[![Build Status Linux](https://travis-ci.org/NLog/NLog.Extensions.Logging.svg?branch=master)](https://travis-ci.org/NLog/NLog.Extensions.Logging)

[![BCH compliance](https://bettercodehub.com/edge/badge/NLog/NLog.Extensions.Logging?branch=components)](https://bettercodehub.com/results/NLog/NLog.Extensions.Logging)
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog.extensions.logging&metric=ncloc)](https://sonarcloud.io/dashboard/?id=nlog.extensions.logging) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog.extensions.logging&metric=bugs)](https://sonarcloud.io/dashboard/?id=nlog.extensions.logging) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog.extensions.logging&metric=vulnerabilities)](https://sonarcloud.io/dashboard/?id=nlog.extensions.logging) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog.extensions.logging&metric=code_smells)](https://sonarcloud.io/project/issues?id=nlog.extensions.logging&resolved=false&types=CODE_SMELL) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog.extensions.logging&metric=duplicated_lines_density)](https://sonarcloud.io/component_measures/domain/Duplications?id=nlog.extensions.logging) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog.extensions.logging&metric=sqale_debt_ratio)](https://sonarcloud.io/dashboard/?id=nlog.extensions.logging) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog.extensions.logging&metric=coverage)](https://sonarcloud.io/component_measures?id=nlog.extensions.logging&metric=coverage) 

[NLog](https://github.com/NLog/NLog) provider for [Microsoft.Extensions.Logging](https://github.com/aspnet/Logging); .NET Core. 
**ASP.NET Core** users should install  [NLog.Web.AspNetCore](https://www.nuget.org/packages/NLog.web.aspnetcore). 


**Note**: Microsoft haven't ported all their classes to .NET standard, so not every target/layout renderer is available. 
Please check [platform support](https://github.com/NLog/NLog/wiki/platform-support)

**Note**: You don't actually have to use Dependency Injection to use NLog in a .NET Core Console application, as described below in "Getting started with .NET Core 2 Console application".  If you don't want to use DI, you can just add the [NLog NuGet package](https://www.nuget.org/packages/NLog) to your project, manually add a `NLog.config` file, and follow the [tutorial](https://github.com/NLog/NLog/wiki/Tutorial#configure-nlog-targets-for-output) to `GetCurrentClassLogger()` and use that for logging.


## Getting started


- [Getting started with ASP.NET Core 2](https://github.com/NLog/NLog.Web/wiki/Getting-started-with-ASP.NET-Core-2)
- [Getting started with .NET Core 2 Console application](https://github.com/NLog/NLog.Extensions.Logging/wiki/Getting-started-with-.NET-Core-2---Console-application)
- [Getting started with ASP.NET Core 1 (csproj - vs2017)](https://github.com/NLog/NLog.Web/wiki/Getting-started-with-ASP.NET-Core-(csproj---vs2017))
- [Getting Started with ASP.NET Core 1 (project.json - vs2015)](https://github.com/NLog/NLog.Web/wiki/Getting-started-with-ASP.NET-Core-(project.json))
- [Multiple blogs to get started with ASP.NET Core and NLog](https://github.com/damienbod/AspNetCoreNlog)




Known issues
---
- auto load of NLog extensions won't work yet. Use `<extensions>` (see [docs](https://github.com/NLog/NLog/wiki/Configuration-file#extensions))
