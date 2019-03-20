# Input/Prompt Dialogs

## Microsoft.TextPrompt

## Microsoft.NumberPrompt

## Microsoft.DateTimePrompt

# Command and control Dialogs

## Microsoft.CallDialog

## Microsoft.GotoDialog

# Rule Dialog

# Rules

# 


# Recognizers

## Microsoft.LuisRecognizer
| Microsoft.LuisRecognizer  | IRecognizer which uses a LUIS model |
|---------------------------|-------------------------------------|
| application.applicationId | Application ID                      |
| application.endpoint      | Endpoint to use                     |
| application.endpointKey   | endpointKey to use                  |
Example:
```json
{
    "$type":"Microsoft.LuisRecognizer",
    "application": {
            "applicationId":"12312313123",
            "endpoint":"http://../",
            "endpointKey":"123123123123",
      }
}
```

## Microsoft.RegexRecognizer
Creates a dialog which uses rules to model

| property | description                        |
|----------|------------------------------------|
| intents  | Map of intentName -> regex pattern |

Example:
```json
{
    "$type":"Microsoft.RegexRecognizer",
    "intents": {
        "Greeting":"/greeting/",
        "TellMeAjoke":".*joke.*"
    }
}
```