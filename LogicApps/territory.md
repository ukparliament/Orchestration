| **Function** | **getlist-territory**                                                   |
|--------------|-------------------------------------------------------------------------|
| Description  | *LogicApp* that obtains the list of territories from the Government's Register.  Execution is controlled by the *processlist-territory* *LogicApp*.  This function takes no parameters.  Territories are used in *ePetitions* when signatories originate from a territory.
| Input        | <https://territory.register.gov.uk/records?page-size=1000&page-index=1> |
| Output       | For each territory --> territory Message --> *ServiceBus*               |

**See Also**: [GDS Register Documentation](https://registers-docs.cloudapps.digital/#api-documentation-for-registers)

**Territory Example**

```
"TW" : {
    "index-entry-number" : "71",
    "entry-number" : "71",
    "entry-timestamp" : "2016-12-15T12:15:07Z",
    "key" : "TW",
    "item" : [ {
      "official-name" : "Taiwan",
      "name" : "Taiwan",
      "territory" : "TW"
    }
```