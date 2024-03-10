$Path = "$( $PSScriptRoot )/version.txt"
$values = Get-Content $Path | Out-String | ConvertFrom-StringData
#$values.Version
#$values.Package

$projects = @(
"QueryBuilder",
"SqlKata.Execution"
)

Function UpdateVersion($csprojPath)
{
    "File: " + $csprojPath | Write-Host
    [xml]$xml = Get-Content $csprojPath

    if ($xml.Project.PropertyGroup.GetType().ToString() -eq "System.Xml.XmlElement")
    {
        $group = $xml.Project.PropertyGroup
    }
    else
    {
        $group = $xml.Project.PropertyGroup[0]
    }

    if ($null -ne $group.Version -and !$values.Package.equals($group.Version))
    {
        "Version: " + $group.Version + " -> " + $values.Package | Write-Host
        $group.Version = $values.Package
    }

    if ($null -ne $group.AssemblyVersion -and !$values.Version.equals($group.AssemblyVersion))
    {
        "AssemblyVersion: " + $group.AssemblyVersion + " -> " + $values.Version | Write-Host
        $group.AssemblyVersion = $values.Version
    }

    if ($null -ne $group.FileVersion -and !$values.Version.equals($group.FileVersion))
    {
        "FileVersion: " + $group.FileVersion + " -> " + $values.Version | Write-Host
        $group.FileVersion = $values.Version
    }

    if ($null -ne $group.Copyright)
    {
        $group.Copyright = "OptimaJet Workflow Engine " + (Get-Date).Year
    }

    if ($null -ne $group.Product)
    {
        $group.Product = "Data Engine"
    }

    $PackageReference = $xml.Project.ItemGroup.PackageReference
    
    foreach ($package in $PackageReference)
    {
        if ($null -eq $package.Include -and $null -eq $package.Version)
        {
            continue;
        }

        $packName = $package.Include.ToString()
        $packVersion = $package.Version.ToString()

        foreach ($project in $projects)
        {
            if ($packName.equals($project) -and !$packVersion.equals($values.Package))
            {
                $packName + " " + $packVersion + " -> " + $values.Package | Write-Host
                $package.Version = $values.Package
                break
            }
        }
    }

    $xml.Save($csprojPath);
    "Saved: " + $csprojPath | Write-Host
}

"Updating projects" | Write-Host
"---------------------------------------" | Write-Host

foreach ($project in $projects)
{
    UpdateVersion([IO.Path]::Combine($PSScriptRoot, "..", $project, $project + ".csproj"))
}