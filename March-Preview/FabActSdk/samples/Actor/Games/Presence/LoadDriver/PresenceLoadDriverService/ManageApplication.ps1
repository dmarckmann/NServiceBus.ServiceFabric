param
(
	[string] $EndPoints,
	[string] $ImageStoreConnectionString,
	[switch] $Clean,
	[switch] $Deploy,
    [string] $AppPackageFolder,
    [string] $GetReplicas
)

$AppPrefix = "PresenceLoadDriver"
$AppPackageName = $AppPrefix + "AppPackage"
$AppName = "fabric:/" + $AppPrefix + "App"
$AppTypeName = $AppPrefix + "AppType"
$AppTypeVersion = "1.0"


function Get-ScriptDirectory 
{ 
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value 
    Split-Path $Invocation.MyCommand.Path 
}

if (!$AppPackageFolder.Length)
{
    $AppPackageFolder = (Get-ScriptDirectory)
    $AppPackageFolder = $AppPackageFolder + "\PackageRoot";
}


if (!$ImageStoreConnectionString.Length)
{
	$ImageStoreConnectionString = "fabric:ImageStore";
}

if (!$Endpoints.Length)
{
    Connect-WindowsFabricCluster
}
else
{
    Connect-WindowsFabricCluster $EndPoints
}

if ($Clean)
{
    Write-Output "Removing Application $AppName"
    Remove-WindowsFabricApplication $AppName -force

    Write-Output "Unregistering ApplicationType $AppTypeName $AppTypeVersion"
    Unregister-WindowsFabricApplicationType $ApptypeName $AppTypeVersion -force
}

if ($Deploy)
{
    Write-Output "Copying Application Package $AppPackageFolder\$AppPackageName to ImageStore $ImageStoreConnectionString"
	Copy-WindowsFabricApplicationPackage -ApplicationPackagePath $AppPackageFolder\$AppPackageName -ImageStoreConnectionString $ImageStoreConnectionString
    
    Write-Output "Registering ApplicationType $AppTypeName $AppTypeVersion"
    Register-WindowsFabricApplicationType $AppPackageName
	
    Get-WindowsFabricApplicationType $AppTypeName
    
    Write-Output "Creating Application $AppName"
    New-WindowsFabricApplication $AppName $AppTypeName $AppTypeVersion
}

if ($GetReplicas)
{
    Get-WindowsFabricApplication $AppName | Get-WindowsFabricService | Get-WindowsFabricPartition | Get-WindowsFabricReplica 
}
