# NLog.Extensions.Logging

[![Build status](https://ci.appveyor.com/api/projects/status/0nrg8cksp4b6tab1/branch/master?svg=true)](https://ci.appveyor.com/project/nlog/nlog-framework-logging/branch/master)
<!--[![Build Status](https://travis-ci.org/NLog/NLog.Extensions.Logging.svg?branch=master)](https://travis-ci.org/NLog/NLog.Extensions.Logging)-->
[![NuGet Pre Release](https://img.shields.io/nuget/vpre/NLog.Extensions.Logging.svg)](https://www.nuget.org/packages/NLog.Extensions.Logging)

[NLog](https://github.com/NLog/NLog) provider for [Microsoft.Extensions.Logging](https://github.com/aspnet/Logging); .NET Core and ASP.NET Core. 


Routes .NET Core log messages to NLog.


How to use
----

1. Create nlog.config in root of your project file.
2.  in startup.cs add in `Configure`

```c#
  public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
  {
      //add NLog to aspnet5
      loggerFactory.AddNLog();

      //configure nlog.config in your project root
      env.ConfigureNLog("nlog.config");
      
      ...
```  
  
Known issues
---
- You need to configure where the nlog.config is located (see example above)
- Installing the NuGet packages [NLog.config](https://www.nuget.org/packages/NLog.Config/) / [NLog.schema](https://www.nuget.org/packages/NLog.Schema/) won't add to your project. 
- unknown if `${basedir}` works
- auto load of NLog extensions won't work yet.


Example
---

config:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true"
      internalLogLevel="Warn"
      internalLogFile="c:\temp\internal.txt">


  <!-- define various log targets -->
  <targets>
    <!-- write logs to file -->
    <target xsi:type="File" name="allfile" fileName="c:\temp\nlog-all-${shortdate}.log"
                 layout="${longdate}|${logger}|${uppercase:${level}}|${message} ${exception}" />

    <target xsi:type="File" name="ownFile" fileName="c:\temp\nlog-own-${shortdate}.log"
              layout="${longdate}|${logger}|${uppercase:${level}}|${message} ${exception}" />

    <target xsi:type="Null" name="blackhole" />
  </targets>

  <rules>
    <!--All logs, including from Microsoft-->
    <logger name="*" minlevel="Trace" writeTo="allfile" />

    <!--Skip Microsoft logs and so log only own logs-->
    <logger name="Microsoft.*" minlevel="Trace" writeTo="blackhole" final="true" />
    <logger name="*" minlevel="Trace" writeTo="ownFile" />
  </rules>
</nlog>
```




