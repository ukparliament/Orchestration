<#
.SYNOPSIS
Sets task variable.

.DESCRIPTION
Sets task variable with callback urls of Logic Apps . Due to the limitation with ARM templates it is splited into series of array variables (converted to json).

.PARAMETER LogicAppsResourceGroupName
Name of the Resource Group where the Logic Apps are.

.PARAMETER LogicAppsNames
Array of names used to generate Logic Apps.

.NOTES
This script is for use as a part of deployment in VSTS only.
#>

Param(
    [Parameter(Mandatory=$true)] [string] $LogicAppsResourceGroupName,
    [Parameter(Mandatory=$true)] [array] $LogicAppsNames
)

$ErrorActionPreference = "Stop"

function Log([Parameter(Mandatory=$true)][string]$LogText){
    Write-Host ("{0} - {1}" -f (Get-Date -Format "HH:mm:ss.fff"), $LogText)
}

$arr=@()
foreach($name in $LogicAppsNames){
    Log "Logic app $name"
    $url=Get-AzureRmLogicAppTriggerCallbackUrl -ResourceGroupName $LogicAppsResourceGroupName -Name "getlist-$name" -TriggerName "manual"
    $arr+=$url.Value
}

Log "Setting variables to use during deployment"
Log "Number of settings: $($arr.Length)"
$json= ConvertTo-Json $arr -Compress
Write-Host "##vso[task.setvariable variable=LogicAppsSetting_url]$json"

Log "Job well done!"