## Orchestraion \ Infrastructure

Needs to mention the `LogicApps\Setting.ps1` file and how overides are working
See also  which already covers the `Setting.ps1` file.

**Summary**

These files complete the configuration of the Inrastructure, creating the:
* Schedule
* ServiceBus
* Sharepoint (connector)

`ServiceBus.json` includes creating multiple queues within the ServiceBus, currently 18.  These 18 can be seen in the
`LogicApps\Setting.ps1` file with each being used by a Get- and Tranpose- function.




