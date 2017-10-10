**Summary**

The folders in this **LogicApps** folder are importing data from external or other parliamentary sources
for storage in the GraphDB.  For each record read a corresponding message is created on the MessageBus.  
Messages are picked up by their corresponding Transform function within the *Orchestrations\Functions* folder.

The running of functions is triggered by scheduled jobs running on the platform and are setup by *Settings.ps1*.
These triggers are scheduled to run daily although this timing is defined in code so is subject to change.

Photos are read from Parliament's internal SharePoint site.  This requires taking special measures to authencicate to
SharePoint.  The standard pattern is still used:
* Get-List
* Process-List [which generate as a message for each photo]
