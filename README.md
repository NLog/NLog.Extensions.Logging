# NLog.Extensions.Logging

[![Build status Windows](https://ci.appveyor.com/api/projects/status/0nrg8cksp4b6tab1/branch/master?svg=true)](https://ci.appveyor.com/project/nlog/nlog-framework-logging/branch/master)

<!-- 
[![Build Status Linux / Mac OS](https://travis-ci.org/NLog/NLog.Extensions.Logging.svg?branch=master)](https://travis-ci.org/NLog/NLog.Extensions.Logging)
-->
[![NuGet Pre Release](https://img.shields.io/nuget/vpre/NLog.Extensions.Logging.svg)](https://www.nuget.org/packages/NLog.Extensions.Logging)

[NLog](https://github.com/NLog/NLog) provider for [Microsoft.Extensions.Logging](https://github.com/aspnet/Logging); .NET Core and ASP.NET Core. 


Routes .NET Core log messages to NLog.


**Note**: Microsoft haven't ported all their classes to .NET standard, so not every target/layout renderer is available. 
Please check [platform support](https://github.com/NLog/NLog/wiki/platform-support)

ASP.NET Core users should also install  [NLog.Web.AspNetCore](https://www.nuget.org/packages/NLog.web.aspnetcore)!


How to use
----
1. Add dependency in project.json
    ```json
     "dependencies": {
        "NLog.Extensions.Logging": "1.0.0-*",
        "NLog.Web.AspNetCore": "4.3.0" //only needed for ASP.NET Core users
      }
    ```

2. Create nlog.config in root of your project file, see [NLog.config example](https://raw.githubusercontent.com/NLog/NLog.Extensions.Logging/03971763546cc70660529bbe28b282304adb7571/examples/aspnet-core-example/src/aspnet-core-example/nlog.config)
3.  in startup.cs add in `Configure`

    ```c#
      using NLog.Extensions.Logging;
    
      public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
      {
          //add NLog to .NET Core
          loggerFactory.AddNLog();
          
          //Enable ASP.NET Core features (NLog.web) - only needed for ASP.NET Core users
          app.AddNLogWeb();

          //needed for non-NETSTANDARD platforms: configure nlog.config in your project root
          env.ConfigureNLog("nlog.config");
          ...
    ```  
    
    ASP.NET Core users should also enabled `IHttpContextAccessor` in startup.cs
    ```c#
           
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            //needed for NLog.Web
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }
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
    

Known issues
---
- Installing the NuGet packages [NLog.config](https://www.nuget.org/packages/NLog.Config/) / [NLog.schema](https://www.nuget.org/packages/NLog.Schema/) won't add to your project. It's recommend to extract (unzip) the NLog.Schema package and place the NLog.XSD in the same folder as NLog.config.
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

  <!-- for ASP.NET Core users -->
  <extensions>
    <add assembly="NLog.Web.AspNetCore"/>
  </extensions>

  <!-- define various log targets -->
  <targets>
     <!-- write logs to file -->
     <target xsi:type="File" name="allfile" fileName="c:\temp\nlog-all-${shortdate}.log"
                 layout="${longdate}|${event-properties:item=EventId.Id}|${logger}|${uppercase:${level}}|${message} ${exception}" />

   <!-- only own logs. Uses some ASP.NET core renderers -->
     <target xsi:type="File" name="ownFile-web" fileName="c:\temp\nlog-own-${shortdate}.log"
             layout="${longdate}|${event-properties:item=EventId.Id}|${logger}|${uppercase:${level}}|  ${message} ${exception}|url: ${aspnet-request-url}|action: ${aspnet-mvc-action}" />


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

#### nlog-all-2017-01-18.log

```
2017-01-18 23:29:25.4588|3|Microsoft.AspNetCore.Hosting.Internal.WebHost|DEBUG|Hosting starting 
2017-01-18 23:29:25.6558|4|Microsoft.AspNetCore.Hosting.Internal.WebHost|DEBUG|Hosting started 
2017-01-18 23:29:25.7762|1|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU5QOINJG" started. 
2017-01-18 23:29:25.8827|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 DEBUG http://localhost:59915/  0 
2017-01-18 23:29:25.9782|9|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU5QOINJG" completed keep alive response. 
2017-01-18 23:29:25.9902|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 126.8128ms 200  
2017-01-18 23:29:28.0534|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/   
2017-01-18 23:29:28.0929|4|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|The request path / does not match a supported file type 
2017-01-18 23:29:28.2884|1|Microsoft.AspNetCore.Routing.RouteBase|DEBUG|Request successfully matched the route with name 'default' and template '{controller=Home}/{action=Index}/{id?}'. 
2017-01-18 23:29:28.3499|1|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|DEBUG|Executing action aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) 
2017-01-18 23:29:28.4019|1|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|INFO|Executing action method aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) with arguments ((null)) - ModelState is Valid 
2017-01-18 23:29:28.4144||HomeController|INFO|Index page says hello 
2017-01-18 23:29:28.4269|2|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|DEBUG|Executed action method aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example), returned result Microsoft.AspNetCore.Mvc.ViewResult. 
2017-01-18 23:29:28.5659|1|Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine|DEBUG|View lookup cache miss for view 'Index' in controller 'Home'. 
2017-01-18 23:29:28.5889|1|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/Home/Index.cshtml' started. 
2017-01-18 23:29:28.9989|2|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/Home/Index.cshtml' completed in 404.4264ms. 
2017-01-18 23:29:29.0259|1|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/Home/Index.cshtml' started. 
2017-01-18 23:29:30.7494|2|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/Home/Index.cshtml' completed in 1718.0443ms. 
2017-01-18 23:29:30.7659|1|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/_ViewStart.cshtml' started. 
2017-01-18 23:29:30.7854|2|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/_ViewStart.cshtml' completed in 7.8932ms. 
2017-01-18 23:29:30.7854|1|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/_ViewStart.cshtml' started. 
2017-01-18 23:29:30.8779|2|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/_ViewStart.cshtml' completed in 76.1982ms. 
2017-01-18 23:29:30.8919|2|Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor|DEBUG|The view 'Index' was found. 
2017-01-18 23:29:30.8919|1|Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor|INFO|Executing ViewResult, running view at path /Views/Home/Index.cshtml. 
2017-01-18 23:29:30.9974|0|Microsoft.Extensions.DependencyInjection.DataProtectionServices|INFO|User profile is available. Using 'C:\Users\j.verdurmen\AppData\Local\ASP.NET\DataProtection-Keys' as key repository and Windows DPAPI to encrypt keys at rest. 
2017-01-18 23:29:31.0884|1|Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine|DEBUG|View lookup cache miss for view '_Layout' in controller 'Home'. 
2017-01-18 23:29:31.0939|1|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/Shared/_Layout.cshtml' started. 
2017-01-18 23:29:31.1399|2|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/Shared/_Layout.cshtml' completed in 41.1951ms. 
2017-01-18 23:29:31.1449|1|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/Shared/_Layout.cshtml' started. 
2017-01-18 23:29:31.2504|2|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/Shared/_Layout.cshtml' completed in 98.1822ms. 
2017-01-18 23:29:31.3734|2|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|INFO|Executed action aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) in 3018.1612ms 
2017-01-18 23:29:31.4269|9|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU5QOINJG" completed keep alive response. 
2017-01-18 23:29:31.4409|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 3387.774ms 200 text/html; charset=utf-8 
2017-01-18 23:29:31.7814|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/favicon.ico   
2017-01-18 23:29:31.8104|2|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|Sending file. Request path: '/favicon.ico'. Physical path: 'X:\nlog\NLog.Extensions.Logging\examples\aspnet-core-example\src\aspnet-core-example\wwwroot\favicon.ico' 
2017-01-18 23:29:31.8294|9|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU5QOINJG" completed keep alive response. 
2017-01-18 23:29:31.8479|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 66.3189ms 200 image/x-icon 
2017-01-18 23:29:39.2429|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/   
2017-01-18 23:29:39.2529|4|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|The request path / does not match a supported file type 
2017-01-18 23:29:39.2529|1|Microsoft.AspNetCore.Routing.RouteBase|DEBUG|Request successfully matched the route with name 'default' and template '{controller=Home}/{action=Index}/{id?}'. 
2017-01-18 23:29:39.2669|1|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|DEBUG|Executing action aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) 
2017-01-18 23:29:39.2669|1|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|INFO|Executing action method aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) with arguments ((null)) - ModelState is Valid 
2017-01-18 23:29:39.2899||HomeController|INFO|Index page says hello 
2017-01-18 23:29:39.2984|2|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|DEBUG|Executed action method aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example), returned result Microsoft.AspNetCore.Mvc.ViewResult. 
2017-01-18 23:29:39.2984|2|Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine|DEBUG|View lookup cache hit for view 'Index' in controller 'Home'. 
2017-01-18 23:29:39.3154|2|Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor|DEBUG|The view 'Index' was found. 
2017-01-18 23:29:39.3154|1|Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor|INFO|Executing ViewResult, running view at path /Views/Home/Index.cshtml. 
2017-01-18 23:29:39.3334|2|Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine|DEBUG|View lookup cache hit for view '_Layout' in controller 'Home'. 
2017-01-18 23:29:39.3334|2|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|INFO|Executed action aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) in 62.5496ms 
2017-01-18 23:29:39.3819|9|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU5QOINJG" completed keep alive response. 
2017-01-18 23:29:39.4054|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 162.5895ms 200 text/html; charset=utf-8 
2017-01-18 23:29:39.6269|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/favicon.ico   
2017-01-18 23:29:39.6524|2|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|Sending file. Request path: '/favicon.ico'. Physical path: 'X:\nlog\NLog.Extensions.Logging\examples\aspnet-core-example\src\aspnet-core-example\wwwroot\favicon.ico' 
2017-01-18 23:29:39.6769|9|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU5QOINJG" completed keep alive response. 
2017-01-18 23:29:39.6954|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 68.349ms 200 image/x-icon 
2017-01-18 23:31:15.7607|6|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU5QOINJG" received FIN. 
2017-01-18 23:31:15.7737|10|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU5QOINJG" disconnecting. 
2017-01-18 23:31:15.7737|7|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU5QOINJG" sending FIN. 
2017-01-18 23:31:15.7887|8|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU5QOINJG" sent FIN with status "0". 
2017-01-18 23:31:15.7977|2|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU5QOINJG" stopped. 
2017-01-18 23:32:39.8830|1|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU5QOINJH" started. 
2017-01-18 23:32:39.8830|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/   
2017-01-18 23:32:39.9005|4|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|The request path / does not match a supported file type 
2017-01-18 23:32:39.9005|1|Microsoft.AspNetCore.Routing.RouteBase|DEBUG|Request successfully matched the route with name 'default' and template '{controller=Home}/{action=Index}/{id?}'. 
2017-01-18 23:32:39.9130|1|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|DEBUG|Executing action aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) 
2017-01-18 23:32:39.9130|1|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|INFO|Executing action method aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) with arguments ((null)) - ModelState is Valid 
2017-01-18 23:32:39.9300||HomeController|INFO|Index page says hello 
2017-01-18 23:32:39.9445|2|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|DEBUG|Executed action method aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example), returned result Microsoft.AspNetCore.Mvc.ViewResult. 
2017-01-18 23:32:39.9445|2|Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine|DEBUG|View lookup cache hit for view 'Index' in controller 'Home'. 
2017-01-18 23:32:39.9580|2|Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor|DEBUG|The view 'Index' was found. 
2017-01-18 23:32:39.9580|1|Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor|INFO|Executing ViewResult, running view at path /Views/Home/Index.cshtml. 
2017-01-18 23:32:39.9740|2|Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine|DEBUG|View lookup cache hit for view '_Layout' in controller 'Home'. 
2017-01-18 23:32:39.9740|2|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|INFO|Executed action aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) in 59.9152ms 
2017-01-18 23:32:40.0250|9|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU5QOINJH" completed keep alive response. 
2017-01-18 23:32:40.0425|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 152.2544ms 200 text/html; charset=utf-8 
2017-01-18 23:32:40.2615|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/favicon.ico   
2017-01-18 23:32:40.2810|2|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|Sending file. Request path: '/favicon.ico'. Physical path: 'X:\nlog\NLog.Extensions.Logging\examples\aspnet-core-example\src\aspnet-core-example\wwwroot\favicon.ico' 
2017-01-18 23:32:40.3280|9|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU5QOINJH" completed keep alive response. 
2017-01-18 23:32:40.3400|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 78.7128ms 200 image/x-icon 
2017-01-18 23:34:41.7558|10|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU5QOINJH" disconnecting. 
2017-01-18 23:34:41.7753|7|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU5QOINJH" sending FIN. 
2017-01-18 23:34:41.7973|8|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU5QOINJH" sent FIN with status "0". 
2017-01-18 23:34:41.8178|2|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU5QOINJH" stopped. 
2017-01-18 23:36:12.2705|3|Microsoft.AspNetCore.Hosting.Internal.WebHost|DEBUG|Hosting starting 
2017-01-18 23:36:12.4640|4|Microsoft.AspNetCore.Hosting.Internal.WebHost|DEBUG|Hosting started 
2017-01-18 23:36:12.5510|1|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU9JVRFCH" started. 
2017-01-18 23:36:12.6935|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 DEBUG http://localhost:59915/  0 
2017-01-18 23:36:12.8110|9|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU9JVRFCH" completed keep alive response. 
2017-01-18 23:36:12.8295|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 155.3934ms 200  
2017-01-18 23:36:16.4743|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/   
2017-01-18 23:36:16.5119|4|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|The request path / does not match a supported file type 
2017-01-18 23:36:16.7058|1|Microsoft.AspNetCore.Routing.RouteBase|DEBUG|Request successfully matched the route with name 'default' and template '{controller=Home}/{action=Index}/{id?}'. 
2017-01-18 23:36:16.7708|1|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|DEBUG|Executing action aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) 
2017-01-18 23:36:16.8198|1|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|INFO|Executing action method aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) with arguments ((null)) - ModelState is Valid 
2017-01-18 23:36:16.8293||HomeController|INFO|Index page says hello 
2017-01-18 23:36:16.8518|2|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|DEBUG|Executed action method aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example), returned result Microsoft.AspNetCore.Mvc.ViewResult. 
2017-01-18 23:36:16.9938|1|Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine|DEBUG|View lookup cache miss for view 'Index' in controller 'Home'. 
2017-01-18 23:36:17.0153|1|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/Home/Index.cshtml' started. 
2017-01-18 23:36:17.4663|2|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/Home/Index.cshtml' completed in 441.4617ms. 
2017-01-18 23:36:17.4923|1|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/Home/Index.cshtml' started. 
2017-01-18 23:36:18.9044|2|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/Home/Index.cshtml' completed in 1407.1852ms. 
2017-01-18 23:36:18.9174|1|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/_ViewStart.cshtml' started. 
2017-01-18 23:36:18.9324|2|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/_ViewStart.cshtml' completed in 9.2933ms. 
2017-01-18 23:36:18.9379|1|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/_ViewStart.cshtml' started. 
2017-01-18 23:36:18.9919|2|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/_ViewStart.cshtml' completed in 46.9855ms. 
2017-01-18 23:36:19.0044|2|Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor|DEBUG|The view 'Index' was found. 
2017-01-18 23:36:19.0044|1|Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor|INFO|Executing ViewResult, running view at path /Views/Home/Index.cshtml. 
2017-01-18 23:36:19.1189|0|Microsoft.Extensions.DependencyInjection.DataProtectionServices|INFO|User profile is available. Using 'C:\Users\j.verdurmen\AppData\Local\ASP.NET\DataProtection-Keys' as key repository and Windows DPAPI to encrypt keys at rest. 
2017-01-18 23:36:19.1969|1|Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine|DEBUG|View lookup cache miss for view '_Layout' in controller 'Home'. 
2017-01-18 23:36:19.1969|1|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/Shared/_Layout.cshtml' started. 
2017-01-18 23:36:19.2479|2|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/Shared/_Layout.cshtml' completed in 39.443ms. 
2017-01-18 23:36:19.2549|1|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/Shared/_Layout.cshtml' started. 
2017-01-18 23:36:19.3739|2|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/Shared/_Layout.cshtml' completed in 111.0883ms. 
2017-01-18 23:36:19.5929|2|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|INFO|Executed action aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) in 2816.0444ms 
2017-01-18 23:36:19.6319|9|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU9JVRFCH" completed keep alive response. 
2017-01-18 23:36:19.6584|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 3184.0505ms 200 text/html; charset=utf-8 
2017-01-18 23:36:19.8779|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/favicon.ico   
2017-01-18 23:36:19.9124|2|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|Sending file. Request path: '/favicon.ico'. Physical path: 'X:\nlog\NLog.Extensions.Logging\examples\aspnet-core-example\src\aspnet-core-example\wwwroot\favicon.ico' 
2017-01-18 23:36:19.9484|9|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU9JVRFCH" completed keep alive response. 
2017-01-18 23:36:19.9704|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 92.6676ms 200 image/x-icon 
2017-01-18 23:36:29.6721|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/   
2017-01-18 23:36:29.6721|4|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|The request path / does not match a supported file type 
2017-01-18 23:36:29.6881|1|Microsoft.AspNetCore.Routing.RouteBase|DEBUG|Request successfully matched the route with name 'default' and template '{controller=Home}/{action=Index}/{id?}'. 
2017-01-18 23:36:29.6881|1|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|DEBUG|Executing action aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) 
2017-01-18 23:36:29.6881|1|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|INFO|Executing action method aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) with arguments ((null)) - ModelState is Valid 
2017-01-18 23:36:29.7091||HomeController|INFO|Index page says hello 
2017-01-18 23:36:29.7091|2|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|DEBUG|Executed action method aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example), returned result Microsoft.AspNetCore.Mvc.ViewResult. 
2017-01-18 23:36:29.7277|2|Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine|DEBUG|View lookup cache hit for view 'Index' in controller 'Home'. 
2017-01-18 23:36:29.7277|2|Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor|DEBUG|The view 'Index' was found. 
2017-01-18 23:36:29.7421|1|Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor|INFO|Executing ViewResult, running view at path /Views/Home/Index.cshtml. 
2017-01-18 23:36:29.7561|2|Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine|DEBUG|View lookup cache hit for view '_Layout' in controller 'Home'. 
2017-01-18 23:36:29.7731|2|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|INFO|Executed action aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) in 74.1014ms 
2017-01-18 23:36:29.8236|9|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU9JVRFCH" completed keep alive response. 
2017-01-18 23:36:29.8386|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 166.5265ms 200 text/html; charset=utf-8 
2017-01-18 23:36:30.0336|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/favicon.ico   
2017-01-18 23:36:30.1151|2|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|Sending file. Request path: '/favicon.ico'. Physical path: 'X:\nlog\NLog.Extensions.Logging\examples\aspnet-core-example\src\aspnet-core-example\wwwroot\favicon.ico' 
2017-01-18 23:36:30.1371|9|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VU9JVRFCH" completed keep alive response. 
2017-01-18 23:36:30.1566|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 123.3436ms 200 image/x-icon 
2017-01-18 23:36:56.8117|3|Microsoft.AspNetCore.Hosting.Internal.WebHost|DEBUG|Hosting starting 
2017-01-18 23:36:56.9572|4|Microsoft.AspNetCore.Hosting.Internal.WebHost|DEBUG|Hosting started 
2017-01-18 23:36:57.1297|1|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VUA190S9S" started. 
2017-01-18 23:36:57.2272|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 DEBUG http://localhost:59915/  0 
2017-01-18 23:36:57.2802|1|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VUA190S9T" started. 
2017-01-18 23:36:57.2942|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/   
2017-01-18 23:36:57.3512|4|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|DEBUG|The request path / does not match a supported file type 
2017-01-18 23:36:57.3602|9|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VUA190S9S" completed keep alive response. 
2017-01-18 23:36:57.3817|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 169.3665ms 200  
2017-01-18 23:36:57.5492|1|Microsoft.AspNetCore.Routing.RouteBase|DEBUG|Request successfully matched the route with name 'default' and template '{controller=Home}/{action=Index}/{id?}'. 
2017-01-18 23:36:57.6107|1|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|DEBUG|Executing action aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) 
2017-01-18 23:36:57.6572|1|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|INFO|Executing action method aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) with arguments ((null)) - ModelState is Valid 
2017-01-18 23:36:57.6572||HomeController|INFO|Index page says hello 
2017-01-18 23:36:57.6842|2|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|DEBUG|Executed action method aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example), returned result Microsoft.AspNetCore.Mvc.ViewResult. 
2017-01-18 23:36:57.8136|1|Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine|DEBUG|View lookup cache miss for view 'Index' in controller 'Home'. 
2017-01-18 23:36:57.8351|1|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/Home/Index.cshtml' started. 
2017-01-18 23:36:58.4256|2|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/Home/Index.cshtml' completed in 580.275ms. 
2017-01-18 23:36:58.4636|1|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/Home/Index.cshtml' started. 
2017-01-18 23:37:01.2131|2|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/Home/Index.cshtml' completed in 2731.0185ms. 
2017-01-18 23:37:01.2266|1|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/_ViewStart.cshtml' started. 
2017-01-18 23:37:01.2446|2|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/_ViewStart.cshtml' completed in 12.4182ms. 
2017-01-18 23:37:01.2506|1|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/_ViewStart.cshtml' started. 
2017-01-18 23:37:01.3141|2|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/_ViewStart.cshtml' completed in 58.1588ms. 
2017-01-18 23:37:01.3301|2|Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor|DEBUG|The view 'Index' was found. 
2017-01-18 23:37:01.3301|1|Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor|INFO|Executing ViewResult, running view at path /Views/Home/Index.cshtml. 
2017-01-18 23:37:01.4346|0|Microsoft.Extensions.DependencyInjection.DataProtectionServices|INFO|User profile is available. Using 'C:\Users\j.verdurmen\AppData\Local\ASP.NET\DataProtection-Keys' as key repository and Windows DPAPI to encrypt keys at rest. 
2017-01-18 23:37:01.5111|1|Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine|DEBUG|View lookup cache miss for view '_Layout' in controller 'Home'. 
2017-01-18 23:37:01.5206|1|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/Shared/_Layout.cshtml' started. 
2017-01-18 23:37:01.5701|2|Microsoft.AspNetCore.Mvc.Razor.Internal.RazorCompilationService|DEBUG|Code generation for the Razor file at '/Views/Shared/_Layout.cshtml' completed in 41.2716ms. 
2017-01-18 23:37:01.5701|1|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/Shared/_Layout.cshtml' started. 
2017-01-18 23:37:01.7111|2|Microsoft.AspNetCore.Mvc.Razor.Internal.DefaultRoslynCompilationService|DEBUG|Compilation of the generated code for the Razor file at '/Views/Shared/_Layout.cshtml' completed in 131.4635ms. 
2017-01-18 23:37:01.8561|2|Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker|INFO|Executed action aspnet_core_example.Controllers.HomeController.Index (aspnet-core-example) in 4240.0228ms 
2017-01-18 23:37:01.8911|9|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VUA190S9T" completed keep alive response. 
2017-01-18 23:37:01.9076|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 4613.3417ms 200 text/html; charset=utf-8 
2017-01-18 23:37:02.3031|1|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request starting HTTP/1.1 GET http://localhost:59915/favicon.ico   
2017-01-18 23:37:02.3311|2|Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware|INFO|Sending file. Request path: '/favicon.ico'. Physical path: 'X:\nlog\NLog.Extensions.Logging\examples\aspnet-core-example\src\aspnet-core-example\wwwroot\favicon.ico' 
2017-01-18 23:37:02.3596|9|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VUA190S9S" completed keep alive response. 
2017-01-18 23:37:02.3596|2|Microsoft.AspNetCore.Hosting.Internal.WebHost|INFO|Request finished in 68.7946ms 200 image/x-icon 
2017-01-18 23:38:47.1142|6|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VUA190S9T" received FIN. 
2017-01-18 23:38:47.1142|6|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VUA190S9S" received FIN. 
2017-01-18 23:38:47.1277|10|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VUA190S9T" disconnecting. 
2017-01-18 23:38:47.1277|10|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VUA190S9S" disconnecting. 
2017-01-18 23:38:47.1277|7|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VUA190S9T" sending FIN. 
2017-01-18 23:38:47.1447|7|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VUA190S9S" sending FIN. 
2017-01-18 23:38:47.1447|8|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VUA190S9T" sent FIN with status "0". 
2017-01-18 23:38:47.1587|8|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VUA190S9S" sent FIN with status "0". 
2017-01-18 23:38:47.1587|2|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VUA190S9T" stopped. 
2017-01-18 23:38:47.1587|2|Microsoft.AspNetCore.Server.Kestrel|DEBUG|Connection id "0HL1VUA190S9S" stopped. 

```

#### nlog-own-2017-01-18.log

```
2017-01-18 23:36:57.6572||HomeController|INFO|  Index page says hello |url: http://localhost/|action: Index

```


### How to run the example (aspnet-core-example)
How to run the [aspnet-core-example](https://github.com/NLog/NLog.Extensions.Logging/tree/master/examples/aspnet-core-example):

1. Install dotnet: http://dot.net 
2. From source: `dotnet run`
3. or, after publish: `dotnet aspnet-core-example.dll`
