[CmdletBinding(PositionalBinding=$false)]
param(
    [int]$BuildNumber,
    [bool]$nobuild=$true,
    [bool]$norestore=$true
)

$ErrorActionPreference = "Stop"
$OutputDirectory = "$PSScriptRoot\.nupkgs"

if(!(Test-Path "version.props"))
{
    Write-Host "You are missing version.props >;|"
    Exit
}

function Invoke-ExpressionEx($expression) {
    Invoke-Expression $expression | Out-Null
    if ($LastExitCode -ne 0) { 
        Write-Host "Error encountered, aborting." -Foreground "Red"
        Exit 1
    }
}

foreach($nuPackage in (Get-ChildItem -Path $OutputDirectory -Filter "*.nupkg" -Recurse))
{
    Remove-Item -Path $nuPackage.FullName -Force
}

$cmd = "dotnet pack /nologo /verbosity:q /p:PackageOutputPath=`"$OutputDirectory`" /p:BuildNumber=$BuildNumber"
Write-Host $cmd

if($nobuild)
{
    $cmd += " --no-build"
}
if($norestore)
{
    $cmd += " --no-restore"
}

Invoke-ExpressionEx $cmd

Write-Host "Zoom zoom and a boom boom ;)" -Foreground "Green"
