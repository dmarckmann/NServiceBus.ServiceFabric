<#
.SYNOPSIS 
Deploys a Windows Fabric application type to a cluster.

.DESCRIPTION
This script deploys a Windows Fabric application type to a cluster.  It is invoked by Visual Studio when deploying a Windows Fabric Application project.

.NOTES
WARNING: This script file is invoked by Visual Studio.  Its parameters must not be altered but its logic can be customized as necessary.

.PARAMETER ParameterFile
Path to the file containing script parameters.

.PARAMETER ApplicationPackagePath
Path to the folder of the packaged Windows Fabric application.

.PARAMETER DoNotCreateApplication
Indicates that the Windows Fabric application should not be created after registering the application type.

.PARAMETER ApplicationParameter
Hashtable of the Windows Fabric application parameters to be used for the application.
#>

[CmdletBinding()]
Param
(
    [Parameter(ParameterSetName="ParameterFile", Mandatory=$true)]
    [String]
    $ParameterFile,

    [Parameter(Mandatory=$true)]
    [String]
    $ApplicationPackagePath,

    [Switch]
    $DoNotCreateApplication,

    [Hashtable]
    $ApplicationParameter
)

$UtilitiesModulePath = (Split-Path $MyInvocation.MyCommand.Path) + "\Utilities.psm1"
Import-Module $UtilitiesModulePath

if ($PsCmdlet.ParameterSetName -eq "ParameterFile")
{
    $Parameters = Read-ParameterFile $ParameterFile
}

$ApplicationManifestPath = "$ApplicationPackagePath\ApplicationManifest.xml"

if (!(Test-Path $ApplicationManifestPath))
{
    throw "$ApplicationManifestPath is not found. You may need to create a package by 'Build' or 'Package'."
}

Write-Host 'Publishing application...'

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

$tmpPackagePath = Copy-Temp $ApplicationPackagePath $names.ApplicationTypeName
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

Copy-WindowsFabricApplicationPackage -ApplicationPackagePath $tmpPackagePath -ImageStoreConnectionString $imageStoreConnectionString -ApplicationPackagePathInImageStore $applicationPackagePathInImageStore

Register-WindowsFabricApplicationType -ApplicationPathInImageStore $applicationPackagePathInImageStore

if (!$DoNotCreateApplication)
{
    [void](New-WindowsFabricApplication -ApplicationName $names.ApplicationName -ApplicationTypeName $names.ApplicationTypeName -ApplicationTypeVersion $names.ApplicationTypeVersion -ApplicationParameter $ApplicationParameter)
    Write-Host 'Create application succeeded'
}