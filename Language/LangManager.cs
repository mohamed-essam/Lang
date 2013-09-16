using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Collections;
using System.IO;

namespace Lang.language
{
    internal class LangException : Exception
    {

        public LangException()
            : base()
        {

        }
        public LangException(string Message)
            : base(Message)
        {

        }
        public LangException(string Message, Exception innerException)
            : base(Message, innerException)
        {

        }
    }

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

        /// <summary>
        /// Creates a new LangManager object with the given LangConsole and file name
        /// </summary>
        /// <param name="console">The LangConsole object that the code will read/write to it</param>
        /// <param name="fileName">The name of the file to be Lexed, Parsed, and Interpreted</param>
        public LangManager(string fileName)
        {
            lexer = new Lexer(this);
            lexer.FileName = fileName;
            parser = new Parser(this);
            interpreter = new Interpreter(this);
            interpreter.FileName = fileName;
        }

        /// <summary>
        /// Updates the code and checks for circular dependecy
        /// </summary>
        /// <param name="code">The new code string</param>
        public void updateCode(string code)
        {
            lexer.updateCode(code);
            if (INTERPRET == true)
            {
                tree = new DependencyTree();
                try
                {
                    bool isBad = tree.DetectCirularDependency(lexer.FileName, code);
                    if (isBad)
                    {
                        throw new Exception("Circular reasoning doesn't work, trust me.");
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Runs the code Given in updateCode(string) Method
        /// </summary>
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
                    interpreter.interpret(true);
                }
                catch (LangException e)
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
                interpreter.interpret(true);
            }
        }

        /// <summary>
        /// Gets the tokens from the variable
        /// </summary>
        /// <returns>An ArrayList object containing Token objects</returns>
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
        EXTENDS,
        BREAK,
        CONTINUE,
        NOT,
        MODIFIER
    }

    public class Token
    {
        internal string lexeme; // The actual string that this Token object was made of
        internal TokenType type; // The TokenType instance that declares what type is this Token
        internal int line; // the line that this Token lies in, for the debugging feature
        internal int startIndex, endIndex; // the start and end indicies of this token, for the debugging feature
        internal string file; // the file where this token is actually taken from, for the debugging feature

        /// <summary>
        /// Creates a new Token object, of the type _type, that was originaly _lexeme,
        /// and it lies on _line, in the file _file, and starts in the index _start, and ends on _end
        /// </summary>
        /// <param name="_lexeme">The string that this token was originated from</param>
        /// <param name="_type">The TokenType that specifies the type of this Token</param>
        /// <param name="_line">The line where this Token lies</param>
        /// <param name="_file">The file where this Token is actually taken from</param>
        /// <param name="_start">The start index of the Token</param>
        /// <param name="_end">The ending index of the Token</param>
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
