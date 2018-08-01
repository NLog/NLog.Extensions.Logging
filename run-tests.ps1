dotnet restore test/NLog.Extensions.Logging.Tests/NLog.Extensions.Logging.Tests.csproj -v minimal
dotnet build test/NLog.Extensions.Logging.Tests/NLog.Extensions.Logging.Tests.csproj  --configuration release -v minimal
dotnet test test/NLog.Extensions.Logging.Tests/NLog.Extensions.Logging.Tests.csproj  --configuration release

exit $LASTEXITCODE