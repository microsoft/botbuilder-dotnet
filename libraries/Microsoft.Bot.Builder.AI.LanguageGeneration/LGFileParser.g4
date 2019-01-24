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
	: templateName newline templateBody
	;

templateName
	: HASH TEXT
	;

templateBody
	: normalTemplateBody						#normalBody
	| conditionalTemplateBody					#conditionalBody
	;

normalTemplateBody
    : (normalTemplateString newline)+
    ;

normalTemplateString
	: DASH (WS|TEXT|EXPRESSION|TEMPLATE_REF)+
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