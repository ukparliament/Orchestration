# Orchestration for Parliamentary data service #

## Overview ##

All artefacts are designed for the Azure Platform Repository consisting of [Logic Apps](https://docs.microsoft.com/en-gb/azure/logic-apps/) and [Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview) that are deployed using combination of [ARM templates](https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-manager-template-walkthrough) and powershell scripts.

## Infrastructure ##
**Infrastructure** sets up the platform on which **Functions** and **LogicApps** operate.  This is deployed using settings defined under **Infrasture** setting up:
* the Virtual Machines (VMs) and clustered VMs
* the Network components and Firewalls
* Storage Devices
* Schedules

Deploment is initiated through `Tasks` defined within the `Build and Release` section in VSTS.  These use ARM templates defined in the Infastruture folder
which are configured using JSON files, such as `Scheduler.json` located in the `Orchestration / Infrastructure` folder which sets up the schedulated tasks
required by data collection functions.

### Environments ###

Components of the infrastructure are organised into the following environments:
* Dev
* Staging
* Live

Developments take place in the `Dev` environment and are realeased to the `Live` environment when development is completed.
The `Dev` environment is also used to try and test new concepts which do not form part of the standard development plans, such as migrating 
Parliament's legacy services onto the Azure platform.

### Components of the Infrastructure ###

VMs perform the computational role for the infrastructure, running the Logic Apps and Functions.  Clustering provides the
mechanism to recover should a VM fail which is expected to happen if Microsoft performs maintence on their physical computers.

#### Network components ####

These components include:
* Network Security Groups (NSGs), comprising of:
  * Inbound rules; and
  * Outbound rule
* Virtual Networks
* Network Clusters
* Network Interface Cards

NSGs protect the componets they safeguard by restricting the network traffic that is permitted to pass through them.  
NSGs define Inbound and Outbound rules which allow (and deny) access to components.  Rules are following
Microsoft's recommendations.

The network components show those which are conneted to the Internet and those which aren't with NSGs protecting those that are.

## LogicApps ##
LogicApps collect data required from a variety of sources, including the Government registers and publications 
already published elsewhere by Parliament.  The data retrieved is stored in the GraphDB by the *Functions* 
in a consistent format for further use.

`Settings.ps1` script generates task variables that are used by ARM templates (*loop.json) to create
workflows. Name property is reused accross Logic Apps,
scheduler jobs and Azure Functions. There are some additional workflows that override default ones.

## Functions ##
Functions may be associated with a variety of areas, including:
* the infrastructure, as with `GraphDBBackup`;
* helping to process the data retrieved, as with `JsonKeyToArrayConverter`;
* providing a consistent way tasks are performed, as with `LogicAppsErrorMessageLog` or `QueueMessagesRetrieval`
* the data, as with `TransformationTerritory`.

Code (C#) that extends functionality of Logic Apps. In order to run it locally local.settings.json file 
as to be added to the project.  Below is the layout of the file:

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
