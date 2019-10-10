# Declarative Dialog and Prompts  BOM - //BUILD 2019
**Doc status: draft (review is needed)**

**Ship vehicle:** 4.x-preview

**Target code complete:** 4/5/2019

**Story arc**: _As a developer, I want to create a sophisticated conversation using the new Adaptive Dialog and Prompts and express them in a declarative form. The a declarative form (JSON) has a 1:1 mapping to core SDK concepts and represents SDK objects. As a develoepr, I can use the declarative form to represent dialgos in programing langauge agnostic way and transfar them between bots/ systems._ 

## Remaining work
- [] Update C# schemas to the latest names and drive consistency across C# and jS
- [] Implement TypeLoader for Javascript (TS) to parity with C# (supporting .lg, .lu, .dialog)
- [] Update JS schema to latest names and drive concistency acorss JS and C#
- [] Identify, review and agree on scope of SDK objects exposed by declarative schemas
- [] Update CLI tools (DialogSchema and DialogLint) to match latest schema updates
- [] [Documenation](#Documentation)
- [] [Samples](#Samples)


### Recognizers

We will support the following recognizers in declarative form 

|      Recognizer type     |         Schema         |
|--------------------------|------------------------|
| RegexRecognizer          |<ul><li>- [x] </li></ul>|
| MultiLanguageRecognizer  |<ul><li>- [x] </li></ul>|
| LuisRecognizer           |<ul><li>- [x] </li></ul>|
| QnARecognizer            |<ul><li>- [ ] </li></ul>|

### Rules 


**Rules** in scope for //BUILD

We will support the following rules (within Adaptive Dialog) in declarative form 

|          Rule name            |         Schema         |                                             Comments                                                    |
|-------------------------------|------------------------|---------------------------------------------------------------------------------------------------------|
| NoMatchRule                   |<ul><li>- [x] </li></ul>|                                                                                                         |
| EventRule                     |<ul><li>- [x] </li></ul>|                                                                                                         |
| IfPropertyRule                |<ul><li>- [x] </li></ul>|                                                                                                         |
| IntentRule                    |<ul><li>- [x] </li></ul>|                                                                                                         |
| WelcomeRule                   |<ul><li>- [x] </li></ul>|                                                                                                         |
| AdaptiveRule (base class)     |<ul><li>  NA  </li></ul>| Not being used in declarative form                                                                      |


**Declarative Dialogs** in scope for //Build

|          Dialog name          |         Schema         |                                             Comments                                                    |
|-------------------------------|------------------------|---------------------------------------------------------------------------------------------------------|
| AdaptiveDialog                |<ul><li>- [x] </li></ul>| The only supported dialog in declarative form?                                                          |


### Prompt 

**Input wrappers**  in scope for //BUILD

We will support the following input wrappers (over SDK prompts) in declarative form.

|      Input type       |         Schema         | Comments                                      |
|-----------------------|------------------------|-----------------------------------------------|
| confirmInput          |<ul><li>- [x] </li></ul>| boolInput -> confirmInput                     |
| choiceInput           |<ul><li>- [ ] </li></ul>|                                               |
| NumberInput           |<ul><li>- [ ] </li></ul>| Lock on which number/int/flot input we support|
| IntegerInput          |<ul><li>- [x] </li></ul>|                                               |
| FloatInput            |<ul><li>- [x] </li></ul>|                                               |
| textInput             |<ul><li>- [x] </li></ul>|                                               |
| dateInput             |<ul><li>- [ ] </li></ul>|                                               |
| timeInput             |<ul><li>- [ ] </li></ul>|                                               |
| attachmentInput       |<ul><li>- [ ] </li></ul>|                                               |


### Steps

**Steps** in scope for //BUILD

|      Step name        |         Schema         |                             Comments                                |
|-----------------------|------------------------|---------------------------------------------------------------------|
| BeginDialog           |<ul><li>- [x] </li></ul>|                                                                     |
| EndDialog             |<ul><li>- [x] </li></ul>|                                                                     |
| ReplaceWithDialog     |<ul><li>- [x] </li></ul>|                                                                     |
| CancelDialog          |<ul><li>- [x] </li></ul>|                                                                     |
| RepeatWithDialog      |<ul><li>- [ ] </li></ul>|                                                                     |
| CodeStep              |<ul><li>- [ ] </li></ul>| Do we want to support for Declarative (if so when?)                 |
| EditArray             |<ul><li>- [x] </li></ul>|                                                                     |
| SaveEntity            |<ul><li>- [x] </li></ul>|                                                                     |
| SetProperty           |<ul><li>- [x] </li></ul>| Need to figure right name compare to SaveEntity/SetProperty         |
| DeleteProperty        |<ul><li>- [x] </li></ul>| Need to figure right name compare to SaveEntity                     |
| IfCondition           |<ul><li>- [x] </li></ul>|                                                                     |
| HttpRequest           |<ul><li>- [x] </li></ul>|                                                                     |
| SendActivity          |<ul><li>- [x] </li></ul>|                                                                     |
| EmitEvent             |<ul><li>- [ ] </li></ul>| Do we want to support this in declarative?                          |
| EndTurn               |<ul><li>- [x] </li></ul>|                                                                     |
| SwitchCondision       |<ul><li>- [ ] </li></ul>|                                                                     |



### Prompt wrappers 

|      Input type       |         C#             |          TS            | Comments                            |
|-----------------------|------------------------|------------------------|-------------------------------------|
| confirmInput          |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>| boolInput -> confirmInput           |
| choiceInput           |<ul><li>- [ ] </li></ul>|<ul><li>- [x] </li></ul>|                                     |
| numberInput           |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|                                     |
| textInput             |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|                                     |
| dateInput             |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|                                     |
| timeInput             |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|                                     |
| attachmentInput       |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|                                     |



### Documentation
- [ ] Conceptual document
    - - [ ] Declarative represenation of dilaogs (schema, memory, expressions, rules, steps)
    - - [ ] Top level schema overview ($Type; $Copy; etc.) 
    - - [ ] Object and Type loading 
    - - [ ] Refernces to LG and Adaptive Dialog 
- [ ] Schema  specification
    - - [ ] Include inline of supported schema types (point to git vs docs?)

### Samples
TBD
Declarative sample per type(?)
