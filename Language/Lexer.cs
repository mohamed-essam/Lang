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
        string code;
        ArrayList tokens;
        int line = 0;
        public string FileName = "";
        int idx;
        LangManager langManager;

        public Lexer(LangManager _langManager, string _code = "")
        {
            code = _code + ' ';
            tokens = new ArrayList();
            langManager = _langManager;
        }

        public void updateCode(string _code)
        {
            tokens.Clear();
            code = _code + ' ';
        }

        void acceptToken(string lexeme, TokenType type, int start, int end)
        {
            Token token = new Token(lexeme, type, line, FileName, start, end);
            tokens.Add(token);
        }

        int decide(char tok, char peek)
        {
            if (peek == '/' && tok == peek)
            {
                acceptToken(tok + "/", TokenType.DIV_INT, idx, idx+1);
                return 1;
            }
            if (peek == '=')
            {
                switch (tok)
                {
                    case '>':
                        acceptToken(tok + "=", TokenType.GREATER_EQUAL, idx, idx+1);
                        return 1;
                    case '<':
                        acceptToken(tok + "=", TokenType.SMALLER_EQUAL, idx, idx + 1);
                        return 1;
                    case '!':
                        acceptToken(tok + "=", TokenType.NOT_EQUAL, idx, idx + 1);
                        return 1;
                    case '=':
                        acceptToken(tok + "=", TokenType.EQUAL_TEST, idx, idx + 1);
                        return 1;
                }
            }
            switch (tok)
            {
                case '+':
                    acceptToken(tok + "", TokenType.PLUS, idx, idx);
                    break;
                case '-':
                    acceptToken(tok + "", TokenType.MINUS, idx, idx);
                    break;
                case '*':
                    acceptToken(tok + "", TokenType.MUL, idx, idx);
                    break;
                case '/':
                    acceptToken(tok + "", TokenType.DIV, idx, idx);
                    break;
                case '(':
                    acceptToken(tok + "", TokenType.L_PARA, idx, idx);
                    break;
                case ')':
                    acceptToken(tok + "", TokenType.R_PARA, idx, idx);
                    break;
                case '^':
                    acceptToken(tok + "", TokenType.POW, idx, idx);
                    break;
                case '<':
                    acceptToken(tok + "", TokenType.SMALLER, idx, idx);
                    break;
                case '>':
                    acceptToken(tok + "", TokenType.GREATER, idx, idx);
                    break;
                case '=':
                    acceptToken(tok + "", TokenType.EQUAL, idx, idx);
                    break;
                case '&':
                    acceptToken(tok + "", TokenType.AND, idx, idx);
                    break;
                case '|':
                    acceptToken(tok + "", TokenType.OR, idx, idx);
                    break;
                case ';':
                    acceptToken(tok + "", TokenType.SEMICOLON, idx, idx);
                    break;
                case ',':
                    acceptToken(tok + "", TokenType.COMMA, idx, idx);
                    break;
                case '%':
                    acceptToken(tok + "", TokenType.MOD, idx, idx);
                    break;
                case '[':
                    acceptToken(tok + "", TokenType.L_BRACK, idx, idx);
                    break;
                case ']':
                    acceptToken(tok + "", TokenType.R_BRACK, idx, idx);
                    break;
                case '\n':
                    line++;
                    break;
                case '.':
                    acceptToken(tok + "", TokenType.DOT, idx, idx);
                    break;
            }
            return 0;
        }

        bool decide(string tok)
        {
            if (tok == "print")
            {
                acceptToken(tok, TokenType.PRINT, idx, idx + 4);
                return true;
            }
            else if (tok == "scan")
            {
                acceptToken(tok, TokenType.SCAN, idx, idx + 3);
                return true;
            }
            else if (tok == "if")
            {
                acceptToken(tok, TokenType.IF, idx, idx + 2);
                return true;
            }
            else if (tok == "elif")
            {
                acceptToken(tok, TokenType.ELIF, idx, idx + 3);
                return true;
            }
            else if (tok == "else")
            {
                acceptToken(tok, TokenType.ELSE, idx, idx + 3);
                return true;
            }
            else if (tok == "endif")
            {
                acceptToken(tok, TokenType.ENDIF, idx, idx + 4);
                return true;
            }
            else if (tok == "for")
            {
                acceptToken(tok, TokenType.FOR, idx, idx + 2);
                return true;
            }
            else if (tok == "endloop")
            {
                acceptToken(tok, TokenType.ENDLOOP, idx, idx + 6);
                return true;
            }
            else if (tok == "as")
            {
                acceptToken(tok, TokenType.AS, idx, idx + 1);
                return true;
            }
            else if (tok == "while")
            {
                acceptToken(tok, TokenType.WHILE, idx, idx + 4);
                return true;
            }
            else if (tok == "stop")
            {
                acceptToken(tok, TokenType.STOP, idx, idx + 3);
                return true;
            }
            else if (tok == "function" || tok == "func")
            {
                if(tok == "function")
                    acceptToken(tok, TokenType.FUNCTION, idx, idx + 7);
                else
                    acceptToken(tok, TokenType.FUNCTION, idx, idx + 3);
                return true;
            }
            else if (tok == "endfunction" || tok == "endfunc")
            {
                if (tok == "endfunction")
                    acceptToken(tok, TokenType.ENDFUNCTION, idx, idx + 10);
                else
                    acceptToken(tok, TokenType.ENDFUNCTION, idx, idx + 6);
                return true;
            }
            else if (tok == "global")
            {
                acceptToken(tok, TokenType.GLOBAL, idx, idx + 5);
                return true;
            }
            else if (tok == "import")
            {
                acceptToken(tok, TokenType.IMPORT, idx, idx + 5);
                return true;
            }
            else if (tok == "class")
            {
                acceptToken(tok, TokenType.CLASS, idx, idx + 4);
                return true;
            }
            else if (tok == "endclass")
            {
                acceptToken(tok, TokenType.ENDCLASS, idx, idx + 7);
                return true;
            }
            else if (tok == "new")
            {
                acceptToken(tok, TokenType.NEW, idx, idx + 2);
                return true;
            }
            else if (tok == "let")
            {
                acceptToken(tok, TokenType.LET, idx, idx + 2);
                return true;
            }
            else if (tok == "return")
            {
                acceptToken(tok, TokenType.RETURN, idx, idx + 5);
                return true;
            }
            else if (tok == "raise")
            {
                acceptToken(tok, TokenType.RAISE, idx, idx + 4);
                return true;
            }
            else if (tok == "try")
            {
                acceptToken(tok, TokenType.TRY, idx, idx + 2);
                return true;
            }
            else if (tok == "endtry")
            {
                acceptToken(tok, TokenType.ENDTRY, idx, idx + 5);
                return true;
            }
            else if (tok == "catch")
            {
                acceptToken(tok, TokenType.CATCH, idx, idx + 4);
                return true;
            }
            else if (tok == "endcatch")
            {
                acceptToken(tok, TokenType.ENDCATCH, idx, idx + 7);
                return true;
            }
            else if (tok == "extends")
            {
                acceptToken(tok, TokenType.EXTENDS, idx, idx + 6);
                return true;
            }
            return false;
        }

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
                            acceptToken(past, TokenType.NUMBER, idx-past.Length, idx-1);
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
                                acceptToken(past, TokenType.ID, idx - past.Length, idx - 1);
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
                                acceptToken(past, TokenType.STRING, idx - past.Length, idx - 1);
                                past = "";
                                isString = false;
                            }
                            else
                            {
                                if (code[idx - 2] == '\\')
                                {
                                    acceptToken(past, TokenType.STRING, idx - past.Length, idx - 1);
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
