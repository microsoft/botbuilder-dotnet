parser grammar LGTemplateParser;

@parser::header {#pragma warning disable 3021} // Disable StyleCop warning CS3021 re CLSCompliant attribute in generated files.
@lexer::header {#pragma warning disable 3021} // Disable StyleCop warning CS3021 re CLSCompliant attribute in generated files.

options { tokenVocab=LGTemplateLexer; }

context: body EOF;

body
    : normalTemplateBody                        #normalBody
    | ifElseTemplateBody                        #ifElseBody
    | switchCaseTemplateBody                    #switchCaseBody
    | structuredTemplateBody                    #structuredBody
    ;

structuredTemplateBody
    : structuredBodyNameLine (((structuredBodyContentLine? STRUCTURED_NEWLINE) | errorStructureLine)+)? structuredBodyEndLine?
    ;

structuredBodyNameLine
    : LEFT_SQUARE_BRACKET (STRUCTURE_NAME | errorStructuredName)
    ;

errorStructuredName
    : (STRUCTURE_NAME|TEXT_IN_STRUCTURE_NAME)*
    ;

structuredBodyContentLine
    : keyValueStructureLine
    | objectStructureLine
    ;

errorStructureLine
    : (STRUCTURE_IDENTIFIER|STRUCTURE_EQUALS|STRUCTURE_OR_MARK|TEXT_IN_STRUCTURE_BODY|EXPRESSION_IN_STRUCTURE_BODY|ESCAPE_CHARACTER_IN_STRUCTURE_BODY)+
    ;

keyValueStructureLine
    : STRUCTURE_IDENTIFIER STRUCTURE_EQUALS keyValueStructureValue (STRUCTURE_OR_MARK keyValueStructureValue)*
    ;

keyValueStructureValue
    : (TEXT_IN_STRUCTURE_BODY|EXPRESSION_IN_STRUCTURE_BODY|ESCAPE_CHARACTER_IN_STRUCTURE_BODY)+
    ;

objectStructureLine
    : EXPRESSION_IN_STRUCTURE_BODY
    ;

structuredBodyEndLine
    : STRUCTURED_BODY_END
    ;

normalTemplateBody
    : templateString+
    ;

templateString
    : normalTemplateString
    | errorTemplateString
    ;

normalTemplateString
    : DASH MULTILINE_PREFIX? (TEXT|EXPRESSION|ESCAPE_CHARACTER)* MULTILINE_SUFFIX?
    ;

errorTemplateString
	: INVALID_TOKEN+
	;

ifElseTemplateBody
    : ifConditionRule+
    ;

ifConditionRule
    : ifCondition normalTemplateBody?
    ;

ifCondition
    : DASH (IF|ELSE|ELSEIF) (WS|TEXT|EXPRESSION)*
    ;

switchCaseTemplateBody
    : switchCaseRule+
    ;

switchCaseRule
    : switchCaseStat normalTemplateBody?
    ;

switchCaseStat
    : DASH (SWITCH|CASE|DEFAULT) (WS|TEXT|EXPRESSION)*
    ;
