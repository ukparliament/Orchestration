**Summary**

The folders in this **LogicApps** folder are importing data from external or other parliamentary sources
for storage in the GraphDB.  Each record read results in the creation of a corresponding message
which is placed on the MessageBus.  
These messages are picked up by their corresponding Transform function within the *Orchestrations\Functions* folder.

The running of functions is triggered by scheduled jobs running on the platform and are setup by the *Settings.ps1* file.
These triggers are scheduled to run daily although this timing is defined in the code so is subject to change.

Photos are read from Parliament's internal SharePoint site.  This requires aking special measures so as to authencicate to
SharePoint.  The standard pattern is still used:
* Get-List
* Process-List [which generate as a message for each photo]
