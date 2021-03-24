$projectFile = "src\NLog.Extensions.Logging\NLog.Extensions.Logging.csproj"
$sonarQubeId = "nlog.extensions.logging"
$github = "nlog/NLog.Extensions.Logging"
$baseBranch = "master"
$framework = "netstandard2.0"
$sonarOrg = "nlog"


if ($env:APPVEYOR_REPO_NAME -eq $github) {

    if (-not $env:sonar_token) {
        Write-warning "Sonar: not running SonarQube, no sonar_token"
        return;
    }
 
    $prMode = $false;
    $branchMode = $false;
     
    if ($env:APPVEYOR_PULL_REQUEST_NUMBER) { 
        # first check PR as that is on the base branch
        $prMode = $true;
        Write-Output "Sonar: on PR $env:APPVEYOR_PULL_REQUEST_NUMBER"
    }
    elseif ($env:APPVEYOR_REPO_BRANCH -eq $baseBranch) {
        Write-Output "Sonar: on base branch ($baseBranch)"
    }
    else {
        $branchMode = $true;
        Write-Output "Sonar: on branch $env:APPVEYOR_REPO_BRANCH"
    }

    dotnet tool install --global dotnet-sonarscanner
    if (-Not $LastExitCode -eq 0) {
        exit $LastExitCode 
    }

    $sonarUrl = "https://sonarcloud.io"
    $sonarToken = $env:sonar_token
    $buildVersion = $env:APPVEYOR_BUILD_VERSION

    dotnet-sonarscanner begin /o:"$sonarOrg" /k:"$sonarQubeId" /d:"sonar.host.url=$sonarUrl" /d:"sonar.login=$sonarToken" /v:"$buildVersion" /d:"sonar.cs.opencover.reportsPaths=coverage.xml" /d:"sonar.github.repository=$github" /d:"sonar.github.oauth=$env:github_auth_token"
    if (-Not $LastExitCode -eq 0) {
        exit $LastExitCode 
    }

    msbuild /t:Rebuild $projectFile /p:targetFrameworks=$framework /verbosity:minimal
    if (-Not $LastExitCode -eq 0) {
        exit $LastExitCode 
    }

    dotnet-sonarscanner end /d:"sonar.login=$env:sonar_token"
    if (-Not $LastExitCode -eq 0) {
        exit $LastExitCode 
    }
}
else {
    Write-Output "Sonar: not running as we're on '$env:APPVEYOR_REPO_NAME'"
}
