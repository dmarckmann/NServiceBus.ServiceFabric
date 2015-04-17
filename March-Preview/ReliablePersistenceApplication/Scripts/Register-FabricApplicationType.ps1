<#
.SYNOPSIS 
Registers a Windows Fabric application type on a cluster.

.DESCRIPTION
This script registers a Windows Fabric application type on a cluster.  It is invoked by Visual Studio when executing the "Register Type" command of a Windows Fabric Application project.

.NOTES
WARNING: This script file is invoked by Visual Studio.  Its parameters must not be altered but its logic can be customized as necessary.

.PARAMETER ParameterFile
Path to the file containing script parameters.

.PARAMETER ApplicationPackagePath
Path to the folder of the packaged Windows Fabric application.
#>

[CmdletBinding()]
Param
(
    [Parameter(ParameterSetName="ParameterFile", Mandatory=$true)]
    [String]
    $ParameterFile,

    [Parameter(Mandatory=$true)]
    [String]
    $ApplicationPackagePath
)

$LocalFolder = (Split-Path $MyInvocation.MyCommand.Path)

if (!$ApplicationPackagePath)
{
    $ApplicationPackagePath = "$LocalFolder\..\"
}

$ApplicationManifestPath = "$ApplicationPackagePath\ApplicationManifest.xml"

if (!(Test-Path $ApplicationManifestPath))
{
    throw "$ApplicationManifestPath is not found. You may need to create a package by 'Build', 'Deploy' or 'Package'."
}

$UtilitiesModulePath = "$LocalFolder\Utilities.psm1"
Import-Module $UtilitiesModulePath

if ($PsCmdlet.ParameterSetName -eq "ParameterFile")
{
    $Parameters = Read-ParameterFile $ParameterFile
}

Write-Host 'Registering application type...'

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

$applicationPackagePathInImageStore = $names.ApplicationTypeName
$tmpPackagePath = Copy-Temp $ApplicationPackagePath $names.ApplicationTypeName

# Get image store connection string
$clusterManifestText = Get-WindowsFabricClusterManifest
$imageStoreConnectionString = Get-ImageStoreConnectionString ([xml] $clusterManifestText)

Copy-WindowsFabricApplicationPackage -ApplicationPackagePath $tmpPackagePath -ImageStoreConnectionString $imageStoreConnectionString -ApplicationPackagePathInImageStore $applicationPackagePathInImageStore

Register-WindowsFabricApplicationType -ApplicationPathInImageStore $applicationPackagePathInImageStore

Write-Host "Finished registering the application type"