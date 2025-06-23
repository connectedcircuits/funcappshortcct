# funcappshortcct
Azure function app short circuit enabler

The `funcappshortcct` module provides a way to enable short circuiting functionality for Azure Function Apps by disabling the trigger and allowing the function to run without being triggered by an event. This is useful for when a downstream service is unavailable for a period of time, and you want to prevent the function from being triggered by events that would normally cause it to run. 