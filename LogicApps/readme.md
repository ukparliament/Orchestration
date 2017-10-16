**Summary**

The folders in this **LogicApps** folder are importing data from external or other parliamentary sources
for storage in the **GraphDB**.  For each record read a corresponding message is created on the MessageBus.  Messages
are consumed by their corresponding Transform function within the [Orchestrations / Functions](https://data-parliament.visualstudio.com/Platform/_git/Orchestration?path=%2FFunctions&version=GBmaster&_a=contents) folder.

*Settings.ps1* defines the schedules to be used by the platform to execute the *getlist-xxx* functions.  *Settings.ps1*
defines the scheduled jobs as an array of schedules.  Their deployment is 
performed by the tasks under the *Data Platform: Logic Apps code* task group and using the settings
in the *Orchestration / LogicApps / SchedulerJobLoop.json* file. There is tight coupling between the names
of the schedules and the names of the **LogicApps**.

When *Settings.ps1* is compiled within its task group it writes a log detailing the compilation.  These log files can be viewed in VSTS
although doing so is beyond the scope of this description.  Should this be considered
useful information to see then contact a member of the Data and Search Team for an explation to view.

Triggers are scheduled to run every evening although this is dependant on the settings in the code, so is liable to change.

Photos are read from Parliament's internal SharePoint site.  This requires taking special measures when deploying 
the SharePoint connector as valid authenication details must be provided during deplyment. Once deplyed this
connector can be used without further authenication.  This authenication requirement also applies to the WebLink connector.
Processing of items follows the standard pattern - **Get** and then followed by **Transform**.
