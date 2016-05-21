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
  using NLog.Extensions.Logging;

  public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
  {
      //add NLog to ASP.NET Core
      loggerFactory.AddNLog();

      //configure nlog.config in your project root
      env.ConfigureNLog("nlog.config");
      
      ...
```  
  
Known issues
---
- You need to configure where the nlog.config is located (see example above)
- Installing the NuGet packages [NLog.config](https://www.nuget.org/packages/NLog.Config/) / [NLog.schema](https://www.nuget.org/packages/NLog.Schema/) won't add to your project. 
- auto load of NLog extensions won't work yet.


Example
---

### NLog.config
In root folder, not wwwroot

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true"
      internalLogLevel="Warn"
      internalLogFile="c:\temp\internal-nlog.txt">

  <!-- define various log targets -->
  <targets>
    <!-- write logs to file -->
    <target xsi:type="File" name="allfile" fileName="c:\temp\nlog-all-${shortdate}.log"
                 layout="${longdate}|${logger}|${uppercase:${level}}|${message} ${exception}" />

   
    <target xsi:type="File" name="ownFile-web" fileName="c:\temp\nlog-own-${shortdate}.log"
             layout="${longdate}|${logger}|${uppercase:${level}}|  ${message} ${exception}" />

    <target xsi:type="Null" name="blackhole" />
  </targets>

  <rules>
    <!--All logs, including from Microsoft-->
    <logger name="*" minlevel="Trace" writeTo="allfile" />

    <!--Skip Microsoft logs and so log only own logs-->
    <logger name="Microsoft.*" minlevel="Trace" writeTo="blackhole" final="true" />
    <logger name="*" minlevel="Trace" writeTo="ownFile-web" />
  </rules>
</nlog>
```

### Example Output

#### nlog-all-2016-05-21.log
```
2016-05-21 13:39:53.6748|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:5000/   
2016-05-21 13:39:53.6858|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|The request path / does not match a supported file type 
2016-05-21 13:39:53.6858|Microsoft.AspNetCore.Routing.RouteBase|DEBUG|Request successfully matched the route with name 'default' and template '{controller=Home}/{action=Index}/{id?}'. 
2016-05-21 13:39:53.6998|Microsoft.AspNetCore.Mvc.Internal.MvcRouteHandler|DEBUG|Executing action aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) 
2016-05-21 13:39:53.6998|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|INFO|Executing action method aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) with arguments () - ModelState is Valid' 
2016-05-21 13:39:53.7158|HomeController|INFO|Index page says hello 
2016-05-21 13:39:53.7318|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|DEBUG|Executed action method aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example), returned result Microsoft.AspNetCore.Mvc.ViewResult.' 
2016-05-21 13:39:53.7538|Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine|DEBUG|View lookup cache hit for view 'Index' in controller 'Home'. 
2016-05-21 13:39:53.7688|Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor|DEBUG|The view 'Index' was found. 
2016-05-21 13:39:53.7788|Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor|INFO|Executing ViewResult, running view at path /Views/Home/Index.cshtml. 
2016-05-21 13:39:53.7788|Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine|DEBUG|View lookup cache hit for view '_Layout' in controller 'Home'. 
2016-05-21 13:39:53.7988|Microsoft.AspNetCore.Mvc.Internal.MvcRouteHandler|INFO|Executed action aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) in 90.0466ms 
2016-05-21 13:39:53.8322|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 156.7643ms 200 text/html; charset=utf-8 
2016-05-21 13:39:53.8672|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKS1DIB3985K" completed keep alive response. 
2016-05-21 13:39:53.8957|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:5000/css/site.min.css?v=_lrVHoJvcjMZQ929rrkg08m5F7_X2ibCOz8cW80kjN0   
2016-05-21 13:39:53.8957|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:5000/js/site.min.js?v=47DEQpj8HBSa-_TImW-5JCeuQeRkm5NMpJWZG3hSuFU   
2016-05-21 13:39:53.8957|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:5000/images/banner1.svg   
2016-05-21 13:39:53.9322|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|The file /css/site.min.css was not modified 
2016-05-21 13:39:53.9577|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|The file /js/site.min.js was not modified 
2016-05-21 13:39:53.9837|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|The file /images/banner1.svg was not modified 
2016-05-21 13:39:54.0022|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|Handled. Status code: 304 File: /css/site.min.css 
2016-05-21 13:39:54.0147|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 118.6529ms 304 text/css 
2016-05-21 13:39:54.0147|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|Handled. Status code: 304 File: /js/site.min.js 
2016-05-21 13:39:54.0147|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|Handled. Status code: 304 File: /images/banner1.svg 
2016-05-21 13:39:54.0367|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 140.226ms 304 application/javascript 
2016-05-21 13:39:54.0447|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 149.1571ms 304 image/svg+xml 
2016-05-21 13:39:54.0447|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKS1DIB3985K" completed keep alive response. 
2016-05-21 13:39:54.1077|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKS1DIB3985L" completed keep alive response. 
2016-05-21 13:39:54.1397|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:5000/images/banner2.svg   
2016-05-21 13:39:54.1232|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:5000/images/banner3.svg   
2016-05-21 13:39:54.1232|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKS1DIB3985M" completed keep alive response. 
2016-05-21 13:39:54.1832|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|The file /images/banner2.svg was not modified 
2016-05-21 13:39:54.2412|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:5000/images/banner4.svg   
2016-05-21 13:39:54.2457|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|The file /images/banner3.svg was not modified 
2016-05-21 13:39:54.2932|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|Handled. Status code: 304 File: /images/banner2.svg 
2016-05-21 13:39:54.2932|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|The file /images/banner4.svg was not modified 
2016-05-21 13:39:54.3157|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|Handled. Status code: 304 File: /images/banner3.svg 
2016-05-21 13:39:54.3242|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 182.8243ms 304 image/svg+xml 
2016-05-21 13:39:54.3607|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 236.4672ms 304 image/svg+xml 
2016-05-21 13:39:54.3607|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|Handled. Status code: 304 File: /images/banner4.svg 
2016-05-21 13:39:54.3877|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKS1DIB3985L" completed keep alive response. 
2016-05-21 13:39:54.4087|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 167.6258ms 304 image/svg+xml 
2016-05-21 13:39:54.4087|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKS1DIB3985K" completed keep alive response. 
2016-05-21 13:39:54.4252|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKS1DIB3985N" started. 
2016-05-21 13:39:54.4402|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKS1DIB3985M" completed keep alive response. 
2016-05-21 13:39:54.4627|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:5000/favicon.ico   
2016-05-21 13:39:54.4757|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|Sending file. Request path: '/favicon.ico'. Physical path: 'X:\_github\nlog\NLog.Framework.logging\examples\aspnet-core-example\src\aspnet-core-example\wwwroot\favicon.ico' 
2016-05-21 13:39:54.5093|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 46.6747ms 200 image/x-icon 
2016-05-21 13:39:54.5536|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKS1DIB3985M" completed keep alive response. 
2016-05-21 13:40:14.3892|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKS1DIB3985N" received FIN. 
2016-05-21 13:40:14.3892|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKS1DIB3985N" disconnecting. 
2016-05-21 13:40:14.3892|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKS1DIB3985N" sending FIN. 
2016-05-21 13:40:14.4072|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKS1DIB3985N" sent FIN with status "0". 
2016-05-21 13:40:14.4072|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKS1DIB3985N" stopped. 
```

#### nlog-own-2016-05-21.log

```
2016-05-21 13:39:53.7158|HomeController|INFO|  Index page says hello 

```


