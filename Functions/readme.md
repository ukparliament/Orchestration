**Path**:    **Functions**

**Description**: The functions being described follow the pattern Transform*Message* where *Message* has been created by it's corresponding getlist-*Message* function in **Logic Apps**
The transformations occuring are:
*  language used in *Message* follows Parliament's ontology
*  Duplicates will not be created; rather duplicates will return the original *Message*. 

**Input**: Messages from the MessageBus
**Output**: *Message* in the standard form.