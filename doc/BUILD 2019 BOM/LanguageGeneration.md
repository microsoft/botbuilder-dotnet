# Language Generation BOM - //BUILD 2019
**Ship vehicle:** 4.x-preview
**Target code complete:** 4/15/2019

**Story arc**: _As a bot developer, I’ve been used to hard coding bot responses and related resources. With .LG file format, MSLG CLI tool, LG resolver runtime, common expression library I can completely de-couple language resources for my bot and manage them in separate files like I have been doing for LU. With that I can now make my bot sound more natural and lively.​_

## Remaining work
- - [X] Find new home for LanguageGenerationRenderer, LGLanguageGenerator
- - [X] Expression analyzer (CCI ask) – we will just provide parse tree + sample. Nothing baked into the library​
- - [X] Wrap up C# and JS libraries – improved library error and exception bubbling, actionable and informative exception messages.​
- - [ ] Parse, collate and translate TS/ JS library that can be surfaced through a CLI tool (or consumed by an UI based authoring experience)
    - - [X] Expose parse functionality through a CLI shell (snap to new one CLI spec from Eyal)
    - - [X] Expose collate
    - - [ ] Expose translate [P1]
- - [X] Support get all parsedTemplate and combine with AnalyzeTemplate API, user can get all template analyzing results
- - [X] Support handle multiple files in TemplateEngine
- - [X] Update implementation to match IF .. ELSEIF instead of the current SWITCH label. [Spec](https://microsoft-my.sharepoint.com/:w:/p/vkannan/ERMS_VL3nEBIhNwlgYAmv-8BIUP3WCM3-XSY-fETqjFOxw?e=0f8zYA)
- - [ ] [Post //BUILD] ~~Add support for SWITCH .. CASE construct in conditional response template. [Spec](https://microsoft-my.sharepoint.com/:w:/p/vkannan/ERMS_VL3nEBIhNwlgYAmv-8BIUP3WCM3-XSY-fETqjFOxw?e=0f8zYA)~~
- - [ ] Final API shape review
- - [ ] Final packaging, naming review
- - [ ] On board to build and release DevOps pipeline
- - [ ] [Documentation](#Documentation)
- - [ ] [Samples](#Samples)

### Functional parity across C# and TS

|           Class              |         C#             |          TS            |
|------------------------------|------------------------|------------------------|
| TextMessageActivityGenerator |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| Analyzer                     |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| Evaluator                    |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| ExpressionAnalyzerVisitor    |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| GetMethodExtension           |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| LanguageGenerationRenderer   |<ul><li>- [x] </li></ul>|<ul><li>- [ ] </li></ul>|
| LGLanguageGenerator          |<ul><li>- [x] </li></ul>|<ul><li>- [ ] </li></ul>|
| StaticChecker                |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| TemplateEngine               |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| ErrorListener                |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| LGTemplate                   |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| LGFileParserVisitor          |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| LGFileParserListener         |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|

### Documentation
- [ ] API reference doc
    - [ ] List all API signatures with example invocation code snippets for C# and TS
- [x] Conceptual document
    - - [x] Language Generation concepts overview
    - - [x] List scenarios based use cases for Language Generation
    - - [x] Ability to specify cards via Language Generation
    - - [x] Ability to specify speak .vs. display variation via Languge Generation
    - - [x] Resoure manager and langauge fall back policies
    - - [x] Grammar correction and other generation capabilites
- [x] .LG file format specification
    - - [x] Include inline .LG file snippets as example for each concept
### Samples

|C#|TS| Sample bot	| Scenarios	 | LG capabilities to demo	| Notes	| Bot/LG Template |
|--|--|---------------|------------|--------------------------|-------|-----------------|
|<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>| MultiTurn Prompt	| Welcome, ask for name, confirm name, ask for age, confirm age, Summary	|1. Pass variables to template Template 2. Template reference - Summary template can reference to name/age confirmation | | template |
|<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>| Card bot	| Send 8 different card types	| Define cards in a template |	|	Template |
|<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>| Adaptive card bot	| Send adaptive card	| Define adaptive card in a template	|	|Template|
|<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>| Suggested action bot	| Send suggested action	 |Define suggested action in a template	|  |	Template|	
|<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>| Core bot	| Core bot template	|No new function. Reference code to use various templates | |	Optional|	
|<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>| Multi-language	| Support multi-language	| Use bot resoure manager and languge fall back policy |	|	Bot |
|<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|Cafe bot | Full blown cafe bot that shows sohpisticated LG use, LG for QnA pairs | All LG concepts | | Bot |

## Post BUILD backlog
- - [ ] Support for SWITCH .. CASE .. DEFAULT in conditional response templates
- - [ ] Support for file scoped template reference in evaluation/ expansion
- - [ ] Support for external file references with json card definitions (Steve Ickman's suggestion)
- - [ ] Plugin to telemetry pipeline for bot analytics.
