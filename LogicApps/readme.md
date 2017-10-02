`Settings.ps1` contains definitions used to configure execution of the **LogicApps** and **Functions** on the platform. After setting up the necessary components
of the infrastructure this progresses to define:
* Schedule's:
    * Frequency
    * Interval
    * Trigger Time
* URIs for the external data.

Schedules execute functions within **LogicApps** which perform data ingestion.  