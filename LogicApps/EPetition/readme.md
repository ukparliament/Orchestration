| **Function**:| **`getlist-epetition()`**                                                 |
|--------------|:------------------------------------------------------------------------|
| Description  | Obtains a list of epetitions from UK Government and Parliament.  Running is sceduled by the scheduler defined by `Infrastructure \ Scheduler.json` |
| Input        | <https://petition.parliament.uk/petitions>                              |
| Output       | For each petition --> Petition Message --> ServiceBus                   |

---

| **Function**:| **`getlistpage-epetition()`**                                                 |
|--------------|:------------------------------------------------------------------------|
| Description  | Logic App that obtains list of epetitions based on the page count from UK Government and Parliament.  Running is sceduled by the scheduler defined by `Infrastructure \ Scheduler.json` |
| Input        | https://petition.parliament.uk/petitions?page=_Page_                    |
| Output       | For each petition --> Petition Message --> ServiceBus                   |

