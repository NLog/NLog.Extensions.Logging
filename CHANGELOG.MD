## Change Log

Date format: (year/month/day)

### v1.7.4 (2021/08/25)

#### ⏩ Performance

- [#513](https://github.com/NLog/NLog.Extensions.Logging/pull/513): NLogLogger - Skip custom message formatting when no parameters (@snakefoot)

#### 🔧 Maintenance

- [#515](https://github.com/NLog/NLog.Extensions.Logging/pull/515): Disable ImplicitNuGetFallbackFolder for tests - fixes NU3005 on AppVeyor (@304NotModified)

#### ↜ Dependencies

- [#516](https://github.com/NLog/NLog.Extensions.Logging/pull/516): Bump NLog from 4.7.10 to 4.7.11 (@dependabot)


### v1.7.3 (2021/06/15)
- [#510](https://github.com/NLog/NLog.Extensions.Logging/pull/510) Reduced code complexity of MicrosoftILoggerTarget (@snakefoot)
- [#509](https://github.com/NLog/NLog.Extensions.Logging/pull/509) Faster checking if LogLevel IsEnabled (#509) (@snakefoot)
- [#507](https://github.com/NLog/NLog.Extensions.Logging/pull/507) Bump NLog from 4.7.9 to 4.7.10 (#507) (@dependabot[bot])
- [#501](https://github.com/NLog/NLog.Extensions.Logging/pull/501) Extended NLogProviderOptions with the option LoggingConfigurationSectionName (#501) (@snakefoot)
- [#500](https://github.com/NLog/NLog.Extensions.Logging/pull/500) Support TargetDefaultWrapper and TargetDefaultParameters in appsettings.json (#500) (@snakefoot)
- [#494](https://github.com/NLog/NLog.Extensions.Logging/pull/494) Fix SonarQube Scanning (#494) (@304NotModified)

### v1.7.2 (2021/03/26)
- [#495](https://github.com/NLog/NLog.Extensions.Logging/pull/495) Bump NLog from 4.7.8 to 4.7.9 (#495) (@dependabot[bot])
- [#490](https://github.com/NLog/NLog.Extensions.Logging/pull/490) MicrosoftConsoleLayoutRenderer - For Hosting Lifetime Startup Messages (#490) (@snakefoot)
- [#488](https://github.com/NLog/NLog.Extensions.Logging/pull/488) NLogLoggingConfiguration support custom section names (#488) (@anellomancante)
- [#482](https://github.com/NLog/NLog.Extensions.Logging/pull/482) NLogProviderOptions - Added ReplaceLoggerFactory and RemoveLoggerFactoryFilter (#482) (@snakefoot)
- [#479](https://github.com/NLog/NLog.Extensions.Logging/pull/479) Enable deterministic build (#479) (@304NotModified)

### v1.7.1 (2021/02/26)
- [#476](https://github.com/NLog/NLog.Extensions.Logging/pull/476) MicrosoftILoggerTarget - Add support for override LoggerName when using ILoggerFactory (#476) (@snakefoot)
- [#470](https://github.com/NLog/NLog.Extensions.Logging/pull/470) MicrosoftILoggerTarget - Reduce chance of deadlock in ILoggerFactory by caching created loggers (#470) (@snakefoot)
- [#468](https://github.com/NLog/NLog.Extensions.Logging/pull/468) MicrosoftILoggerTarget - With ILoggerFactory as constructor parameter (#468) (@snakefoot)
- [#463](https://github.com/NLog/NLog.Extensions.Logging/pull/463) Added unit-test for NLogProviderOptions.IncludeActivtyIdsWithBeginScope (#463) (@snakefoot)
- [#469](https://github.com/NLog/NLog.Extensions.Logging/pull/469) Update Nuget package description with ASP.NET Core so it matches NLog Package (#469) (@snakefoot)
- [#467](https://github.com/NLog/NLog.Extensions.Logging/pull/467) Bump NLog from 4.7.6 to 4.7.7 (#467) (@dependabot[bot])
- [#477](https://github.com/NLog/NLog.Extensions.Logging/pull/477) Bump NLog from 4.7.7 to 4.7.8 (#477) (@dependabot[bot])

### v1.7.0 (2020/12/29)
- [#456](https://github.com/NLog/NLog.Extensions.Logging/pull/456) Added NLogProviderOptions.IncludeActivtyIdsWithBeginScope and added .NET 5 build (#456) (@snakefoot)
- [#451](https://github.com/NLog/NLog.Extensions.Logging/pull/451) NLogBeginScopeParser - Reduce allocation for BeginScope with message template (#451) (@snakefoot)
- [#461](https://github.com/NLog/NLog.Extensions.Logging/pull/461) Move to PackageIcon and PackageLicenseExpression for NuGet package (#461) (@snakefoot)
- [#450](https://github.com/NLog/NLog.Extensions.Logging/pull/458) Bump NLog from 4.7.4 to 4.7.6 (#450, #458) (@dependabot[bot])

### v1.6.5 (2020/08/27)
- [#444](https://github.com/NLog/NLog.Extensions.Logging/pull/444) Fix ShutdownOnDispose to work with isolated LogFactory (#444) (@snakefoot)
- [#439](https://github.com/NLog/NLog.Extensions.Logging/pull/439) Added SourceLink (#439) (@304NotModified)
- [#429](https://github.com/NLog/NLog.Extensions.Logging/pull/429) Fix documentation of default (#429) (@304NotModified)
- [#448](https://github.com/NLog/NLog.Extensions.Logging/pull/448) Bump NLog from 4.7.3 to 4.7.4 (#448) (@dependabot[bot])

### v1.6.4 (2020/05/20)
- [#419](https://github.com/NLog/NLog.Extensions.Logging/pull/423) LogFactory.Setup - Make RegisterConfigSettings public (#423) (@snakefoot)
- [#411](https://github.com/NLog/NLog.Extensions.Logging/pull/426) Bump NLog to 4.7.2 (#426) (@dependabot-preview[bot])

### v1.6.3 (2020/05/11)
- [#419](https://github.com/NLog/NLog.Extensions.Logging/pull/419) Reading JSON config - Trim keys as Json allows spaces in keys (#419) (@snakefoot)
- [#411](https://github.com/NLog/NLog.Extensions.Logging/pull/411) Bump NLog to 4.7.0 (#411) (@dependabot-preview[bot])
- [#418](https://github.com/NLog/NLog.Extensions.Logging/pull/418) Introduced LoadConfigurationFromSection extension method for LogFactory.Setup (#410, #412, #418) (@snakefoot)

### v1.6.2 (2020/03/16)
- [#380](https://github.com/NLog/NLog.Extensions.Logging/pull/380) NLogLoggerFactory - Emulate Microsoft LoggerFactory by returning same ILogger instance (#380) (@snakefoot)
- [#393](https://github.com/NLog/NLog.Extensions.Logging/pull/393) AddNLog overload with NLogProviderOptions and LoggingConfiguration (#393) (@JeffreyMercado)
- [#407](https://github.com/NLog/NLog.Extensions.Logging/pull/407) Improve AddNLog with LoggingConfiguration to handle custom LogFactory-instance (#407) (@snakefoot)
- [#408](https://github.com/NLog/NLog.Extensions.Logging/pull/408) Update NLog dependency to 4.6.8 (#408) (@304NotModified)

### v1.6.1 (2019/10/19)
- [#350](https://github.com/NLog/NLog.Extensions.Logging/pull/350) Fixed conflict issue between UseNLog and default log providers (@snakefoot)

### v1.6.0 (2019/10/07)
- [#332](https://github.com/NLog/NLog.Extensions.Logging/pull/332) Support .NET Core 3.0 (@304NotModified)
- [#342](https://github.com/NLog/NLog.Extensions.Logging/pull/342) ILoggerFactory is obsolete in .NET Core 3. Instead use ILoggingBuilder.AddNLog (#342) (@snakefoot)
- [#345](https://github.com/NLog/NLog.Extensions.Logging/pull/345) Added AddNLog with functor to create/initialize LogFactory for NLogLoggingProvider (@snakefoot)
- [#344](https://github.com/NLog/NLog.Extensions.Logging/pull/344) Introduce AddNLog with NLog-config parameter (#344) (@snakefoot)
- [#333](https://github.com/NLog/NLog.Extensions.Logging/pull/333) + [#334](https://github.com/NLog/NLog.Extensions.Logging/pull/334) Minor optimizations in parsing of LogEvent property-names

### v1.5.4 (2019/09/06)
- [#325](https://github.com/NLog/NLog.Extensions.Logging/pull/325) Fix dispose for AddNLog (on ILoggingBuilder) and no double logging when also using UseNLog (@snakefoot)

### v1.5.3 (2019/08/28)
- [#322](https://github.com/NLog/NLog.Extensions.Logging/pull/322) Fixed memory leak in NLogLoggingConfiguration for autoreload

### v1.5.2 (2019/07/14)
- [#314](https://github.com/NLog/NLog.Extensions.Logging/pull/314) Added NLogProviderOptions.ShutdownOnDispose (#314) (@304NotModified)
- [#315](https://github.com/NLog/NLog.Extensions.Logging/pull/315) Update NLog to 4.6.5 (#315) (@304NotModified)

### V1.5.1 (2019/06/05)
- [#289](https://github.com/NLog/NLog.Extensions.Logging/pull/289) NLogBeginScopeParser - Improved exception handling (@snakefoot)
- [#299](https://github.com/NLog/NLog.Extensions.Logging/pull/299) Update dependency NLog ver. 4.6.4 (@snakefoot)

### V1.5 (2019/04/30)
- [#263](https://github.com/NLog/NLog.Extensions.Logging/pull/263) Support for loading NLog config from appsettings.json using NLogLoggingConfiguration
- [#263](https://github.com/NLog/NLog.Extensions.Logging/pull/263) Improved performance and reduced allocation for ILogger.BeginScope
- [#260](https://github.com/NLog/NLog.Extensions.Logging/pull/260) Introduced MicrosoftILoggerTarget for logging in Azure Lambda (Ex. output with JsonLayout)
- [#272](https://github.com/NLog/NLog.Extensions.Logging/pull/272) Better handling of IndexOutOfRangeException when invalid log-message format

### V1.5 RC (2019/04/01)

See https://github.com/NLog/NLog.Extensions.Logging/milestone/24?closed=1

### v1.4 (2019/01/17)
- [#257](https://github.com/NLog/NLog.Extensions.Logging/pull/257) Updated to Microsoft.Extensions.Logging ver. 2.1.0 with support for ${configsetting}
- [#248](https://github.com/NLog/NLog.Extensions.Logging/pull/248) Configure NLogProviderOptions from appsettings.json
- [#257](https://github.com/NLog/NLog.Extensions.Logging/pull/258) Create snupkg packages

### v1.3 (2018/09/22)
- [#245](https://github.com/NLog/NLog.Extensions.Logging/pull/245) Added ${configsetting} for appsettings.json and others (@snakefoot, @linmasaki)
- [#246](https://github.com/NLog/NLog.Extensions.Logging/pull/246) Fixing bug throwing ArgumentOutOfRangeException if input is empty list (@thorhalvor)
- [#236](https://github.com/NLog/NLog.Extensions.Logging/pull/236) BeginScope always return scope-object even on error (@snakefoot)
- [#243](https://github.com/NLog/NLog.Extensions.Logging/pull/243) Enhancements for .NET Core 2 examples (@bryan5989)

### v1.2.1 (2018/08/05)
- [#233](https://github.com/NLog/NLog.Extensions.Logging/pull/233) Fixed null-reference-exception when parsing empty scope-property collection (@snakefoot)
- [#232](https://github.com/NLog/NLog.Extensions.Logging/pull/232) BeginScope: support for non-serializable objects + performance improvement (20-50% cpu, 35% times less allocations) (#232) (@snakefoot)
- [#229](https://github.com/NLog/NLog.Extensions.Logging/pull/229) BeginScope should accept any type of KeyValuePair (Swallow exceptions) (@snakefoot)

### v1.2 (2018/07/20)
- [#223](https://github.com/NLog/NLog.Extensions.Logging/pull/223) BeginScope accepts any type of KeyValuePair and Dictionary  (@snakefoot)


### v1.1 (2018/05/25)
- [#213](https://github.com/NLog/NLog.Extensions.Logging/pull/213) Added support for using NLog MessageTemplate Parser (NLogProviderOptions.ParseMessageTemplates)  (@snakefoot)


### v1.0.2 (2018/05/10)
- [#220](https://github.com/nlog/NLog.Extensions.Logging/pull/220) Exclude NLog 5 pre-releases as dependency  (@aweebs, @304NotModified)
- [#217](https://github.com/nlog/NLog.Extensions.Logging/pull/217) Update NLog dependency,  (@304NotModified)

### v1.0.1 (2018/04/07)
- [#210](https://github.com/nlog/NLog.Extensions.Logging/pull/210) Automatic loading of hidden assemblies for callsite logic in NetCore2 (@snakefoot)
- [#206](https://github.com/nlog/NLog.Extensions.Logging/pull/206) Add Alias attribute to NLogLoggerProvider (@leewadhams)
- [#207](https://github.com/nlog/NLog.Extensions.Logging/pull/207) Update dependencies (@304NotModified)

### v1.0 (2018/03/25)
- [#205](https://github.com/nlog/NLog.Extensions.Logging/pull/205) Release 1.0 (@304NotModified)
- [#204](https://github.com/nlog/NLog.Extensions.Logging/pull/204) CallSite should recognize all Microsoft.Extensions.Logging.ILogger (@snakefoot)
- [#203](https://github.com/nlog/NLog.Extensions.Logging/pull/203) Update NetCore2 ConsoleExample with LogManager.Shutdown (@snakefoot)
- [#200](https://github.com/nlog/NLog.Extensions.Logging/pull/200) ConfigureHiddenAssemblies only needed for NetCoreApp1 (@snakefoot)

### 1.0.0-rtm-rc7 (2018/02/20)
- [#199](https://github.com/nlog/NLog.Extensions.Logging/pull/199) Prepare RC7 (@snakefoot)
- [#194](https://github.com/nlog/NLog.Extensions.Logging/pull/194) MessageProperties - Support list with single property (@snakefoot)
- [#195](https://github.com/nlog/NLog.Extensions.Logging/pull/195) Using LogManager.LoadConfiguration (@snakefoot)

### 1.0.0-rtm-rc6 (2018/01/15)
- [#193](https://github.com/nlog/NLog.Extensions.Logging/pull/193) Support for UAP10/UWP, Update NLog dependency (@304NotModified, @snakefoot)

### 1.0.0-rtm-rc5 (2017/12/21)
- [#186](https://github.com/nlog/NLog.Extensions.Logging/pull/186) RC5 version and changelog (@304NotModified)
- [#185](https://github.com/nlog/NLog.Extensions.Logging/pull/185) Removed System.Runtime for net451 (@snakefoot)
- [#184](https://github.com/nlog/NLog.Extensions.Logging/pull/184) Better hide assemblies for ${callsite} (@304NotModified)
- [#183](https://github.com/nlog/NLog.Extensions.Logging/pull/183) Cleanup #if (@304NotModified)
- [#182](https://github.com/nlog/NLog.Extensions.Logging/pull/182) Remove old examples (@304NotModified)

### 1.0.0-rtm-rc4 (2017/12/04)
- [#181](https://github.com/nlog/NLog.Extensions.Logging/pull/181) Upgrade to NLog 4.5 rc2, remove nlog 5 reference, version to rc4 (@304NotModified)
- [#178](https://github.com/nlog/NLog.Extensions.Logging/pull/178) Move files into component folders (@304NotModified)
- [#180](https://github.com/nlog/NLog.Extensions.Logging/pull/180) NLogMessageParameterList - Refactor validation of parameterList (@snakefoot)
- [#177](https://github.com/nlog/NLog.Extensions.Logging/pull/177) Fix: Captured parameter name has unwanted @ or $ prefix (@304NotModified)
- [#176](https://github.com/nlog/NLog.Extensions.Logging/pull/176) Refactor: split methods, docs, error handling, smaller methodes and consistency  (#176) (@304NotModified)

### 1.0.0-rtm-rc3 (2017/11/30)
- [#174](https://github.com/nlog/NLog.Extensions.Logging/pull/174) 1.0.0-rtm-rc3 + changelog (@304NotModified)
- [#173](https://github.com/nlog/NLog.Extensions.Logging/pull/173) Fix callsite (@304NotModified)
- [#171](https://github.com/nlog/NLog.Extensions.Logging/pull/171) Implement .NET standard 2 features for .NET 4.6.1 + update dependencies + messagetemplates (@snakefoot)
- [#162](https://github.com/nlog/NLog.Extensions.Logging/pull/162) Auto patch file version on AppVeyor (@304NotModified)
- [#167](https://github.com/nlog/NLog.Extensions.Logging/pull/167) Update NetCore2 ConsoleExample to include the minimumlevel. (@seblucas)


### 1.0.0-rtm-rc2 (2017/10/19)

#### Fixes

- [#159](https://github.com/nlog/NLog.Extensions.Logging/pull/159) Fix Recursive resource lookup bug (@304NotModified)
- [#158](https://github.com/nlog/NLog.Extensions.Logging/pull/158) Update dependencies version range (@304NotModified)


### 1.0.0-rtm-rc1 (2017/10/16)

####  breaking changes:
Breaking changes in behavior!


- [#154](https://github.com/nlog/NLog.Extensions.Logging/pull/154) Enable by default: CaptureMessageTemplates & CaptureMessageProperties (@304NotModified)
- [#151](https://github.com/nlog/NLog.Extensions.Logging/pull/151) NLogProviderOptions: added IgnoreEmptyEventId, enabled by default. (@snakefoot)
- [#151](https://github.com/nlog/NLog.Extensions.Logging/pull/151) NLogProviderOptions: set default EventIdSeparator to underscore. (@snakefoot)


#### Features
  
- [#125](https://github.com/nlog/NLog.Extensions.Logging/pull/125) Capture structured logging parameter names (@snakefoot)
- [#148](https://github.com/nlog/NLog.Extensions.Logging/pull/148) MessageProperties - Allow capture of message properties (CaptureMessageProperties option) (@snakefoot)
- [#152](https://github.com/nlog/NLog.Extensions.Logging/pull/152) Introduced NetStandard1.5 by updating to NLog 4.5-beta07 (@snakefoot)

#### Other

- [#143](https://github.com/nlog/NLog.Extensions.Logging/pull/143) Added Console Example to git (@304NotModified)
- [#142](https://github.com/nlog/NLog.Extensions.Logging/pull/142) Enabled Travis build (@304NotModified)

### v1.0.0-rtm-beta7 (2017/09/29)
- [#138](https://github.com/nlog/NLog.Extensions.Logging/pull/138) 1.0.0-rtm-beta7 release notes and version (@304NotModified)
- [#141](https://github.com/nlog/NLog.Extensions.Logging/pull/141) Enable XML docs + docs (@304NotModified)
- [#137](https://github.com/nlog/NLog.Extensions.Logging/pull/137) Cleanup, update docs (@304NotModified)
- [#136](https://github.com/nlog/NLog.Extensions.Logging/pull/136) Support .NET 4.6.1 + Microsoft.Extensions.Logging 2.0, running tests for .NET 4.6.1, coreapp1.1 and coreapp2.0 (@304NotModified)
- [#134](https://github.com/nlog/NLog.Extensions.Logging/pull/134) remove unneeded version prefix (@304NotModified)
- [#135](https://github.com/nlog/NLog.Extensions.Logging/pull/135) Added NLogLoggerFactory (@304NotModified)
