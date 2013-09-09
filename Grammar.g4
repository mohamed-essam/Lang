grammar Grammar;

/*
 * Parser Rules
 */

 statement_list
	:	statement*
	;

 statement
	:	simple_stat SEMICOLON
	;

 compound_stat
	:	if_stat
	|	while_stat
	|	for_stat
	|	function_stat
	|	class_stat
	;

 class_stat
	:	CLASS ID
		(
		(var_list)*
		(function_stat)*
		)*
		ENDCLASS
	;

 var_list
	:	(ID (COMMA ID)* SEMICOLON)?
	;

 if_stat
	:	IF L_PARA expression R_PARA
		statement_list
		(
		ELIF L_PARA expression R_PARA
		statement_list
		)*
		(
		ELSE
		statement_list
		)?
		ENDIF
	;

 while_stat
	:	WHILE L_PARA expression R_PARA
		statement_list
		ENDLOOP
	;

 for_stat
	:	FOR L_PARA bind_stat SEMICOLON expression SEMICOLON bind_stat R_PARA
		statement_list
		ENDLOOP
	;

 function_stat
	:	FUNCTION ID L_PARA (ID (AS ID)?(COMMA ID (AS ID)?)*)? R_PARA
		(GLOBAL ID(COMMA ID)*)?
		statement_list
		ENDFUNCTION
	;

 try_stat
	:	TRY
		stat_list
		ENDTRY
		(
		CATCH L_PARA ID R_PARA
		stat_list
		ENDCATCH
		)?
	;

 simple_stat
	:	bind_stat
	|	print_stat
	|	break_stat
	|	cont_stat
	;

 import_stat
	:	IMPORT ID(COMMA ID)*
	;

 func_call_stat
	:	ID L_PARA (expression (COMMA expression)*)? R_PARA
	;

 bind_stat
	:	LET id EQUAL expression (COMMA id EQUAL expression)*
	;

 print_stat
	:	PRINT expression (COMMA expression)*
	;

 scan_stat
	:	SCAN id (COMMA id)* (AS ID)?
	;
 
 break_stat
	:	BREAK
	;

 cont_stat
	:	CONTINUE
	;

 stop_stat
	:	STOP
	;

 expression	
	:	logic_expr
	;

 logic_expr:
	:	comp_expr ((AND|OR) comp_expr)*
	;

 comp_expr
	:	add_expr ((GREATER|SMALLER|EQUAL) add_expr)*
	;

 add_expr
	:	mul_expr ((PLUS|MINUS) mul_expr)*
	;

 mul_expr
	:	pow_expr ((DIV|MUL|MOD) pow_expr)*
	;

 pow_expr
	:	factor (POW pow_expr)?
	;

 factor
	:	NUMBER
	|	id
	|	MINUS factor
	|	L_PARA expression R_PARA
	;
	
 id
	:	simple_stat
	|	ID (L_BRACK expression R_BRACK)* (DOT ID (L_BRACK expression R_BRACK)*)*
	;

/*
 * Lexer Rules
 */

 SEMICOLON : ';';


 PLUS : '+';
 MINUS : '-';
 MUL : '*';
 DIV : '/';
 POW : '^';
 MOD : '%';
 DOT : '.';


 L_PARA : '(';
 R_PARA : ')';
 L_BRACK : '[';
 R_BRACK : ']';


 GREATER : '>';
 GREATER_EQUAL : '>=';
 SMALLER : '<';
 SMALLER_EQUAL : '<=';
 EQUAL : '=';
 EQUAL_TEST : '==';
 NOT_EQUAL : '!=';


 AND : '&';
 OR : '|';


 PRINT : 'print';
 SCAN : 'scan';
 AS : 'as';
 
 
 COMMA : ',';


 IF : 'if';
 ELIF : 'elif';
 ELSE : 'else';
 ENDIF : 'endif';

 IMPORT : 'import';

 FOR : 'for';
 ENDLOOP : 'endloop';
 BREAK : 'break';
 CONTINUE : 'continue';
 WHILE : 'while';
 FUNCTION : 'function';
 ENDFUNCTION : 'endfunction';

 CLASS : 'class';
 ENDCLASS : 'endclass';

 STOP : 'stop';

 NUMBER : DIGIT+ ('.' DIGIT*)?;
 ID : CHAR (CHAR | DIGIT)*;
 
 fragment CHAR : 'a'..'z'|'A'..'Z'|'_';
 fragment DIGIT : '0'..'9';

WS
	:	' ' -> channel(HIDDEN)
	;
