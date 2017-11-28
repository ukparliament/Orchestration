## Orhestration \ Functions

Functions provide processing capabilities to support the Orchestrations and, more generally, the platform.  Some
functions are following a consistent naming pattern, which is described below.  Others not following this pattern 
have suitable and helpful function names .

**Path**: `Transformation*Message*`

**Description**: Functions conforming to the __Transformation*Message*__ convention follow the same pattern.  *Message*
has been created within the appropriate **Logic App** and eritten to the ServiceBus.  Transformations then:
* use a consistent language in the *Message* defined by Parliament's ontology
* avoid creating duplicates by returning the original *Message* if a duplicate would result.

**Input**: Messages from the **MessageBus**

**Output**: Transformed *Message* in a standard form based on the ontology.

The processing of these messages are defined in the **Logic Apps** of the
__data-orchestration*YYYYMMDD*__ **Resource Group** in their **Logic App Designer**.  The __data-orchestration__
resource groups include the date **and time** of their deployment in their names.

----
**Path**: `Functions\GraphDBBackup`

**Description**: Backs up this database to the data storage platform.
The backup is currently run daily at 5am to Azure's backup data platform resource. This is scheduled
in the `SchedulerJobLoop.json` file in the `LogicApps` folder.  This schedule is subject to change.
Currently retention is permanent but is expected to be reduced if an enhancement from Microsoft 
can manage retention automatically.

---
**Path**: `GraphDBConnector.cs`

**Description** Returns the connector to the GraphDB on the environment it is called.

---

**Path**: `IdRetrieval.cs`

**Description**: Helper class to read data (Subject and Value) from the triple store, with the option to create data when it's not present.

---

**Path**: `Logger.cs`

**Description**: Helper class to log messages to the event log.  Functionality included is:
* Messages are logged with a Severity Level;
* the ability to log durations, recording the start and end times
* Log messages as errors.

Logs are managed by Operations Management Suite (OMS) on the platform. Through OMS the following can be performed:

* log analysis 
* raising of alerts
* historic storage of events
