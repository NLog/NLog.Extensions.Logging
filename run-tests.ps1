dotnet restore test/NLog.Extensions.Logging.Tests -v minimal -p:DisableImplicitNuGetFallbackFolder=true
dotnet restore test/NLog.Extensions.Hosting.Tests -v minimal -p:DisableImplicitNuGetFallbackFolder=true
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

exit $LastExitCode
