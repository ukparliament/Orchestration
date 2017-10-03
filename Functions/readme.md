These sections form the orchestrations of the platform.  They are hosted on the platforn's rerources descibed and configured using ARM (Azure Resource Manager)
templates. 

**Path**:    **Functions**

**Description**: Functions conforming to a Transform*Message* convention follow the same pattern.  *Message* has been created by it's
corresponding getlist-*Message* function in **Logic Apps**.  Transformations:
*  use the consistent language defined by Parliament's ontology in the *Message* 
*  avoid creating duplicates; rather duplicates will return the original *Message*.

**Input**: Messages from the MessageBus

**Output**: Transformed *Message* in a standard form based on the ontology.

----

**Path**: `GraphDBBackup`

**Description**: Backs up this database. The GraphDB is the data storage platform for the entrire platform.
The backup is performed daily to Azure's backup data platform resource and is scheduled by the platform.
Currently retention is permanent but will be reduced if an expected enhancement from
Microsoft reducing the retention is introcuced.

---

**Path**: `IdRetrieval.cs`

**Description**: Helper class to read data (Subject, Value) from the triple store with the option to create data when it's not present.

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