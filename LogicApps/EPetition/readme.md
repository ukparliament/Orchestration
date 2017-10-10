Two functions for retrieving ePetitions from Parliament's website.

The first gets only the first page of ePetitions.  This includes a header which provides the total number of
pages and is used to loop across all pages using the second function.  The logic for this is
contained in the *getlist-epetition* Logic App.


| **Function**:| **`getlist-epetition()`**                                                 |
|--------------|:--------------------------------------------------------------------------|
| Description  | Obtains the first page of ePetitions.  Running is scheduled by the scheduler defined by `Infrastructure \ Scheduler.json` |
| Input        | <https://petition.parliament.uk/petitions>                                |
| Output       |                                                                           |

---

| **Function**:| **`getlistpage-epetition()`**                                                 |
|--------------|:------------------------------------------------------------------------|
| Description  | Obtains a list of ePetitions for the specified page.  Running is scheduled by the scheduler defined by `Infrastructure \ Scheduler.json` |
| Input        | https://petition.parliament.uk/petitions?page=_Page_                    |
| Output       | For each petition --> Petition Message --> ServiceBus                   |

