<#
.SYNOPSIS 
Unregisters a Windows Fabric application type on a cluster.

.DESCRIPTION
This script unregisters a Windows Fabric application type on a cluster.  It is invoked by Visual Studio when executing the "Unregister Type" command of a Windows Fabric Application project.

.NOTES
WARNING: This script file is invoked by Visual Studio.  Its parameters must not be altered but its logic can be customized as necessary.

.PARAMETER ParameterFile
Path to the file containing script parameters.

.PARAMETER ApplicationManifestPath
Path to the application manifest of the Windows Fabric application.
#>

[CmdletBinding()]
Param
(
    [Parameter(ParameterSetName="ParameterFile", Mandatory=$true)]
    [String]
    $ParameterFile,

    [String]
    $ApplicationManifestPath
)

$LocalFolder = (Split-Path $MyInvocation.MyCommand.Path)

if (!$ApplicationManifestPath)
{
    $ApplicationManifestPath = "$LocalFolder\..\ApplicationManifest.xml"
}

if (!(Test-Path $ApplicationManifestPath))
{
    throw "$ApplicationManifestPath is not found."
}

$UtilitiesModulePath = "$LocalFolder\Utilities.psm1"
Import-Module $UtilitiesModulePath

if ($PsCmdlet.ParameterSetName -eq "ParameterFile")
{
    $Parameters = Read-ParameterFile $ParameterFile
}

Write-Host 'Unregistering application type...'

try
{
    [void](Connect-WindowsFabricCluster)
}
catch [System.Fabric.FabricObjectClosedException]
{
    Write-Warning "Windows Fabric cluster may not be connected."
    throw
}

# Get image store connection string
$clusterManifestText = Get-WindowsFabricClusterManifest
$imageStoreConnectionString = Get-ImageStoreConnectionString ([xml] $clusterManifestText)

$names = Get-Names -ApplicationManifestPath $ApplicationManifestPath
if (!$names)
{
    return
}

$applicationPackagePathInImageStore = $names.ApplicationTypeName

$reg = Get-WindowsFabricApplicationType -ApplicationTypeName $names.ApplicationTypeName
if ($reg)
{
    $reg | Unregister-WindowsFabricApplicationType -Force
}

Remove-WindowsFabricApplicationPackage -ApplicationPackagePathInImageStore $applicationPackagePathInImageStore -ImageStoreConnectionString $imageStoreConnectionString

Write-Host "Finished unregistering the application type"