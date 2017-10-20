# Orchestration for Parliamentary data service #

## Overview ##

All artefacts are designed for the Azure Platform Repository consisting of [Logic Apps](https://docs.microsoft.com/en-gb/azure/logic-apps/) and [Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview) that are deployed using combination of [ARM templates](https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-manager-template-walkthrough) and powershell scripts.

## Infrastructure ##
**Infrastructure** sets up the platform on which **Functions** and **LogicApps** operate.  This is deployed using
settings defined under **Infrasture** setting up:
* the Virtual Machines (VMs) and clustered VMs
* the Network components and Firewalls
* Storage Devices
* Schedules

Deploment is initiated through `Tasks` defined within the `Build and Release` section in VSTS.  These use ARM templates
defined in the Infastruture folder which are configured using JSON files, such as `Scheduler.json` located in
the `Orchestration / Infrastructure` folder which sets up the schedulated tasks
required by data collection functions.

### Environments ###

Components of the infrastructure are organised into the following major environments:
* Staging
* Live

The Staging environment
hosts the platform used by the WebSite with their DevCI webite during its development.  When complete
the Data and Search platform is deployed to `Live` and utilised by the Beta webite at https://beta.parliament.uk/.

Additional environments are created by the Data and Search Team to meet specific development objectives such as when 
migrating existing legacy services to the Azure platform.

### Components of the Infrastructure ###

VMs perform the computational role for the infrastructure running the Logic Apps and Functions.  Clustering provides the
means to continue processing should one VM fail.  This is to be expected if, for instance, Microsoft perform maintence on
their physical computers hosting these VMs.

#### Network components ####

These components include:
* Network Security Groups (NSGs), comprising of:
  * Inbound rules; and
  * Outbound rule
* Virtual Networks
* Network Clusters
* Network Interface Cards

NSGs protect the componets they safeguard by restricting the network traffic that is permitted to pass through them.  
NSGs define Inbound and Outbound rules which allow (and deny) access to components.  Rules follow Microsoft's recommendations
for the components used on the platform.

The network components show those which are conneted to the Internet and those which aren't;  NSGs are protecting those that are.

## LogicApps ##
LogicApps collect data required from a variety of sources, including the Government registers and publications 
already published elsewhere by Parliament.  The data retrieved is stored in the GraphDB by the *Functions* 
in a consistent format for further use.

Control of execution is managed by an `Logic app` (also called a Workflow) defined in Azure.  Each `Logic app` is defined through a JSON file which
uses an ARM template to implement the desired functionality.  The `Logic app` is exported
from Azure using the *Logic App Code View* operation.  This JSON file is saved in VSTS and deployed in Azure
within a *Step* of a deployment.

For example:
* The `Logic app` *getlist-membermnis* reads the list of Members from [here](http://data.parliament.uk/membersdataplatform/open/OData.svc/Members)
* The *getlist-membermnis* `Logic app` processes each of the Members returned by the *external* data source
* *getlist-membermnis* sends a message to the MessageBus for each Member ...
* ... which are subscribed to by *processlist-membermnis* in the latest `data-orchestration_yyyymmss_` Resource Group

The deployment of these components can be seen under `Deploy Logic Apps code`
[here](https://data-parliament.visualstudio.com/Platform/_release?releaseId=952&definitionId=16&_a=release-logs)


`Settings.ps1` script generates task variables that are used by ARM templates (*loop.json) to create
workflows. Name property is reused accross Logic Apps,
scheduler jobs and Azure Functions. There are some additional workflows that override default ones.

## Functions ##
Functions may be associated with a variety of areas, including:
* the infrastructure, as with `GraphDBBackup`;
* helping to process the data retrieved, as with `JsonKeyToArrayConverter`;
* providing a consistent way tasks are performed, as with `LogicAppsErrorMessageLog` or `QueueMessagesRetrieval`
* the data, as with `TransformationTerritory`.

Code (C#) that extends functionality of LogicApps. In order to run it locally *local.settings.json* file 
has to be added to the project.  Below is the layout of the file:

```json
{
  "IsEncrypted": false,
  "Values": {
    "CUSTOMCONNSTR_Data": "",
    "SubscriptionKey": "",
    "IdNamespace": "",
    "SchemaNamespace": "",
    "ApplicationInsightsInstrumentationKey": "",
    "CUSTOMCONNSTR_ServiceBus": ""
  }
}
```

VS 2017 Preview is the only one that compiles Azure Functions right now [according to Microsoft](https://blogs.msdn.microsoft.com/webdev/2017/05/10/azure-function-tools-for-visual-studio-2017/).
