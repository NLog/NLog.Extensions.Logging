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

[NLog.Extensions.Logging](https://www.nuget.org/packages/NLog.Extensions.Logging) makes it possible to use NLog together with [Microsoft ILogger](https://github.com/NLog/NLog.Extensions.Logging/wiki/NLog-GetCurrentClassLogger-and-Microsoft-ILogger)-abstraction and Dependency Injection.
It provides extension methods to register NLog as LoggingProvider for Microsoft Extension Logging using `AddNLog()` or `UseNLog()`.
> Note if using **ASP.NET Core** then instead install [NLog.Web.AspNetCore](https://www.nuget.org/packages/NLog.web.aspnetcore).

[NLog.Extensions.Logging](https://www.nuget.org/packages/NLog.Extensions.Logging) also makes it possible to load [NLog Configurations from appsettings.json](https://github.com/NLog/NLog.Extensions.Logging/wiki/NLog-configuration-with-appsettings.json)

Notice the standard [NLog NuGet package](https://www.nuget.org/packages/NLog) is enough for using NLog Logger with simple console application on the .NET Core platform.
Just add `NLog.config` file to the project, and follow the [tutorial](https://github.com/NLog/NLog/wiki/Tutorial#configure-nlog-targets-for-output) for using `GetCurrentClassLogger()`.

### Getting Started Tutorials:

- [Getting started for ASP.NET Core 6](https://github.com/NLog/NLog/wiki/Getting-started-with-ASP.NET-Core-6)
- [Getting started for ASP.NET Core 5](https://github.com/NLog/NLog/wiki/Getting-started-with-ASP.NET-Core-5)
- [Getting started for ASP.NET Core 3](https://github.com/NLog/NLog/wiki/Getting-started-with-ASP.NET-Core-3)
- [Getting started for ASP.NET Core 2](https://github.com/NLog/NLog/wiki/Getting-started-with-ASP.NET-Core-2)
- [Getting started for .NET Core 2 Console application](https://github.com/NLog/NLog/wiki/Getting-started-with-.NET-Core-2---Console-application)
- [How to use structured logging](https://github.com/NLog/NLog/wiki/How-to-use-structured-logging)
- [Blog posts for how to get started with ASP.NET Core and NLog](https://github.com/damienbod/AspNetCoreNlog)
