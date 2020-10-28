parser grammar LUFileParser;

options { tokenVocab=LUFileLexer; }

file
	: paragraph+? EOF
	;

paragraph
    : newline
    | nestedIntentSection
    | simpleIntentSection
    | entitySection
    | newEntitySection
    | importSection
    | referenceSection
    | qnaSection
    | modelInfoSection
    ;

// Treat EOF as newline to hanle file end gracefully
// It's possible that parser doesn't even have to handle NEWLINE, 
// but before the syntax is finalized, we still keep the NEWLINE in grammer 
newline
    : WS* (NEWLINE | EOF)
    ;

errorString
    : (WS|INVALID_TOKEN_DEFAULT_MODE)+
    ;

nestedIntentSection
    : nestedIntentNameLine nestedIntentBodyDefinition
    ;

nestedIntentNameLine
    : WS* HASH WS* nestedIntentName
    ;

nestedIntentName
    : nameIdentifier (WS|nameIdentifier)*
    ;

nameIdentifier
    : IDENTIFIER (DOT IDENTIFIER)*
    ;

nestedIntentBodyDefinition
    : subIntentDefinition+
    ;

subIntentDefinition
    : WS* HASH simpleIntentSection
    ;

simpleIntentSection
    : intentDefinition
    ;

intentDefinition
	: intentNameLine intentBody?
	;

intentNameLine
	: WS* HASH HASH? WS* intentName
	;

intentName
    : nameIdentifier (WS|nameIdentifier)*
    ;

intentBody
	: WS* normalIntentBody
	;

normalIntentBody
    : WS* ((normalIntentString newline) | errorString)+
    ;

normalIntentString
	: WS* DASH (WS|TEXT|EXPRESSION|ESCAPE_CHARACTER)*
	;

newEntitySection
    : newEntityDefinition
    ;

newEntityDefinition
    : newEntityLine newEntityListbody?
    ;

newEntityListbody
    : ((normalItemString newline) | errorString)+
    ;

newEntityLine
    : WS* AT WS* newEntityType? WS* (newEntityName|newEntityNameWithWS)? WS* newEntityRoles? WS* newEntityUsesFeatures? WS* EQUAL? WS* (newCompositeDefinition|newRegexDefinition)? newline
    ;

newCompositeDefinition
    : NEW_COMPOSITE_ENTITY
    ;

newRegexDefinition
    : NEW_REGEX_ENTITY
    ;

newEntityType
    : NEW_ENTITY_TYPE_IDENTIFIER
    ;

newEntityRoles
    : HAS_ROLES_LABEL? WS* newEntityRoleOrFeatures
    ;

newEntityUsesFeatures
    : HAS_FEATURES_LABEL WS* newEntityRoleOrFeatures
    ;

newEntityRoleOrFeatures
    : (NEW_ENTITY_IDENTIFIER|NEW_ENTITY_IDENTIFIER_WITH_WS) (WS* COMMA WS* (NEW_ENTITY_IDENTIFIER|NEW_ENTITY_IDENTIFIER_WITH_WS))*
    ;

newEntityName
    : NEW_ENTITY_IDENTIFIER (WS* PHRASE_LIST_LABEL)?
    ;

newEntityNameWithWS
    : NEW_ENTITY_IDENTIFIER_WITH_WS (WS* PHRASE_LIST_LABEL)?
    ;

entitySection
    : entityDefinition
    ;

entityDefinition
    : entityLine entityListBody?
    ;
    
entityLine
    : WS* DOLLAR entityName? COLON_MARK? entityType?
    ;

entityName
    : (ENTITY_TEXT|WS)+
    ;

entityType
    : (compositeEntityIdentifier|regexEntityIdentifier|ENTITY_TEXT|COLON_MARK|WS)+
    ;

compositeEntityIdentifier
    : COMPOSITE_ENTITY
    ;

regexEntityIdentifier
    : REGEX_ENTITY
    ;

entityListBody
    : ((normalItemString newline) | errorString)+
    ;

normalItemString
    : WS* DASH (WS|TEXT|EXPRESSION|ESCAPE_CHARACTER)*
    ;

importSection
    : importDefinition
    ;

importDefinition
    : WS* IMPORT WS*
    ;

referenceSection
    : referenceDefinition
    ;

referenceDefinition
    : WS* REFERENCE WS*
    ;

qnaSection
    : qnaDefinition
    ;

qnaDefinition
    : qnaSourceInfo? qnaIdMark? qnaQuestion moreQuestionsBody qnaAnswerBody promptSection?
    ;

qnaSourceInfo
    : WS* QNA_SOURCE_INFO
    ;

qnaIdMark
    : WS* QNA_ID_MARK
    ;
    
qnaQuestion
    : WS* QNA questionText
    ;

questionText
    : QNA_TEXT*
    ;

moreQuestionsBody
    : WS* ((moreQuestion newline) | errorQuestionString)*
    ;

moreQuestion
    : DASH (WS|TEXT)*
    ;

errorQuestionString
    : (WS|INVALID_TOKEN_DEFAULT_MODE)+
    ;

qnaAnswerBody
    : ((filterSection? multiLineAnswer)|(multiLineAnswer filterSection?))
    ;

filterSection
    : WS* FILTER_MARK (filterLine | errorFilterLine)+
    ;

promptSection
    : WS* PROMPT_MARK (filterLine | errorFilterLine)+
    ;

filterLine
    : WS* DASH (WS|TEXT)* newline
    ;

errorFilterLine
    : (WS|INVALID_TOKEN_DEFAULT_MODE)+
    ;

multiLineAnswer
    : WS* MULTI_LINE_TEXT
    ;

modelInfoSection
    : modelInfoDefinition
    ;

modelInfoDefinition
    : WS* MODEL_INFO
    ;