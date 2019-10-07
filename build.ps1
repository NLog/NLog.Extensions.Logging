# restore and builds all projects as release.
# creates NuGet package at \artifacts
dotnet --version

$versionPrefix = "1.6.0"
$versionSuffix = ""
$versionFile = $versionPrefix + "." + ${env:APPVEYOR_BUILD_NUMBER}
$versionProduct = $versionPrefix;
if (-Not $versionSuffix.Equals(""))
	{ $versionProduct = $versionProduct + "-" + $versionSuffix }

dotnet restore .\src\NLog.Extensions.Logging\
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

dotnet restore .\src\NLog.Extensions.Hosting\
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

msbuild /t:Pack .\src\NLog.Extensions.Logging\ /p:targetFrameworks='"net451;net461;netstandard1.3;netstandard1.5;netstandard2.0;netcoreapp3.0"' /p:VersionPrefix=$versionPrefix /p:VersionSuffix=$versionSuffix /p:FileVersion=$versionFile /p:ProductVersion=$versionProduct /p:Configuration=Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg /p:PackageOutputPath=..\..\artifacts /verbosity:minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

msbuild /t:Pack .\src\NLog.Extensions.Hosting\ /p:targetFrameworks='"netstandard2.0;netcoreapp3.0"' /p:VersionPrefix=$versionPrefix /p:VersionSuffix=$versionSuffix /p:FileVersion=$versionFile /p:ProductVersion=$versionProduct /p:Configuration=Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg /p:PackageOutputPath=..\..\artifacts /verbosity:minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

exit $LastExitCode
