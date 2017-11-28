<#
.SYNOPSIS
Sets app settings in Azure Functions.

.DESCRIPTION
Sets app settings in Azure Functions.

.PARAMETER APIResourceGroupName
Name of the Resource Group where the API Management is.

.PARAMETER APIManagementName
Name of the API Management.

.NOTES
This script is for use as a part of deployment in VSTS only.
#>

Param(
    [Parameter(Mandatory=$true)] [string] $APIResourceGroupName,
    [Parameter(Mandatory=$true)] [string] $APIManagementName,
    [Parameter(Mandatory=$true)] [string] $OrchestrationResourceGroupName,
    [Parameter(Mandatory=$true)] [string] $AzureFunctionsName,
	[Parameter(Mandatory=$true)] [string] $APIPrefix
    
)

$ErrorActionPreference = "Stop"

function Log([Parameter(Mandatory=$true)][string]$LogText){
    Write-Host ("{0} - {1}" -f (Get-Date -Format "HH:mm:ss.fff"), $LogText)
}

Log "Get API Management context"
$management=New-AzureRmApiManagementContext -ResourceGroupName $APIResourceGroupName -ServiceName $APIManagementName

Log "Retrives subscription"
$apiProductOrchestration=Get-AzureRmApiManagementProduct -Context $management -Title "$APIPrefix - Parliament [Orchestration]"
$subscription=Get-AzureRmApiManagementSubscription -Context $management -ProductId $apiProductOrchestration.ProductId
$subscriptionKey=$subscription.PrimaryKey

Log "Retrives Logic app trigger - sharepoint item"
$triggerSharepoint=Get-AzureRmLogicAppTriggerCallbackUrl -ResourceGroupName $OrchestrationResourceGroupName -Name "getitem-sharepoint" -TriggerName "manual"
$sharepointUrl=$triggerSharepoint.Value.Replace("?","/lists/{listId}/ids/{id}?")

Log "Gets current app settings"
$webApp = Get-AzureRmwebApp -ResourceGroupName $OrchestrationResourceGroupName -Name $AzureFunctionsName

$webAppSettings = $webApp.SiteConfig.AppSettings
$settings=@{}
foreach($set in $webAppSettings){ 
    $settings[$set.Name]=$set.Value
}

Log "Sets new subscription key"
$settings["SubscriptionKey"]=$subscriptionKey
Set-AzureRmWebApp -ResourceGroupName $OrchestrationResourceGroupName -Name $AzureFunctionsName -AppSettings $settings

Log "Gets current connection strings"
$connectionStrings=$webApp.SiteConfig.ConnectionStrings
$connections = @{}
foreach($connection in $connectionStrings){
	if (($connection.Name -ne "Data") -or ($connection.Name -ne "SharepointItem")) {
		$connections[$connection.Name]=@{Type=if ($connection.Type -eq $null){"Custom"}else{$connection.Type.ToString()};Value=$connection.ConnectionString}
	}
}

Log "Sets new url for sharepoint"
$connections["SharepointItem"]=@{Type="Custom";Value=$sharepointUrl}
Log "Sets new data connection"
$connections["Data"]=@{Type="Custom";Value="https://$APIManagementName.azure-api.net/$APIPrefix/graph-store"}
Set-AzureRmWebApp -ResourceGroupName $OrchestrationResourceGroupName -Name $AzureFunctionsName -ConnectionStrings $connections

Log "Job well done!"