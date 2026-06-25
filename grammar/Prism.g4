grammar Prism;

compilationUnit: declaration* EOF;

declaration: variableDeclaration | functionDeclaration;

variableDeclaration: EXTERN? VAR MUT? IDENTIFIER typeSpecifier? ('=' expression)? ';';

functionDeclaration: EXTERN? FUNC IDENTIFIER '(' parameterList? ')' returnType? (functionBody | ';');

parameter: MUT? IDENTIFIER typeSpecifier?;

parameterList: parameter (',' parameter)*;

returnType: '->' type_reference;

block: '{' statement* '}';

expression_body: '=>' expression;

functionBody: block
| (expression_body ';');

typeSpecifier: ':' type_reference;

type_reference: IDENTIFIER;

statement: variableDeclaration | returnStatement | expressionStatement | block;

expressionStatement: expression ';';

returnStatement: RETURN expression ';';

literal: TRUE | FALSE | FLOAT | INT | STRING | CHARACTER;

argumentList: argument (',' argument)*;

argument: argumentName? argumentValue;

argumentName: IDENTIFIER ':';

argumentValue: expression;

simpleName: IDENTIFIER;

namedEntity: simpleName;

expression: literal #literalExpression
    | simpleName #identifierExpression
    | '(' expression ')' #parenthesizedExpression
    | expression '(' argumentList ')' #invocationExpression
    | expression postfixOperator #postfixExpression
    | 'nameof' '(' namedEntity ')' #nameofExpression
    | unaryOperator expression #unaryExpression
    | expression multiplicitiveOperator expression #multiplicitiveExpression
    | expression additiveOperator expression # additiveExpression
    | expression shiftOperator expression #shiftExpression
    | expression '<=>' expression #threeWayComparisonExpression
    | expression comparisonOperator expression #comparisonExpression
    | expression equalityOperator expression #equalityExpression
    | expression '&' expression #bitwiseAndExpression
    | expression '^' expression #bitwiseXorExpression
    | expression '|' expression #bitwiseOrExpression
    | expression '&&' expression #logicalAndExpression
    | expression '||' expression #logicalOrExpression
    | expression '??' expression #nullCoallesingExpression
    | expression '?' expression ':' expression #ternaryExpression
    | expression '=' expression #assignment
    | expression compoundAssignmentOperator expression #compoundAssignment;

postfixOperator: '++' | '--';

unaryOperator: '++' | '--' | '+' |'-' |'~' |'!';

multiplicitiveOperator: '*' | '/' | '%';

additiveOperator: '+' | '-';

shiftOperator: '<<' | rightShift | unsignedRightShift;

rightShift: '>' '>';
unsignedRightShift: '>' '>' '>';

comparisonOperator: '<' | '<=' | '>' | '>=';

equalityOperator: '==' | '!=';

compoundAssignmentOperator: '+=' | '-=' | '*=' | '/=' | '%=' | '<<=' | rightShiftAssignment | unsignedRightShiftAssignment | '&=' | '^=' | '|=';

rightShiftAssignment: '>' '>=';
unsignedRightShiftAssignment: '>' '>' '>=';

VAR: 'var';
FUNC: 'func';
EXTERN: 'extern';
RETURN: 'return';
MUT: 'mut';

TRUE: 'true';
FALSE: 'false';

IDENTIFIER: '@'? [A-Za-z_][A-Za-z0-9_]*;
FLOAT: DIGIT+ '.' DIGIT* EXPONENT?
    | DIGIT+ EXPONENT;
INT: DIGIT+;
STRING: '"' (ESCAPE_SEQUENCE | ~["\\\r\n])* '"';
CHARACTER: '\''  (ESCAPE_SEQUENCE | ~['\\\r\n]) '\'';

LINE_COMMENT: '//' (~[\r\n])* -> channel(HIDDEN);
DOCUMENTATION_COMMENT: '/**' .*? '*/' -> channel(HIDDEN);
BLOCK_COMMENT: '/*' .*? '*/' -> channel(HIDDEN);
WHITESPACE: [ \t\r\n]+ -> channel(HIDDEN);

fragment EXPONENT: [eE] [+-]? DIGIT+;

fragment DIGIT: [0-9];

fragment ESCAPE_SEQUENCE: '\\' [btnfr"'\\]
    | '\\' OCTAL OCTAL? OCTAL?
    | '\\' 'u' HEX HEX HEX HEX;

fragment OCTAL: [0-7];
fragment HEX: [0-9a-fA-F];
