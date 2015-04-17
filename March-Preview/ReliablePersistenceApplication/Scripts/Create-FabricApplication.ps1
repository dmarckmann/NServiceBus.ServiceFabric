<#
.SYNOPSIS 
Creates an instance of a Windows Fabric application type.

.DESCRIPTION
This script creates an instance of a Windows Fabric application type.  It is invoked by Visual Studio when executing the "Create Application" command of a Windows Fabric Application project.

.NOTES
WARNING: This script file is invoked by Visual Studio.  Its parameters must not be altered but its logic can be customized as necessary.

.PARAMETER ParameterFile
Path to the file containing script parameters.

.PARAMETER ApplicationManifestPath
Path to the application manifest of the Windows Fabric application.

.PARAMETER ApplicationParameter
Hashtable of the Windows Fabric application parameters to be used for the application.
#>

[CmdletBinding()]
Param
(
    [Parameter(ParameterSetName="ParameterFile", Mandatory=$true)]
    [String]
    $ParameterFile,
    
    [String]
    $ApplicationManifestPath,
    
    [Hashtable]
    $ApplicationParameter
)

$LocalFolder = (Split-Path $MyInvocation.MyCommand.Path)

if (!$ApplicationManifestPath)
{
    $ApplicationManifestPath = "$LocalFolder\..\ApplicationManifest.xml"
}

if (!(Test-Path $ApplicationManifestPath))
{
    throw "$ApplicationManifestPath is not found. You may need to create a package by 'Build' or 'Package'."
}

$UtilitiesModulePath = "$LocalFolder\Utilities.psm1"
Import-Module $UtilitiesModulePath

if ($PsCmdlet.ParameterSetName -eq "ParameterFile")
{
    $Parameters = Read-ParameterFile $ParameterFile
}

try
{
    [void](Connect-WindowsFabricCluster)
}
catch [System.Fabric.FabricObjectClosedException]
{
    Write-Warning "Windows Fabric cluster may not be connected."
    throw
}

$names = Get-Names -ApplicationManifestPath $ApplicationManifestPath
if (!$names)
{
    return
}

New-WindowsFabricApplication -ApplicationName $names.ApplicationName -ApplicationTypeName $names.ApplicationTypeName -ApplicationTypeVersion $names.ApplicationTypeVersion -ApplicationParameter $ApplicationParameter