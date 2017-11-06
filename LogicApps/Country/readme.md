| **Function** | **`getlist-country()`**                                                 |
|--------------|:-----------------------------------------------------------------------|
| Description  | Obtains list of countries from Government Register and creates individual message for each country.  Running is scheduled by the scheduler defined by `Infrastructure\Scheduler.json` |
| Input        | <https://country.register.gov.uk/records?page-size=1000&page-index=1> |
| Output       | For each country --> Country Message --> ServiceBus                   |


**See Also**: [GDS Register Documentation](https://registers-docs.cloudapps.digital/#api-documentation-for-registers)

**Country Example**

    "PT" : {
        "index-entry-number" : "147",
        "entry-number" : "147",
        "entry-timestamp" : "2016-04-05T13:23:05Z",
        "key" : "PT",
        "item" : [ {
          "country" : "PT",
          "official-name" : "The Portuguese Republic",
          "name" : "Portugal",
          "citizen-names" : "Portuguese"
        } ]


where the key being used is `"key"`