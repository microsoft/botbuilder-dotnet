- Introduces common expression language
- List use cases for common expression language

# Common Expression Language Concepts

Bots like any other application requires use of expressions to evaluate outcome of a condition based on runtime information available in memory or to the dialog or the language generation system. 

Common expression language was put together to address this core need as well as to rationalize and snap to a common expression language that will be used across Bot Builder SDK and other conversational AI components that need an expression language.

See [here](./reference-doc.md) for API reference.

***_An expression is a sequence that can contain one or more [operators](#Operators), [variables](#Variables), [explicit values](#Explicit-values), [pre-built functions](#Functions) or [Language Generation templates](../fileformats/lg/README.md#Template)._***

## Operators

| Operator	|                                  Functionality                                            |[Prebuilt function](todo) equivalent|
|-----------|-------------------------------------------------------------------------------------------|-----------------------------------|
|+          |Arithmetic operator – addition. E.g. A + B	                                                |Add                                |
|-	        |Arithmetic operator – subtraction. E.g. A – B	                                            |Sub                                |
|*	        |Arithmetic operator – multiplication. E.g. A * B	                                        |Mul                                |
|/	        |Arithmetic operator – division. E.g. A / B	                                                |Div                                |
|^	        |Arithmetic operator – exponentiation. E.g. A ^ B	                                        |Exp                                |
|%	        |Arithmetic operator – modulus. E.g. A % B	                                                |Mod                                |
|==	        |Comparison operator – equals. E.g. A == B	                                                |Equals                             |
|!=	        |Comparison operator – Not equals. E.g. A != B	                                            |Composed as Not(Equals())          |
|>	        |Comparison operator – Greater than. A > B	                                                |Greater                            |
|<	        |Comparison operator – Less than. A < B	                                                    |Less                               |
|>= 	    |Comparison operator – Greater than or equal. A >= B	                                    |greaterOrEquals                    |
|<=	        |Comparison operator – Less than or equal. A <= B	                                        |lessOrEquals                       |
|&	        |Concatenation operator. Operands will always be cast to string – E.g. A & B	            |N/A                                |
|&&	        |Logical operator – AND. E.g. exp1 && exp2	                                                |And                                |
|\|\|	    |Logical operator – OR. E.g. exp1 \|\| exp2	                                                |Or                                 |
|!	        |Logical operator – NOT. E.g. !exp1	                                                        |Not                                |
|'	        |Used to wrap a string literal. E.g. 'myValue'	                                            |N/A                                |
|"	        |Used to wrap a string literal. E.g. "myValue"	                                            |N/A                                |
|[]	        |Used to denote a Template. E.g. [MyTemplate].                                              |N/A                                |
|[]	        |Used to refer to an item in a list by its index. E.g. A[3]	                                |N/A                                |
|{}	        |Used to denote an expression. E.g. {A == B}.                                               |N/A                                |
|{}	        |Used to denote a variable in template expansion. E.g. {myVariable}	                        |N/A                                |
|()	        |Enforces precedence order and groups sub expressions into larger expressions. E.g. (A+B)*C	|N/A                                |
|.	        |Property selector. E.g. myObject.Property1	                                                |N/A                                |
|@{}	    |Used to denote parts of a multi-line value that requires evaluation	                    |N/A                                |
|\	        |Escape character for templates, expressions.                                               |N/A                                |
|@entityName|Short hand notation that expands to turn.entities.entityName                               |N/A                                |
|$propertyName|Short hand notation that expands to dialog.result.property                               |N/A                                |
|#intentName|Short hand notation that expands to turn.intents.intentName                                |N/A                                |

## Variables
Variables are always referenced by their name. E.g. {myVariable}
Variables can be complex objects. In which case they are referenced either using the property selector operator e.g. myParent.myVariable or using the item index selection operator. E.g. myParent.myList[0]. or using the [parameters](TODO) function. 

## Explicit values
Explicit values are enclosed in single quotes 'myExplicitValut' or double quotes - "myExplicitValue".

## Pre-built functions
See [Here](./prebuilt-functions.md) for a complete list of prebuilt functions supported by the common expression language library. 

**Legend**: Milestone R0 = //BUILD 2019; R1 = GA; R2 = post GA
See [here](https://docs.microsoft.com/en-us/azure/logic-apps/workflow-definition-language-functions-reference) for a complete list of functions supported by WDL. Functions listed here are a proper superset of the functions list supported by WDL.
All function names are follow camel casing for naming. 
All function names are case sensitive. 

- [String functions](#String-functions)
- [Collection functions](#Collection-functions)
- [Logical comparison functions](#Logical-comparison-functions)
- [Conversion functions](#Conversion-functions)
- [Math functions](#Math-functions)
- [Date and time functions](#Date-and-time-functions)
- [Object manipulation and construction functions](#Object-manipulation-and-construction-functions)
- [URI parsing functions](#URI-parsing-functions)

### String functions
|Function	|Explanation|	Milestone|
|-----------|-----------|------------|
|replace|	Replaces a substring within a specified string. Replace(source, oldText, newText). Case sensitive.	|R0|
|replaceIgnoreCase|	Replaces a substring within a specified string. replaceIgnoreCase(source, oldText, newText). Case in-sensitive	|R0|
|split	|Returns an array that contains substrings based on the delimiter specified. Split(sourceString, delimiterCharacter)	|R0|
|substring	|Returns characters from a string. Substring(sourceString, startPos, endPos). startPos cannot be less than 0. endPos greater than source strings length will be taken as the max length of the string	|R0|
|toLower	|Convert a string to all upper case characters	|R0|
|toUpper	|Convert a string to all lower case characters	|R0|
|trim	|Remove leading and trailing white spaces from a string	|R0|
|endsWith	|	|R1|
|startsWith	|	|R1|
|countWord	|	|R1|
|removeRepeatedSuffix	|	|R1|
|AddOrdinal	|e.g. addOrdinal(10) = 10th	|R1|
|guid	|	|R2|
|indexOf	|	|R2|
|lastIndexOf	|	|R2|

### Collection functions
|Function	|Explanation|	Milestone|
|-----------|-----------|------------|
|contains	|Works to find an item in a string or to find an item in an array or to find a parameter in a complex object. E.g. contains(‘hello world, ‘hello); contains([‘1’, ‘2’], ‘1’); contains({“foo”:”bar”}, “foo”)	|R0|
|parameters	|Returns the value of the specified parameter. Can be nested to walk through complex objects. Can include array index if parameter value is an array. e.g. for {‘foo’:’bar’}, parameters(‘foo’) will return ‘bar’; for {‘foo’: [‘bar’, ‘bar2’]}, parameters(‘foo’)[0] will return ‘bar’; for {‘foo’: {‘bar’ : ‘bar2’}}, parameters(‘foo’).bar will return ‘bar2’	|R0|
|empty	|Check if the collection is empty	|R0|
|first	|Returns the first item from the collection	|R0|
|join 	|Return a string that has all the items from an array and has each character separated by a delimiter. Join(collection, delimiter). Join(createArray(‘a’,’b’), ‘.’) = “a.b”	|R0|
|last 	|Returns the last item from the collection	|R0|
|count	|Returns the number of items in the collection	|R0|
|union	|	|R1|
|intersection 	|Returns a collection that only has common items across the specified collections	|R1|
|skip	|	|R2|
|take	|	|R2|
|item 	|	|R2|
|filterNotEqual	|	|R2|
|subArray	|Returns a sub-array from specified start and end position	|R2|

### Logical comparison functions
|Function	|Explanation|	Milestone|
|-----------|-----------|------------|
|and	|Logical and. Returns true if all specified expressions evaluate to true.	|R0|
|equals	|Comparison equal. Returns true if specified values are equal	|R0|
|greater	|Comparison greater than	|R0|
|greaterOrEquals	| Comparison greater than or equal to. greaterOrEquals(exp1, exp2)	|R0|
|if	| if(exp, valueIfTrue, valueIfFalse)	|R0|
|less	|	Comparison less than opearation|R0|
|lessOrEquals	|	Comparison less than or equal operation|R0|
|not	|	Logical not opearator|R0|
|or	| Logical OR opearation.	|R0|

### Conversion functions
|Function	|Explanation|	Milestone|
|-----------|-----------|------------|
|float	|Return floating point representation of the specified string or the string itself if conversion is not possible	|R0|
|int	|Return integer representation of the specified string or the string itself if conversion is not possible	|R0|
|json	|Return JSON type value for a string	|R0|
|string	|Return string version of the specified value	|R0|
|bool	|Return Boolean representation of the specified string. Bool(‘true’), bool(1)	|R0|
|createArray	|Create an array from multiple inputs	|R0|
|array	|Create a new array with single value 	|R2|
|binary	|	|R2|
|dataUri	|	|R2|
|dataUriToBinary	|	|R2|
|dataUriToString	|	|R2|
|decodeBase64	|	|R2|
|decodeDataUri	|	|R2|
|decodeUriComponent	|	|R2|
|encodeUriComponent	|	|R2|
|base64	|	|R2|
|base64ToBinary	|	|R2|
|base64ToString	|	|R2|
|uriComponent	|	|R2|
|uriComponentToBinary	|	|R2|
|uriComponentToString	|	|R2|
|xml	|	|R2|

### Math functions
|Function	|Explanation|	Milestone|
|-----------|-----------|------------|
|add	|Mathematical and. Accepts two parameters	|R0|
|div	|Mathematical division	|R0|
|max	|Returns the largest value from a collection	|R0|
|min	|	|R0|
|mod	|Returns remainder from dividing two numbers	|R0|
|mul	|Mathematical multiplication	|R0|
|rand	|Returns a random number between specified min and max value – rand(<minValue>, <maxValue>)	|R0|
|sub	|Mathematical subtraction	|R0|
|sum	|Returns sum of numbers in an array	|R0|
|average	|Returns the average of numbers in an array	|R0|
|exp	|Exponentiation function. Exp(base, exponent)	|R0|
|range	|	|R2|

### Date and time functions
|Function	|Explanation|	Milestone|
|-----------|-----------|------------|
|addDays	|Add number of specified days to a given timestamp	|R0|
|addHours	|Add specified number of hours to a given timestamp	|R0|
|addMinutes	|Add specified number of minutes to a given timestamp	|R0|
|addSeconds	|Add specified number of seconds to a given timestamp	|R0|
|dayOfMonth	|Returns day of month for a given timestamp or timex expression.	|R0|
|dayOfWeek	|Returns day of the week for a given timestamp	|R0|
|dayOfYear	|Returns day of the year for a given timestamp	|R0|
|formatDateTime	|https://docs.microsoft.com/en-us/azure/logic-apps/workflow-definition-language-functions-reference#formatdatetime
|R0|
|subtractFromTime	|https://docs.microsoft.com/en-us/azure/logic-apps/workflow-definition-language-functions-reference#subtractfromtime
|R0|
|utcNow	|Returns current timestamp as string	|R0|
|dateReadBack	|Use the date-time library to provide a date readback. dateReadBack(currentDate, targetDate). 
E.g. dateReadBack(‘2016/05/30’,’2016/05/23’)=>"Yesterday"	|R0|
|month	|Returns the month of given timestamp	|R0|
|date	|Returns date for a given timestamp	|R0|
|year	|Returns year for the given timestamp	|R0|
|getTimeOfDay	|Returns time of day for a given timestamp (midnight = 12AM, morning = 12:01AM – 11:59PM, noon = 12PM, afternoon = 12:01PM -05:59PM, evening = 06:00PM – 10:00PM, night = 10:01PM – 11:59PM) 	|R0|
|getFutureTime	|https://docs.microsoft.com/en-us/azure/logic-apps/workflow-definition-language-functions-reference#getfuturetime|
R1|
|getPastTime	|https://docs.microsoft.com/en-us/azure/logic-apps/workflow-definition-language-functions-reference#getpasttime|
R1|
|addToTime	|https://docs.microsoft.com/en-us/azure/logic-apps/workflow-definition-language-functions-reference#formatdatetime|
R2|
|convertFromUtc	|	|R2|
|convertTimeZone	|https://docs.microsoft.com/en-us/azure/logic-apps/workflow-definition-language-functions-reference#formatdatetime|
R2|
|convertToUtc	|	|R2|
|startOfDay	|Start of day timestamp for a given timestamp	|R2|
|startOfHour	|	|R2|
|startOfMonth	|	|R2|
|ticks	|	|R2|

### Object manipulation and construction functions
|Function	|Explanation|	Milestone|
|-----------|-----------|------------|
|json	|Convert a given string JSON notation into a JSON object	R0|
|addProperty	|Add a new property to a given json object	R0|
|removeProperty	|Remove a property from given json object	R0|
|setProperty	|Set the value for a given property in a given json object	R0|
|coalesce	|	|R2|
|xpath	|	|R2|

### URI parsing functions
|Function	|Explanation|	Milestone|
|-----------|-----------|------------|
|uriHost	|	R2|
|uriPath	|	R2|
|uriPathAndQuery|		|R2|
|uriPort	|	|R2|
|uriQuery	|	|R2|
|uriScheme	|	|R2|
