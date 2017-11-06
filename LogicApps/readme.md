**Summary of LogicApps**

The folders in the *LogicApps* folder import data from external or other parliamentary sources
for storage in *GraphDB*.  For each record read a corresponding message is created in the appropriate queue on the *MessageBus*.  Messages
are consumed by their corresponding **Transform** function within the
[Orchestrations\Functions](https://data-parliament.visualstudio.com/Platform/_git/Orchestration?path=%2FFunctions&version=GBmaster&_a=contents) folder.

*Settings.ps1* defines the schedules to be used by the platform to execute the *getlist-xxx* functions.
Their deployment is performed the tasks under the *Data Platform: Logic Apps code* task group and using the settings
in the *Orchestration\LogicApps\SchedulerJobLoop.json* file. There is tight coupling between the names
of the schedules and the names of their associated *LogicApps*.

*Settings.ps1* defines a generic pattern for the:
* Schedules
* Names of ListApp 
* Message names

This can be overriden where the generic pattern is not suitable, as occues with the processing of Countries.
This uses `Orchestration\LogicApps\Country\GetList.json` to deploy the specific settings
required to process Countries.  This defines an ARM template which is deployed in the `Create workflow - Country list`
task within the `Data Platform: Logic Apps code` task group.

When *Settings.ps1* is compiled within its task group it writes a log detailing the compilation.  These log files can be viewed in VSTS
although doing so is beyond the scope of this description.  Should this be considered
useful information to see then contact a member of the Data and Search Team for a demostration.

Triggers are scheduled to run every evening although this is dependant on the settings in the code, so is subject to change.

Photos are read from Parliament's internal SharePoint site.  This requires taking special measures when deploying 
the SharePoint connector as valid authenication details must be provided during deployment. Once deployed this
connector can be used without further authenication.  This authenication requirement also applies to the WebLink connector.
Processing of items follows the standard pattern - **Get** followed by **Transform**.
