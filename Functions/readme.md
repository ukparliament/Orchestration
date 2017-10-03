These sections form the orchestrations of the platform.  They are hosted on the platforn's rerources descibed and configured using ARM (Azure Resource Manager)
templates. 

**Path**:    **Functions**

**Description**: Functions conforming to a Transform*Message* convention follow the same pattern.  *Message* has been created by it's
corresponding getlist-*Message* function in **Logic Apps**.  Transformations:
*  use the consistent language defined by Parliament's ontology in the *Message* 
*  avoid creating duplicates; rather duplicates will return the original *Message*.

**Input**: Messages from the MessageBus

**Output**: Transformed *Message* in a standard form.

----

**Path**: `GraphDBBackup`

**Description**: Backs up this database. The GraphDB is the data storage platform for the entrire platform.
The backup is performed daily to Azure's backup data platform resource and is scheduled by the platform.
Currently retention is permanent but will be reduced if an expected enhancement from
Microsoft reducing the retention is introcuced.
