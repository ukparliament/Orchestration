**Summary**

The folders in this **LogicApps** folder are importing data from external or other parliamentary sources
for storage in the GraphDB.  The records read result in the creation of a message which is placed on the MessageBus.  
These messages are picked up by their corresponding Transform function within the Orchestrations\Functions folder.

The running of functions is triggered by scheduled jobs running on the platform and are setup by the *Settings.ps1* file.
These triggers are scheduled to run daily although this timing is defined by the code so is subject to change.

Photos are read from the internal SharePoint site and includes special functionality to authencicate to SharePoint.