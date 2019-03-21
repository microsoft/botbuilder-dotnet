# Language Generation
Language generation is the umbrella term for generating appropriate output to the user.

Output can be a complex subject
* multiple languages for text
* personality 
* nuances for speech rendering
* cards
etc.

# .LG file format
.lg file will be a lot similar to the .lu file (to keep consistency across our files). As an overarching goal, we will use simple markdown conventions as much as possible and add additional syntax and semantics only where needed. 

See [here](../tests/Microsoft.Bot.Builder.AI.LanguageGeneration.Tests/Examples)

## Concepts expressed in .lg file
### Template 
At the core of rule based LG is the concept of a template. A template has a 
- Name
- Collection of text values or a collection of conditions with 
    - Condition expression (optional, default expression always evaluates to true)
    - Collection of text value.
- The text value in a template can refer to one or more other templates
- Text value can include SSML/ markdown content
- Circular references when nesting templates should not be supported and the service/ parser should handle this gracefully

### References to templates
Reference to another named template are denoted using [templateName] notation. 
- Optionally, you can pass specific entities to a template resolution call using [templateName(entity1, entity2)] notation.

### Entity 
- Denoted via {entityName} notation when used directly within a text value. 
- When used as a parameter within a pre-built functions or as a parameter to a template resolution call, they are simply expressed as entityName
- Entities can have one of the following types and are defined the same way as they are defined in LU files. 
    - Types: String, Int, Long, Float, Double, Bool, DateTime
        - $<entityName>:<entityType>
    - Example: 
        - $partySize : Int
        - $deliveryAddress : String
- Entities can optionally support a list of additional decorations like "Say-as" tags, "Capitalization" etc. They are represented as a list of <attribute>=<value> pairs on the entity type definition line
    - Example: 
        - $deliveryAddress : String say-as = Address

### Pre-built helper functions
These are a collection of pre-defined helper functions available for developers to use in condition evaluation or inline in a text value. 
For now, we only support [these](#Functions) pre-built functions. Of course the goal is to support a lot more (propoer sub-set of WDL functions)
- These will denoted using {functionName(param1, param2, …)} notation.
    - "isEqual(foo, bar)" as a condition expression
    - "Sure, I will set an alarm for {dateReadOut(alarmDateTime)} {timeOfDay(alarmTime)}" as a text value
- Pre-built function calls as well as template resolution calls can accept either an entity name or an explicit string value. 
    - E.g. "isEqual(foo, 'expected value')" or "[templateName(entity1, 'AM')]"
- Pre-built functions support nested calls. 
    - E.g. "isEqual(timeOfDay(alarmDateTime), 'morning')"

#### Comments
Comments are expressed via the standard markdown notation - '> this is a comment and the entire line will be treated as a comment'
- For multi-line comments, each line needs to start with the '>' prefix

### Collections
Collections are expressed using standard markdown list notation. So you can use - or * or n. as prefix
- E.g. here's a list of possible variation text values
    - Text value 1
    - Text value 2
    -	…
### Escape character
- Use '\' as escape character. E.g. "You can say cheese and tomato \[toppings are optional\]"

### Compositions
List of all possible text value composition types: 
- Simple text - "good morning"
- With entity reference - "good morning, {userName}"
- Template reference - "[wPhrase]"
- Text and template reference - "[wPhrase], Human!"
- Predefined macro use with - 
    - entity as parameter - "{timeOfDay(alarmTime)}"
    - inline string value as parameter - "{timeOfDay('2AM')}"
- Template reference with 
    - explicit entity as parameter - "[time-of-day-readout(alarmTime)]"
    - inline string value as parameter - "[time-of-day-readout('2AM')]"
- Markdown content
    - ```markdown #Possible locations -Seattle -Bellevue ```
- SSML content
    - "I will deliver the order by <emphasis>tomorrow</emphasis>"

### Template references
Template references can be a fully resolved link (for better readability and navigation in markdown renderers)
```markdown
# template1
- foo
- bar

# template2
- [template1](#template1)

# Greeting
- [template1](./1.lu#template1)
```

## Expressions in conditions
Conditions are expressed in the common expression language. An expression is a sequence that can contain one or more [operators](#Operators), [variables](#Variables), [functions](#Functions), [explicit values](#Explicit-values) or [templates](#Template)

## Operators
| Operator	| Functionality |	Function equivalent |
|-----------|---------------|-----------------------|
|==	        |Comparison operator – equals. E.g. A == B	|Equals|
|!=	        |Comparison operator – Not equals. E.g. A != B	|Composed as Not(Equals())|
|>	        |Comparison operator – Greater than. A > B	|Greater|
|<	        |Comparison operator – Less than. A < B	|Less|
|>= 	    |Comparison operator – Greater than or equal. A >= B	|greaterOrEquals|
|<=	        |Comparison operator – Less than or equal. A <= B	|lessOrEquals|
|&&	        |Logical operator – AND. E.g. exp1 && exp2	|And|
|\|\|	    |Logical operator – OR. E.g. exp1 || exp2	|Or|
|!	        |Logical operator – NOT. E.g. !exp1	|Not|
|'	        |Used to wrap a string literal. E.g. ‘myValue’	|N/A|
|[]	        |Used to denote a Template. E.g. [MyTemplate]. Used to refer to an item in a list by its index. E.g. A[3]	|N/A|
|{}	        |Used to denote an expression. E.g. {A == B}.Used to denote a variable in template expansion. E.g. {myVariable}	|N/A|
|.	        |Property selector. E.g. myObject.Property1	|N/A|
|@{}	    |Used to denote parts of a multi-line value that requires evaluation	|N/A|
|\	        |Escape character for templates, expressions. E.g. \[myTemplate\] will escape template expansion for myTemplate. 	|N/A|

## Functions
For now, we only support [these](#Functions) pre-built functions. Of course the goal is to support a lot more (propoer sub-set of WDL functions)

- count
- join
- foreach
- map
- mapjoin
- humanize

See [here](../tests/Microsoft.Bot.Builder.AI.LanguageGeneration.Tests/Examples) for additional examples that showcase pre-built function use cases. 

## Variables
Variables are always referenced by their name. E.g. {myVariable}
Variables can be complex objects. In which case they are referenced either using the property selector operator e.g. myParent.myVariable or using the item index selection operator. E.g. myParent.myList[0]. or using the parameters function. 

## Explicit values
Explicit values are enclosed in ''. E.g. 'myExplicitValue'

## Examples

```markdown
> This is a comment
```

```markdown
> Welcome Phrase template
> LG runtime will pick a text value from the one-of collection list at random.
# wPhrase
- Hi
- Hello
- Hiya 
- Hi
```

```markdown
> Welcome Phrase template
> LG runtime will pick a text value from the one-of collection list at random.
# wPhrase
- Hi
- Hello
- Hiya
 
> Using a template in another template
> Sometimes the bot will say 'Hi' and other times it will say 'Hi :)'
# welcome-user
- [wPhrase]
- [wPhrase] :)
```

```markdown
> Welcome Phrase template
> LG runtime will pick a text value from the one-of collection list at random.
# wPhrase
- Hi
- Hello
- Hiya
 
> Using a template in another template
> Sometimes the bot will say 'Hi' and other times it will say 'Hi :)'
# welcome-user
- [wPhrase]
- [wPhrase] :)
 
> Using entity references
# welcome-user
- [wPhrase]
- [wPhrase] {userName}
- [wPhrase] {userName} :)

> Conditional response template
> Outer list is condition expression; L2 list is one-of collection
# time-of-day-readout
- CASE: {timeOfDay} = morning
    - Good morning
    - Morning! 
- DEFAULT:
    - Good evening
    - Evening! 
    
> Using template references within text values
# time-of-day-readout
- CASE: {timeOfDay} = morning
    - [wPhrase] Good morning
    - Morning! 
- CASE: {timeOfDay} = evening
    - [wPhrase] Good evening
    - Evening!
- DEFAULT:
    - [wPhrase] Good night 

> Using entity references within text values
# time-of-day-readout
- CASE: {timeOfDay} = morning
    - [wPhrase] Good morning {userName}
    - Morning! 
- DEFAULT: 
    - [wPhrase] Good evening
    - Evening! 
 ```
 
 ```markdown
 > You can define entity types anywhere. Valid types are String, Int, Long, Float, Double, Bool, DateTime
$timeOfDay : datetime

> You can add say-as attributions on entities like this. This is particularly helpful for text to speech synthesis in being able to read out the entity correctly.
$ address : string say-as = Address
```

See [here](../tests/Microsoft.Bot.Builder.AI.LanguageGeneration.Tests/Examples)
