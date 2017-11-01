Two APIs to retrieve ePetitions from Parliament's website.

The first only gets the first page of ePetitions.  This includes a header which provides the _total number of
pages_, allowing the looping through all pages using the second function.  The logic for this is
controlled and contained in the *getlist-epetition* Logic App, getting the first page and then looping through
all pages.


| **Function**    | getlist-epetition()                                                          |
|-----------------|:-----------------------------------------------------------------------------|
| **Description** | Executed by the schedule defined in `Infrastructure \ Scheduler.json` which triggers the *getlist-epetition* Logic App.  This API obtains the first page of ePetitions, and includes the _total number of pages_.   |
| **Input**       | https://petition.parliament.uk/petitions                                     |
| **Output**      | _Total number of pages_                                                      |

---

| **Function**     | getlistpage-epetition(<page>)                                           |
|------------------|:------------------------------------------------------------------------|
| **Description**  | Obtains a list of ePetitions for the specified <page>.  This is executed by the *getlist-epetition* Logic App after processing the response from **getlist-epetition** |
| **Input**        | https://petition.parliament.uk/petitions?page=_pageNumber_                    |
| **Output**       | For each petition --> Petition Message --> ServiceBus                   |
