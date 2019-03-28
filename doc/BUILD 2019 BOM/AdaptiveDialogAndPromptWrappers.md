# Adaptive Dialog and prompt wrappers BOM - //BUILD 2019
**Ship vehicle:** 4.x-preview
**Target code complete:** 4/5/2019

**Story arc**: _As a bot developer, I’ve been using the prompt, waterfall and component dialog systems available in V4. These absolutely put me in control of managing my bot's conversations however they also require me to write a bunch of boiler plate code for sophisticated conversation modelling concepts like building a dialog dispatcher, ability handle interruptions elegantly and to build a pluggable and extensible dialog system._ 

_The new Adaptive dialog and the prompt wrappers simplify sohpisticated conversation modelling primitives, eliminate much of the boiler plate code and helps me focus on the model of the conversation rather than the mechanics of dialog management_

## Remaining work
- - [ ] Close on scope of work for //BUILD for Adaptive dialogs - [recognizers](#Recognizers), [steps](#Steps), [rules](#Rules), [interruption handling](#Interrpution-handling) supported; [prompt wrappers](#Prompt-wrappers)
- - [ ] Close on naming for rules, steps, prompt wrappers and interruption handling scopes
- - [ ] Close on scope of prompt wrappers
    - C# implementation introduces properties on existing prompts. TS does not have any implementation of the new properties
    - TS added new prompt++ (suffix input) that does binding to memory as well as existential check before prompting. C# does not appear to do any existential checks
    - C# has float prompt added while TS does not have this. 
- - [ ] Functional parity across C# and TS for planned set of steps, rules, interruption handling and prompt wrappers
    - - [ ] Class level consistency
    - - [ ] Property, Methods and method signatures are consistent
    - - [ ] Functionally consistent
- - [ ] [Documentation](#Documentation)
- - [ ] [Samples](#Samples)

### Recognizers

We will support the following recognizers in Adaptive dialogs

|      Recognizer type     |         C#             |          TS            |
|--------------------------|------------------------|------------------------|
| RegexRecognizer          |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| MultiLanguageRecognizer  |<ul><li>- [x] </li></ul>|<ul><li>- [ ] </li></ul>|
| LuisRecognizer           |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| QnARecognizer            |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|

### Rules 

**Rules** in scope for //BUILD

|        Rule name          |         C#             |          TS            |
|---------------------------|------------------------|------------------------|
| ActivityRule              |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| BeginDialogRule           |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| DefaultRule               |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| EventRule                 |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| IfPropertyRule            |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| IntentRule                |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| PlanningRule              |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| StateTransitionRule       |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| ReplacePlanRule           |<ul><li>- [x] </li></ul>|<ul><li>- [ ] </li></ul>|
| UtteranceRecognizerRule   |<ul><li>- [x] </li></ul>|<ul><li>- [ ] </li></ul>|
| WelcomeRule               |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|

### Steps

**Steps** in scope for //BUILD

|      Step name        |         C#             |          TS            |
|-----------------------|------------------------|------------------------|
| BaseCallDialog        |<ul><li>- [x] </li></ul>|<ul><li>- [ ] </li></ul>|
| CallDialog            |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| CancelDialog          |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| ChangeCollection      |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| ChangeList            |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| CodeStep              |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| ClearProperty         |<ul><li>- [x] </li></ul>|<ul><li>- [ ] </li></ul>|
| DoStepsBeforeTags     |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| DoStepsLater          |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| EmitEvents            |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| EndDialog             |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| ForEach               |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| ForEachPage           |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| GotoDialog            |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| IfNotProperty         |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| IfProperty            |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| HttpRequest           |<ul><li>- [x] </li></ul>|<ul><li>- [ ] </li></ul>|
| OnCatch               |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| RepeatDialog          |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| SaveEntity            |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| SendActivity          |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| SendCharts            |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| SendList              |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| SendPlanTitle         |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| SetProperty           |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| WaitForInput          |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|

### Interrpution handling

**Interruption rules** in scope for //BUILD

|     Interruption handling     |         C#             |          TS            |
|-------------------------------|------------------------|------------------------|
| doStepsAndResumeCurrentDialog |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|
| doStepsAfterCurrentDialog     |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|
| replaceDialog                 |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|
| continueCurrentDialog         |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|

### Prompt wrappers 

We will add new \<entityType\>Input class that does the follwing - 
1. Accepts a property to bind to off the new V3 style memory scope in V4
2. Performs existential check before prompting
3. Grounds input to the specified property if the input from user machines the type of entity expected. 

|      Input type       |         C#             |          TS            |
|-----------------------|------------------------|------------------------|
| boolInput             |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| chocieInput           |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| numberInput           |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| textInput             |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|


### Documentation
- [ ] API reference doc
    - [ ] List all API signatures with example invocation code snippets for C# and TS
- [ ] Conceptual document
    - [ ] Introduces Adaptive dialog
    - [ ] Lists use cases for Adaptive dialog. When would you use adaptive .vs. waterfall .vs. component .vs. roll your own

### Samples

|        Sample bot	            |         C#             |          TS            |
|-------------------------------|------------------------|------------------------|
| Core bot                      |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|
| To-do bot with regex          |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|
| To-do bot with LUIS           |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|
| To-do bot with interruptions  |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|
| To-do bot with QnA            |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|
| Cafe Bot                      |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|
| VA template                   |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|

