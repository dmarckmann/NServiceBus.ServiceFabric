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
    [switch] $GetStatus,
    [switch] $ShowActorMap
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

if ($Clean)
{
    Remove-WindowsFabricApplication $AppName -force
    Get-WindowsFabricApplicationType $AppTypeName | Unregister-WindowsFabricApplicationType -Force
    Write-Output "Demo Application and Application Types cleaned from the cluster."

    Remove-WindowsFabricService -ServiceName fabric:/VisualObjectsWebApp/VisualObjectsWebService -Force
    Remove-WindowsFabricApplication -ApplicationName fabric:/VisualObjectsWebApp -Force
    Unregister-WindowsFabricApplicationType -ApplicationTypeName VisualObjectsWebApp -ApplicationTypeVersion 1.0 -Force
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

if ($Deploy)
{
    Copy-WindowsFabricApplicationPackage -ApplicationPackagePath $DemoFolder\V1\PackageRoot\$AppPackageName -ImageStoreConnectionString $ImageStoreConnectionString
    Register-WindowsFabricApplicationType $AppPackageName

    Copy-WindowsFabricApplicationPackage -ApplicationPackagePath $DemoFolder\V2\PackageRoot\$AppPackageName -ImageStoreConnectionString $ImageStoreConnectionString
    Register-WindowsFabricApplicationType $AppPackageName

    Get-WindowsFabricApplicationType $AppTypeName
    New-WindowsFabricApplication $AppName $AppTypeName 1.0.0.0 -ApplicationParameter $AppParameters

    Copy-WindowsFabricApplicationPackage -ApplicationPackagePath $DemoFolder\VisualObjectsWebService\bin\VisualObjectsWebApp -ImageStoreConnectionString $ImageStoreConnectionString
    Register-WindowsFabricApplicationType VisualObjectsWebApp
    New-WindowsFabricApplication -ApplicationName fabric:/VisualObjectsWebApp -ApplicationTypeName VisualObjectsWebApp -ApplicationTypeVersion 1.0
}

if ($GetStatus)
{
    Get-WindowsFabricApplication | Get-WindowsFabricService | Get-WindowsFabricPartition | Get-WindowsFabricReplica 
}

if ($ShowActorMap)
{
    $ActorServiceUri = $ActorServiceUriMap["IVisualObject"]
    $actorNodeMappings = @()
    $actorIdStrings = @( "Visual Object # 0", "Visual Object # 1", "Visual Object # 2", "Visual Object # 3", "Visual Object # 4" )

    foreach($actorIdString in $actorIdStrings)
    {
        $aId = New-Object System.Fabric.Actors.ActorId $actorIdString
        $partitionKey = $aId.GetPartitionKey();

        $rsp = Resolve-WindowsFabricService -PartitionKindUniformInt64 -ServiceName $ActorServiceUri -PartitionKey $partitionKey
        $replicas = Get-WindowsFabricReplica $rsp.PartitionId | where {($_.ReplicaRole -eq "Primary")}

        if ($replicas.Count -gt 0)
        {
            $objReplica = $replicas[0]
            $objReplica | Add-Member -type NoteProperty -name ActorId -value $actorIdString
            $actorNodeMappings += $objReplica

            Try
            {
                $DeployedCodePackage = Get-WindowsFabricDeployedCodePackage -NodeName $objReplica.NodeName -ApplicationName $AppName -CodePackageName Code -ServiceManifestName VisualObjectsActorServicePkg
                $objReplica | Add-Member -type NoteProperty -name ProcessId -value $DeployedCodePackage.EntryPoint.ProcessId
            }
            Catch
            {
                $objReplica | Add-Member -type NoteProperty -name ProcessId -value n/a
            }
        }
    }

    $actorNodeMappings | Sort-Object -Property NodeName| ft -Property ActorId,NodeName,ProcessId,PartitionId,ReplicaId -AutoSize 
}
