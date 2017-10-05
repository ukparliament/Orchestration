**Folders**

Folders under `Orhestration`:
* Functions
* Infrastructure
* LogicApps

The container creating Orchestrations is primarily focused on providing the data platform servicing the WebSite.  The WebSite
uses APIs to retrieve the data required from the platform for publication; the platform services these requests.
The scope is not just limited to the WebSite, allowing any external source to make similar requests to meet their own needs.

Folder  | Description
------- | -----------
Infrastructure | Sets up the infrastructure necesssary for the Orchestration such as the ServiceBus to store messages which trigger functionality, schedules which trigger the retrieval of data hosted externally and other storage used by the functions.
LogicApps | Responding to messages on the service bus that are created by scheduled tasks on the Infrastructure.
Functions | Providing a function library used by the contents of Orchestration
