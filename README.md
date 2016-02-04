# NLog.Framework.logging

[![Build status](https://ci.appveyor.com/api/projects/status/0nrg8cksp4b6tab1/branch/master?svg=true)](https://ci.appveyor.com/project/nlog/nlog-framework-logging/branch/master)

NLog provider for [Microsoft.Framework.Logging](https://github.com/aspnet/Logging); DNX and ASP.NET 5.


Routes ASP.NET 5 log messages to NLog.


How to use
----

1. Create nlog.config in root of your project file.
2.  in startup.cs add

```c#
  public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
  {
      //add NLog to aspnet5
      loggerFactory.AddNLog();

      //configure nlog.config in your project root
      env.ConfigureNLog("nlog.config");
  }
```  
  
Known issues
---
- you need to configure to load nlog.config (see example)
- Installing the NuGet packages [NLog.config](https://www.nuget.org/packages/NLog.Config/) / [NLog.schema](https://www.nuget.org/packages/NLog.Schema/) won't add to your project. 
- `${basedir}` refers to `C:\Users\...\.dnx\runtimes\dnx-clr-win-x86.1.0.0-rc1-update1\bin`
- auto load of NLog extensions won't work yet.
- This work only for "Microsoft.Extensions.Logging" RC1 as there are breaking changes in RC2 (not released yet)


Please give feedback!
