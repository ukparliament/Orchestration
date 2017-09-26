| **Function** | **getlist-territory**                                                   |
|--------------|-------------------------------------------------------------------------|
| Description  | Logic App that obtains list of territories from Government Register.  This function takes no parameters.  |
| Input        | <https://territory.register.gov.uk/records?page-size=1000&page-index=1> |
| Output       | For each territory ► territory Message ► ServiceBus                     |
