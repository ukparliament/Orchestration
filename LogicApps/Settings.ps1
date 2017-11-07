<#
.SYNOPSIS
Sets task variable.

.DESCRIPTION
Sets task variable. Due to the limitation with ARM templates it is splited into series of array variables (converted to json).

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
        "queueReadBatchSize"=25;
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
        "queueReadBatchSize"=25;
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
        "queueReadBatchSize"=25;
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
        "queueReadBatchSize"=25;
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
        "queueReadBatchSize"=25;
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
        "queueReadBatchSize"=25;
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
        "queueReadBatchSize"=25;
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
        "queueReadBatchSize"=25;
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
        "queueReadBatchSize"=25;
    }
	New-Object -TypeName PSObject -Property @{
        "name"="governmentincumbencymnis";
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/MemberGovernmentPosts?`$select=MemberGovernmentPost_Id";
        "listAcceptHeader"="application/atom+xml";
        "foreachObject"="@json(body('Get_List')).feed.entry";
        "idObject"="@{item().id}";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="17:30";
        "queueReadBatchSize"=25;
    }
	New-Object -TypeName PSObject -Property @{
        "name"="houseincumbencymnis";
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/MemberLordsMembershipTypes?`$select=MemberLordsMembershipType_Id";
        "listAcceptHeader"="application/atom+xml";
        "foreachObject"="@json(body('Get_List')).feed.entry";
        "idObject"="@{item().id}";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="17:45";
        "queueReadBatchSize"=50;
    }
	New-Object -TypeName PSObject -Property @{
        "name"="partymembershipmnis";
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/MemberParties?`$select=MemberParty_Id";
        "listAcceptHeader"="application/atom+xml";
        "foreachObject"="@json(body('Get_List')).feed.entry";
        "idObject"="@{item().id}";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="18:00";
        "queueReadBatchSize"=25;
    }
	New-Object -TypeName PSObject -Property @{
        "name"="seatincumbencymnis";
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/MemberConstituencies?`$select=MemberConstituency_Id";
        "listAcceptHeader"="application/atom+xml";
        "foreachObject"="@json(body('Get_List')).feed.entry";
        "idObject"="@{item().id}";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="18:15";
        "queueReadBatchSize"=25;
    }
    New-Object -TypeName PSObject -Property @{
        "name"="epetition";
        "listUri"="";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="22:10";
        "queueReadBatchSize"=25;
    }
	New-Object -TypeName PSObject -Property @{
        "name"="territory";
        "listUri"="";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="21:01:01";
        "queueReadBatchSize"=25;
    }
	New-Object -TypeName PSObject -Property @{
        "name"="country";
        "listUri"="";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="21:02:01";
        "queueReadBatchSize"=25;
    }
	New-Object -TypeName PSObject -Property @{
        "name"="committeemnis";
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/Committees?`$select=Committee_Id";
        "listAcceptHeader"="application/atom+xml";
        "foreachObject"="@json(body('Get_List')).feed.entry";
        "idObject"="@{item().id}";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="21:05";
        "queueReadBatchSize"=25;
    }
	New-Object -TypeName PSObject -Property @{
        "name"="membercommitteemnis";
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/MemberCommittees?`$select=MemberCommittee_Id";
        "listAcceptHeader"="application/atom+xml";
        "foreachObject"="@json(body('Get_List')).feed.entry";
        "idObject"="@{item().id}";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="21:25";
        "queueReadBatchSize"=25;
    }
	New-Object -TypeName PSObject -Property @{
        "name"="weblink";
        "listUri"="";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="21:30";
        "queueReadBatchSize"=25;
    }
	New-Object -TypeName PSObject -Property @{
        "name"="photo";
        "listUri"="";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="21:35";
        "queueReadBatchSize"=25;
    }
	New-Object -TypeName PSObject -Property @{
        "name"="governmentpostmnis";
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/GovernmentPosts?`$select=GovernmentPost_Id";
        "listAcceptHeader"="application/atom+xml";
        "foreachObject"="@json(body('Get_List')).feed.entry";
        "idObject"="@{item().id}";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="20:20";
        "queueReadBatchSize"=25;
    }
)

$variableNames=@("name","listUri","listAcceptHeader","foreachObject","idObject","frequency","interval","triggerTime","queueReadBatchSize")

Log "Setting variables to use during deployment"
Log "Number of settings: $($logicAppVariable.Length)"
foreach ($key in $variableNames) {
    Log "Key: $key"
    $arr=($logicAppVariable | Select-Object $key).$key
    $json= ConvertTo-Json $arr -Compress
    Write-Host "##vso[task.setvariable variable=LogicAppsSetting_$key]$json"
}
Write-Host "##vso[task.setvariable variable=SubscriptionKeyOrchestration]$subscriptionKey"

Log "Job well done!"