# Orchestration for Parliamentary data service #

## Overview ##

All artefacts are designed for Azure platform. Repository consist of [Logic Apps](https://docs.microsoft.com/en-gb/azure/logic-apps/) and [Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview) that are deployed using combination of [ARM templates](https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-manager-template-walkthrough) and powershell scripts.

## LogicApps ##
LogicApps collect data required from varied and various sources, such as from Government registers and publications already published by Parliament. The data retrieved is stored in the GraphDB by the *Functions* in a consistent format for further use.

Settings.ps1 script generates task variables that are used by ARM templates (*loop.json) to create workflows. Name property is reused accross Logic Apps, scheduler jobs and Azure Functions. There are some additional workflows that override defualt ones.

## Functions ##
Functions may be associated with different areas, inluding:
* the infrasture, as with `GraphDBBackup`;
* helping to process the data retrieved, as with `JsonKeyToArrayConverter`;
* the data, as with `TransformationTerritory`.

Code (C#) that extends functionality of Logic Apps. In order to run it locally local.settings.json file has to be added to the project.
Below is the layout of the file:

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
