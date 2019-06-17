﻿grammar CommonRegex;

parse
 : alternation EOF
 ;

alternation
 : expr ('|' expr)*
 ;

expr
 : element*
 ;

element
 : atom quantifier?
 ;

// QUANTIFIERS
//
//         ?           0 or 1, greedy
//         ?+          0 or 1, possessive
//         ??          0 or 1, lazy
//         *           0 or more, greedy
//         *+          0 or more, possessive
//         *?          0 or more, lazy
//         +           1 or more, greedy
//         ++          1 or more, possessive
//         +?          1 or more, lazy
//         {n}         exactly n
//         {n,m}       at least n, no more than m, greedy
//         {n,m}+      at least n, no more than m, possessive
//         {n,m}?      at least n, no more than m, lazy
//         {n,}        n or more, greedy
//         {n,}+       n or more, possessive
//         {n,}?       n or more, lazy

quantifier
 : '?' quantifier_type
 | '+' quantifier_type
 | '*' quantifier_type
 | '{' number '}' quantifier_type
 | '{' number ',' '}' quantifier_type
 | '{' number ',' number '}' quantifier_type
 ;

quantifier_type
 : '+'
 | '?'
 | /* nothing */
 ;

// CHARACTER CLASSES
//
//         [...]       positive character class
//         [^...]      negative character class
//         [x-y]       range (can be used for hex characters)

character_class
 : '[' '^' cc_atom+ ']'
 | '[' cc_atom+ ']'
 ;

// CAPTURING
//
//         (...)           capturing group
//         (?<name>...)    named capturing group (.NET, javascript)
//         (?:...)         non-capturing group
//         (?|...)         non-capturing group; reset group numbers for
//                          capturing groups in each alternative

capture
 : '(' '?' '<' name '>' alternation ')'
 | '(' alternation ')'
 ;

non_capture
 : '(' '?' ':' alternation ')'
 | '(' '?' '|' alternation ')'
 ;

// OPTION SETTING(C# format, for javascript should be wrapped in code)
//
//         (?i)            caseless
//         (?J)            allow duplicate names
//         (?m)            multiline
//         (?s)            single line (dotall)
//         (?U)            default ungreedy (lazy)
//         (?x)            extended (ignore white space)

option
 : '(' '?' option_flag+ ')'
 ;

option_flag
 : 'i'
 | 'J'
 | 'm'
 | 's'
 | 'U'
 | 'x'
 ;

atom
 : shared_atom
 | literal
 | character_class
 | capture
 | non_capture
 | option
 | Dot
 | Caret
 | StartOfSubject
 | EndOfSubjectOrLine
 | OneDataUnit
 | ExtendedUnicodeChar
 ;

cc_atom
 : cc_literal Hyphen cc_literal
 | shared_atom
 | cc_literal
 ;

shared_atom
 : ControlChar
 | DecimalDigit
 | NotDecimalDigit
 | HorizontalWhiteSpace
 | NotHorizontalWhiteSpace
 | NotNewLine
 | CharWithProperty
 | CharWithoutProperty
 | NewLineSequence
 | WhiteSpace
 | NotWhiteSpace
 | VerticalWhiteSpace
 | NotVerticalWhiteSpace
 | WordChar
 | NotWordChar 
 ;

literal
 : shared_literal
 | CharacterClassEnd
 ;

cc_literal
 : shared_literal
 | Dot
 | CharacterClassStart
 | Caret
 | QuestionMark
 | Plus
 | Star
 | EndOfSubjectOrLine
 | Pipe
 | OpenParen
 | CloseParen
 ;

shared_literal
 : octal_char
 | letter
 | digit
 | BellChar
 | EscapeChar
 | FormFeed
 | NewLine
 | CarriageReturn
 | Tab
 | HexChar
 | OpenBrace
 | CloseBrace
 | Comma
 | Hyphen
 | LessThan
 | GreaterThan
 | SingleQuote
 | Underscore
 | Colon
 | Hash
 | Equals
 | Exclamation
 | Ampersand
 | OtherChar
 ;

number
 : digits
 ;

octal_char
 : ( Backslash (D0 | D1 | D2 | D3) octal_digit octal_digit
   | Backslash octal_digit octal_digit                     
   )

 ;

octal_digit
 : D0 | D1 | D2 | D3 | D4 | D5 | D6 | D7
 ;
 
digits
 : digit+
 ;

digit
 : D0 | D1 | D2 | D3 | D4 | D5 | D6 | D7 | D8 | D9
 ;

name
 : alpha_nums
 ;

alpha_nums
 : (letter | Underscore) (letter | Underscore | digit)*
 ;
 
non_close_parens
 : non_close_paren+
 ;

non_close_paren
 : ~CloseParen
 ;

letter
 : ALC | BLC | CLC | DLC | ELC | FLC | GLC | HLC | ILC | JLC | KLC | LLC | MLC | NLC | OLC | PLC | QLC | RLC | SLC | TLC | ULC | VLC | WLC | XLC | YLC | ZLC |
   AUC | BUC | CUC | DUC | EUC | FUC | GUC | HUC | IUC | JUC | KUC | LUC | MUC | NUC | OUC | PUC | QUC | RUC | SUC | TUC | UUC | VUC | WUC | XUC | YUC | ZUC
 ;


// CHARACTERS
//
//         \a         alarm, that is, the BEL character (hex 07)
//         \cx        "control-x", where x is any ASCII character
//         \e         escape (hex 1B)
//         \f         form feed (hex 0C)
//         \n         newline (hex 0A)
//         \r         carriage return (hex 0D)
//         \t         tab (hex 09)
//         \ddd       character with octal code ddd, or backreference
//         \xhh       character with hex code hh
//         \x{hhh..}  character with hex code hhh..
BellChar       : '\\a';
ControlChar    : '\\c';
EscapeChar     : '\\e';
FormFeed       : '\\f';
NewLine        : '\\n';
CarriageReturn : '\\r';
Tab            : '\\t';
Backslash      : '\\';
HexChar        : '\\x' ( HexDigit HexDigit
                       | '{' HexDigit HexDigit HexDigit+ '}'
                       )
               ;

// CHARACTER TYPES
//
//         .          any character except newline;
//                      in dotall mode, any character whatsoever
//         \C         one data unit, even in UTF mode (best avoided)
//         \d         a decimal digit
//         \D         a character that is not a decimal digit
//         \h         a horizontal white space character
//         \H         a character that is not a horizontal white space character
//         \N         a character that is not a newline
//         \p{xx}     a character with the xx property
//         \P{xx}     a character without the xx property
//         \R         a newline sequence
//         \s         a white space character
//         \S         a character that is not a white space character
//         \v         a vertical white space character
//         \V         a character that is not a vertical white space character
//         \w         a "word" character
//         \W         a "non-word" character
//         \X         an extended Unicode sequence

Dot                     : '.';
OneDataUnit             : '\\C';
DecimalDigit            : '\\d';
NotDecimalDigit         : '\\D';
HorizontalWhiteSpace    : '\\h';
NotHorizontalWhiteSpace : '\\H';
NotNewLine              : '\\N';
CharWithProperty        : '\\p{' UnderscoreAlphaNumerics '}';
CharWithoutProperty     : '\\P{' UnderscoreAlphaNumerics '}';
NewLineSequence         : '\\R';
WhiteSpace              : '\\s';
NotWhiteSpace           : '\\S';
VerticalWhiteSpace      : '\\v';
NotVerticalWhiteSpace   : '\\V';
WordChar                : '\\w';
NotWordChar             : '\\W';
ExtendedUnicodeChar     : '\\X';

CharacterClassStart  : '[';
CharacterClassEnd    : ']';
Caret                : '^';
Hyphen               : '-';

QuestionMark : '?';
Plus         : '+';
Star         : '*';
OpenBrace    : '{';
CloseBrace   : '}';
Comma        : ',';

// ANCHORS AND SIMPLE ASSERTIONS
//
//         ^           start of subject
//                      also after internal newline in multiline mode
//         \A          start of subject
//         $           end of subject
//                      also before newline at end of subject
//                      also before internal newline in multiline mode

StartOfSubject                 : '\\A'; 
EndOfSubjectOrLine             : '$';

Pipe        : '|';
OpenParen   : '(';
CloseParen  : ')';
LessThan    : '<';
GreaterThan : '>';
SingleQuote : '\'';
Underscore  : '_';
Colon       : ':';
Hash        : '#';
Equals      : '=';
Exclamation : '!';
Ampersand   : '&';

ALC : 'a';
BLC : 'b';
CLC : 'c';
DLC : 'd';
ELC : 'e';
FLC : 'f';
GLC : 'g';
HLC : 'h';
ILC : 'i';
JLC : 'j';
KLC : 'k';
LLC : 'l';
MLC : 'm';
NLC : 'n';
OLC : 'o';
PLC : 'p';
QLC : 'q';
RLC : 'r';
SLC : 's';
TLC : 't';
ULC : 'u';
VLC : 'v';
WLC : 'w';
XLC : 'x';
YLC : 'y';
ZLC : 'z';

AUC : 'A';
BUC : 'B';
CUC : 'C';
DUC : 'D';
EUC : 'E';
FUC : 'F';
GUC : 'G';
HUC : 'H';
IUC : 'I';
JUC : 'J';
KUC : 'K';
LUC : 'L';
MUC : 'M';
NUC : 'N';
OUC : 'O';
PUC : 'P';
QUC : 'Q';
RUC : 'R';
SUC : 'S';
TUC : 'T';
UUC : 'U';
VUC : 'V';
WUC : 'W';
XUC : 'X';
YUC : 'Y';
ZUC : 'Z';

D1 : '1';
D2 : '2';
D3 : '3';
D4 : '4';
D5 : '5';
D6 : '6';
D7 : '7';
D8 : '8';
D9 : '9';
D0 : '0';

OtherChar : . ;

// fragments
fragment UnderscoreAlphaNumerics : ('_' | AlphaNumeric)+;
fragment AlphaNumerics           : AlphaNumeric+;
fragment AlphaNumeric            : [a-zA-Z0-9];
fragment NonAlphaNumeric         : ~[a-zA-Z0-9];
fragment HexDigit                : [0-9a-fA-F];
fragment ASCII                   : [\u0000-\u007F];

