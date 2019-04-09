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

function Set-Base64TaskVariable([Parameter(Mandatory=$true)][string]$VariableName, [Parameter(Mandatory=$true)][Object[]]$VariableValue){
	$json=ConvertTo-Json $VariableValue -Compress
	$base64=[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($json))
    Write-Host "##vso[task.setvariable variable=$VariableName]$base64"
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
	Sql = 6
}

$logicAppVariable=@(
    New-Object  -TypeName PSObject -Property @{
        "name"="constituencymnis";
		"sourceKind"=[SourceType]::Mnis;
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/Constituencies?`$select=Constituency_Id";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="Hour";
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
        "frequency"="Hour";
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
        "frequency"="Hour";
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
		"frequency"="Hour";
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
        "frequency"="Hour";
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
        "frequency"="Hour";
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
        "frequency"="Hour";
        "interval"=24;
        "triggerTime"="19:50";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
    <#New-Object -TypeName PSObject -Property @{
        "name"="membermnis";
		"sourceKind"=[SourceType]::Mnis;
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/Members?`$select=Member_Id";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="Hour";
        "interval"=24;
        "triggerTime"="20:20";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }#>
	New-Object -TypeName PSObject -Property @{
        "name"="governmentincumbencymnis";
		"sourceKind"=[SourceType]::Mnis;
        "listUri"="http://data.parliament.uk/membersdataplatform/open/OData.svc/MemberGovernmentPosts?`$select=MemberGovernmentPost_Id";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="Hour";
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
        "frequency"="Hour";
        "interval"=24;
        "triggerTime"="22:35";
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
        "frequency"="Hour";
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
        "frequency"="Hour";
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
        "frequency"="Hour";
        "interval"=24;
        "triggerTime"="22:10";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="territory";
		"sourceKind"=[SourceType]::GovernmentRegister;
        "listUri"="https://territory.register.gov.uk/records?page-size=1000&page-index=1";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="Hour";
        "interval"=24;
        "triggerTime"="21:01:01";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="country";
		"sourceKind"=[SourceType]::GovernmentRegister;
        "listUri"="https://country.register.gov.uk/records?page-size=1000&page-index=1";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="Hour";
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
        "frequency"="Hour";
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
        "frequency"="Hour";
        "interval"=24;
        "triggerTime"="21:25";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="weblink";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="9d21ee83-90a7-4c99-8648-523e5eaee734";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="Hour";
        "interval"=24;
        "triggerTime"="21:30";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="photo";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="838713ed-b86c-45f5-855b-d7bac1bee94b";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="Hour";
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
        "frequency"="Hour";
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
        "frequency"="Hour";
        "interval"=24;
        "triggerTime"="20:15";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="houseseattype";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="fa0d3696-92be-4506-93c9-13f5e9dd6b80";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="Hour";
        "interval"=24;
        "triggerTime"="21:36";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="lordsseat";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="37847503-9069-467b-be45-ffed7c405863";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="Hour";
        "interval"=24;
        "triggerTime"="21:45";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="lordsseatincumbency";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="5c1600fd-34ed-4227-b6f8-dcb03ed6d8be";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="Hour";
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
        "frequency"="Hour";
        "interval"=24;
        "triggerTime"="20:01";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="departmentgovernmentorganisation";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="9855a9e1-54e1-431d-b6dd-bf0455d1b244";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="Hour";
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
        "frequency"="Hour";
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
        "frequency"="Hour";
        "interval"=24;
        "triggerTime"="21:03";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="contactpointhouse";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="e26c9ee4-8c90-4d89-aeb3-449506027ea5";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="Hour";
        "interval"=24;
        "triggerTime"="17:01";
        "queueReadBatchSize"=50;
		"queueReadInterval"=30;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="committee";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="934ea442-0a9b-491f-99b9-cfb18a016b45";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="Hour";
        "interval"=24;
        "triggerTime"="19:01";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="committeetype";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="f18a9772-f950-4d45-ab90-08d4f592e08c";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="Hour";
        "interval"=24;
        "triggerTime"="17:02";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="parliamentperiod";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="cf5e06e7-53b8-4c44-94d5-b9cd0d3e2a33";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="Hour";
        "interval"=24;
        "triggerTime"="17:03";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="answeringbodymnis";
		"sourceKind"=[SourceType]::Mnis;
        "listUri"="http://data.parliament.uk/MembersDataPlatform/open/OData.svc/AnsweringBodies?`$select=AnsweringBody_Id";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="Hour";
        "interval"=24;
        "triggerTime"="20:35";
        "queueReadBatchSize"=150;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="questionwrittenanswer";
		"sourceKind"=[SourceType]::Custom;
        "listUri"="";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="Hour";
        "interval"=24;
        "triggerTime"="23:50";
        "queueReadBatchSize"=100;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
		New-Object -TypeName PSObject -Property @{
        "name"="questionwrittenanswercorrection";
		"sourceKind"=[SourceType]::Custom;
        "listUri"="";
        "listAcceptHeader"="";
        "foreachObject"="";
        "idObject"="";
        "frequency"="Hour";
        "interval"=24;
        "triggerTime"="23:30";
        "queueReadBatchSize"=100;
		"queueReadInterval"=90;
		"queueReadFrequency"="Second";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="lordsseatincumbencyinterruption";
		"sourceKind"=[SourceType]::Sharepoint;
        "listUri"="919f9534-241d-4f78-9e48-5b37e539aa39";
        "listAcceptHeader"="";
        "foreachObject"="@json('')";
        "idObject"="@json('')";
        "frequency"="Hour";
        "interval"=24;
        "triggerTime"="17:04";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="procedure";
		"sourceKind"=[SourceType]::Sql;
        "listUri"="select Id from [Procedure] where ModifiedAt>'@{addHours(utcNow(),-2)}' union select Id from DeletedProcedure where DeletedAt>'@{addHours(utcNow(),-2)}'";
        "listAcceptHeader"="";
        "foreachObject"="@coalesce(body('Get_items')?['resultsets']?['Table1'],json('[]'))";
        "idObject"="@{items('For_each')?['Id']}";
        "frequency"="Hour";
        "interval"=1;
        "triggerTime"="00:30";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="procedurestep";
		"sourceKind"=[SourceType]::Sql;
        "listUri"="select Id from ProcedureStep where ModifiedAt>'@{addHours(utcNow(),-2)}' union select Id from DeletedProcedureStep where DeletedAt>'@{addHours(utcNow(),-2)}'";
        "listAcceptHeader"="";
        "foreachObject"="@coalesce(body('Get_items')?['resultsets']?['Table1'],json('[]'))";
        "idObject"="@{items('For_each')?['Id']}";
        "frequency"="Hour";
        "interval"=1;
        "triggerTime"="00:31";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="procedureroute";
		"sourceKind"=[SourceType]::Sql;
		"listUri"="select Id from ProcedureRoute where ModifiedAt>'@{addHours(utcNow(),-2)}' union select Id from DeletedProcedureRoute where DeletedAt>'@{addHours(utcNow(),-2)}'";
        "listAcceptHeader"="";
        "foreachObject"="@coalesce(body('Get_items')?['resultsets']?['Table1'],json('[]'))";
        "idObject"="@{items('For_each')?['Id']}";
        "frequency"="Hour";
        "interval"=1;
        "triggerTime"="00:34";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="procedureworkpackage";
		"sourceKind"=[SourceType]::Sql;
        "listUri"="select Id from ProcedureWorkPackagedThing where ModifiedAt>'@{addHours(utcNow(),-2)}' union select Id from DeletedProcedureWorkPackagedThing where DeletedAt>'@{addHours(utcNow(),-2)}'";
        "listAcceptHeader"="";
        "foreachObject"="@coalesce(body('Get_items')?['resultsets']?['Table1'],json('[]'))";
        "idObject"="@{items('For_each')?['Id']}";
        "frequency"="Hour";
        "interval"=1;
        "triggerTime"="00:36";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="procedureworkpackagedthing";
		"sourceKind"=[SourceType]::Sql;
        "listUri"="select Id from ProcedureWorkPackagedThing where ModifiedAt>'@{addHours(utcNow(),-2)}' union select Id from DeletedProcedureWorkPackagedThing where DeletedAt>'@{addHours(utcNow(),-2)}'";
        "listAcceptHeader"="";
        "foreachObject"="@coalesce(body('Get_items')?['resultsets']?['Table1'],json('[]'))";
        "idObject"="@{items('For_each')?['Id']}";
        "frequency"="Hour";
        "interval"=1;
        "triggerTime"="00:38";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="procedureworkpackagedpreceding";
		"sourceKind"=[SourceType]::Sql;
        "listUri"="select Id from ProcedureWorkPackagedThingPreceding where ModifiedAt>'@{addHours(utcNow(),-2)}' union select Id from DeletedProcedureWorkPackagedThingPreceding where DeletedAt>'@{addHours(utcNow(),-2)}'";
        "listAcceptHeader"="";
        "foreachObject"="@coalesce(body('Get_items')?['resultsets']?['Table1'],json('[]'))";
        "idObject"="@{items('For_each')?['Id']}";
        "frequency"="Hour";
        "interval"=1;
        "triggerTime"="00:46";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="procedurebusinessitem";
		"sourceKind"=[SourceType]::Sql;
        "listUri"="select Id from ProcedureBusinessItem where ModifiedAt>'@{addHours(utcNow(),-2)}' union select Id from DeletedProcedureBusinessItem where DeletedAt>'@{addHours(utcNow(),-2)}'";
        "listAcceptHeader"="";
        "foreachObject"="@coalesce(body('Get_items')?['resultsets']?['Table1'],json('[]'))";
        "idObject"="@{items('For_each')?['Id']}";
        "frequency"="Hour";
        "interval"=1;
        "triggerTime"="00:47";
        "queueReadBatchSize"=50;
		"queueReadInterval"=1;
		"queueReadFrequency"="Minute";
    }
	New-Object -TypeName PSObject -Property @{
        "name"="procedurelaying";
		"sourceKind"=[SourceType]::Sql;
        "listUri"="select Id from ProcedureLaying where ModifiedAt>'@{addHours(utcNow(),-2)}' union select Id from DeletedProcedureLaying where DeletedAt>'@{addHours(utcNow(),-2)}'";
        "listAcceptHeader"="";
        "foreachObject"="@coalesce(body('Get_items')?['resultsets']?['Table1'],json('[]'))";
        "idObject"="@{items('For_each')?['Id']}";
        "frequency"="Hour";
        "interval"=1;
        "triggerTime"="00:49";
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
			Set-Base64TaskVariable -VariableName "LogicAppsSettings_$kind" -VariableValue $settings
            break
        }
		"$([SourceType]::Sql)" {            
            $settings=($logicAppVariable | Where-Object sourceKind -EQ $kind | Select-Object name, listUri, foreachObject, idObject)
			Set-Base64TaskVariable -VariableName "LogicAppsSettings_$kind" -VariableValue $settings
            break
        }
        default {
            $settings=($logicAppVariable | Where-Object sourceKind -EQ $kind | Select-Object name, listUri)
			Set-Base64TaskVariable -VariableName "LogicAppsSettings_$kind" -VariableValue $settings
			break
        }
    }    
}
$settings=($logicAppVariable | Select-Object name, frequency, interval, triggerTime, queueReadBatchSize, queueReadInterval, queueReadFrequency)
$currentDate=Get-Date -Format "yyyy-MM-dd"
Set-Base64TaskVariable -VariableName "LogicAppsSettings_all" -VariableValue $settings
Write-Host "##vso[task.setvariable variable=SubscriptionKeyOrchestration]$subscriptionKey"
Write-Host "##vso[task.setvariable variable=CurrentDate]$currentDate"


Log "Job well done!"