[CmdletBinding(PositionalBinding=$false)]
param(
    [bool] $CreatePackages,
    [bool] $RunTests = $true,
    [int] $BuildNumber, 
    [bool] $SourceLinkEnable = $false
)

$ErrorActionPreference = "Stop"

if(!(Test-Path "version.props"))
{
    Write-Host "You are missing version.props, I need it please"
    Exit
}

function Invoke-ExpressionEx($expression) {
    Invoke-Expression $expression
    if ($LastExitCode -ne 0) { 
        Write-Host "Error encountered, aborting." -Foreground "Red"
        Exit 1
    }
}

$stdSwitches = " /nologo /verbosity:q /p:BuildNumber=$BuildNumber"
if($SourceLinkEnable)
{
    $stdSwitches += " /p:CI=true"
}

$versionProps = [xml](Get-Content "version.props")
$versionPrefix = $versionProps.Project.PropertyGroup.VersionPrefix
$versionSuffix = $versionProps.Project.PropertyGroup.VersionSuffix

Write-Host "Run Parameters:" -ForegroundColor Cyan
Write-Host "Version: $versionPrefix"
Write-Host "Version Suffix: $versionSuffix"
Write-Host "BuildNumber: $BuildNumber"
Write-Host "CreatePackages: $CreatePackages"
Write-Host "RunTests: $RunTests"
<<<<<<< HEAD
Write-Host "Base Version: $(CalculateVersion)"

$packageOutputFolder = "$PSScriptRoot\.nupkgs"
$projectsToBuild =
    'SqlKata.QueryBuilder'

$testsToRun =
    'SqlKata.QueryBuilder.Tests'    

if (!$Version -and !$BuildNumber) {
    Write-Host "ERROR: You must supply either a -Version or -BuildNumber argument. `
  Use -Version `"4.0.0`" for explicit version specification, or `
  Use -BuildNumber `"12345`" for generation using <semver.txt>-<buildnumber>" -ForegroundColor Yellow
    Exit 1
}

if ($PullRequestNumber) {
    Write-Host "Building for a pull request (#$PullRequestNumber), skipping packaging." -ForegroundColor Yellow
    $CreatePackages = $false
}
=======
>>>>>>> update-build-scripts

Write-Host "Cleaning" -ForegroundColor "Magenta"
Invoke-ExpressionEx ('dotnet msbuild /t:Clean' + $stdSwitches)

Write-Host "Restoring" -ForegroundColor "Magenta"
Invoke-ExpressionEx ('dotnet msbuild /t:Restore' + $stdSwitches)

Write-Host "Building" -ForegroundColor "Magenta"
Invoke-ExpressionEx ('dotnet msbuild /t:Build' + $stdSwitches)

# Tests should really be in their own subfolder but oh well
$testProjects = Get-ChildItem -Path $PSScriptRoot -Filter "*Tests.csproj" -Recurse
Write-Host "Testing (found " -NoNewLine -ForegroundColor "Magenta"
Write-Host ("{0}" -f $testProjects.Count) -ForegroundColor "Yellow" -NoNewLine
Write-Host " test projects)" -ForegroundColor "Magenta"

foreach($testProject in  $testProjects)
{
    Invoke-ExpressionEx ("dotnet test /nologo -v q --no-restore --no-build "+$testProject.FullName)
}

Write-Host "Success! :D" -ForegroundColor "Green"
Exit 0
