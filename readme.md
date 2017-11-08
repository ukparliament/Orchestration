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

Components of the infrastructure are organised into the following environments:
* Staging
* Live

The Staging environment
hosts the platform used by the WebSite with their DevCI webite during its development.  When complete
the Data and Search platform is deployed to `Live` and utilised by the Beta webite at https://beta.parliament.uk/.

Additional environments are created by the Data and Search Team to meet specific development objectives such as when 
migrating existing legacy services to the Azure platform.

### Components of the Infrastructure ###

VMs perform the computational role for the infrastructure running the `LogicApps` and `Functions`.  Clustering provides the
means to continue processing should one VM fail.  This is to be expected if, for instance, Microsoft perform maintence on
their physical computers hosting these VMs.

#### Network components ####

These components include:
* Network Security Groups [(NSGs)](https://docs.microsoft.com/en-us/azure/virtual-network/virtual-networks-nsg), comprising of:
  * Inbound rules; and
  * Outbound rule
* Virtual Networks
* Network Clusters
* Network Interface Cards

NSGs protect the components they are associated with by restricting the network traffic permitted to pass through them.  
NSGs define inbound and outbound rules which allow (or deny) access to components.  These rules follow
[Microsoft's recommendations](https://docs.microsoft.com/en-us/azure/api-management/api-management-using-with-vnet)
for their associated components.

The network components show those connected to the Internet and those that aren't; NSGs are protecting those that are.

## LogicApps ##
`LogicApps` (which can also be called *Workflows*) collect data from a variety of sources, including:
* Government registers
* data already published elsewhere by Parliament.

The data retrieved is stored in `GraphDB` by `Functions`.  This data is stored in a consistent and predictable format, aiding its reuse.

Control of execution is managed by a `LogicApp` defined and editable in Azure, resulting in a JSON file which
uses an ARM template to implement the prescribed functionality.  The `LogicApp` is exported
from Azure using the *Logic App Code View* operation.  This JSON file is stored in VSTS and deployed to Azure
within a *Step* of a deployment.

For example:
* The `LogicApp` *getlist-membermnis* reads the list of Members from [here](http://data.parliament.uk/membersdataplatform/open/OData.svc/Members) ...
* ... and processes each of the Members returned from this data source.
* *getlist-membermnis* sends a message to the `MessageBus` for each Member ...
* ... which are subscribed to by *processlist-membermnis*, found in the latest `data-orchestration_yyyymmdd_` Resource Group
* For each Member message another message with the Member's MNIS details is posted on the `MessageBus` and consumed by the *update-membermnis* `LogicApp` 
* These messages are subscribed to by the *update-membermnis* `LogicApp` ...
* ... which uses the *Orchestration\Functions\TransformationMemberMnis\TransformationMemberMnis.cs* function to save this data.
* *TransformationMemberMnis.cs* inherits *BaseTransformtation.cs* which provides the writing or updating of this data in GraphDB.

The deployment of these components can be seen under `Deploy Logic Apps code`, 
[here](https://data-parliament.visualstudio.com/Platform/_release?releaseId=952&definitionId=16&_a=release-logs).
The script generates task variables that are used by ARM templates (in the <*name*>loop.json files) to create
each `LogicApp`.  The *name* property is reused accross:
* the `LogicApps`
* the scheduler jobs and
* the Azure Functions.

There are some additional workflows that override the default ones.  Default `LogicApps` are created using the
`Orchestration\LogicApps\Settings.ps1` file.  Any `LogicApp` can then by overriden, as happens
with the `getlist-country` `LogicApp` which is overriden by the `Orchestration\LogicApps\Country\GetList.json` `LogicApp`.
The override occurs through the deployment by deploying the all of the defaults followed by deploying an
override.

#Testing

Nearly of works - I get my diagram but not my "MMMMMMMMMMy NSGs" label.  Good enough.

See https://blogs.msmvps.com/molausson/2014/12/28/use-visual-studio-online-markdown-as-your-wiki/
![MMMMMMMMMMy NSGs](/Infrastructure%2FDiagrams%2FNSGs.jpg)

> These are insert picture tests - without success so don't use this style - use the one above

> Rel Path ![My picture label A](./Platform/_git/Orchestration?_a=contents&path=%2FInfrastructure%2FDiagrams%2FNSGs.jpg)
> Abs Path ![My picture label B](https://data-parliament.visualstudio.com/Platform/_git/Orchestration?_a=contents&path=%2FInfrastructure%2FDiagrams%2FNSGs.jpg)
> Abs Path ![My picture label C]($/project/Orchestration?_a=contents&path=%2FInfrastructure%2FDiagrams%2FNSGs.jpg)
> https://data-parliament.visualstudio.com/Platform/_git/Orchestration?_a=preview&path=%2Freadme.md
> https://data-parliament.visualstudio.com/DefaultCollection/_git/Platform/Orchestration?_a=preview&path=%2Freadme.md

##Functions
Functions may be associated with a variety of areas, including:
* the infrastructure, as with `GraphDBBackup`;
* helping to process the data retrieved, as with `JsonKeyToArrayConverter`;
* providing a consistent way tasks are performed, as with `LogicAppsErrorMessageLog` or `QueueMessagesRetrieval`
* the data, as with `TransformationTerritory`.

C# code extends functionality of `LogicApps`. In order to run it locally *local.settings.json* file 
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
