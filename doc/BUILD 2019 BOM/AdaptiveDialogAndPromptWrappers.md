# Adaptive Dialog and prompt wrappers BOM - //BUILD 2019
**Ship vehicle:** 4.x-preview
**Target code complete:** 4/5/2019

**Story arc**: _As a bot developer, I’ve been using the prompt, waterfall and component dialog systems available in V4. These absolutely put me in control of managing my bot's conversations however they also require me to write a bunch of boiler plate code for sophisticated conversation modelling concepts like building a dialog dispatcher, ability handle interruptions elegantly and to build a pluggable and extensible dialog system._ 

_The new Adaptive dialog and the event model simplify sophisticated conversation modelling primitives, eliminate much of the boiler plate code and helps me focus on the model of the conversation rather than the mechanics of dialog management_

## Remaining work
- - [x] ~~Close on scope of work for //BUILD for Adaptive dialogs - [recognizers](#Recognizers), [steps](#Steps), [rules](#Rules), [interruption handling](#Interrpution-handling) supported; [prompt wrappers](#Prompt-wrappers)~~
- - [x] ~~Close on naming for rules, steps, prompt wrappers and interruption handling scopes~~
- - [x] ~~Close on scope of prompt wrappers~~
    - ~~C# implementation introduces properties on existing prompts. TS does not have any implementation of the new properties~~
    - ~~TS added new prompt++ (suffix input) that does binding to memory as well as existential check before prompting. C# does not appear to do any existential checks~~
    - ~~C# has float prompt added while TS does not have this.~~
- - [ ] Breaking change for OnContinueDialogAsync - roll back breaking change. [Steve]
    - - [ ] Redo of consultation and event bubbling [Steve]
- - [ ] Alignment on Bot + Run method. C# needs DialogManager [Tom; 4/16]
- - [ ] Prompting into turn scope - event payload values; activity [Tom; 4/17]
- - [ ] Document and ship debugger [Vishwac + Tom]
- - [ ] Input DCR on C# [Tom; Carlos]
- - [ ] ~~make sure card recognizer is part of adaptive on C# side.~~ [Tom]
- - [ ] ~~Typescript~~
    - - [ ] Packages for npm; daily updates of packages
    - - [ ] Close on support for Switch...Case..Default step. Exists in C# but not in TS. [Tom and Steve]
    - - [x] LG and common expression language integration on TS. [Steve]
    - - [ ] Typeloader and resource explorer for TS [Carlos; ETA 4/17]
    - - [ ] QnADialog step [Steve]    
    - - [ ] langauge generation renderer [Carlos]
    - - [ ] TextMessageActivityGenerator [Carlos]
    - - [ ] ActivityTemplate [Carlos]
- - [ ] Functional parity across C# and TS for planned set of steps, rules, interruption handling and prompt wrappers
    - - [ ] Class level consistency
    - - [ ] Property, Methods and method signatures are consistent
    - - [ ] Functionally consistent
- - [ ] [Documentation](#Documentation)
- - [ ] [Samples](#Samples)

Deferred to post //BUILD
- - [ ] Move dialog internal state to a dialog_internal or dialogInternal scope and not have this under 'dialog' scope. 
- - [ ] LG integration per Adaptive dialog via outputGeneration. Model this similar to recognizer.
### Post //BUILD
- - [ ] Move dialog internal state to a dialog_internal or dialogInternal scope and not have this under 'dialog' scope. 
- - [ ] Inputs
    - - [ ] dateInput
    - - [ ] timeInput
    - - [ ] attachmentInput

### Recognizers

We will support the following recognizers in Adaptive dialogs

|      Recognizer type     |         C#             |          TS            |
|--------------------------|------------------------|------------------------|
| RegexRecognizer          |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| MultiLanguageRecognizer  |<ul><li>- [x] </li></ul>|<ul><li>- [ ] </li></ul>|
| LuisRecognizer           |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| ~~QnARecognizer~~        |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|


### Rules 

**Rules** in scope for //BUILD

|          Rule name            |         C#             |          TS            |                                             Comments                                                    |
|-------------------------------|------------------------|------------------------|---------------------------------------------------------------------------------------------------------|
| unrecognizedIntentRule        |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>| rename DefaultRule -> NoMatchRule -> noneIntentRule -> unrecognizedIntentRule                                          |
| EventRule                     |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|                                                                                                         |
| IfPropertyRule                |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>| Table this - follow up; command?                                                                        |
| IntentRule                    |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|                                                                                                         |
| ~~WelcomeRule~~               |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|                                                                                                         |
| AdaptiveRule (base class)     |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>| rename rule -> AdaptiveRule and get rid of IRule                                                        |
| ~~BeginDialogRule~~           |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>| achieve via top level property as peer to rule that covers the initial set of steps when dialog begins. |
| ~~StateTransitionRule~~       |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|                                                                                                         |
| ~~ReplacePlanRule~~           |<ul><li>- [x] </li></ul>|<ul><li>- [ ] </li></ul>|                                                                                                         |
| ~~UtteranceRecognizerRule~~   |<ul><li>- [x] </li></ul>|<ul><li>- [ ] </li></ul>|                                                                                                         |
| ~~ActivityRule~~              |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|                                                                                                         |

### Steps

**Steps** in scope for //BUILD

|      Step name        |         C#             |          TS            |                             Comments                                |
|-----------------------|------------------------|------------------------|---------------------------------------------------------------------|
| BeginDialog           |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>| CallDialog -> BeginDialog                                           |
| EndDialog             |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|                                                                     |
| ReplaceDialog         |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>| GotoDialog -> ReplaceWithDialog                                     |
| CancelDialog          |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|                                                                     |
| RepeatDialog          |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|                                                                     |
| CodeStep              |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|                                                                     |
| EditArray             |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>| ChangeList -> EditArray; move to functions in expression language|
| SetProperty           |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|                                                                     |
| DeleteProperty        |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>| ClearProperty -> DeleteProperty                                     |
| IfCondition           |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>| IfProperty -> IfCondition                                           |
| SwitchCondition       |<ul><li>- [x] </li></ul>|<ul><li>- [ ] </li></ul>|                                            |
| HttpRequest           |<ul><li>- [x] </li></ul>|<ul><li>- [ ] </li></ul>| Post //BUILD - Ability for service to return activity json; logic for automatically sending typing activity after Xms                                                                         |
| SendActivity          |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|                                                                     |
| EmitEvent             |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>| EmitEvents -> EmitEvent                                             |
| EndTurn               |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>| WaitForInput -> EndTurn                                             |
| ~~EditSteps~~              |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>| Should allow plan push, pop, insert at position                     |
| ReplaceSteps           |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|                                                                     |
| QnADialog              |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|                                                                     |
| SaveEntity      |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>| SetProperty - '@EntityName' resolves to turn.entities.EntityName[0] |
| ~~EditPlanTitle~~     |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>| SetPlanTitle -> EditPlanTitle                                       |
| ~~OnCatch~~           |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|                                                                     |
| ~~ChangeCollection~~  |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>| use setproperty for objects/map                                     |
| ~~IfNotProperty~~     |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|                                                                     |
| ~~DoStepsBeforeTags~~ |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>| If we bring them back, we should align with EditPlan..              |
| ~~DoStepsLater~~      |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|                                                                     |
| ~~ForEach~~           |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|                                                                     |
| ~~ForEachPage~~       |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|                                                                     |
| ~~SendCharts~~        |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>| Do a sample with this to demonstrate a custom step.                 |
| ~~SendList~~          |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|                                                                     |

### Prompt wrappers 

We will add new \<entityType\>Input class that does the follwing - 
1. Accepts a property to bind to off the new V3 style memory scope in V4
2. Performs existential check before prompting
3. Grounds input to the specified property if the input from user machines the type of entity expected. 
4. Accepts constraints - min, max, ...

|      Input type       |         C#             |          TS            | Comments                            |
|-----------------------|------------------------|------------------------|-------------------------------------|
| confirmInput          |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>| boolInput -> confirmInput           |
| choiceInput           |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|                                     |
| numberInput           |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|                                     |
| textInput             |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|                                     |
| ~~dateInput~~             |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|                                     |
| ~~timeInput~~             |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|                                     |
| ~~attachmentInput~~       |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|                                     |
| ~~OAuthPrompt~~       |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|                                     |
| ~~ActivityPrompt~~    |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|                                     |

### Interrpution handling

CUT PlanChangeType off rule.

<strike>**Interruption plan change types** in scope for //BUILD

|     Interruption handling     |         C#             |          TS            |
|-------------------------------|------------------------|------------------------|
| doSteps                       |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>| 
| doStepsBeforeTags             |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| doStepsLater                  |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| endPlan                       |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| newPlan                       |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
| replacePlan                   |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|
</strike>


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

