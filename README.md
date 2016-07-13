# NLog.Extensions.Logging

[![Build status Windows](https://ci.appveyor.com/api/projects/status/0nrg8cksp4b6tab1/branch/master?svg=true)](https://ci.appveyor.com/project/nlog/nlog-framework-logging/branch/master)
[![Build Status Linux / Mac OS](https://travis-ci.org/NLog/NLog.Extensions.Logging.svg?branch=master)](https://travis-ci.org/NLog/NLog.Extensions.Logging)
[![NuGet Pre Release](https://img.shields.io/nuget/vpre/NLog.Extensions.Logging.svg)](https://www.nuget.org/packages/NLog.Extensions.Logging)

[NLog](https://github.com/NLog/NLog) provider for [Microsoft.Extensions.Logging](https://github.com/aspnet/Logging); .NET Core and ASP.NET Core. 


Routes .NET Core log messages to NLog.


How to use
----
1. Add dependency in project.json
    ```xml
     "dependencies": {
        "NLog.Extensions.Logging": "1.0.0-*"
      }
    ```

2. Create nlog.config in root of your project file, see [NLog.config example](https://raw.githubusercontent.com/NLog/NLog.Extensions.Logging/03971763546cc70660529bbe28b282304adb7571/examples/aspnet-core-example/src/aspnet-core-example/nlog.config)
3.  in startup.cs add in `Configure`

    ```c#
      using NLog.Extensions.Logging;
    
      public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
      {
          //add NLog to ASP.NET Core
          loggerFactory.AddNLog();
    
    
          //needed for non-NETSTANDARD platforms: configure nlog.config in your project root
          env.ConfigureNLog("nlog.config");
          ...
    ```  

4. Include NLog.config for publishing in project.json:

    ```json
       "publishOptions": {
            "include": [
                "wwwroot",
                "Views",
                "appsettings.json",
                "web.config",
                "nlog.config"
            ]
        },
    ```
    
Notes:

- NLog.Config is found automatically in RC2. 
  
Known issues
---
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
                 layout="${longdate}|${event-properties:item=EventId.Id}|${logger}|${uppercase:${level}}|${message} ${exception}" />

   
    <target xsi:type="File" name="ownFile-web" fileName="c:\temp\nlog-own-${shortdate}.log"
             layout="${longdate}|${event-properties:item=EventId.Id}|${logger}|${uppercase:${level}}|  ${message} ${exception}" />

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
### Log statments

In HomeController.cs

```c#
    public class HomeController : Controller
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        public IActionResult Index()
        {
            Logger.Info("Index page says hello");
            return View();
        }
        
        ...
        
````


### Example Output

#### nlog-all-2016-05-21.log

```
2016-06-16 08:34:51.3565|3|Microsoft.AspNetCore.Hosting.Internal.WebHost|DEBUG|Hosting starting 
2016-06-16 08:34:51.4785|4|Microsoft.AspNetCore.Hosting.Internal.WebHost|DEBUG|Hosting started 
2016-06-16 08:34:51.5425|1|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KG6" started. 
2016-06-16 08:34:51.5425|1|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KG5" started. 
2016-06-16 08:34:51.6395|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/   
2016-06-16 08:34:51.6395|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 DEBUG http://localhost:59915/  0 
2016-06-16 08:34:51.7545|4|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|The request path / does not match a supported file type 
2016-06-16 08:34:51.7545|1|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|DEBUG requests are not supported 
2016-06-16 08:34:52.0105|1|Microsoft.AspNetCore.Routing.RouteBase|DEBUG|Request successfully matched the route with name 'default' and template '{controller=Home}/{action=Index}/{id?}'. 
2016-06-16 08:34:52.0105|1|Microsoft.AspNetCore.Routing.RouteBase|DEBUG|Request successfully matched the route with name 'default' and template '{controller=Home}/{action=Index}/{id?}'. 
2016-06-16 08:34:52.0675|1|Microsoft.AspNetCore.Mvc.Internal.MvcRouteHandler|DEBUG|Executing action aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) 
2016-06-16 08:34:52.0675|1|Microsoft.AspNetCore.Mvc.Internal.MvcRouteHandler|DEBUG|Executing action aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) 
2016-06-16 08:34:52.1985|1|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|INFO|Executing action method aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) with arguments () - ModelState is Valid' 
2016-06-16 08:34:52.1985|1|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|INFO|Executing action method aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) with arguments () - ModelState is Valid' 
2016-06-16 08:34:52.1985||HomeController|INFO|Index page says hello 
2016-06-16 08:34:52.1985||HomeController|INFO|Index page says hello 
2016-06-16 08:34:52.2195|2|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|DEBUG|Executed action method aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example), returned result Microsoft.AspNetCore.Mvc.ViewResult.' 
2016-06-16 08:34:52.2195|2|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|DEBUG|Executed action method aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example), returned result Microsoft.AspNetCore.Mvc.ViewResult.' 
2016-06-16 08:34:52.3705|1|Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine|DEBUG|View lookup cache miss for view 'Index' in controller 'Home'. 
2016-06-16 08:34:52.3705|1|Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine|DEBUG|View lookup cache miss for view 'Index' in controller 'Home'. 
2016-06-16 08:34:52.3945|1|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/Home/Index.cshtml' started. 
2016-06-16 08:34:52.8245|2|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/Home/Index.cshtml' completed in 422.8517ms. 
2016-06-16 08:34:52.8426|1|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/Home/Index.cshtml' started. 
2016-06-16 08:34:54.5946|2|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/Home/Index.cshtml' completed in 1747.6902ms. 
2016-06-16 08:34:54.6035|1|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/_ViewStart.cshtml' started. 
2016-06-16 08:34:54.6035|2|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/_ViewStart.cshtml' completed in 3.9646ms. 
2016-06-16 08:34:54.6185|1|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/_ViewStart.cshtml' started. 
2016-06-16 08:34:54.6686|2|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/_ViewStart.cshtml' completed in 45.653ms. 
2016-06-16 08:34:54.6806|2|Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor|DEBUG|The view 'Index' was found. 
2016-06-16 08:34:54.6806|2|Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor|DEBUG|The view 'Index' was found. 
2016-06-16 08:34:54.6806|1|Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor|INFO|Executing ViewResult, running view at path /Views/Home/Index.cshtml. 
2016-06-16 08:34:54.6806|1|Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor|INFO|Executing ViewResult, running view at path /Views/Home/Index.cshtml. 
2016-06-16 08:34:54.8665|1|Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine|DEBUG|View lookup cache miss for view '_Layout' in controller 'Home'. 
2016-06-16 08:34:54.8665|1|Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine|DEBUG|View lookup cache miss for view '_Layout' in controller 'Home'. 
2016-06-16 08:34:54.8725|1|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/Shared/_Layout.cshtml' started. 
2016-06-16 08:34:54.9276|2|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/Shared/_Layout.cshtml' completed in 45.8784ms. 
2016-06-16 08:34:54.9326|1|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/Shared/_Layout.cshtml' started. 
2016-06-16 08:34:55.0385|2|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/Shared/_Layout.cshtml' completed in 101.793ms. 
2016-06-16 08:34:55.1975|2|Microsoft.AspNetCore.Mvc.Internal.MvcRouteHandler|INFO|Executed action aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) in 3126.2448ms 
2016-06-16 08:34:55.2245|6|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KG6" received FIN. 
2016-06-16 08:34:55.1975|2|Microsoft.AspNetCore.Mvc.Internal.MvcRouteHandler|INFO|Executed action aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) in 3123.3629ms 
2016-06-16 08:34:55.1975|1|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KG7" started. 
2016-06-16 08:34:55.2616|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 3640.8441ms 200 text/html; charset=utf-8 
2016-06-16 08:34:55.2395|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 3616.0454ms 200 text/html; charset=utf-8 
2016-06-16 08:34:55.2616|1|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KG8" started. 
2016-06-16 08:34:55.2616|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/lib/bootstrap/dist/css/bootstrap.css   
2016-06-16 08:34:55.2616|1|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KG9" started. 
2016-06-16 08:34:55.2885|9|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KG5" completed keep alive response. 
2016-06-16 08:34:55.2885|9|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KG6" completed keep alive response. 
2016-06-16 08:34:55.2945|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/css/site.css   
2016-06-16 08:34:55.3056|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/lib/jquery/dist/jquery.js   
2016-06-16 08:34:55.2945|14|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|INFO|Connection id "0HKSLM1938KG6" communication error Error -4077 ECONNRESET connection reset by peer
2016-06-16 08:34:55.3056|6|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|The file /lib/bootstrap/dist/css/bootstrap.css was not modified 
2016-06-16 08:34:55.3235|6|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|The file /css/site.css was not modified 
2016-06-16 08:34:55.3056|10|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KG6" disconnecting. 
2016-06-16 08:34:55.3235|6|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|The file /lib/jquery/dist/jquery.js was not modified 
2016-06-16 08:34:55.3555|8|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|Handled. Status code: 304 File: /lib/bootstrap/dist/css/bootstrap.css 
2016-06-16 08:34:55.3555|8|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|Handled. Status code: 304 File: /css/site.css 
2016-06-16 08:34:55.3555|2|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KG6" stopped. 
2016-06-16 08:34:55.3715|8|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|Handled. Status code: 304 File: /lib/jquery/dist/jquery.js 
2016-06-16 08:34:55.3715|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 109.1999ms 304 text/css 
2016-06-16 08:34:55.3715|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 85.2485ms 304 text/css 
2016-06-16 08:34:55.3895|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 83.9817ms 304 application/javascript 
2016-06-16 08:34:55.3996|9|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KG7" completed keep alive response. 
2016-06-16 08:34:55.3996|9|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KG8" completed keep alive response. 
2016-06-16 08:34:55.3996|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/lib/bootstrap/dist/js/bootstrap.js   
2016-06-16 08:34:55.3996|1|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KGA" started. 
2016-06-16 08:34:55.3996|9|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KG9" completed keep alive response. 
2016-06-16 08:34:55.4195|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/js/site.js?v=EWaMeWsJBYWmL2g_KkgXZQ5nPe-a3Ichp0LEgzXczKo   
2016-06-16 08:34:55.4195|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/images/banner2.svg   
2016-06-16 08:34:55.4305|6|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|The file /lib/bootstrap/dist/js/bootstrap.js was not modified 
2016-06-16 08:34:55.4305|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/images/banner1.svg   
2016-06-16 08:34:55.4305|6|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|The file /js/site.js was not modified 
2016-06-16 08:34:55.4305|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/lib/bootstrap/dist/fonts/glyphicons-halflings-regular.woff2   
2016-06-16 08:34:55.4485|6|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|The file /images/banner2.svg was not modified 
2016-06-16 08:34:55.4485|8|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|Handled. Status code: 304 File: /lib/bootstrap/dist/js/bootstrap.js 
2016-06-16 08:34:55.4485|6|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|The file /images/banner1.svg was not modified 
2016-06-16 08:34:55.4655|8|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|Handled. Status code: 304 File: /js/site.js 
2016-06-16 08:34:55.4655|6|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|The file /lib/bootstrap/dist/fonts/glyphicons-halflings-regular.woff2 was not modified 
2016-06-16 08:34:55.4655|8|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|Handled. Status code: 304 File: /images/banner2.svg 
2016-06-16 08:34:55.4805|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 71.7164ms 304 application/javascript 
2016-06-16 08:34:55.4805|8|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|Handled. Status code: 304 File: /images/banner1.svg 
2016-06-16 08:34:55.4805|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 70.1508ms 304 application/javascript 
2016-06-16 08:34:55.4955|8|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|Handled. Status code: 304 File: /lib/bootstrap/dist/fonts/glyphicons-halflings-regular.woff2 
2016-06-16 08:34:55.4955|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 74.089ms 304 image/svg+xml 
2016-06-16 08:34:55.4955|9|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KG5" completed keep alive response. 
2016-06-16 08:34:55.4955|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 74.8835ms 304 image/svg+xml 
2016-06-16 08:34:55.5125|9|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KG7" completed keep alive response. 
2016-06-16 08:34:55.5125|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 82.2974ms 304 application/font-woff2 
2016-06-16 08:34:55.5125|9|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KG8" completed keep alive response. 
2016-06-16 08:34:55.5255|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/images/banner4.svg   
2016-06-16 08:34:55.5255|9|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KGA" completed keep alive response. 
2016-06-16 08:34:55.4305|1|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KGB" started. 
2016-06-16 08:34:55.5425|9|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KG9" completed keep alive response. 
2016-06-16 08:34:55.5425|6|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|The file /images/banner4.svg was not modified 
2016-06-16 08:34:55.5585|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/images/banner3.svg   
2016-06-16 08:34:55.5585|8|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|Handled. Status code: 304 File: /images/banner4.svg 
2016-06-16 08:34:55.5735|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 50.9481ms 304 image/svg+xml 
2016-06-16 08:34:55.5735|6|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|The file /images/banner3.svg was not modified 
2016-06-16 08:34:55.5735|9|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KG5" completed keep alive response. 
2016-06-16 08:34:55.5885|8|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|Handled. Status code: 304 File: /images/banner3.svg 
2016-06-16 08:34:55.5885|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 37.2668ms 304 image/svg+xml 
2016-06-16 08:34:55.5885|9|Microsoft.AspNetCore.Server.Kestrel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60|DEBUG|Connection id "0HKSLM1938KGB" completed keep alive response.
```

#### nlog-own-2016-05-21.log

```
2016-05-21 13:39:53.7158|HomeController|INFO|  Index page says hello 

```


### How to run the example (aspnet-core-example)
How to run the [aspnet-core-example](https://github.com/NLog/NLog.Extensions.Logging/tree/master/examples/aspnet-core-example):

1. Install dotnet: http://dot.net 
2. From source: `dotnet run`
3. or, after publish: `dotnet aspnet-core-example.dll`
