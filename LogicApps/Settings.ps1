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

Enum SourceType
{
	Mnis = 1
	Sharepoint = 2
	External = 3
	GovernmentRegister = 4
	Custom = 5
}

$logicAppVariable=@(
    New-Object  -TypeName PSObject -Property @{
        "name"="constituencymnis";
		"sourceKind"=[SourceType]::Mnis;
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/Constituencies?`$select=Constituency_Id";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="17:00";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }     
    New-Object  -TypeName PSObject -Property @{
        "name"="constituencyos";
		"sourceKind"=[SourceType]::External;
        "listUri"="http://data.ordnancesurvey.co.uk/datasets/os-linked-data/apis/sparql?query=construct+%7B%3Fs+a+%3Chttp%3A%2F%2Fdata.ordnancesurvey.co.uk%2Fontology%2Fadmingeo%2FWestminsterConstituency%3E.%7D+WHERE+%7B%3Fs+a+%3Chttp%3A%2F%2Fdata.ordnancesurvey.co.uk%2Fontology%2Fadmingeo%2FWestminsterConstituency%3E.%7D";
        "listAcceptHeader"="application/rdf+xml";
        "foreachObject"="@json(body('Get_List'))['rdf:RDF']['j.0:WestminsterConstituency']";
        "idObject"="@item()['@rdf:about']";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="18:30";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
    New-Object  -TypeName PSObject -Property @{
        "name"="constituencyosni";
		"sourceKind"=[SourceType]::External;
        "listUri"="https://gisservices.spatialni.gov.uk/arcgisc/rest/services/OpenData/OSNIOpenData_50KBoundaries/MapServer/3/query?returnIdsOnly=true&where=1%3D1&f=JSON";
        "listAcceptHeader"="text/plain";
        "foreachObject"="@json(body('Get_List')).objectIds";
        "idObject"="@item()";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="18:45";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
    New-Object  -TypeName PSObject -Property @{
        "name"="contactpointseatmnis";
		"sourceKind"=[SourceType]::Mnis;
        "listUri"="http://data.parliament.uk/MembersDataPlatform/open/OData.svc/MemberAddresses?`$select=MemberAddress_Id&`$filter=AddressType_Id%20eq%201%20or%20AddressType_Id%20eq%203";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
		"frequency"="hour";
        "interval"=24;
        "triggerTime"="18:50";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
    New-Object  -TypeName PSObject -Property @{
        "name"="contactpointelectoralmnis";
		"sourceKind"=[SourceType]::Mnis;
        "listUri"="http://data.parliament.uk/MembersDataPlatform/open/OData.svc/MemberAddresses?`$select=MemberAddress_Id&`$filter=AddressType_Id%20eq%204";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="19:15";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
    New-Object  -TypeName PSObject -Property @{
        "name"="contactpointpersonmnis";
		"sourceKind"=[SourceType]::Mnis;
        "listUri"="http://data.parliament.uk/MembersDataPlatform/open/OData.svc/MemberAddresses?`$select=MemberAddress_Id&`$filter=AddressType_Id%20eq%205";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="19:45";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
    New-Object -TypeName PSObject -Property @{
        "name"="partymnis";
		"sourceKind"=[SourceType]::Mnis;
        "listUri"="http://data.parliament.uk/MembersDataPlatform/open/OData.svc/Parties?`$select=Party_Id";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="19:50";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
    New-Object -TypeName PSObject -Property @{
        "name"="membermnis";
		"sourceKind"=[SourceType]::Mnis;
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/Members?`$select=Member_Id";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="20:20";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="governmentincumbencymnis";
		"sourceKind"=[SourceType]::Mnis;
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/MemberGovernmentPosts?`$select=MemberGovernmentPost_Id";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="17:30";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="oppositionincumbencymnis";
		"sourceKind"=[SourceType]::Mnis;
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/MemberOppositionPosts?`$select=MemberOppositionPost_Id";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="23:55";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="partymembershipmnis";
		"sourceKind"=[SourceType]::Mnis;
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/MemberParties?`$select=MemberParty_Id";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="18:00";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="seatincumbencymnis";
		"sourceKind"=[SourceType]::Mnis;
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/MemberConstituencies?`$select=MemberConstituency_Id";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="18:15";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
    New-Object -TypeName PSObject -Property @{
        "name"="epetition";
		"sourceKind"=[SourceType]::Custom;
        "listUri"="";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="22:10";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="territory";
		"sourceKind"=[SourceType]::GovernmentRegister;
        "listUri"="";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="21:01:01";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="country";
		"sourceKind"=[SourceType]::GovernmentRegister;
        "listUri"="";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="21:02:01";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="committeemnis";
		"sourceKind"=[SourceType]::Mnis;
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/Committees?`$select=Committee_Id";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="21:05";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="membercommitteemnis";
		"sourceKind"=[SourceType]::Mnis;
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/MemberCommittees?`$select=MemberCommittee_Id";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="21:25";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="weblink";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="7cb6d78b-5f45-49d2-bb77-009f71a83390";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="21:30";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="photo";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="2460c6bf-f26e-4049-94d0-c6096e036f3a";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="21:35";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="governmentpostmnis";
		"sourceKind"=[SourceType]::Mnis;
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/GovernmentPosts?`$select=GovernmentPost_Id";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="20:00";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="oppositionpostmnis";
		"sourceKind"=[SourceType]::Mnis;
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/OppositionPosts?`$select=OppositionPost_Id";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="20:15";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="houseseattype";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="85fe54cd-d26a-43af-88a5-9063f747c4a9";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="21:36";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="lordsseat";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="4a000e4a-aa78-4338-806d-92c0dd42e1a6";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="21:45";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="lordsseatincumbency";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="a9c47d77-9ccb-44cc-bc90-c3d07ea8aa0e";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="20:05";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="departmentmnis";
		"sourceKind"=[SourceType]::Custom;
        "listUri"="";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="20:01";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="departmentgovernmentorganisation";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="a55100f4-8dd9-43a3-8614-3a3f121c8fbf";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="20:10";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="committeechairincumbencymnis";
		"sourceKind"=[SourceType]::Mnis;
        "listUri"="http://data.parliament.uk/MembersDataPlatform/open/OData.svc/MemberCommitteeChairs?`$select=MemberCommitteeChair_Id";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="20:40";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="committeelaymembermnis";
		"sourceKind"=[SourceType]::Mnis;
        "listUri"="http://data.parliament.uk/MembersDataPlatform/open/OData.svc/CommitteeLayMembers?`$select=CommitteeLayMember_Id";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="21:03";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="contactpointhouse";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="261f09cd-9c4e-4e76-a928-b7b8c5b3cdcd";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="17:01";
        "queueReadBatchSize"=50;
		"queueReadInterval"=30;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="committee";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="035fa54f-492a-4008-93da-5ee912a2d0cf";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="19:01";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="committeetype";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="774e8a14-a613-4948-a645-a434ef6a8f1f";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="17:02";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="parliamentperiod";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="ffcfa882-ee6d-4467-b146-68cd62924256";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="hour";
        "interval"=24;
        "triggerTime"="17:03";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
)

Log "Setting variables to use during deployment"
foreach ($kind in [enum]::GetValues([SourceType])) {
	switch ($kind) {
        "$([SourceType]::Custom)" {
            break
        }
        "$([SourceType]::External)" {            
            $settings=($logicAppVariable | Where-Object sourceKind -EQ $kind | Select-Object name, listUri, listAcceptHeader, foreachObject, idObject)
            $json=ConvertTo-Json @{settings=$settings} -Compress
            Write-Host "##vso[task.setvariable variable=LogicAppsSetting_$kind]$json"
            break
        }
        default {
            $settings=($logicAppVariable | Where-Object sourceKind -EQ $kind | Select-Object name, listUri)
			$json=ConvertTo-Json @{settings=$settings} -Compress
            Write-Host "##vso[task.setvariable variable=LogicAppsSetting_$kind]$json"
            break
        }
    }    
}
$json=($logicAppVariable | Select-Object name | ConvertTo-Json -Compress)
Write-Host "##vso[task.setvariable variable=LogicAppsSetting_name]$json"
Write-Host "##vso[task.setvariable variable=SubscriptionKeyOrchestration]$subscriptionKey"

Log "Job well done!"