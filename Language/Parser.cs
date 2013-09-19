using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Collections;

namespace Lang.language
{
    internal class ParserException : LangException
    {

        public ParserException()
            : base()
        {

        }
        public ParserException(string Message)
            : base(Message)
        {

        }
        public ParserException(string Message, Exception innerException)
            : base(Message, innerException)
        {

        }
    }


    #region Node
    abstract public class Node : IDisposable
    {
        internal NodeType nodeType; // The NodeType of this Node
        internal Token token; // The Token this Node was created from

        /// <summary>
        /// Creates a new Node
        /// </summary>
        /// <param name="_nodeType">The NodeType this Node will be created from</param>
        /// <param name="_token">The Token this Node will be created from</param>
        public Node(NodeType _nodeType, Token _token)
        {
            nodeType = _nodeType;
            token = _token;
        }

        abstract public void Dispose();
    }

    public enum NodeType
    {
        ID, STRING, BRACKETS,
        NUMBER,
        PLUS, MINUS, MUL, DIV, POW,
        GREATER, SMALLER, EQUAL,
        GREATER_EQUAL, SMALLER_EQUAL, NOT_EQUAL, EQUAL_TEST, SEMICOLON,
        AND, OR,
        STATEMENT, STATEMENT_LIST,
        DIV_INT,
        MOD,
        DOT,
        PARAS,
        NOT
    }
    #endregion

    #region factors
    public class Number : Node
    {
        internal string value; // The value of this Number in string format

        /// <summary>
        /// Creates a new Number object
        /// </summary>
        /// <param name="_value">The string value of the Number</param>
        /// <param name="_token">The Token this Node will be created from</param>
        public Number(string _value, Token _token)
            : base(NodeType.NUMBER, _token)
        {
            value = _value;
        }

        public override string ToString()
        {
            return value;
        }

        public override void Dispose()
        {
            token = null;
            value = null;
        }
    }

    public class ID : Node
    {
        public string name; // The name of this ID

        /// <summary>
        /// Creates a new ID object
        /// </summary>
        /// <param name="_name">The name of this ID</param>
        /// <param name="_token">The token this Node will be created from</param>
        public ID(string _name, Token _token)
            : base(NodeType.ID, _token)
        {
            name = _name;
        }

        public override string ToString()
        {
            return name;
        }

        public override void Dispose()
        {
            token = null;
            name = null;
        }
    }

    public class StringVal : Node
    {
        public string val; // The string

        /// <summary>
        /// Creates a new StringVal object
        /// </summary>
        /// <param name="_val">The value of this string</param>
        /// <param name="_token">The Token thin Node will be created from</param>
        public StringVal(string _val, Token _token)
            : base(NodeType.STRING, _token)
        {
            val = _val;
        }

        public override string ToString()
        {
            return '"' + val + '"';
        }
        public override void Dispose()
        {
            token = null;
            val = null;
        }


    }

    public class ClassMember
    {
        internal ArrayList Modifiers;
        internal string name;

        public ClassMember(ArrayList _mods, string _name)
        {
            Modifiers = _mods;
            name = _name;
        }

        public override string ToString()
        {
            string mods = "";
            foreach (string mod in Modifiers)
            {
                mods += mod + " ";
            }
            return mods + name + ";";
        }
    }

    public class ClassMethod
    {
        internal ArrayList Modifiers;
        internal FunctionStatement statement;

        public ClassMethod(ArrayList _mods, FunctionStatement _stat)
        {
            Modifiers = _mods;
            statement = _stat;
        }

        public override string ToString()
        {
            string mods = "";
            foreach (string mod in Modifiers)
            {
                mods += mod + " ";
            }
            return mods + statement.ToString();
        }
    }
    #endregion

    public class BinaryOperator : Node
    {
        internal Node left, right; // The left and right Nodes
        internal string lexeme; // The lexeme of this node

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_nodeType">The NodeType this BinaryOperator will be created from</param>
        /// <param name="_left">The Left Node this BinaryOperator will be created from</param>
        /// <param name="_right">The Right Node this BinaryOperator will be created from</param>
        /// <param name="_token">The Token this Node will be created from</param>
        public BinaryOperator(NodeType _nodeType, Node _left, Node _right, Token _token)
            : base(_nodeType, _token)
        {
            left = _left;
            right = _right;
            lexeme = _token.lexeme;
        }

        public override string ToString()
        {
            switch (nodeType)
            {
                case NodeType.BRACKETS:
                    return left.ToString() + "[" + right.ToString() + "]";
                case NodeType.DOT:
                    return left.ToString() + "." + right.ToString();
            }
            return left.ToString() + " " + lexeme + " " + right.ToString();
        }

        public override void Dispose()
        {
            token = null;
            left.Dispose();
            right.Dispose();
            lexeme = null;
        }

    }

    public class UnaryOperator : Node
    {
        internal Node child;
        internal string lexeme;

        public UnaryOperator(NodeType _nodeType, Node _child, Token _token)
            : base(_nodeType, _token)
        {
            child = _child;
            lexeme = _token.lexeme;
        }

        public override void Dispose()
        {
            child.Dispose();
            lexeme = null;
        }

        public override string ToString()
        {
            switch (lexeme)
            {
                case "(":
                    return "(" + child.ToString() + ")";
                case "-":
                    return "-" + child.ToString();
            }
            return "";
        }
    }

    #region statements
    public enum StatementType
    {
        BIND,
        PRINT,
        SCAN,
        IF,
        FOR,
        BREAK,
        CONTINUE,
        WHILE,
        STOP,
        FUNCTION_CALL,
        FUNCTION,
        IMPORT,
        CLASS,
        CLASS_NEW,
        CLASS_FUNCTION,
        RETURN,
        RAISE,
        TRY
    }

    abstract public class Statement : Node
    {
        public StatementType type;

        public Statement(StatementType _type, Token _token)
            : base(NodeType.STATEMENT, _token)
        {
            type = _type;
        }
    }

    public class StatementList : Node
    {
        public ArrayList statements; // ArrayList of Statement objects

        /// <summary>
        /// Creates a new StatementList object
        /// </summary>
        /// <param name="_statements">The statements that this StatementList contains</param>
        /// <param name="_token">The Token this Node will be created from</param>
        public StatementList(ArrayList _statements, Token _token)
            : base(NodeType.STATEMENT_LIST, _token)
        {
            statements = _statements;
        }

        public string ToString(bool addSpaces = false)
        {
            string ret = "";
            foreach (Statement stat in statements)
            {
                ret += stat.ToString();
                if (stat.type == StatementType.BIND || stat.type == StatementType.BREAK || stat.type == StatementType.CLASS_FUNCTION || stat.type == StatementType.CLASS_NEW || stat.type == StatementType.CONTINUE || stat.type == StatementType.FUNCTION_CALL || stat.type == StatementType.IMPORT || stat.type == StatementType.PRINT || stat.type == StatementType.RAISE || stat.type == StatementType.RETURN || stat.type == StatementType.SCAN || stat.type == StatementType.STOP)
                {
                    ret += ";";
                }
                ret += "\n";
            }
            if (addSpaces)
            {
                ret = "    " + ret;
                for (int i = 0; i < ret.Length - 1; i++)
                {
                    if (ret[i] == '\n')
                    {
                        ret = ret.Substring(0, i + 1) + "    " + ret.Substring(i + 1);
                        i += 4;
                    }
                }
            }
            return ret;
        }

        public override void Dispose()
        {
            token = null;
            foreach (Statement stat in statements)
            {
                stat.Dispose();
            }
            statements.Clear();
            statements = null;
        }
    }

    #region simple_stats

    public class RaiseStatement : Statement
    {
        public Node expr;

        public RaiseStatement(Node _expr, Token _token)
            : base(StatementType.RAISE, _token)
        {
            expr = _expr;
        }

        public override void Dispose()
        {
            expr.Dispose();
            expr = null;
        }

        public override string ToString()
        {
            return "raise " + expr.ToString();
        }
    }

    public class BindStatement : Statement
    {
        public Node Id;
        public Node expr;
        public ArrayList extras;

        public BindStatement(Node _ID, Node _expr, ArrayList _extras, Token _token)
            : base(StatementType.BIND, _token)
        {
            Id = _ID;
            expr = _expr;
            extras = _extras;
            if (_extras == null)
            {
                extras = new ArrayList();
            }
        }

        public override void Dispose()
        {
            token = null;
            Id.Dispose();
            expr.Dispose();
            if (extras != null)
            {
                foreach (BindStatement stat in extras)
                {
                    stat.Dispose();
                }
                extras.Clear();
            }
            extras = null;
        }

        public override string ToString()
        {
            string others = "";
            foreach (BindStatement stat in extras)
            {
                others += ", " + stat.Id.ToString() + " = " + stat.expr.ToString();
            }
            return "let " + Id.ToString() + " = " + expr.ToString() + others;
        }
    }

    public class PrintStatement : Statement
    {
        public Node expr;
        public ArrayList extras;

        public PrintStatement(Node _expr, Token _token)
            : base(StatementType.PRINT, _token)
        {
            expr = _expr;
            extras = new ArrayList();
        }

        public override void Dispose()
        {
            token = null;
            expr.Dispose();
            expr = null;
            foreach (PrintStatement stat in extras)
            {
                stat.Dispose();
            }
            extras.Clear();
            extras = null;
        }

        public override string ToString()
        {
            string others = "";
            foreach (PrintStatement stat in extras)
            {
                others += ", " + stat.expr.ToString();
            }
            return "print " + expr.ToString() + others;
        }
    }

    public class ScanStatement : Statement
    {
        public Node expr;
        public ID scanType;
        public ArrayList extras;


        public ScanStatement(Node _expr, ID _type, ArrayList _extras, Token _token)
            : base(StatementType.SCAN, _token)
        {
            expr = _expr;
            scanType = _type;
            extras = _extras;
        }

        public override void Dispose()
        {
            token = null;
            expr.Dispose();
            expr = null;
            if (scanType != null)
            {
                scanType.Dispose();
                scanType = null;
            }
            foreach (ScanStatement stat in extras)
            {
                stat.Dispose();
            }
            extras.Clear();
            extras = null;
        }

        public override string ToString()
        {
            string others = "";
            foreach (ScanStatement stat in extras)
            {
                others += ", " + stat.expr.ToString();
            }
            string ret = "scan " + expr.ToString() + others;
            if (scanType != null)
            {
                ret += " as " + scanType.ToString();
            }
            return ret;
        }
    }

    public class FunctionCallStatement : Statement
    {
        public string name;
        public ArrayList parameters;

        public FunctionCallStatement(string _name, ArrayList _parameters, Token _token)
            : base(StatementType.FUNCTION_CALL, _token)
        {
            name = _name;
            parameters = _parameters;
        }

        public override void Dispose()
        {
            token = null;
            name = null;
            foreach (Node node in parameters)
            {
                node.Dispose();
            }
            parameters = null;
        }

        public override string ToString()
        {
            string param = "";
            for (int i = 0; i < parameters.Count; i++)
            {
                if (i > 0)
                    param += ", ";
                param += ((Node)parameters[i]).ToString();
            }
            return name + "(" + param + ")";
        }
    }

    public class ImportStatement : Statement
    {
        public ArrayList imports;

        public ImportStatement(ArrayList _imports, Token _token)
            : base(StatementType.IMPORT, _token)
        {
            imports = _imports;
        }

        public override void Dispose()
        {
            token = null;
            imports.Clear();
            imports = null;
        }

        public override string ToString()
        {
            string imps = "";
            for (int i = 0; i < imports.Count; i++)
            {
                if (i > 0)
                    imps += ", ";
                imps += (string)imports[i];
            }
            return "import " + imps;
        }
    }

    public class StoppingStatement : Statement
    {
        public StoppingStatement(StatementType _type, Token _token)
            : base(_type, _token)
        {

        }

        public override void Dispose()
        {
            token = null;
        }

        public override string ToString()
        {
            return "stop";
        }
    }

    public class ClassInitStatement : Statement
    {
        public FunctionCallStatement constructor;

        public ClassInitStatement(FunctionCallStatement _constructor, Token _token)
            : base(StatementType.CLASS_NEW, _token)
        {
            constructor = _constructor;
        }

        public override void Dispose()
        {

            constructor.Dispose();
            constructor = null;
        }

        public override string ToString()
        {
            return "new " + constructor.ToString();
        }
    }

    public class ClassFuncStatement : Statement
    {
        public Node expr;

        public ClassFuncStatement(Node _expr)
            : base(StatementType.CLASS_FUNCTION, _expr.token)
        {
            expr = _expr;
        }

        public override void Dispose()
        {
            expr.Dispose();
            expr = null;
        }

        public override string ToString()
        {
            return expr.ToString();
        }
    }

    public class ReturnStatement : Statement
    {
        public Node expr;

        public ReturnStatement(Node _expr, Token _token)
            : base(StatementType.RETURN, _token)
        {
            expr = _expr;
        }

        public override void Dispose()
        {
            expr.Dispose();
            expr = null;
        }

        public override string ToString()
        {
            return "return " + expr.ToString();
        }
    }

    #endregion

    #region compound_stats
    public class IfData
    {
        public Node expr;
        public StatementList stats;

        public IfData(Node _expr, StatementList _stats)
        {
            expr = _expr;
            stats = _stats;
        }

        public void Dispose()
        {
            expr.Dispose();
            expr = null;
            stats.Dispose();
        }

        public override string ToString()
        {
            return "if(" + expr.ToString() + ")\n" + stats.ToString(true);
        }
    }

    public class IfStatement : Statement
    {
        public ArrayList data;
        public StatementList elseData; // nullable

        public IfStatement(ArrayList _data, StatementList _elseData, Token _token)
            : base(StatementType.IF, _token)
        {
            data = _data;
            elseData = _elseData;
        }

        public override void Dispose()
        {
            token = null;
            foreach (IfData dat in data)
            {
                dat.Dispose();
            }
            data.Clear();
            data = null;
            if (elseData != null)
                elseData.Dispose();
            elseData = null;
        }

        public override string ToString()
        {
            string ifs = "";
            for (int i = 0; i < data.Count; i++)
            {
                if (i > 0)
                    ifs += "el";
                ifs += ((IfData)data[i]).ToString();
            }
            if (elseData != null)
            {
                ifs += "else\n" + elseData.ToString(true);
            }
            ifs += "endif\n";
            return ifs;
        }
    }

    public class ForStatement : Statement
    {
        public BindStatement init_stat, inc_stat;
        public Node check_expr;
        public StatementList stats;

        public ForStatement(BindStatement _init, Node _check, BindStatement _inc, StatementList _stats, Token _token)
            : base(StatementType.FOR, _token)
        {
            init_stat = _init;
            check_expr = _check;
            inc_stat = _inc;
            stats = _stats;
        }

        public override void Dispose()
        {
            token = null;
            init_stat.Dispose();
            inc_stat.Dispose();
            check_expr.Dispose();
            stats.Dispose();
            init_stat = inc_stat = null;
            check_expr = stats = null;
        }

        public override string ToString()
        {
            string init = "", check = "", inc = "", stats;
            if (init_stat != null)
                init = init_stat.ToString();
            if (check_expr != null)
                check = check_expr.ToString();
            if (inc_stat != null)
                inc = inc_stat.ToString();
            stats = this.stats.ToString(true);
            return "for(" + init + "; " + check + "; " + inc + ")\n" + stats + "endloop\n";
        }
    }

    public class WhileStatement : Statement
    {
        public Node check_expr;
        public StatementList stats;

        public WhileStatement(Node _check, StatementList _stats, Token _token)
            : base(StatementType.WHILE, _token)
        {
            check_expr = _check;
            stats = _stats;
        }

        public override void Dispose()
        {
            token = null;
            check_expr.Dispose();
            stats.Dispose();
            check_expr = stats = null;
        }

        public override string ToString()
        {
            return "while(" + check_expr.ToString() + ")\n" + stats.ToString(true) + "endloop\n";
        }
    }

    public class Parameter
    {
        public string name;
        public string type;

        public Parameter(string _name, string _type = "any")
        {
            name = _name;
            type = _type;
        }

        public void Dispose()
        {
            name = null;
            type = null;
        }

        public override string ToString()
        {
            if (type == "any")
            {
                return name + " as " + type;
            }
            return name;
        }
    }

    public class FunctionStatement : Statement
    {
        public string name;
        public ArrayList parameters;
        public StatementList stats;
        public ArrayList globals;

        public FunctionStatement(string _name, ArrayList _parameters, StatementList _stats, ArrayList _globals, Token _token)
            : base(StatementType.FUNCTION, _token)
        {
            name = _name;
            parameters = _parameters;
            stats = _stats;
            globals = _globals;
            if (globals == null)
                globals = new ArrayList();
        }

        public override void Dispose()
        {
            token = null;
            name = null;
            if (parameters != null)
            {
                foreach (Parameter param in parameters)
                {
                    param.Dispose();
                }
                parameters.Clear();
                parameters = null;
            }
            if (stats != null)
            {
                stats.Dispose();
                stats = null;
            }
            if (globals != null)
            {
                globals.Clear();
                globals = null;
            }
        }

        public override string ToString()
        {
            string param = "";
            foreach (Parameter para in parameters)
            {
                if (param.Length > 0)
                    param += ", ";
                param += para.ToString();
            }
            string globs = "";
            foreach (string glob in globals)
            {
                if (globs.Length > 0)
                    globs += ", ";
                globs += glob;
            }
            if (globs.Length > 0)
            {
                globs = "\n    global " + globs + ";";
            }
            return "func " + name + "(" + param + ")" + globs + "\n" + stats.ToString(true) + "endfunc\n";
        }
    }

    public class ClassStatement : Statement
    {
        public string name;
        public ArrayList vars;
        public Hashtable methods;
        public ArrayList constructors;
        public Hashtable staticMembers;
        public Hashtable staticPermissions;
        public ID parent;

        public ClassStatement(string _name, ArrayList _vars, Hashtable staticMembs, Hashtable staticPerms, Hashtable _methods, ArrayList _constructors, Token _token, ID _parent)
            : base(StatementType.CLASS, _token)
        {
            name = _name;
            vars = _vars;
            methods = _methods;
            constructors = _constructors;
            parent = _parent;
            staticMembers = staticMembs;
            staticPermissions = staticPerms;
        }

        public override void Dispose()
        {
            name = null;
            vars.Clear();
            foreach (DictionaryEntry stat in methods)
            {
                foreach (FunctionStatement statt in ((ArrayList)stat.Value))
                {
                    statt.Dispose();
                }
            }
            methods.Clear();
            methods = null;
            foreach (FunctionStatement stat in constructors)
            {
                stat.Dispose();
            }
            constructors.Clear();
            constructors = null;
        }

        public override string ToString()
        {
            string vrs = "";
            foreach (ClassMember vr in vars)
            {
                vrs += vr.ToString() + "\n";
            }
            if (vrs.Length > 0)
            {
                vrs = "    " + vrs;
            }
            string consts = "";
            foreach (FunctionStatement stat in constructors)
            {
                consts += "\n\n";
                consts += stat.ToString();
            }
            string mthds = "";
            foreach (DictionaryEntry dic in methods)
            {
                mthds += "\n\n";
                ArrayList funcs = (ArrayList)dic.Value;
                foreach (ClassMethod stat in funcs)
                {
                    mthds += stat.ToString();
                }
            }
            string ret = vrs + consts + mthds;
            for (int i = 0; i < ret.Length - 1; i++)
            {
                if (ret[i] == '\n')
                {
                    ret = ret.Substring(0, i + 1) + "    " + ret.Substring(i + 1);
                    i += 4;
                }
            }
            string header = "class " + name;
            if (parent != null)
            {
                header += " extends " + parent.name;
            }
            return header + "\n" + ret + "\nendclass\n";
        }
    }

    public class TryStatement : Statement
    {
        public StatementList stats;
        public ID catchID; // nullable
        public StatementList catchStats; // nullable

        public TryStatement(StatementList _stat, Token _token)
            : base(StatementType.TRY, _token)
        {
            stats = _stat;
            catchID = null;
            catchStats = null;
        }

        public TryStatement(StatementList _stats, ID _catchID, StatementList _catchStats, Token _token)
            : base(StatementType.TRY, _token)
        {
            stats = _stats;
            catchStats = _catchStats;
            catchID = _catchID;
        }

        public override void Dispose()
        {
            stats.Dispose();
            if (catchID != null)
                catchID.Dispose();
            if (catchStats != null)
                catchStats.Dispose();
            stats = catchStats = null;
            catchID = null;
        }

        public override string ToString()
        {
            string try_body = "try\n" + stats.ToString() + "endtry\n";
            if (catchID != null)
            {
                try_body += "catch(" + catchID.name + ")\n" + catchStats.ToString() + "endcatch\n";
            }
            return try_body;
        }
    }

    #endregion
    #endregion

    public class Parser
    {
        ArrayList tokens;
        int p;
        Token lookAhead;
        LangManager langManager;

        /// <summary>
        /// Creates a new Parser object
        /// </summary>
        /// <param name="_langManager">The LangManager for error reporting</param>
        public Parser(LangManager _langManager)
        {
            langManager = _langManager;
        }

        /// <summary>
        /// Updates the token list
        /// </summary>
        /// <param name="_tokens">The ArrayList that contains the Tokens</param>
        public void updateTokens(ArrayList _tokens)
        {
            p = 0;
            tokens = _tokens;
            lookAhead = (Token)tokens[p];
        }

        #region tools
        // TOOLS
        void move()
        {
            p++;
            if (p >= tokens.Count)
            {
                lookAhead = (Token)tokens[tokens.Count - 1];
            }
            else
            {
                lookAhead = (Token)tokens[p];
            }
        }

        Token read()
        {
            if (p + 1 >= tokens.Count)
            {
                return lookAhead = (Token)tokens[tokens.Count - 1];
            }
            lookAhead = (Token)tokens[p + 1];
            return (Token)tokens[p++];
        }

        bool hasMoreTokens()
        {
            return p < tokens.Count;
        }

        void match(TokenType type)
        {
            if (lookAhead.type != type)
            {
                if (langManager.INTERPRET)
                {
                    langManager.lastErrorToken = lookAhead;
                }
                else
                {
                    langManager.lastLiveErrorToken = lookAhead;
                }
                throw new ParserException("Line " + lookAhead.line + ": " + "Syntax error: expected " + type + " instead of " + lookAhead.type);
            }
        }

        bool isNext(TokenType type)
        {
            return lookAhead.type == type;
        }

        void retract()
        {
            lookAhead = (Token)tokens[--p];
        }
        #endregion
        // Parser
        /// <summary>
        /// Starts the parsing process
        /// </summary>
        /// <returns>A StatementList object containing the tree</returns>
        public Node parse()
        {
            return prog();
        }

        #region top_stats

        /// <summary>
        /// Parses the Tokens given
        /// </summary>
        /// <returns>A StatementList object that contains the tree of the code</returns>
        Node prog()
        {
            Node node = statement_list();
            move();//EOF
            return node;
        }

        Node statement_list()
        {
            Statement stat;
            ArrayList statList = new ArrayList();
            while ((stat = statement()) != null)
            {
                statList.Add(stat);
            }
            return new StatementList(statList, null);
        }

        Statement statement()
        {
            Statement stat = compound_stat();
            if (stat == null)
            {
                stat = simple_stat();
                if (stat != null)
                {
                    match(TokenType.SEMICOLON);
                    move(); // ';'
                }
                else
                {
                    Node lastTry = expression(false);
                    if (lastTry != null)
                    {
                        if (lastTry is BinaryOperator)
                        {
                            match(TokenType.SEMICOLON);
                            move();
                            return new ClassFuncStatement(lastTry);
                        }
                        return null;
                    }
                    return null;
                }
            }
            return stat;
        }

        #endregion

        #region compound_stats
        Statement compound_stat()
        {
            Statement stat = if_stat();
            if (stat == null)
                stat = for_stat();
            if (stat == null)
                stat = while_stat();
            if (stat == null)
                stat = function_stat();
            if (stat == null)
                stat = class_stat();
            if (stat == null)
                stat = try_stat();
            return stat;
        }

        TryStatement try_stat()
        {
            if (!isNext(TokenType.TRY))
                return null;
            Token try_token = read();
            StatementList stats = (StatementList)statement_list();
            match(TokenType.ENDTRY);
            move();
            if (!isNext(TokenType.CATCH))
            {
                return new TryStatement(stats, try_token);
            }
            match(TokenType.CATCH);
            move();
            match(TokenType.L_PARA);
            move();
            match(TokenType.ID);
            ID id = new ID(lookAhead.lexeme, lookAhead);
            move();
            match(TokenType.R_PARA);
            move();
            StatementList cStats = (StatementList)statement_list();
            match(TokenType.ENDCATCH);
            move();
            return new TryStatement(stats, id, cStats, try_token);
        }

        IfStatement if_stat()
        {
            if (isNext(TokenType.IF))
            {
                Token ifToken = read();
                match(TokenType.L_PARA);
                read();
                Node node = expression();
                match(TokenType.R_PARA);
                read();
                ArrayList data = new ArrayList();
                StatementList ifstats = (StatementList)statement_list();
                data.Add(new IfData(node, ifstats));
                while (isNext(TokenType.ELIF))
                {
                    read();
                    match(TokenType.L_PARA);
                    read();
                    node = expression();
                    match(TokenType.R_PARA);
                    read();
                    ifstats = (StatementList)statement_list();
                    data.Add(new IfData(node, ifstats));
                }
                StatementList elseNode = null;
                if (isNext(TokenType.ELSE))
                {
                    read();
                    ifstats = (StatementList)statement_list();
                    elseNode = ifstats;
                }
                match(TokenType.ENDIF);
                read();
                return new IfStatement(data, elseNode, ifToken);
            }
            return null;
        }

        ForStatement for_stat()
        {
            if (isNext(TokenType.FOR))
            {
                Token ForToken = read();
                match(TokenType.L_PARA);
                move();
                BindStatement init_stat = bind_stat();
                match(TokenType.SEMICOLON);
                move();
                Node check_expr = expression();
                match(TokenType.SEMICOLON);
                move();
                BindStatement inc_stat = bind_stat();
                match(TokenType.R_PARA);
                move();
                StatementList stats = (StatementList)statement_list();
                match(TokenType.ENDLOOP);
                move();
                return new ForStatement(init_stat, check_expr, inc_stat, stats, ForToken);
            }
            return null;
        }

        WhileStatement while_stat()
        {
            if (isNext(TokenType.WHILE))
            {
                Token WhileToken = read();
                match(TokenType.L_PARA);
                move();
                Node expr = expression();
                match(TokenType.R_PARA);
                move();
                StatementList stats = (StatementList)statement_list();
                match(TokenType.ENDLOOP);
                move();
                return new WhileStatement(expr, stats, WhileToken);
            }
            return null;
        }

        FunctionStatement function_stat()
        {
            if (isNext(TokenType.FUNCTION))
            {
                Token FuncToken = read();
                match(TokenType.ID);
                string name = lookAhead.lexeme;
                move();
                match(TokenType.L_PARA);
                move();
                ArrayList parameters = new ArrayList();
                if (isNext(TokenType.ID))
                {
                    string param_name = lookAhead.lexeme;
                    move();
                    if (isNext(TokenType.AS))
                    {
                        move();
                        string param_type = lookAhead.lexeme;
                        move();
                        parameters.Add(new Parameter(param_name, param_type));
                    }
                    else
                    {
                        parameters.Add(new Parameter(param_name));
                    }
                    while (isNext(TokenType.COMMA))
                    {
                        move();
                        param_name = lookAhead.lexeme;
                        move();
                        if (isNext(TokenType.AS))
                        {
                            move();
                            string param_type = lookAhead.lexeme;
                            move();
                            parameters.Add(new Parameter(param_name, param_type));
                        }
                        else
                        {
                            parameters.Add(new Parameter(param_name));
                        }
                    }
                }
                match(TokenType.R_PARA);
                move();
                ArrayList globals = new ArrayList();
                if (isNext(TokenType.GLOBAL))
                {
                    move();
                    match(TokenType.ID);
                    globals.Add(lookAhead.lexeme);
                    move();
                    while (isNext(TokenType.COMMA))
                    {
                        move();
                        match(TokenType.ID);
                        globals.Add(lookAhead.lexeme);
                        move();
                    }
                    match(TokenType.SEMICOLON);
                    move();
                }
                StatementList stats = (StatementList)statement_list();
                match(TokenType.ENDFUNCTION);
                move();
                return new FunctionStatement(name, parameters, stats, globals, FuncToken);
            }
            return null;
        }

        ClassStatement class_stat()
        {
            if (!isNext(TokenType.CLASS))
            {
                return null;
            }
            Token classToken = read();
            match(TokenType.ID);
            string name = lookAhead.lexeme;
            move();
            ID parent = null;
            if (isNext(TokenType.EXTENDS))
            {
                move();
                parent = new ID(lookAhead.lexeme, lookAhead);
                move();
            }
            ArrayList vars = new ArrayList();
            Hashtable methods = new Hashtable();
            ArrayList constructors = new ArrayList();
            Hashtable staticMembers = new Hashtable();
            Hashtable staticPermissions = new Hashtable();
            while (true)
            {
                if (isNext(TokenType.MODIFIER))
                {
                    ArrayList mods = new ArrayList();
                    bool isStatic = false;
                    while (isNext(TokenType.MODIFIER))
                    {
                        if (lookAhead.lexeme == "static")
                        {
                            isStatic = true;
                        }
                        mods.Add(lookAhead.lexeme);
                        move();
                    }

                    if (isNext(TokenType.ID))
                    {
                        match(TokenType.ID);
                        if (isStatic)
                        {
                            staticMembers[lookAhead.lexeme] = new LangNumber(0, langManager.interpreter);
                            staticPermissions[lookAhead.lexeme] = mods;
                        }
                        else
                        {
                            vars.Add(new ClassMember(mods, lookAhead.lexeme));
                        }
                        move();
                        while (isNext(TokenType.COMMA))
                        {
                            move();
                            match(TokenType.ID);
                            if (isStatic)
                            {
                                staticMembers[lookAhead.lexeme] = new LangNumber(0, langManager.interpreter);
                                staticPermissions[lookAhead.lexeme] = mods;
                            }
                            else
                            {
                                vars.Add(new ClassMember(mods, lookAhead.lexeme));
                            }
                            move();
                        }
                        match(TokenType.SEMICOLON);
                        move();
                    }
                    else
                    {
                        match(TokenType.FUNCTION);
                        FunctionStatement stat = function_stat();
                        if (stat.name == name)
                        {
                            constructors.Add(stat);
                        }
                        else
                        {
                            if (!methods.ContainsKey(stat.name))
                            {
                                methods[stat.name] = new ArrayList();
                            }
                            ((ArrayList)methods[stat.name]).Add(new ClassMethod(mods, stat));
                        }
                    }
                }
                else if (isNext(TokenType.ID))
                {
                    vars.Add(new ClassMember(new ArrayList(), lookAhead.lexeme));
                    move();
                    while (isNext(TokenType.COMMA))
                    {
                        move();
                        match(TokenType.ID);
                        vars.Add(new ClassMember(new ArrayList(), lookAhead.lexeme));
                        move();
                    }
                    match(TokenType.SEMICOLON);
                    move();
                }
                else if (isNext(TokenType.FUNCTION))
                {
                    FunctionStatement stat = function_stat();
                    if (stat.name == name)
                    {
                        constructors.Add(stat);
                    }
                    else
                    {
                        if (!methods.ContainsKey(stat.name))
                        {
                            methods[stat.name] = new ArrayList();
                        }
                        ((ArrayList)methods[stat.name]).Add(new ClassMethod(new ArrayList(),stat));
                    }
                }
                else
                {
                    break;
                }
            }
            match(TokenType.ENDCLASS);
            move();
            return new ClassStatement(name, vars, staticMembers, staticPermissions, methods, constructors, classToken, parent);
        }
        #endregion

        #region simple_stats
        Statement simple_stat(bool isExpression = false)
        {
            Statement stat = break_stat();
            if (stat == null && !isExpression)
                stat = stop_stat();
            if (stat == null && !isExpression)
                stat = continue_stat();
            if (stat == null && !isExpression)
                stat = print_stat();
            if (stat == null)
                stat = scan_stat();
            if (stat == null)
                stat = bind_stat();
            if (stat == null)
                stat = func_call_stat();
            if (stat == null)
                stat = import_stat();
            if (stat == null)
                stat = class_init_stat();
            if (stat == null)
                stat = return_statement();
            if (stat == null)
                stat = raise_statement();
            return stat;
        }

        Statement print_stat()
        {
            if (isNext(TokenType.PRINT))
            {
                Token printToken = read();// 'print'
                Node expr = expression();
                PrintStatement stat = new PrintStatement(expr, printToken);
                while (isNext(TokenType.COMMA))
                {
                    move();
                    expr = expression();
                    stat.extras.Add(new PrintStatement(expr, printToken));
                }
                return stat;
            }
            return null;
        }

        Statement scan_stat()
        {
            if (isNext(TokenType.SCAN))
            {
                Token scanToken = read();// 'scan'
                match(TokenType.ID);
                Node expr = factor();
                ArrayList extras = new ArrayList();
                while (isNext(TokenType.COMMA))
                {
                    move();
                    match(TokenType.ID);
                    Node Id = factor();
                    extras.Add(new ScanStatement(Id, null, new ArrayList(), scanToken));
                }
                ID id = null;
                if (isNext(TokenType.AS))
                {
                    move();
                    match(TokenType.ID);
                    id = (ID)factor();
                    for (int i = 0; i < extras.Count; i++)
                    {
                        ScanStatement stat = ((ScanStatement)(extras[i]));
                        stat.scanType = id;
                        extras[i] = stat;
                    }
                }
                return new ScanStatement(expr, id, extras, scanToken);
            }
            return null;
        }

        BindStatement bind_stat()
        {
            if (!isNext(TokenType.LET))
                return null;
            move();
            Node node = dot_expr();
            try
            {
                match(TokenType.EQUAL);
            }
            catch (ParserException)
            {
                match(TokenType.REF_EQUAL);
            }
            Token _token = read();
            Node expr = expression();
            ArrayList extras = new ArrayList();
            while (isNext(TokenType.COMMA))
            {
                Node node_ = factor();
                Token token_ = read();
                Node expr_ = expression();
                extras.Add(new BindStatement(node_, expr_, new ArrayList(), token_));
            }
            return new BindStatement(node, expr, extras, _token);
        }

        Statement break_stat()
        {
            if (isNext(TokenType.BREAK))
            {
                Token breakToken = read();
                return new StoppingStatement(StatementType.BREAK, breakToken);
            }
            return null;
        }

        Statement continue_stat()
        {
            if (isNext(TokenType.CONTINUE))
            {
                return new StoppingStatement(StatementType.CONTINUE, read());
            }
            return null;
        }

        Statement stop_stat()
        {
            if (isNext(TokenType.STOP))
            {
                return new StoppingStatement(StatementType.STOP, read());
            }
            return null;
        }

        Statement func_call_stat()
        {
            if (isNext(TokenType.ID))
            {
                Token id = read();
                if (isNext(TokenType.L_PARA))
                {
                    move(); // (
                    if (isNext(TokenType.R_PARA))
                    {
                        move();
                        return new FunctionCallStatement(id.lexeme, new ArrayList(), id);
                    }
                    else
                    {
                        Node node = expression();
                        ArrayList parameters = new ArrayList();
                        parameters.Add(node);
                        while (isNext(TokenType.COMMA))
                        {
                            move();
                            node = expression();
                            parameters.Add(node);
                        }
                        move();// )
                        return new FunctionCallStatement(id.lexeme, parameters, id);
                    }
                }
                else
                {
                    retract();
                }
                return null;
            }
            return null;
        }

        Statement import_stat()
        {
            if (!isNext(TokenType.IMPORT))
                return null;
            Token importToken = read();
            match(TokenType.ID);
            ArrayList imports = new ArrayList();
            imports.Add(lookAhead.lexeme);
            move();
            while (isNext(TokenType.COMMA))
            {
                move();
                match(TokenType.ID);
                imports.Add(lookAhead.lexeme);
                move();
            }
            return new ImportStatement(imports, importToken);
        }

        ClassInitStatement class_init_stat()
        {
            if (!isNext(TokenType.NEW))
                return null;
            Token newToken = read();
            FunctionCallStatement stat = (FunctionCallStatement)func_call_stat();
            if (stat == null)
            {
                if (langManager.INTERPRET)
                {
                    langManager.lastErrorToken = lookAhead;
                }
                else
                {
                    langManager.lastLiveErrorToken = lookAhead;
                }
                throw new ParserException("Line " + newToken.line + ": " + "expected a constructor after 'new' token");
            }
            return new ClassInitStatement(stat, newToken);
        }

        ReturnStatement return_statement()
        {
            if (!isNext(TokenType.RETURN))
                return null;
            Token ret_token = read();
            Node expr = expression();
            return new ReturnStatement(expr, ret_token);
        }

        RaiseStatement raise_statement()
        {
            if (!isNext(TokenType.RAISE))
            {
                return null;
            }
            Token raise_tok = read();
            Node expr = expression();
            return new RaiseStatement(expr, raise_tok);
        }

        #endregion

        #region expressions
        Node expression(bool MustFind = true)
        {
            return logic_expr(MustFind);
        }

        Node logic_expr(bool MustFind = true)
        {
            Node node = comp_expr(MustFind);
            while (isNext(TokenType.AND) || isNext(TokenType.OR))
            {
                Token op = read();
                Node right = comp_expr(MustFind);
                NodeType nodeType = NodeType.PLUS;
                if (op.type == TokenType.AND)
                {
                    nodeType = NodeType.AND;
                }
                else if (op.type == TokenType.OR)
                {
                    nodeType = NodeType.OR;
                }
                node = new BinaryOperator(nodeType, node, right, op);
            }
            return node;
        }

        Node comp_expr(bool MustFind = true)
        {
            Node node = add_expr(MustFind);
            while (isNext(TokenType.GREATER_EQUAL) || isNext(TokenType.GREATER) || isNext(TokenType.SMALLER) || isNext(TokenType.SMALLER_EQUAL) || isNext(TokenType.EQUAL_TEST) || isNext(TokenType.NOT_EQUAL))
            {
                Token op = read();
                Node right = add_expr(MustFind);
                NodeType nodeType = NodeType.PLUS;
                if (op.type == TokenType.GREATER)
                {
                    nodeType = NodeType.GREATER;
                }
                else if (op.type == TokenType.GREATER_EQUAL)
                {
                    nodeType = NodeType.GREATER_EQUAL;
                }
                else if (op.type == TokenType.SMALLER)
                {
                    nodeType = NodeType.SMALLER;
                }
                else if (op.type == TokenType.SMALLER_EQUAL)
                {
                    nodeType = NodeType.SMALLER_EQUAL;
                }
                else if (op.type == TokenType.EQUAL_TEST)
                {
                    nodeType = NodeType.EQUAL_TEST;
                }
                else if (op.type == TokenType.NOT_EQUAL)
                {
                    nodeType = NodeType.NOT_EQUAL;
                }
                node = new BinaryOperator(nodeType, node, right, op);
            }
            return node;
        }

        Node add_expr(bool MustFind = true)
        {
            Node node = mul_expr(MustFind);
            while (isNext(TokenType.MINUS) || isNext(TokenType.PLUS))
            {
                Token op = read();
                Node right = mul_expr(MustFind);
                NodeType nodeType = NodeType.PLUS;
                if (op.type == TokenType.PLUS)
                {
                    nodeType = NodeType.PLUS;
                }
                else if (op.type == TokenType.MINUS)
                {
                    nodeType = NodeType.MINUS;
                }
                node = new BinaryOperator(nodeType, node, right, op);
            }
            return node;
        }

        Node mul_expr(bool MustFind = true)
        {
            Node node = pow_expr(MustFind);
            while (isNext(TokenType.MUL) || isNext(TokenType.DIV) || isNext(TokenType.DIV_INT) || isNext(TokenType.MOD))
            {
                Token op = read();
                Node right = pow_expr(MustFind);
                NodeType nodeType = NodeType.DIV;
                if (op.type == TokenType.MUL)
                {
                    nodeType = NodeType.MUL;
                }
                else if (op.type == TokenType.DIV)
                {
                    nodeType = NodeType.DIV;
                }
                else if (op.type == TokenType.DIV_INT)
                {
                    nodeType = NodeType.DIV_INT;
                }
                else if (op.type == TokenType.MOD)
                {
                    nodeType = NodeType.MOD;
                }
                node = new BinaryOperator(nodeType, node, right, op);
            }
            return node;
        }

        Node pow_expr(bool MustFind = true)
        {
            Node node = dot_expr(MustFind);
            if (isNext(TokenType.POW))
            {
                Token powToken = read();
                return new BinaryOperator(NodeType.POW, node, pow_expr(MustFind), powToken);
            }
            return node;
        }

        Node dot_expr(bool MustFind = true)
        {
            Node node = factor(MustFind);
            while (isNext(TokenType.DOT) || isNext(TokenType.L_BRACK))
            {
                Token dotToken = read();
                NodeType type = NodeType.DOT;
                if (dotToken.type == TokenType.L_BRACK)
                {
                    type = NodeType.BRACKETS;
                }
                Node _right;
                if (type == NodeType.DOT)
                {
                    _right = new ID(lookAhead.lexeme, lookAhead);
                    move();
                    if (isNext(TokenType.L_PARA))
                    {
                        retract();
                        _right = func_call_stat();
                    }
                }
                else
                {
                    _right = expression(MustFind);
                    move();
                }
                node = new BinaryOperator(type, node, _right, dotToken);
            }
            return node;
        }

        Node factor(bool MustFind = true)
        {
            if (isNext(TokenType.NOT))
            {
                Token lookedAhead = lookAhead;
                move();
                Node node = new UnaryOperator(NodeType.NOT, factor(), lookedAhead);
                return node;
            }
            else if (isNext(TokenType.MINUS))
            {
                Token lookedAhead = lookAhead;
                move();
                Node node = new UnaryOperator(NodeType.MINUS, factor(), lookedAhead);
                return node;
            }
            else if (isNext(TokenType.NUMBER))
            {
                Token lookedAhead = lookAhead;
                move();
                return new Number(lookedAhead.lexeme, lookedAhead);
            }
            else if (isNext(TokenType.ID))
            {
                Statement stat = simple_stat(true);
                if (stat != null)
                {
                    return stat;
                }
                Token lookedAhead = lookAhead;
                move();
                return new ID(lookedAhead.lexeme, lookedAhead);
            }
            else if (isNext(TokenType.STRING))
            {
                Token lookedAhead = lookAhead;
                move();
                return new StringVal(lookedAhead.lexeme, lookedAhead);
            }
            else if (isNext(TokenType.L_PARA))
            {
                Token tok = read();// (
                Node node = expression();
                node = new UnaryOperator(NodeType.PARAS, node, tok);
                match(TokenType.R_PARA);
                move();// )
                return node;
            }
            else if (isNext(TokenType.NEW))
            {
                return class_init_stat();
            }
            else
            {
                Node node = simple_stat(true);
                if (node != null)
                {
                    return node;
                }
            }
            if (MustFind)
            {
                if (langManager.INTERPRET)
                {
                    langManager.lastErrorToken = lookAhead;
                }
                else
                {
                    langManager.lastLiveErrorToken = lookAhead;
                }
                throw new ParserException("Line " + lookAhead.line + ": " + "Expected an expression, " + Convert.ToString(lookAhead.type) + " was found instead");
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}
