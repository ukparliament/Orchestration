Needs to mention the `LogicApps\Setting.ps1` file and how these overides are working
See also  which already covers the `Setting.ps1` file.

**Summary**

These files complete the configuration of the Inrastructure, creating the:
* Schedule
* ServiceBus
* Sharepoint (connector)

The use of `Scheduler.json` is described in `LogicApps\Readme.md` ... **WRONG**
 
The ServiceBus includes the creatiog of mutiple queues, currently 18.  These can be seen in the
`LogicApps\Setting.ps1` file.

The 