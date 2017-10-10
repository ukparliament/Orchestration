**Summary**

The folders in this **LogicApps** folder are importing data from external or other parliamentary sources
for storage in the GraphDB.  For each record read a corresponding message is created on the MessageBus.  
Messages are consumed by their corresponding Transform function within the *Orchestrations\Functions* folder.

*Settings.ps1* defines the schedules to be used by the platform to execute the *getlist-xxx* functions.  The *Settings.ps1*
defines the scheduled jobs as an array of schedules with their deployment to the platform being
performed by the tasks under the *Data Platform: Logic Apps code* task group and the settings
in the *Orchestration / LogicApps/SchedulerJobLoop.json* file. There is tight coupling between the names
of the schedules and the names of the logic apps.

Triggers are scheduled to run daily although this timing is dependant on the settings in the code so is subject to change.

Photos are read from Parliament's internal SharePoint site.  This requires taking special measures when deploying 
the SharePoint connector as valid authenication details must be provided during deplyment; once deplyed this
connector can be used without further authenication.

Photos follow the standard pattern:
* Get-List
* Process-List [which generate as a message for each photo]
