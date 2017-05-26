<#
.SYNOPSIS
Sets task variable.

.DESCRIPTION
Sets task variable. Due to the limitation with ARM templates it is splited into series of array variables (converted to json).

.PARAMETER AzureFunctionsResourceGroupName
Name of the Resource Group where the Azure Functions are.

.PARAMETER AzureFunctionsName
Name of the Azure FUnctions container.

.NOTES
This script is for use as a part of deployment in VSTS only.
#>

Param(
    [Parameter(Mandatory=$true)] [string] $AzureFunctionsResourceGroupName,
    [Parameter(Mandatory=$true)] [string] $AzureFunctionsName
)

$ErrorActionPreference = "Stop"

function Log([Parameter(Mandatory=$true)][string]$LogText){
    Write-Host ("{0} - {1}" -f (Get-Date -Format "HH:mm:ss.fff"), $LogText)
}

$logicAppVariable=@(
    New-Object  -TypeName PSObject -Property @{
        "name"="constituencymnis";
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/Constituencies?`$select=Constituency_Id";
        "listAcceptHeader"="application/atom+xml";
        "foreachObject"="@json(body('Get_List')).feed.entry";
        "idObject"="@{item().id}";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="17:00";
        "queueReadBatchSize"=50;
    }     
    New-Object  -TypeName PSObject -Property @{
        "name"="constituencyos";
        "listUri"="http://data.ordnancesurvey.co.uk/datasets/os-linked-data/apis/sparql?query=construct+%7B%3Fs+a+%3Chttp%3A%2F%2Fdata.ordnancesurvey.co.uk%2Fontology%2Fadmingeo%2FWestminsterConstituency%3E.%7D+WHERE+%7B%3Fs+a+%3Chttp%3A%2F%2Fdata.ordnancesurvey.co.uk%2Fontology%2Fadmingeo%2FWestminsterConstituency%3E.%7D";
        "listAcceptHeader"="application/rdf+xml";
        "foreachObject"="@json(body('Get_List'))['rdf:RDF']['j.0:WestminsterConstituency']";
        "idObject"="@item()['@rdf:about']";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="18:30";
        "queueReadBatchSize"=50;
    }
    New-Object  -TypeName PSObject -Property @{
        "name"="constituencyosni";
        "listUri"="https://gisservices.spatialni.gov.uk/arcgisc/rest/services/OpenData/OSNIOpenData_50KBoundaries/MapServer/3/query?returnIdsOnly=true&where=1%3D1&f=JSON";
        "listAcceptHeader"="text/plain";
        "foreachObject"="@json(body('Get_List')).objectIds";
        "idObject"="@item()";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="18:45";
        "queueReadBatchSize"=50;
    }
    New-Object  -TypeName PSObject -Property @{
        "name"="contactpointseatmnis";
        "listUri"="http://data.parliament.uk/MembersDataPlatform/open/OData.svc/MemberAddresses?`$select=MemberAddress_Id&`$filter=AddressType_Id%20eq%201%20or%20AddressType_Id%20eq%203";
        "listAcceptHeader"="application/atom+xml";
        "foreachObject"="@json(body('Get_List')).feed.entry";
        "idObject"="@{item().id}";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="18:50";
        "queueReadBatchSize"=50;
    }
    New-Object  -TypeName PSObject -Property @{
        "name"="contactpointelectoralmnis";
        "listUri"="http://data.parliament.uk/MembersDataPlatform/open/OData.svc/MemberAddresses?`$select=MemberAddress_Id&`$filter=AddressType_Id%20eq%204";
        "listAcceptHeader"="application/atom+xml";
        "foreachObject"="@json(body('Get_List')).feed.entry";
        "idObject"="@{item().id}";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="19:45";
        "queueReadBatchSize"=50;
    }
    New-Object  -TypeName PSObject -Property @{
        "name"="contactpointpersonmnis";
        "listUri"="http://data.parliament.uk/MembersDataPlatform/open/OData.svc/MemberAddresses?`$select=MemberAddress_Id&`$filter=AddressType_Id%20eq%205";
        "listAcceptHeader"="application/atom+xml";
        "foreachObject"="@json(body('Get_List')).feed.entry";
        "idObject"="@{item().id}";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="20:15";
        "queueReadBatchSize"=50;
    }
    New-Object -TypeName PSObject -Property @{
        "name"="lordstypemnis";
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/LordsMembershipTypes?`$select=LordsMembershipType_Id";
        "listAcceptHeader"="application/atom+xml";
        "foreachObject"="@json(body('Get_List')).feed.entry";
        "idObject"="@{item().id}";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="20:15";
        "queueReadBatchSize"=50;
    }
    New-Object -TypeName PSObject -Property @{
        "name"="partymnis";
        "listUri"="http://data.parliament.uk/MembersDataPlatform/open/OData.svc/Parties?`$select=Party_Id";
        "listAcceptHeader"="application/atom+xml";
        "foreachObject"="@json(body('Get_List')).feed.entry";
        "idObject"="@{item().id}";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="20:20";
        "queueReadBatchSize"=50;
    }
    New-Object -TypeName PSObject -Property @{
        "name"="membermnis";
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/Members?`$select=Member_Id";
        "listAcceptHeader"="application/atom+xml";
        "foreachObject"="@json(body('Get_List')).feed.entry";
        "idObject"="@{item().id}";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="20:30";
        "queueReadBatchSize"=50;
    }
    New-Object -TypeName PSObject -Property @{
        "name"="epetition";
        "listUri"="";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="21:30";
        "queueReadBatchSize"=50;
    }
)

Log "Getting master key from Azure Functions"
$funcProperties=Invoke-AzureRmResourceAction -ResourceGroupName $AzureFunctionsResourceGroupName -ResourceType Microsoft.Web/sites/config -ResourceName "$AzureFunctionsName/publishingcredentials" -Action list -ApiVersion 2015-08-01 -Force
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $funcProperties.properties.publishingUserName,$funcProperties.properties.publishingPassword)))
$masterKeyResponse=Invoke-RestMethod -Uri "https://$AzureFunctionsName.scm.azurewebsites.net/api/functions/admin/masterkey" -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -Method GET

Log "Checking connection to $AzureFunctionsName"
$checkCounter=1
do 
{
    try
    {
        Log "Attempt number $checkCounter"
        $respond=Invoke-WebRequest -Uri "https://$AzureFunctionsName.azurewebsites.net" -Method Get -TimeoutSec 15 -UseBasicParsing
    }
    catch
    {
        if ($checkCounter -eq 10)
        {
            Throw "Cannot connect after $checkCounter attempts"
        }
        $respond=$null
        $checkCounter+=1
        Start-Sleep 30
    }
}
until ($respond -and ($respond.StatusCode -eq 200))

Log "Retrieving functions' keys"
foreach ($setting in $logicAppVariable){
    Log "Function $($setting.name)"
    $functionKeyResponse=Invoke-RestMethod -Uri "https://$AzureFunctionsName.azurewebsites.net/admin/functions/Transformation$($setting.name)/keys?code=$($masterKeyResponse.masterKey)" -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -Method GET
    $setting | Add-Member @{"transformationFunctionUri" = "https://$AzureFunctionsName.azurewebsites.net/api/Transformation$($setting.name)?code=$($functionKeyResponse.keys[0].value)"}
}

$variableNames=@("name","listUri","listAcceptHeader","foreachObject","idObject","frequency","interval","triggerTime","transformationFunctionUri","queueReadBatchSize")

Log "Setting variables to use during deployment"
Log "Number of settings: $($logicAppVariable.Length)"
foreach ($key in $variableNames) {
    Log "Key: $key"
    $arr=($logicAppVariable | Select-Object $key).$key
    $json= ConvertTo-Json $arr -Compress
    Write-Host "##vso[task.setvariable variable=LogicAppsSetting_$key]$json"
}

Log "Job well done!"