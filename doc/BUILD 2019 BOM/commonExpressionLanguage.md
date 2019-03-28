# Common Expression Language BOM - //BUILD 2019
**Ship vehicle:** 4.x-preview
**Target code complete:** 4/15/2019

**Story arc**: _As a developer, I understand the value the common expression language provides and can use it across the Bot Builder Core SDK components including language generation & declarative dialogs._

## Remaining work
- - [ ] Implement Common Expression Language [Spec](https://microsoft.sharepoint.com/:w:/t/ConversationalAI785/EfIx5-gPhE5HlAGhvNEoKLIBo0AeoWmq1ITRhai2q2trLA?e=x9uKyb)
- - [ ] Rewrite parser using ANTLR
- - [ ] Implement R0 set of pre-built functions for C# and JS
- - [ ] Implement support for short hand entity resolution - $entityName, #entityName, @entityName
- - [ ] Close on JSON .vs. native object support in expression language
- - [ ] Pick up new changes from Chris McConnell on expression evaluate, parse tree walk
- - [ ] Wire up expression language in LG subsystem
- - [ ] Wire up expression language support in Adaptive dialog rules and steps
- - [ ] Final API shape review
- - [ ] Final packaging, naming review
- - [ ] On board to build and release DevOps pipeline
- - [ ] [Documentation](#Documentation)
- - [ ] [Samples](#Samples)

### Functional parity across C# and TS

|        Class              |         C#             |          TS            |
|---------------------------|------------------------|------------------------|
| BuiltInFunctions          |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|
| Exceptions                |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|
| ExpressionEngine          |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|
| ExpressionErrorListener   |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|
| ExpressionEvaluator       |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|
| MethodBinder              |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|
| PropertyBinder            |<ul><li>- [ ] </li></ul>|<ul><li>- [ ] </li></ul>|

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
