grammar Prism;

compilationUnit: declaration* EOF;

declaration: variableDeclaration | functionDeclaration;

variableDeclaration: EXTERN? VAR MUT? IDENTIFIER typeSpecifier? (EQUAL expression)? SEMICOLON;

functionDeclaration: EXTERN? FUNC IDENTIFIER LPAREN parameterList? RPAREN returnType? (functionBody | SEMICOLON);

parameter: MUT? IDENTIFIER typeSpecifier?;

parameterList: parameter (COMMA parameter)*;

returnType: ARROW typeReference;

block: LBRACE statement* RBRACE;

expressionBody: EQUAL_ARROW expression;

functionBody: block
| (expressionBody SEMICOLON);

typeSpecifier: COLON typeReference;

typeReference: IDENTIFIER;

statement: variableDeclaration | returnStatement | expressionStatement | block;

expressionStatement: expression SEMICOLON;

returnStatement: RETURN expression SEMICOLON;

literal: TRUE | FALSE | FLOAT | INT | STRING | CHARACTER;

argumentList: argument (',' argument)*;

argument: argumentName? argumentValue;

argumentName: IDENTIFIER COLON;

argumentValue: expression;

simpleName: IDENTIFIER;

namedEntity: simpleName;

expression: literal #literalExpression
    | simpleName #identifierExpression
    | LPAREN expression RPAREN #parenthesizedExpression
    | expression LPAREN argumentList RPAREN #invocationExpression
    | expression postfixOperator #postfixExpression
    | NAMEOF LPAREN namedEntity RPAREN #nameofExpression
    | unaryOperator expression #unaryExpression
    | expression multiplicitiveOperator expression #multiplicitiveExpression
    | expression additiveOperator expression # additiveExpression
    | expression shiftOperator expression #shiftExpression
    | expression SPACESHIP expression #threeWayComparisonExpression
    | expression comparisonOperator expression #comparisonExpression
    | expression equalityOperator expression #equalityExpression
    | expression AMP expression #bitwiseAndExpression
    | expression CARET expression #bitwiseXorExpression
    | expression PIPE expression #bitwiseOrExpression
    | expression AMP_AMP expression #logicalAndExpression
    | expression PIPE_PIPE expression #logicalOrExpression
    | expression QUESTION_QUESTION expression #nullCoallesingExpression
    | expression QUESTION expression COLON expression #ternaryExpression
    | expression EQUAL expression #assignment
    | expression compoundAssignmentOperator expression #compoundAssignment;

postfixOperator: PLUS_PLUS | MINUS_MINUS;

unaryOperator: PLUS_PLUS | MINUS_MINUS | PLUS | MINUS | TILDE | EXCLAIM | AMP | STAR;

multiplicitiveOperator: STAR | SLASH | PERCENT;

additiveOperator: PLUS | MINUS;

shiftOperator: LESS_LESS | GREATER_GREATER | GREATER_GREATER_GREATER;

comparisonOperator: LESS | LESS_EQUAL | GREATER | GREATER_EQUAL;

equalityOperator: EQUAL_EQUAL | EXCLAIM_EQUAL;

compoundAssignmentOperator: PLUS_EQUAL | MINUS_EQUAL | STAR_EQUAL | SLASH_EQUAL | PERCENT_EQUAL | LESS_LESS_EQUAL
    | GREATER_GREATER_EQUAL | GREATER_GREATER_GREATER_EQUAL | AMP_EQUAL | CARET_EQUAL | PIPE_EQUAL;

VAR: 'var';
FUNC: 'func';
EXTERN: 'extern';
RETURN: 'return';
MUT: 'mut';
NAMEOF: 'nameof';

TRUE: 'true';
FALSE: 'false';

LBRACKET: '[';
RBRACKET: ']';
LPAREN: '(';
RPAREN: ')';
LBRACE: '{';
RBRACE: '}';
PERIOD: '.';
ELLIPSIS: '...';
AMP: '&';
AMP_AMP: '&&';
AMP_EQUAL: '&=';
STAR: '*';
STAR_EQUAL: '*=';
PLUS: '+';
PLUS_PLUS: '++';
PLUS_EQUAL: '+=';
MINUS: '-';
ARROW: '->';
MINUS_MINUS: '--';
MINUS_EQUAL: '-=';
TILDE: '~';
EXCLAIM: '!';
EXCLAIM_EQUAL: '!=';
SLASH: '/';
SLASH_EQUAL: '/=';
PERCENT: '%';
PERCENT_EQUAL: '%=';
LESS: '<';
LESS_LESS: '<<';
LESS_EQUAL: '<=';
LESS_LESS_EQUAL: '<<=';
SPACESHIP: '<=>';
GREATER: '>';
GREATER_GREATER: '>>';
GREATER_GREATER_GREATER: '>>>';
GREATER_EQUAL: '>=';
GREATER_GREATER_EQUAL: '>>=';
GREATER_GREATER_GREATER_EQUAL: '>>>=';
CARET: '^';
CARET_EQUAL: '^=';
PIPE: '|';
PIPE_PIPE: '||';
PIPE_EQUAL: '|=';
QUESTION: '?';
QUESTION_QUESTION: '??';
COLON: ':';
COLON_COLON: '::';
SEMICOLON: ';';
EQUAL: '=';
EQUAL_ARROW: '=>';
EQUAL_EQUAL: '==';
COMMA: ',';

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
