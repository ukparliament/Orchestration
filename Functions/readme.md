## Orhestration \ Functions

Functions provide processing capabilities to support the Orchestrations and the platform more generalyl.  Some
functions are following a consistent naming pattern which is described here.  Others not following this pattern 
have helpful function names .

**Path**:   Functions\\Transform*Message*

**Description**: Functions conforming to the Transform*Message* convention follow the same pattern.  *Message*
has been created by it's corresponding getlist-*Message* function in **Logic Apps**.  Transformations:
* use a consistent language defined by Parliament's ontology in the *Message* 
* avoid creating duplicates; rather duplicates will return the original *Message*.

**Input**: Messages from the MessageBus

**Output**: Transformed *Message* in a standard form based on the ontology.

----
**Path**: `GraphDBBackup`

**Description**: Backs up this database to the data storage platform.
The backup is run daily at 5am to Azure's backup data platform resource. This is scheduled in the `SchedulerJobLoop.json` file in the `LogicApps` folder.
Currently retention is permanent but will be reduced if an expected enhancement from Microsoft can manage this retention better and automatically.

---
**Path**: `GraphDBConnector.cs`

**Description** Returns the connector to the GraphDB on the environment this is being execuited on.

---

**Path**: `IdRetrieval.cs`

**Description**: Helper class to read data (Subject and Value) from the triple store with the option to create data when it's not present.

---

**Path**: `Logger.cs`

**Description**: Helper class to log messages to the event log.  Functionality included is:
* Messages are logged with a Severity Level;
* the ability to log durations, recording the start and end times
* Log messages as errors.

Logs are managed by Operations Management Suite (OMS) on the platform. Through OMS the following can be performed:

* log analysis 
* raising of alerts
* historic storage