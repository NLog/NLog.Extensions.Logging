dotnet restore test/NLog.Extensions.Logging.Tests -v minimal
dotnet restore test/NLog.Extensions.Hosting.Tests -v minimal

dotnet build test/NLog.Extensions.Logging.Tests  --configuration release -v minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

dotnet build test/NLog.Extensions.Hosting.Tests  --configuration release -v minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

dotnet test test/NLog.Extensions.Logging.Tests  --configuration release
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

dotnet test test/NLog.Extensions.Hosting.Tests  --configuration release
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

dotnet restore
dotnet list ./ package --vulnerable --include-transitive | findstr /S /c:"has the following vulnerable packages"
if (-Not $LastExitCode -eq 1)
{
	dotnet list ./ package --vulnerable --include-transitive
	exit 1
}

dotnet publish -r win-x64 -c release --self-contained .\examples\NetCore2\HostingExample
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }