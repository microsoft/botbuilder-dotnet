# Common Expression Language BOM - //BUILD 2019
**Ship vehicle:** 4.x-preview
**Target code complete:** 4/15/2019

**Story arc**: _As a developer, I understand the value the common expression language provides and can use it across the Bot Builder Core SDK components including language generation & declarative dialogs._

## Remaining work
- - [ ] Implement Common Expression Language [Spec](https://microsoft.sharepoint.com/:w:/t/ConversationalAI785/EfIx5-gPhE5HlAGhvNEoKLIBo0AeoWmq1ITRhai2q2trLA?e=x9uKyb)
- - [X] Rewrite parser using ANTLR
- - [X] Implement R0 set of pre-built functions for C# and JS
    - - [X] Migrate to new style C#
    - - [X] Migrate to new style JS
- - [X] Implement support for short hand entity resolution - $entityName, #entityName, @entityName
- - [X] Close on JSON .vs. native object support in expression language
- - [X] Pick up new changes from Chris McConnell on expression evaluate, parse tree walk
- - [X] Wire up expression language in LG subsystem
- - [X] Wire up expression language support in Adaptive dialog rules and steps
- - [ ] Add support for explicit values with both 'value' and "value". [Spec](https://microsoft.sharepoint.com/:w:/t/ConversationalAI785/EfIx5-gPhE5HlAGhvNEoKLIBo0AeoWmq1ITRhai2q2trLA?e=zX8HSY)
- - [ ] New pre-built functions ask
    - - [ ] forEach(collection, iterator, expression)
    - - [ ] match(regExp)
- - [ ] Final API shape review
- - [ ] Final packaging, naming review
- - [ ] On board to build and release DevOps pipeline
- - [ ] [Documentation](#Documentation)
- - [ ] [Samples](#Samples)

### Functional parity across C# and TS

|        Class              |         C#             |          TS            |
|---------------------------|------------------------|------------------------|
| **Exression**             |                        |                        |
| BuiltInFunctions          |<ul><li>- [X] </li></ul>|<ul><li>- [X] </li></ul>|
| Constants                 |<ul><li>- [X] </li></ul>|<ul><li>- [X] </li></ul>|
| Expression                |<ul><li>- [X] </li></ul>|<ul><li>- [X] </li></ul>|
| ExpressionEvaluator       |<ul><li>- [X] </li></ul>|<ul><li>- [X] </li></ul>|
| ExpressionType            |<ul><li>- [X] </li></ul>|<ul><li>- [X] </li></ul>|
| Extensions                |<ul><li>- [X] </li></ul>|<ul><li>- [X] </li></ul>|
| IExpressionParser         |<ul><li>- [X] </li></ul>|<ul><li>- [X] </li></ul>|
| **Epxression.Parser**     |                        |                        |
| ExpressionEngine          |<ul><li>- [X] </li></ul>|<ul><li>- [X] </li></ul>|
| ErrorListener             |<ul><li>- [X] </li></ul>|<ul><li>- [X] </li></ul>|

### Documentation
- [ ] API reference doc
    - [ ] List all API signatures with example invocation code snippets for C# and TS
- [ ] Conceptual document
    - [ ] Introduces common expression language
    - [ ] Lists use cases for common expression language in Bot Builder
- [ ] Expression language specification
- [ ] Pre-built functions documentation with examples, function signatures

### Samples
No samples are planned that are specific to expression language library since this by preview is not intended to be used as a standalone library. 
- - [ ] Include pointers to samples that use expressison lanauge in
    - Language Generation samples
    - Adaptive Dialog samples
    - Declarative Dialog samples
