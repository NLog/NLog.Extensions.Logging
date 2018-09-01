dotnet restore test/NLog.Extensions.Logging.Tests -v minimal
dotnet restore test/NLog.Extensions.Hosting.Tests -v minimal
dotnet build test/NLog.Extensions.Logging.Tests  --configuration release -v minimal
dotnet build test/NLog.Extensions.Hosting.Tests  --configuration release -v minimal
dotnet test test/NLog.Extensions.Logging.Tests  --configuration release
dotnet test test/NLog.Extensions.Hosting.Tests  --configuration release

exit $LASTEXITCODE