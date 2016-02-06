# NLog.Framework.Logging

[![Build status](https://ci.appveyor.com/api/projects/status/0nrg8cksp4b6tab1/branch/master?svg=true)](https://ci.appveyor.com/project/nlog/nlog-framework-logging/branch/master)
[![Build Status](https://travis-ci.org/NLog/NLog.Framework.Logging.svg?branch=master)](https://travis-ci.org/NLog/NLog.Framework.Logging)
[![NuGet Pre Release](https://img.shields.io/nuget/vpre/NLog.Framework.Logging.svg)](https://www.nuget.org/packages/NLog.Framework.Logging)

[NLog](https://github.com/NLog/NLog) provider for [Microsoft.Framework.Logging](https://github.com/aspnet/Logging); DNX and ASP.NET 5.


Routes ASP.NET 5 log messages to NLog.


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
- `${basedir}` refers to `C:\Users\...\.dnx\runtimes\dnx-clr-win-x86.1.0.0-rc1-update1\bin`
- auto load of NLog extensions won't work yet.
- This work only for "Microsoft.Extensions.Logging" RC1 as there are breaking changes in RC2 (not released yet)
- You need NLog 4.4 (alpha now) for CoreCLR


Please give feedback [here](https://github.com/NLog/NLog.Framework.Logging/issues/8)!


Example output
---

```
2016-02-05 00:36:59.6172 DEBUG Hosting starting
2016-02-05 00:36:59.7027 DEBUG Hosting started
2016-02-05 00:37:00.0777 INFO Request starting HTTP/1.1 DEBUG http://localhost:54159/ text/html 
2016-02-05 00:37:00.0777 INFO Request starting HTTP/1.1 GET http://localhost:54159/  
2016-02-05 00:37:00.2046 DEBUG The request path / does not match a supported file type
2016-02-05 00:37:00.2046 DEBUG DEBUG requests are not supported
2016-02-05 00:37:01.4286 DEBUG Request successfully matched the route with name 'default' and template '{controller=Home}/{action=Index}/{id?}'.
2016-02-05 00:37:01.4406 DEBUG Request successfully matched the route with name 'default' and template '{controller=Home}/{action=Index}/{id?}'.
2016-02-05 00:37:05.2140 DEBUG Executing action testApplication.Controllers.HomeController.Index
2016-02-05 00:37:05.2140 DEBUG Executing action testApplication.Controllers.HomeController.Index
2016-02-05 00:37:05.3980 INFO Executing action method testApplication.Controllers.HomeController.Index with arguments () - ModelState is Valid'
2016-02-05 00:37:05.3980 INFO Executing action method testApplication.Controllers.HomeController.Index with arguments () - ModelState is Valid'
2016-02-05 00:37:05.4090 DEBUG Executed action method testApplication.Controllers.HomeController.Index, returned result Microsoft.AspNet.Mvc.ViewResult.'
2016-02-05 00:37:05.4260 DEBUG Executed action method testApplication.Controllers.HomeController.Index, returned result Microsoft.AspNet.Mvc.ViewResult.'
2016-02-05 00:37:08.6830 DEBUG The view 'Index' was found.
2016-02-05 00:37:08.6830 DEBUG The view 'Index' was found.
2016-02-05 00:37:08.7090 INFO Executing ViewResult, running view at path /Views/Home/Index.cshtml.
2016-02-05 00:37:08.7280 INFO Executing ViewResult, running view at path /Views/Home/Index.cshtml.
2016-02-05 00:37:12.8167 INFO User profile is available. Using 'C:\Users\j.verdurmen\AppData\Local\ASP.NET\DataProtection-Keys' as key repository and Windows DPAPI to encrypt keys at rest.
2016-02-05 00:37:25.6597 INFO Executed action testApplication.Controllers.HomeController.Index in 2.0422ms
2016-02-05 00:37:25.6597 INFO Executed action testApplication.Controllers.HomeController.Index in 2.0391ms
2016-02-05 00:37:25.6917 INFO Request finished in 2,5563ms 200 text/html; charset=utf-8
2016-02-05 00:37:25.8107 INFO Request finished in 2,5703ms 200 text/html; charset=utf-8
```
