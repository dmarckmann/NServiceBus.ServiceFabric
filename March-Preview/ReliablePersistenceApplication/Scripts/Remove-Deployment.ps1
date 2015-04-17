<#
.SYNOPSIS 
Removes the deployment of a Windows Fabric application on a cluster.

.DESCRIPTION
This script removes the deployment of a Windows Fabric application on a cluster.  It is invoked by Visual Studio when executing the "Remove Deployment" command of a Windows Fabric Application project.

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

Write-Host "Removing deployment..."

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

$app = Get-WindowsFabricApplication -ApplicationName $names.ApplicationName
if ($app)
{
    $app | Remove-WindowsFabricApplication -Force
}

$reg = Get-WindowsFabricApplicationType -ApplicationTypeName $names.ApplicationTypeName
if ($reg)
{
    $reg | Unregister-WindowsFabricApplicationType -Force
}

Remove-WindowsFabricApplicationPackage -ApplicationPackagePathInImageStore $applicationPackagePathInImageStore -ImageStoreConnectionString $imageStoreConnectionString

Write-Host "Finished removing the deployment"