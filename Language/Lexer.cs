using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Collections;

namespace Lang.language
{
    public class Lexer
    {
        string code; // The code that would be lexed, Can be updated through updateCode(string)
        ArrayList tokens; // The ArrayList object that contains the tokens, returned with lex()
        int line = 0; // The line the Lexer is currently processing
        public string FileName = ""; // The file the Lexer is currently processing
        int idx; // The index the Lexer is currently processing
        LangManager langManager; // The LangManager that created this Lexer, for error reporting
        static private Hashtable reservedKeywords; // The Hashtable that contains the lexeme of the word tokens to it's TokenType
        static private Hashtable reservedSpecialCharacters; // The Hashtable that contains the lexeme of the character tokens to it's TokenType

        /// <summary>
        /// Creates a new Lexer object with _langManager as it's error reporting instance, and calls the reserved Init function if they're not initialized.
        /// </summary>
        /// <param name="_langManager">The LangManager that actually created this instance</param>
        /// <param name="_code">optional, The code that this Lexer will start with</param>
        public Lexer(LangManager _langManager, string _code = "")
        {
            if (reservedKeywords == null)
            {
                InitReservedKeywords();
                InitReservedSpecialCharacters();
            }
            code = _code + ' ';
            tokens = new ArrayList();
            langManager = _langManager;
        }

        /// <summary>
        /// Initializes the reserved keywords, Called only once.
        /// </summary>
        static private void InitReservedKeywords()
        {
            reservedKeywords = new Hashtable();
            
            // ----------------------------------- Simple Statements
            reservedKeywords["let"] = TokenType.LET;
            reservedKeywords["print"] = TokenType.PRINT;
            reservedKeywords["scan"] = TokenType.SCAN;
            reservedKeywords["return"] = TokenType.RETURN;
            reservedKeywords["global"] = TokenType.GLOBAL;
            reservedKeywords["as"] = TokenType.AS;
            reservedKeywords["break"] = TokenType.BREAK;
            reservedKeywords["continue"] = TokenType.CONTINUE;
            reservedKeywords["stop"] = TokenType.STOP;
            reservedKeywords["import"] = TokenType.IMPORT;
            reservedKeywords["new"] = TokenType.NEW;
            reservedKeywords["raise"] = TokenType.RAISE;
            reservedKeywords["extends"] = TokenType.EXTENDS;

            // ----------------------------------- Compound Statements
            reservedKeywords["if"] = TokenType.IF;
            reservedKeywords["elif"] = TokenType.ELIF;
            reservedKeywords["else"] = TokenType.ELSE;
            reservedKeywords["endif"] = TokenType.ENDIF;
            reservedKeywords["for"] = TokenType.FOR;
            reservedKeywords["while"] = TokenType.WHILE;
            reservedKeywords["endloop"] = TokenType.ENDLOOP;
            reservedKeywords["try"] = TokenType.TRY;
            reservedKeywords["catch"] = TokenType.CATCH;
            reservedKeywords["endtry"] = TokenType.ENDTRY;
            reservedKeywords["endcatch"] = TokenType.ENDCATCH;
            reservedKeywords["func"] = TokenType.FUNCTION;
            reservedKeywords["function"] = TokenType.FUNCTION;
            reservedKeywords["endfunc"] = TokenType.ENDFUNCTION;
            reservedKeywords["endfunction"] = TokenType.ENDFUNCTION;
            reservedKeywords["class"] = TokenType.CLASS;
            reservedKeywords["endclass"] = TokenType.ENDCLASS;
        }

        /// <summary>
        /// initializes the reserved characters, Called only once.
        /// </summary>
        static private void InitReservedSpecialCharacters()
        {
            reservedSpecialCharacters = new Hashtable();

            // --------------------------- Mathematical Operators
            reservedSpecialCharacters["+"] = TokenType.PLUS;
            reservedSpecialCharacters["-"] = TokenType.MINUS;
            reservedSpecialCharacters["*"] = TokenType.MUL;
            reservedSpecialCharacters["/"] = TokenType.DIV;
            reservedSpecialCharacters["//"] = TokenType.DIV_INT;
            reservedSpecialCharacters["%"] = TokenType.MOD;
            reservedSpecialCharacters["^"] = TokenType.POW;

            // --------------------------- Logical Operators
            reservedSpecialCharacters["<"] = TokenType.SMALLER;
            reservedSpecialCharacters[">"] = TokenType.GREATER;
            reservedSpecialCharacters["="] = TokenType.EQUAL;
            reservedSpecialCharacters[">="] = TokenType.GREATER_EQUAL;
            reservedSpecialCharacters["<="] = TokenType.SMALLER_EQUAL;
            reservedSpecialCharacters["!="] = TokenType.NOT_EQUAL;
            reservedSpecialCharacters["=="] = TokenType.EQUAL_TEST;
            reservedSpecialCharacters["&"] = TokenType.AND;
            reservedSpecialCharacters["|"] = TokenType.OR;

            // --------------------------- Other
            reservedSpecialCharacters[";"] = TokenType.SEMICOLON;
            reservedSpecialCharacters[","] = TokenType.COMMA;
            reservedSpecialCharacters["."] = TokenType.DOT;
            reservedSpecialCharacters["("] = TokenType.L_PARA;
            reservedSpecialCharacters[")"] = TokenType.R_PARA;
            reservedSpecialCharacters["["] = TokenType.L_BRACK;
            reservedSpecialCharacters["]"] = TokenType.R_BRACK;
        }

        /// <summary>
        /// Updates the code that would be lexed when lex() is called.
        /// </summary>
        /// <param name="_code">The code to lex</param>
        public void updateCode(string _code)
        {
            tokens.Clear();
            code = _code + ' ';
        }

        /// <summary>
        /// Creates a new token with the lexeme and type provided.
        /// </summary>
        /// <param name="lexeme">the lexeme the Token will be initialized with</param>
        /// <param name="type">the TokenType that the Token will initialized with</param>
        void acceptToken(string lexeme, TokenType type)
        {
            Token token = new Token(lexeme, type, line, FileName, idx - lexeme.Length, idx);
            tokens.Add(token);
        }

        /// <summary>
        /// Creates a new token with the lexeme and type provided.
        /// </summary>
        /// <param name="lexeme">the lexeme the Token will be initialized with</param>
        /// <param name="type">the TokenType that the Token will initialized with</param>
        /// <param name="idx">the index where this Token starts</param>
        void acceptToken(string lexeme, TokenType type, int idx)
        {
            Token token = new Token(lexeme, type, line, FileName, idx, idx + lexeme.Length);
            tokens.Add(token);
        }

        /// <summary>
        /// Decides the TokenType of the character, 
        /// the next character is given for two character Tokens.
        /// </summary>
        /// <param name="tok_">The character that this function will operate on</param>
        /// <param name="peek_">The character that follows, for two character Tokens</param>
        /// <returns>1 or 0, depending on whether the function took <paramref name="peek_"/> or not</returns>
        int decide(char tok_, char peek_)
        {
            string tok = tok_ + "";
            string peek = peek_ + "";
            if (peek == "/" && tok == peek)
            {
                acceptToken(tok + "/", TokenType.DIV_INT, idx);
                return 1;
            }
            if (peek == "=")
            {
                if (reservedSpecialCharacters.ContainsKey(tok + peek))
                {
                    acceptToken(tok + peek, (TokenType)reservedSpecialCharacters[tok + peek], idx);
                    return 1;
                }
            }
            if (reservedSpecialCharacters.ContainsKey(tok))
            {
                acceptToken(tok, (TokenType)reservedSpecialCharacters[tok], idx);
            }
            return 0;
        }

        /// <summary>
        /// Decides the TokenType of the word given.
        /// </summary>
        /// <param name="tok">The word to decide</param>
        /// <returns>Whether it is reserved or not</returns>
        bool decide(string tok)
        {
            if (reservedKeywords.ContainsKey(tok))
            {
                acceptToken(tok, (TokenType)reservedKeywords[tok]);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Starts the lexing process
        /// </summary>
        /// <returns>ArrayList object that contains all the tokens generated</returns>
        public ArrayList lex()
        {
            string past = "";
            line = 1;
            bool isNum = false, isID = false, isString = false;
            bool isComment = false;
            for (idx = 0; idx < code.Length; idx++)
            {
                if (isComment)
                {
                    if (code[idx] != '\n')
                    {
                        continue;
                    }
                    else
                    {
                        isComment = false;
                        line++;
                    }
                }
                else if (past.Length > 0 || isString)
                {
                    if (isNum)
                    {
                        if ((code[idx] >= '0' && code[idx] <= '9') || code[idx] == '.')
                        {
                            past = past + code[idx];
                        }
                        else if ((code[idx] >= 'a' && code[idx] <= 'z') || (code[idx] >= 'A' && code[idx] <= 'Z') || code[idx] == '_')
                        {
                            if (langManager.INTERPRET)
                            {
                                langManager.lastErrorToken = new Token(past, TokenType.NUMBER, line, FileName, idx - past.Length, idx - 1);
                            }
                            else
                            {
                                langManager.lastLiveErrorToken = new Token(past, TokenType.NUMBER, line, FileName, idx - past.Length, idx - 1);
                            }
                            throw new Exception("Line "+line+": "+"Syntax Error: Invalid number format");
                        }
                        else
                        {
                            acceptToken(past, TokenType.NUMBER);
                            past = "";
                            if (code[idx] == '\n')
                            {
                                isComment = false;
                                line++;
                            }
                            if (code[idx] != ' ' && code[idx] != '#' && code[idx] != '\n')
                                idx += decide(code[idx], code[idx + 1]);
                            if (code[idx] == '#')
                            {
                                isComment = true;
                            }
                        }
                    }
                    else if (isID)
                    {
                        if ((code[idx] >= '0' && code[idx] <= '9') || (code[idx] >= 'a' && code[idx] <= 'z') || (code[idx] >= 'A' && code[idx] <= 'Z') || code[idx] == '_')
                        {
                            past = past + code[idx];
                        }
                        else
                        {
                            if(!decide(past))
                            {
                                acceptToken(past, TokenType.ID);
                            }
                            past = "";
                            if (code[idx] == '\n')
                            {
                                isComment = false;
                                line++;
                            }
                            if (code[idx] != ' ' && code[idx] != '#' && code[idx] != '\n')
                                idx += decide(code[idx], code[idx + 1]);
                            if (code[idx] == '#')
                            {
                                isComment = true;
                            }
                        }
                    }
                    else if (isString)
                    {
                        if (code[idx] != '"')
                        {
                            past = past + code[idx];
                        }
                        else
                        {
                            if (code[idx - 1] != '\\')
                            {
                                acceptToken(past, TokenType.STRING);
                                past = "";
                                isString = false;
                            }
                            else
                            {
                                if (code[idx - 2] == '\\')
                                {
                                    acceptToken(past, TokenType.STRING);
                                    past = "";
                                    isString = false;
                                }
                                else
                                {
                                    past = past.Substring(0, past.Length - 1);
                                    past = past + code[idx];
                                }
                            }
                        }
                    }
                }
                else if (code[idx] == '"')
                {
                    isString = true;
                    isNum = false;
                    isID = false;
                }
                else if (code[idx] >= '0' && code[idx] <= '9')
                {
                    past = past + code[idx];
                    isNum = true;
                    isID = false;
                }
                else if ((code[idx] >= 'a' && code[idx] <= 'z') || (code[idx] >= 'A' && code[idx] <= 'Z') || code[idx] == '_')
                {
                    past = past + code[idx];
                    isNum = false;
                    isID = true;
                }
                else
                {
                    past = "";
                    if (code[idx] == '\n')
                    {
                        isComment = false;
                        line++;
                    }
                    if (code[idx] != ' ' && code[idx] != '#' && code[idx] != '\n')
                        idx += decide(code[idx], code[idx + 1]);
                    if (code[idx] == '#')
                    {
                        isComment = true;
                    }
                }
            }
            tokens.Add(new Token("", TokenType.EOF, line, FileName, idx, idx));
            return tokens;
        }
    }
}
