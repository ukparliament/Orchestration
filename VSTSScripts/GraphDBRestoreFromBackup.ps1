<#
.SYNOPSIS
Restore GraphDB from backup.

.DESCRIPTION
Restore GraphDB from backup.

.PARAMETER OrchestrationResourceGroupName
Name of the Resource Group where the Azure Functions are.

.PARAMETER AzureFunctionsName
Name of the Azure Functions.

.PARAMETER BackupUrl
Url to backup file (trig format).

.NOTES
This script is for use as a part of deployment in VSTS only.
#>

Param(
	[Parameter(Mandatory=$true)] [string] $APIResourceGroupName,
	[Parameter(Mandatory=$true)] [string] $APIManagementName,
	[Parameter(Mandatory=$true)] [string] $ReleaseProductId,
	[Parameter(Mandatory=$true)] [string] $APIPrefix,
    [Parameter(Mandatory=$true)] [string] $OrchestrationResourceGroupName,
    [Parameter(Mandatory=$true)] [string] $AzureFunctionsName,
	[Parameter(Mandatory=$true)] [string] $BackupUrl,
    [Parameter(Mandatory=$true)] [string] $PowershellModuleDirectory
)
$ErrorActionPreference = "Stop"

Import-Module -Name $PowershellModuleDirectory\Write-LogToHost.psm1
Import-Module -Name $PowershellModuleDirectory\Get-APIMSubscriptionKey.psm1

$apiBaseUrl="https://api.parliament.uk"

Write-LogToHost "Retrive trigger code for $AzureFunctionsName"
$properties=Invoke-AzureRmResourceAction -ResourceGroupName $OrchestrationResourceGroupName -ResourceType Microsoft.Web/sites/config -ResourceName "$AzureFunctionsName/publishingcredentials" -Action list -ApiVersion 2015-08-01 -Force
$base64Info=[Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $properties.properties.publishingUserName,$properties.properties.publishingPassword)))
$masterKeyResponse=Invoke-RestMethod -Uri "https://$AzureFunctionsName.scm.azurewebsites.net/api/functions/admin/masterkey" -Headers @{Authorization=("Basic {0}" -f $base64Info)} -Method GET

Write-LogToHost "Restore data from $BackupUrl"
Invoke-RestMethod -Uri "https://$AzureFunctionsName.azurewebsites.net/api/GraphDBRestore?code=$($masterKeyResponse.masterKey)" -Method Post -ContentType "application/json" -Body (ConvertTo-Json @{backupUrl="$BackupUrl"}) -TimeoutSec 30 -Verbose

$subscriptionKey=Get-APIMSubscriptionKey -APIResourceGroupName $APIResourceGroupName -APIManagementName $APIManagementName -ProductId $ReleaseProductId

$header=@{"Ocp-Apim-Subscription-Key"="$subscriptionKey";"Api-Version"="$APIPrefix"}

function Get-JMXAttribute([Parameter(Mandatory=$true)][string]$AttributeName){
    Log "Gets $AttributeName on master"
    $bodyTxt=@{
        "type"="read";
        "mbean"= "ReplicationCluster:name=ClusterInfo/Master";
        "attribute"= "$AttributeName";
    }
    $bodyJson=ConvertTo-Json $bodyTxt
    $response=Invoke-RestMethod -Uri "$apiBaseUrl/jmx" -Method Post -ContentType "application/json" -Body $bodyJson -Headers $header -TimeoutSec 15
    $response.value
}

Write-LogToHost "Wait 30sec"
Start-Sleep -Seconds 30

$result=1
$counter=0;
while($result -ne 0) {
    Write-LogToHost "Counter $counter"
    $status=Get-JMXAttribute -AttributeName "NumberOfTriples"
    if ($status -gt 223){
        Write-LogToHost "Number of triples: $status"
        break
    }
	if ($status -le 223){
        Write-LogToHost "Wait 30 seconds, response ($status)"
        $counter++
        Start-Sleep -Seconds 30
    }
    if (($counter -gt 30) -and ($status -le 223)) {
        throw "Invalid number of triples ($status)"
    }
}


Write-LogToHost "Job well done!"