# NLog.Extensions.Hosting

[![](https://sonarcloud.io/api/project_badges/measure?project=nlog.extensions.logging&branch=master&metric=reliability_rating)](https://sonarcloud.io/dashboard/?id=nlog.extensions.logging&branch=master) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog.extensions.logging&branch=master&metric=sqale_rating)](https://sonarcloud.io/dashboard/?id=nlog.extensions.logging&branch=master) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog.extensions.logging&branch=master&metric=vulnerabilities)](https://sonarcloud.io/dashboard/?id=nlog.extensions.logging&branch=master) 

Integrates NLog as Logging provider for Microsoft.Extensions.Logging, by just calling `UseNLog()` on the application HostBuilder.

Providing features like:

- Capture [structured message properties](https://github.com/NLog/NLog.Extensions.Logging/wiki/NLog-properties-with-Microsoft-Extension-Logging) from the [Microsoft ILogger](https://github.com/NLog/NLog.Extensions.Logging/wiki/NLog-GetCurrentClassLogger-and-Microsoft-ILogger)
- Capture [scope context properties](https://github.com/NLog/NLog/wiki/ScopeProperty-Layout-Renderer) from the Microsoft ILogger `BeginScope`
- Load NLog configuration from [appsettings.json](https://github.com/NLog/NLog.Extensions.Logging/wiki/NLog-configuration-with-appsettings.json)
- Routing logging output to multiple destinations via the available [NLog Targets](https://nlog-project.org/config/?tab=targets)
- Enrich logging output with additional context details via the available [NLog LayoutRenderers](https://nlog-project.org/config/?tab=layout-renderers)
- Rendering logging output into standard formats like JSON, CVS, W3C ELF and XML using [NLog Layouts](https://nlog-project.org/config/?tab=layouts).

If using ASP.NET Core then check [NLog.Web.AspNetCore](https://www.nuget.org/packages/NLog.Web.AspNetCore).

Supported platforms:

 - .NET 6, 7, 8, 9 and 10
 - .NET Standard 2.0 and 2.1
 - .NET 4.6.2 - 4.8

Register NLog as logging provider:

```csharp
var hostBuilder = new HostBuilder().UseNLog();
```

Useful Links

- [Home Page](https://nlog-project.org/)
- [Change Log](https://github.com/NLog/NLog.Extensions.Logging/releases)
- [Tutorial](https://github.com/NLog/NLog/wiki/Tutorial)
- [Logging Troubleshooting](https://github.com/NLog/NLog/wiki/Logging-troubleshooting)
- [Have a question?](https://stackoverflow.com/questions/tagged/nlog)
