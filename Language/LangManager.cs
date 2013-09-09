using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Collections;

namespace Lang.language
{
    public class LangManager
    {
        private bool TRY = true;
        private bool PARSE = true;
        internal bool INTERPRET = true;

        internal string lastException = "";
        internal string lastLiveException = "";
        internal Token lastErrorToken;
        internal Token lastLiveErrorToken;

        internal Lexer lexer;
        internal Parser parser;
        internal Interpreter interpreter;

        ArrayList tokens;
        Node root;
        DependencyTree tree;

        public LangManager(LangConsole console, string fileName)
        {
            lexer = new Lexer(this);
            lexer.FileName = fileName;
            parser = new Parser(this);
            interpreter = new Interpreter(console, this);
            interpreter.FileName = fileName;
        }

        public void updateCode(string code)
        {
            lexer.updateCode(code);
            if (INTERPRET == true)
            {
                tree = new DependencyTree();
                bool isBad = tree.DetectCirularDependency("", code);
                if (isBad)
                {
                    throw new Exception("Circular reasoning doesn't work, trust me.");
                }
            }
        }

        public void run()
        {
            if (TRY)
            {
                try
                {
                    lastException = "";
                    tokens = lexer.lex();
                    if (!PARSE)
                        return;
                    parser.updateTokens(tokens);
                    root = parser.parse();
                    if (!INTERPRET)
                        return;
                    interpreter.updateRoot((StatementList)root);
                    interpreter.interpret();
                }
                catch (Exception e)
                {
                    if (INTERPRET)
                        lastException = e.Message;
                    else
                        lastLiveException = e.Message;
                }
            }
            else
            {
                lastException = "";
                tokens = lexer.lex();
                if (!PARSE)
                    return;
                parser.updateTokens(tokens);
                root = parser.parse();
                if (!INTERPRET)
                    return;
                interpreter.updateRoot((StatementList)root);
                interpreter.interpret();
            }
        }

        public ArrayList getTokens()
        {
            return tokens;
        }

        internal void Dispose()
        {
            root.Dispose();
            foreach (Token token in tokens)
            {
                token.Dispose();
            }
            tokens.Clear();
            interpreter.Dispose();
            GC.Collect();
        }
    }

    public enum TokenType
    {
        ID, STRING,
        NUMBER,
        PLUS, MINUS, MUL, DIV, POW, DIV_INT, MOD,
        GREATER, SMALLER, EQUAL,
        GREATER_EQUAL, SMALLER_EQUAL, NOT_EQUAL, EQUAL_TEST, SEMICOLON,
        AND, OR,
        L_PARA, R_PARA, PRINT, SCAN, L_BRACK, R_BRACK,
        IF, ELIF, ELSE, ENDIF,
        FOR, ENDLOOP,
        WHILE,
        STOP,
        EOF,
        AS, COMMA,
        FUNCTION,
        ENDFUNCTION,
        GLOBAL,
        IMPORT,
        CLASS,
        ENDCLASS,
        DOT,
        NEW,
        LET,
        RETURN,
        RAISE,
        TRY,
        ENDTRY,
        CATCH,
        ENDCATCH,
        EXTENDS
    }

    public class Token
    {
        internal string lexeme;
        internal TokenType type;
        internal int line;
        internal int startIndex, endIndex;
        internal string file;

        public Token(string _lexeme, TokenType _type, int _line, string _file, int _start, int _end)
        {
            lexeme = _lexeme;
            type = _type;
            line = _line;
            file = _file;
            startIndex = _start;
            endIndex = _end;
        }

        public void Dispose()
        {
            lexeme = null;
        }
    }
}
