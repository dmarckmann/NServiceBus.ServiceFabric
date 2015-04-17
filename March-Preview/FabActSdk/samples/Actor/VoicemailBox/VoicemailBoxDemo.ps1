param
(
    [string] $EndPoints,
    [string] $ImageStoreConnectionString,
    [string] $DemoFolder,
    [switch] $Clean,
    [switch] $Upgrade,
    [switch] $Downgrade,
    [switch] $Deploy,
    [switch] $GetUpgradeProgress,
    [switch] $GetStatus
)

function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value
    Split-Path $Invocation.MyCommand.Path
}

if (!$DemoFolder.Length)
{
    $DemoFolder = (Get-ScriptDirectory)
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

. $DemoFolder\V1\_ActorApplicationVariables.ps1
$FabActSDkPath = $DemoFolder + "\..\..\..\bin"
Add-Type -Path $FabActSDkPath\System.Fabric.Services.dll

$AppParameters = @{}

if ($Deploy)
{
    Copy-WindowsFabricApplicationPackage -ApplicationPackagePath $DemoFolder\V1\PackageRoot\$AppPackageName -ImageStoreConnectionString $ImageStoreConnectionString
    Register-WindowsFabricApplicationType $AppPackageName

    Copy-WindowsFabricApplicationPackage -ApplicationPackagePath $DemoFolder\V2\PackageRoot\$AppPackageName -ImageStoreConnectionString $ImageStoreConnectionString
    Register-WindowsFabricApplicationType $AppPackageName

    Get-WindowsFabricApplicationType $AppTypeName
    New-WindowsFabricApplication $AppName $AppTypeName 1.0.0.0 -ApplicationParameter $AppParameters
}

if ($Clean)
{
    Remove-WindowsFabricApplication $AppName -force
    Get-WindowsFabricApplicationType $AppTypeName | Unregister-WindowsFabricApplicationType -force
    Write-Output "Demo Application and Application Types cleaned from the cluster."
}

if ($Upgrade)
{
    Start-WindowsFabricApplicationUpgrade -ApplicationName $AppName -ApplicationTypeVersion 2.0.0.0 -UnmonitoredAuto -ApplicationParameter $AppParameters
}

if ($GetUpgradeProgress)
{
    Get-WindowsFabricApplicationUpgrade $AppName
}

if ($Downgrade)
{
   Start-WindowsFabricApplicationUpgrade -ApplicationName $AppName -ApplicationTypeVersion 1.0.0.0 -UnmonitoredAuto -ApplicationParameter $AppParameters
}

if ($GetStatus)
{
    Get-WindowsFabricApplication | Get-WindowsFabricService | Get-WindowsFabricPartition | Get-WindowsFabricReplica
}
