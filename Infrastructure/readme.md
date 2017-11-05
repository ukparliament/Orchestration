## Orchestraion \ Infrastructure

Needs to mention the `LogicApps\Setting.ps1` file and how overides are working.

**Summary**

These files complete the configuration of the Inrastructure, creating the:
* Schedule
* ServiceBus
* Sharepoint (connector)

`ServiceBus.json` includes creating multiple queues within the ServiceBus, currently 18.  These
are seen in the `LogicApps\Setting.ps1` file with each being used by pairs of
`Get-` and `Tranpose-` functions.




