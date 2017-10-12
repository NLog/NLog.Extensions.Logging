# NLog.Extensions.Logging

[![Build status Windows](https://ci.appveyor.com/api/projects/status/0nrg8cksp4b6tab1/branch/master?svg=true)](https://ci.appveyor.com/project/nlog/nlog-framework-logging/branch/master)
[![Build Status Linux](https://travis-ci.org/NLog/NLog.Extensions.Logging.svg?branch=master)](https://travis-ci.org/NLog/NLog.Extensions.Logging)


[![NuGet Pre Release](https://img.shields.io/nuget/vpre/NLog.Extensions.Logging.svg)](https://www.nuget.org/packages/NLog.Extensions.Logging)

[NLog](https://github.com/NLog/NLog) provider for [Microsoft.Extensions.Logging](https://github.com/aspnet/Logging); .NET Core. 
**ASP.NET Core** users should install  [NLog.Web.AspNetCore](https://www.nuget.org/packages/NLog.web.aspnetcore). 

Routes .NET Core log messages to NLog.


**Note**: Microsoft haven't ported all their classes to .NET standard, so not every target/layout renderer is available. 
Please check [platform support](https://github.com/NLog/NLog/wiki/platform-support)

ASP.NET Core
----

------
 ℹ️  Missing the trace en debug logs in .NET Core 2? Set `ILoggingBuilder.SetMinimumLevel()`

-----

:warning: Not all targets and layout renders are implemented for .NET Standard. See the [Platform support table](https://github.com/NLog/NLog/wiki/platform-support)

-----


**ASP.NET Core** users should install  [NLog.Web.AspNetCore](https://www.nuget.org/packages/NLog.web.aspnetcore)!
This was needed to support also the non-ASP.NET users.

NLog.Web.AspNetCore has a dependency on this library, so there is no need to directly install it anymore. 


####  Upgrading from alpha version?


Upgrading from the alphas? Some methods are moved to the [NLog.Web.AspNetCore package](https://www.nuget.org/packages/NLog.web.aspnetcore).

Since the beta there is a `ConfigureNLog` on `ILoggerFactory` and on `IHostingEnvironment`. The difference is how the base path (for relative paths) are determined. `IHostingEnvironment` uses `IHostingEnvironment.ContentRootPath` and `ILoggerFactory` uses `System.AppContext.BaseDirectory`.  ASP.NET Core users should use `IHostingEnvironment.ConfigureNLog`. NON-ASP.NET Core users (e.g. console applications), use `ILoggerFactory.ConfigureNLog`.

Due to the move of `IHostingEnvironment.ConfigureNLog` to NLog.Web.AspNetCore, the namespace of `IHostingEnvironment.ConfigureNLog` has been changed.

How to use
----

- [Getting Started with ASP.NET Core (project.json - vs2015)](https://github.com/NLog/NLog.Web/wiki/Getting-started-with-ASP.NET-Core-(project.json))
- [Getting started with ASP.NET Core (csproj - vs2017)](https://github.com/NLog/NLog.Web/wiki/Getting-started-with-ASP.NET-Core-(csproj---vs2017))
- [Multiple blogs to get started with ASP.NET Core and NLog](https://github.com/damienbod/AspNetCoreNlog)
    

Known issues
---
- Installing the NuGet packages [NLog.config](https://www.nuget.org/packages/NLog.Config/) / [NLog.schema](https://www.nuget.org/packages/NLog.Schema/) won't add to your project. It's recommend to extract (unzip) the NLog.Schema package and place the NLog.XSD in the same folder as NLog.config.
- auto load of NLog extensions won't work yet. Use `<extensions>` (see below)

.NET Core issues: 

- `${basedir}` isn't working will in .NET Core
- `LogManager.GetCurrentClassLogger()` will use the filename instead of the full class name (class name and namespace, like in NLog 4). This will be fixed in the final of NLog 5 (after the release of NETSTANDARD 2.0)







### How to run the example (aspnet-core-example)
How to run the [aspnet-core-examples](https://github.com/NLog/NLog.Extensions.Logging/tree/master/examples):

1. Install dotnet: http://dot.net 
2. From source: `dotnet run`
3. or, after publish: `dotnet aspnet-core-example.dll`
