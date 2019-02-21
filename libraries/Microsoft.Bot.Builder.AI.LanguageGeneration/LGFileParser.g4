parser grammar LGFileParser;

options { tokenVocab=LGFileLexer; }

file
	: paragraph+? EOF
	;

paragraph
    : newline
    | templateDefinition
    ;

// Treat EOF as newline to hanle file end gracefully
// It's possible that parser doesn't even have to handle NEWLINE, 
// but before the syntax is finalized, we still keep the NEWLINE in grammer 
newline
    : NEWLINE
    | EOF
    ;

templateDefinition
	: templateNameLine newline templateBody
	;

templateNameLine
	: HASH templateName parameters?
	;

templateName
    : IDENTIFIER (DOT IDENTIFIER)*
    ;

parameters
    : OPEN_PARENTHESIS IDENTIFIER (COMMA IDENTIFIER)* CLOSE_PARENTHESIS
    ;

templateBody
	: normalTemplateBody						#normalBody
	| conditionalTemplateBody					#conditionalBody
	;

normalTemplateBody
    : (normalTemplateString newline)+
    ;

normalTemplateString
	: DASH (WS|TEXT|EXPRESSION|TEMPLATE_REF|TEXT_SEPARATOR|MULTI_LINE_TEXT)*
	;

conditionalTemplateBody
    : caseRule+ defaultRule
    ;

caseRule
    : caseCondition newline normalTemplateBody 
    ;

defaultRule
    : defaultCondition newline normalTemplateBody
    ;

caseCondition
	: DASH CASE EXPRESSION
	;
defaultCondition
    : DASH DEFAULT
    ;