## Change Log

Date format: (year/month/day)

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
