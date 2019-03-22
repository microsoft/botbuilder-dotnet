# Memory
Memory (aka state) is the data which is being tracked for your dialogs.  There are a number
of scoped memory which you can use. All memory properties are property bags, meaning you 
can store arbitrary information on them.

## User Memory
User memory is persistent data scoped to the id of the user you are conversing with.  
    
Example user memory expressions

    user.name
    user.address.city

## Conversation Memory
Conversation memory is persistent data scoped to the id of the conversation you are having.  

Example conversation memory expressions

    conversation.hasAcceptedTOU
    conversation.dateStarted
    conversation.lastMaleReference
    conversation.lastFemaleReference
    conversation.lastLocationReference

## Dialog Memory
Dialog memory is persistent data scoped for a giving executing dialog, providing an area
for each dialog to have internal persistent bookkeeping.  

Example dialog memory expressions

    dialog.orderStarted
    dialog.shoppingCart

## Turn Memory
Turn memory is non-persistent data scoped for *the current turn only*, providing a place to
share data for the lifetime of the current turn.  

Example turn memory expressions

    turn.DialogEvent
    turn.recognitionResult

# Input/Prompt Dialogs
Dialogs are used to model interactions with the user. Input/Prompt dialogs are dialogs which
gather typed information with validation.

## Microsoft.TextPrompt Dialog
The **Microsoft.TextPrompt** dialog is used to gather text input which matches pattern.

| Property        | Description                                                                 |
|-----------------|-----------------------------------------------------------------------------|
| property        | the property this input dialog is bound to                                  |
| pattern         | regex to validate input                                                     |
| initialPrompt   | the LG response to send to start the prompt                                 |
| retryPrompt     | the LG response to send to prompt the user to try again                     |
| noMatchResponse | the LG response to send to tell the user the value didn't match the pattern |

Example:
```json
{
    "$type":"Microsoft.TextPrompt",
    "property": "user.name",
    "pattern":"[0-9a-zA-Z].*",
    "initialPrompt":"What is your name?",
    "retryPrompt":"Let's try again, what is your name?",
    "noMatchResponse":"That response didn't match the pattern"
}
```


## Microsoft.IntegerPrompt Dialog
The **Microsoft.IntegerPrompt** dialog is used to gather a integer input from the user.

| Property         | Description                                                           |
|------------------|-----------------------------------------------------------------------|
| property         | the property this input dialog is bound to                            |
| minValue         | The min value which is valid                                          |
| maxValue         | The max value which is valid                                          |
| initialPrompt    | the LG response  to start the prompt                                  |
| retryPrompt      | the LG response  to prompt the user to try again                      |
| noMatchResponse  | the LG response to tell the user that a number wasn't even recognized |
| tooSmallResponse | the LG response to tell the user the value was too small              |
| tooLargeResponse | the LG response  to tell the user the value was too large             |

Example:
```json
{
    "$type":"Microsoft.IntegerPrompt",
    "property": "user.age",
    "minValue": 0,
    "maxValue": 120,
    "initialPrompt":"What is your age?",
    "retryPrompt":"Let's try again, what is your age?",
    "noMatchResponse":"I didn't recognize an age.",
    "tooSmallResponse":"Nobody is younger than 0",
    "tooLargeResponse":"You don't look Nobody is older than that!"
}
```

## Microsoft.FloatPrompt
The **Microsoft.FloatPrompt** dialog is used to gather a float input from the user.

| Property         | Description                                                           |
|------------------|-----------------------------------------------------------------------|
| property         | the property this input dialog is bound to                            |
| minValue         | The min value which is valid                                          |
| maxValue         | The max value which is valid                                          |
| initialPrompt    | the LG response  to start the prompt                                  |
| retryPrompt      | the LG response  to prompt the user to try again                      |
| noMatchResponse  | the LG response to tell the user that a number wasn't even recognized |
| tooSmallResponse | the LG response to tell the user the value was too small              |
| tooLargeResponse | the LG response  to tell the user the value was too large             |

Example:
```json
{
    "$type":"Microsoft.FloatPrompt",
    "property": "user.height",
    "minValue": 0.0,
    "maxValue": 3.0,
    "initialPrompt":"What is your height in meters?",
    "retryPrompt":"Let's try again, what is your height in meters?",
    "noMatchResponse":"I didn't recognize an number.",
    "tooSmallResponse":"Nobody is shorter than 0",
    "tooLargeResponse":"Nobody is that tall!"
}
```

## Microsoft.DateTimePrompt
The **Microsoft.DateTimePrompt** dialog is used to gather a date/time from the user.

| Property         | Description                                                                 |
|------------------|-----------------------------------------------------------------------------|
| property         | the property this input dialog is bound to                                  |
| minValue         | The min value which is valid                                                |
| maxValue         | The max value which is valid                                                |
| initialPrompt    | the LG response  to start the prompt                                        |
| retryPrompt      | the LG response  to prompt the user to try again                            |
| noMatchResponse  | the LG response to tell the user that a date or time wasn't even recognized |
| tooSmallResponse | the LG response to tell the user the value was too small                    |
| tooLargeResponse | the LG response  to tell the user the value was too large                   |

Example:
```json
{
    "$type":"Microsoft.DateTimePrompt",
    "property": "user.birthdate",
    "minValue": "1900-01-01",
    "maxValue": "2019-01-01",
    "initialPrompt":"What is your birth date?",
    "retryPrompt":"Let's try again, what is your birthdate?",
    "noMatchResponse":"I didn't recognize an date.",
    "tooSmallResponse":"I don't think anyone can be born that long ago!",
    "tooLargeResponse":"You can't be born in the future!"
}
```

# Dialog Steps
Dialog Steps are special dialog primitives which are used to control the flow of the conversation.

## Microsoft.CallDialog Step
The **Microsoft.CallDialog** step gives you the ability to call another dialog and bind it's result
to memory.  Some dialogs don't have data binding built in, and CallDialog gives you the 
ability to do that.  

> NOTE: When the called dialog is completed, the caller will continue.

| Property        | Description                                                     |
|-----------------|-----------------------------------------------------------------|
| property        | the property in memory to store the result of the called dialog |
| dialog (string) | the id (string) of a dialog to be called                        |
| dialog (object) | an inline dialog definition to be called                        |

Example CallDialog using dialogid
```json
{
    "$type":"Microsoft.CallDialog",
    "property": "user.address",
    "dialog": "GetAddressDialog"
}
```

Example CallDialog using inline dialog definition
```json
{
    "$type":"Microsoft.CallDialog",
    "property": "user.cat.name",
    "dialog": {
        "$type":"Microsoft.TextPrompt",
        "initialPrompt":"What is your cat's name?"
    }
}
```

## Microsoft.GotoDialog Step
The **Microsoft.GotoDialog** step gives you the ability to **replace** the current dialog
with another dialog and bind it's result to memory.  Some dialogs don't have 
data binding built in, and GotoDialog gives you the 
ability to do that. 

> NOTE: When the called dialog is completed, the caller will NOT continue as it has been replaced.

| Property        | Description                                                     |
|-----------------|-----------------------------------------------------------------|
| property        | the result of the called dialog will be stored in this property |
| dialog (string) | the id (string) of a dialog to be called                        |
| dialog (object) | an inline dialog definition to be called                        |

Example CallDialog using dialogid
```json
{
    "$type":"Microsoft.GotoDialog",
    "property": "user.address",
    "dialog": "GetAddressDialog"
}
```

Example GotoDialog using inline dialog definition
```json
{
    "$type":"Microsoft.GotoDialog",
    "property": "user.cat.name",
    "dialog": {
        "$type":"Microsoft.TextPrompt",
        "initialPrompt":"What is your cat's name?"
    }
}
```


## Microsoft.EndDialog Step
The **Microsoft.EndDialog** step gives you the ability to **end** the calling dialog
returning a result to caller.  

> NOTE: EndDialog ends the *caller* and returns a value

| Property | Description                                                                     |
|----------|---------------------------------------------------------------------------------|
| result   | an expression against memory which will be returned as the result to the caller |

Example returning the dialog's .address property as the result of the dialog to the caller.
```json
{
    "$type":"Microsoft.EndDialog",
    "result":"dialog.address"
}
```

## Microsoft.CancelDialog Step
The **Microsoft.CancelDailog** step gives you the ability to **cancel** the calling dialog
returning a cancelation result to the caller of the original dialog 

> NOTE: CancelDialog cancels the dialog that calls it and return cancellation event to the parent of the caller.


| Property | Description                                                                     |
|----------|---------------------------------------------------------------------------------|

Example returning the dialog's .address property as the result of the dialog to the caller.
```json
{
    "$type":"Microsoft.CancelDialog"
}
```

## Microsoft.SendActivity Step
The **Microsoft.SendActivity** step gives you the ability to send an activity to the user.  The
activity can be an inline LG template, or an actual Activity definition. 

| Property          | Description                            |
|-------------------|----------------------------------------|
| activity (string) | an inline LG template for the activity |
| activity (object) | an Activity object definition          |

Example with LG template inline 
```json
{
    "$type":"Microsoft.SendActivity",
    "activity":"Hi {user.name}. How are you?"
}
```

Example with activity  
```json
{
    "$type":"Microsoft.SendActivity",
    "activity": 
    {
        "type":"typing"
    }
}
```

## Microsoft.IfProperty Step
The **Microsoft.IfProperty** step gives you the ability to inspect memory and **branch** between dialogs.

| Property   | Description                                                                       |
|------------|-----------------------------------------------------------------------------------|
| expression | a expression to evaluate                                                          |
| ifTrue     | a dialogId or dialog or array of dialogs (steps) to execute if expression is true |
| ifFalse    | a dialogId or dialog or array of dialogs (steps) to execute if expression is true |

Example with LG template inline 
```json
{
    "$type":"Microsoft.IfProperty",
    "expression":"user.age < 18",
    "ifTrue": "TellChildrenToGoAwayDialog",
    "ifFalse": [
        "ShowTermsOfUseDialog",
        {
            "$type":"Microsoft.SendActivicty",
            "activity":"Thanks for accepting our terms of use!"
        }
    ]
}
```

# Microsoft.AdaptiveDialog
The **Microsoft.AdaptiveDialog** is a new dialog which ties everything together into an adaptive package
* It was designed to be declarative from the start
* It allows you to think in sequences but allow for rules to dynamically adjust to context.
* It supports rich eventing, interruption, cancelation and execution planning semantics
* It supports extensibility points for recognition, rules and machine learning

## Event Model
The AdaptiveDialog models input as Events called DialogEvents.  This gives us a clean model for
capturing and bubbling information such as cancellatio, requests for help, etc.

### Events
Here are the events we have defined so far.  

> NOTE: The event types and names are in flux and will probably change

| Event               | Description                                       |
|---------------------|---------------------------------------------------|
| BeginDialog         | Fired when a dialog is start                      |
| ActivityReceived    | Fired when a new activity comes in                |
| UtteranceRecognized | Fired when an intent is recognized                |
| Fallback            | Fired when nobody else has handled an event       |
| PlanStarted         | Fired when a plan is started                      |
| PlanSaved           | Fires when a plan is saved                        |
| PlanEnded           | Fires when a plan successful ends                 |
| PlanResumed         | Fires when a plan is resumed from an interruption |
| ConsultDialog       | fired when consulting                             |

## Rules
Rules are essentially plugins which are consulted when there are new events and based 
on the result and the policies active.  Every rule is made up of
* Some condition which must be satisfied
* Steps to execute (technically, steps to add to the *plan* to execute.)

There are a number of built in rules and you can easily create new rules.

### Microsoft.BeginDialogRule
The **Microsoft.BeginDialogRule** rule is used to trigger based on a dialog starting

| Property   | Description                                                                 |
|------------|-----------------------------------------------------------------------------|
| steps      | collection of dialogs/dialog steps to add to the plan if conditions are met |
| expression | additional expression as a constraint expressed against memory (OPTIONAL)   |
| changeType | policy which specifies where the steps should be inserted into the plan     |

Example
```json
{
    "$type":"Microsoft.BeginDialogRule",
    "steps": [
        ...
    ]
}
```

### Microsoft.EventRule rule
The **Microsoft.EventRule** rule is used to trigger based on events.

| Property   | Description                                                                 |
|------------|-----------------------------------------------------------------------------|
| events     | array of events to trigger on                                               |
| expression | additional expression as a constraint expressed against memory (OPTIONAL)   |
| steps      | collection of dialogs/dialog steps to add to the plan if conditions are met |
| changeType | policy which specifies where the steps should be inserted into the plan     |

Example
```json
{
    "$type":"Microsoft.EventRule",
    "events": ["ActivityReceived"],
    "expression":"user.age > 18",
    "steps": [
        ...
    ]
}
```

### Microsoft.IntentRule
The **Microsoft.IntentRule** rule is triggered if an intent and/or entities are  recognized.

| Property   | Description                                                                        |
|------------|------------------------------------------------------------------------------------|
| intent     | intent which should be recognized                                                  |
| entities   | array of names of entities which must be recognized                                |
| expression | additional expression as a constraint expressed against memory (OPTIONAL)          |
| steps      | a dialogId or inline Dialog or array of dialogIds/dialogs/steps to add to the plan |
| changeType | policy which specifies where the steps should be inserted into the plan            |

Example
```json
{
    "$type":"Microsoft.IntentRule",
    "intent":"Greeting",
    "steps": "GreetUserDialog"
}
```


### Microsoft.WelcomeRule
The **Microsoft.WelcomeRule** rule is triggered if a new plan is started or a new user is recognized.

| Property   | Description                                                                        |
|------------|------------------------------------------------------------------------------------|
| expression | additional expression as a constraint expressed against memory (OPTIONAL)          |
| steps      | a dialogId or inline Dialog or array of dialogIds/dialogs/steps to add to the plan |
| changeType | policy which specifies where the steps should be inserted into the plan            |

Example
```json
{
    "$type":"Microsoft.WelcomeRule",
    "steps": "ShowTermsOfUseDialog"
}
```

### Microsoft.DefaultRule
The **Microsoft.DefaultRule** rule is triggered only if nothing else handles an event.

| Property   | Description                                                                 |
|------------|-----------------------------------------------------------------------------|
| expression | additional expression as a constraint expressed against memory (OPTIONAL)   |
| steps      | collection of dialogs/dialog steps to add to the plan if conditions are met |
| changeType | policy which specifies where the steps should be inserted into the plan     |

Example
```json
{
    "$type":"Microsoft.DefaultRule",
    "steps": [
        ...chitchat or whatever...
    ]
}
```

## Recognizers
Recognizers are components which inspect input and generates intents and entities as output.

### Microsoft.LuisRecognizer
The **Microsoft.LuisRecognizer** is a component for using luis.ai service to generate intents 
and entities.

| Property                  | Description        |
|---------------------------|--------------------|
| applicationId | Application ID     |
| endpoint      | Endpoint to use    |
| endpointKey   | endpointKey to use |

Example:
```json
{
    "$type":"Microsoft.LuisRecognizer",
    "applicationId":"12312313123",
    "endpoint":"http://../",
    "endpointKey":"123123123123"
}
```

### Microsoft.RegexRecognizer
The **Microsoft.RegexRecognizer** is a component for using regular expressions to generate 
intents and entities.

| property | description                        |
|----------|------------------------------------|
| intents  | Map of intentName -> regex pattern |

Example:
```json
{
    "$type":"Microsoft.RegexRecognizer",
    "intents": {
        "Greeting":"/greeting/",
        "TellMeAjoke":".*joke.*",
        "Help":"/help/",
    }
}
```
