**Function**:   **`getlist-epetition`**

**Description**:    Logic App that obtains list of epetitions from UK Government and Parliament.

**Input**:  <https://petition.parliament.uk/petitions>

**Output**: For each petition --> Petition Message --> ServiceBus           



---

**Function**:   **`getlistpage-epetition`**

**Description**:     Logic App that obtains list of epetitions from UK Government and Parliament for a specific page.  If _Page_ does not exist then no petitions are returned.

**Input**: https://petition.parliament.uk/petitions?page=_Page_

**Output**: For each petition --> Petition Message --> ServiceBus  