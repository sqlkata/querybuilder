<#

.SYNOPSIS
SqlKata.QueryBuilder build script


.PARAMETER BuildNumber
The number for this build

.PARAMETER PullRequestNumber
(If Applicable) The pull request number

.PARAMETER RunTests
Switch to enable to execution of tests in the solution

.PARAMETER SourceLinkEnable
Switch to enable a build property for SourceLink

.PARAMETER DebugBuild
Switch to produce a debug build

#>
[CmdletBinding(PositionalBinding=$false)]
param(
    [int] $BuildNumber,
    [int] $PullRequestNumber,
    [switch] $RunTests,
    [switch] $SourceLinkEnable,
    [switch] $DebugBuild
)
$ErrorActionPreference = "Stop"
$msgColor = @{Default="White"; Heading="Cyan"; Danger="Red"; Success="Green"; Attention="Yellow"}
$BuildConfiguration = 'Release'
$DefaultCIBuildSuffix = 'ci'

function Die ($message) {
    Write-Host ">>> ERROR:`t$message`n" -ForegroundColor $msgColor.Danger
    Exit 1
}

function isValidColor($color)
{
    return $msgColor.Values -contains $color
}

function Msg ($message, $color = $msgColor.Default, $newLine=$true)
{
    $valid = isValidColor($color)
    if(!$valid) { $color = $msgColor.Default }

    if($newLine)
    {
        Write-Host $message -ForegroundColor $color
    }
    else
    {
        Write-Host $message -ForegroundColor $color -NoNewline
    }
}

function Done()
{
    Msg "`n>>> Success! :D`n" $msgColor.Success
    Exit 0
}

function Invoke-ExpressionEx($expression) {
    try
    {
        if($DebugBuild -eq $false) { Invoke-Expression $expression | Out-Null }
        else
        {
            Msg "`tInvoking Expression: $expression" $msgColor.Default
            Invoke-Expression $expression
        }
        if (!$? -or $LastExitCode -ne 0)
        {
            throw "Non zero return code: $LastExitCode"
        }
    } catch {
        Die "Error encountered while invoking expression, aborting.`n`t`tExpression: `"$expression`"`n`t`tMessage: `"$PSItem`""
    }
}

Msg "`n>>> SqlKata QueryBuilder Build Script`n" $msgColor.Heading
if($DebugBuild)
{
    Msg "`tDEBUG BUILD" $msgColor.Attention
    $BuildConfiguration = 'Debug'
}
if($BuildNumber -eq 0 -and $PullRequestNumber -eq 0) { Die "Build Number or Pull Request Number must be supplied" }
if(!(Test-Path "version.props")) { Die "Unable to locate required file: version.props" }
$outputPath = "$PSScriptRoot\.nupkgs"
$stdSwitches = " /p:Configuration=$BuildConfiguration /nologo /verbosity:d /p:BuildNumber=$BuildNumber"

if($SourceLinkEnable)
{
    $stdSwitches += " /p:CI=true"
}

$versionProps = [xml](Get-Content "version.props")

if(-not $Env:APPVEYOR_REPRO_TAG)
{
    if([string]::IsNullOrWhiteSpace($versionProps.Project.PropertyGroup.VersionSuffix))
    {
        $versionProps.Project.PropertyGroup.VersionSuffix=$DefaultCIBuildSuffix
        $versionProps.Save("$PSScriptRoot\version.props")
    }
}

$versionPrefix = $versionProps.Project.PropertyGroup.VersionPrefix
$versionSuffix = $versionProps.Project.PropertyGroup.VersionSuffix

Msg "`tRun Parameters:" $msgColor.Attention
Msg "`t`tVersion: $versionPrefix"
Msg "`t`tVersion Suffix: $versionSuffix"
Msg "`t`tBuild Number: $BuildNumber"
Msg "`t`tRun Tests: $RunTests"
Msg "`t`tSourceLink Enable: $SourceLinkEnable`n"

Msg "`tCleaning" $msgColor.Attention
Invoke-ExpressionEx ('dotnet msbuild /t:Clean' + $stdSwitches)

Msg "`tRestoring" $msgColor.Attention
Invoke-ExpressionEx ('dotnet msbuild /t:Restore' + $stdSwitches)

Msg "`tBuilding" $msgColor.Attention
Invoke-ExpressionEx ('dotnet msbuild /t:Build' + $stdSwitches)

if($RunTests)
{
    # Tests should really be in their own subfolder but oh well
    $testProjects = Get-ChildItem -Path $PSScriptRoot -Filter "*Tests.csproj" -Recurse
    Msg "`tTesting (found " $msgColor.Attention $false
    Msg ("{0}" -f $testProjects.Count) $msgColor.Attention $false
    Msg " test projects)" $msgColor.Attention

    foreach($testProject in  $testProjects)
    {
        Msg "`t`t- $testProject" $msgColor.Attention
        Invoke-ExpressionEx ("dotnet test /nologo -v d /p:Configuration=$BuildConfiguration --no-restore --no-build "+$testProject.FullName)
        Msg "`t`t`tOK" $msgColor.Success
    }
}

if($PullRequestNumber -gt 0)
{
    Msg "`tSkipping package creation; Current build is for pull request #$PullRequestNumber" $msgColor.Attention
    Done
}

Msg "`tPackaging" $msgColor.Attention
if(!(Test-Path $outputPath)) { Invoke-ExpressionEx "md $outputPath" }
foreach($nuPackage in (Get-ChildItem -Path $OutputDirectory -Filter "*.nupkg" -Recurse))
{
    Remove-Item -Path $nuPackage.FullName -Force
}

$packCmd = "dotnet pack /nologo /verbosity:d --output=`"$outputPath`" /p:Configuration=$BuildConfiguration /p:BuildNumber=$BuildNumber --no-build --no-restore"
Invoke-ExpressionEx $packCmd
foreach($nuPackage in (Get-ChildItem -Path $OutputDirectory -Filter "*.nupkg" -Recurse))
{
    Msg "`t`t+ $nuPackage" $msgColor.Success
}


Done

