<#
.SYNOPSIS
Get settings for API app.

.DESCRIPTION
Retrieves value of of API Management and sets task variable.

.PARAMETER APIResourceGroupName
Name of the Resource Group where the API Management is.

.NOTES
This script is for use as a part of deployment in VSTS only.
#>

Param(
	[Parameter(Mandatory=$true)] [string] $APIResourceGroupName
)
$ErrorActionPreference = "Stop"

function Log([Parameter(Mandatory=$true)][string]$LogText){
    Write-Host ("{0} - {1}" -f (Get-Date -Format "HH:mm:ss.fff"), $LogText)
}

Log "Get API Management"
$apiManagement=Get-AzureRmApiManagement -ResourceGroupName $APIResourceGroupName

Log "Setting variables to use during deployment"
Write-Host "##vso[task.setvariable variable=APIManagementIP]$($apiManagement.StaticIPs[0])"

Log "Job well done!"