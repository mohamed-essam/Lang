﻿using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Collections;

namespace Lang.language
{
    #region Node
    abstract public class Node
    {
        internal NodeType nodeType;
        internal Token token;

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
        DOT
    }
    #endregion

    #region factors
    public class Number : Node
    {
        internal string value;

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
        public string name;

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
        public string val;

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
    #endregion

    public class BinaryOperator : Node
    {
        internal Node left, right;
        internal string lexeme;

        public BinaryOperator(NodeType _nodeType, Node _left, Node _right, string _lexeme, Token _token)
            : base(_nodeType, _token)
        {
            left = _left;
            right = _right;
            lexeme = _lexeme;
        }

        public override string ToString()
        {
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
        public ArrayList statements;

        public StatementList(ArrayList _statements, Token _token)
            : base(NodeType.STATEMENT_LIST, _token)
        {
            statements = _statements;
        }

        public override string ToString()
        {
            string ret = "";
            foreach (Statement stat in statements)
            {
                ret += stat.ToString();
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
            scanType.Dispose();
            scanType = null;
            foreach (ScanStatement stat in extras)
            {
                stat.Dispose();
            }
            extras.Clear();
            extras = null;
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
    }

    public class IfStatement:Statement
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
            foreach (Parameter param in parameters)
            {
                param.Dispose();
            }
            parameters.Clear();
            parameters = null;
            stats.Dispose();
            stats = null;
            globals.Clear();
            globals = null;
        }
    }

    public class ClassStatement : Statement
    {
        public string name;
        public ArrayList vars;
        public Hashtable methods;
        public ArrayList constructors;
        public ID parent;

        public ClassStatement(string _name, ArrayList _vars, Hashtable _methods, ArrayList _constructors, Token _token, ID _parent)
            : base(StatementType.CLASS, _token)
        {
            name = _name;
            vars = _vars;
            methods = _methods;
            constructors = _constructors;
            parent = _parent;
        }

        public override void Dispose()
        {
            name = null;
            vars.Clear();
            foreach (FunctionStatement stat in methods)
            {
                stat.Dispose();
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
            if(catchID != null)
                catchID.Dispose();
            if(catchStats != null)
                catchStats.Dispose();
            stats = catchStats = null;
            catchID = null;
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

        public Parser(LangManager _langManager)
        {
            langManager = _langManager;
        }

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
                lookAhead = (Token)tokens[tokens.Count-1];
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
                throw new Exception("Line "+lookAhead.line+": "+"Syntax error: expected "+type+" instead of "+lookAhead.type);
            }
        }

        void retract()
        {
            lookAhead = (Token)tokens[--p];
        }
        #endregion
        // Parser
        public Node parse()
        {
            return prog();
        }

        #region top_stats

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
                        match(TokenType.SEMICOLON);
                        move();
                        return new ClassFuncStatement(lastTry);
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
            if (lookAhead.type != TokenType.TRY)
                return null;
            Token try_token = read();
            StatementList stats = (StatementList)statement_list();
            match(TokenType.ENDTRY);
            move();
            if (lookAhead.type != TokenType.CATCH)
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
            if (lookAhead.type == TokenType.IF)
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
                while (lookAhead.type == TokenType.ELIF)
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
                if (lookAhead.type == TokenType.ELSE)
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
            if (lookAhead.type == TokenType.FOR)
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
            if (lookAhead.type == TokenType.WHILE)
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
            if (lookAhead.type == TokenType.FUNCTION)
            {
                Token FuncToken = read();
                match(TokenType.ID);
                string name = lookAhead.lexeme;
                move();
                match(TokenType.L_PARA);
                move();
                ArrayList parameters = new ArrayList();
                if (lookAhead.type == TokenType.ID)
                {
                    string param_name = lookAhead.lexeme;
                    move();
                    if (lookAhead.type == TokenType.AS)
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
                    while (lookAhead.type == TokenType.COMMA)
                    {
                        move();
                        param_name = lookAhead.lexeme;
                        move();
                        if (lookAhead.type == TokenType.AS)
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
                if (lookAhead.type == TokenType.GLOBAL)
                {
                    move();
                    match(TokenType.ID);
                    globals.Add(lookAhead.lexeme);
                    move();
                    while (lookAhead.type == TokenType.COMMA)
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
            if (lookAhead.type != TokenType.CLASS)
            {
                return null;
            }
            Token classToken = read();
            match(TokenType.ID);
            string name = lookAhead.lexeme;
            move();
            ID parent = null;
            if (lookAhead.type == TokenType.EXTENDS)
            {
                move();
                parent = new ID(lookAhead.lexeme, lookAhead);
                move();
            }
            ArrayList vars = new ArrayList();
            Hashtable methods = new Hashtable();
            ArrayList constructors = new ArrayList();
            while (true)
            {
                if (lookAhead.type == TokenType.ID)
                {
                    vars.Add(lookAhead.lexeme);
                    move();
                    while (lookAhead.type == TokenType.COMMA)
                    {
                        move();
                        match(TokenType.ID);
                        vars.Add(lookAhead.lexeme);
                        move();
                    }
                    match(TokenType.SEMICOLON);
                    move();
                }
                else if (lookAhead.type == TokenType.FUNCTION)
                {
                    FunctionStatement stat = function_stat();
                    if (stat.name == name)
                    {
                        constructors.Add(stat);
                    }
                    else
                    {
                        methods[stat.name] = stat;
                    }
                }
                else
                {
                    break;
                }
            }
            match(TokenType.ENDCLASS);
            move();
            return new ClassStatement(name, vars, methods, constructors, classToken, parent);
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
            if (lookAhead.type == TokenType.PRINT)
            {
                Token printToken = read();// 'print'
                Node expr = expression();
                PrintStatement stat = new PrintStatement(expr, printToken);
                while (lookAhead.type == TokenType.COMMA)
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
            if (lookAhead.type == TokenType.SCAN)
            {
                Token scanToken = read();// 'scan'
                match(TokenType.ID);
                Node expr = factor();
                ArrayList extras = new ArrayList();
                while (lookAhead.type == TokenType.COMMA)
                {
                    move();
                    match(TokenType.ID);
                    Node Id = factor();
                    extras.Add(new ScanStatement(Id, null, new ArrayList(), scanToken));
                }
                ID id = null;
                if (lookAhead.type == TokenType.AS)
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
            if (lookAhead.type != TokenType.LET)
                return null;
            move();
            Node node = dot_expr();
            Token _token = read();
            Node expr = expression();
            ArrayList extras = new ArrayList();
            while (lookAhead.type == TokenType.COMMA)
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
            if (lookAhead.type == TokenType.ID && lookAhead.lexeme == "break")
            {
                Token breakToken = read();
                return new StoppingStatement(StatementType.BREAK, breakToken);
            }
            return null;
        }

        Statement continue_stat()
        {
            if (lookAhead.type == TokenType.ID && lookAhead.lexeme == "continue")
            {
                return new StoppingStatement(StatementType.CONTINUE, read());
            }
            return null;
        }

        Statement stop_stat()
        {
            if (lookAhead.type == TokenType.STOP)
            {
                return new StoppingStatement(StatementType.STOP, read());
            }
            return null;
        }

        Statement func_call_stat()
        {
            if (lookAhead.type == TokenType.ID)
            {
                Token id = read();
                if (lookAhead.type == TokenType.L_PARA)
                {
                    move(); // (
                    if (lookAhead.type == TokenType.R_PARA)
                    {
                        move();
                        return new FunctionCallStatement(id.lexeme, new ArrayList(), id);
                    }
                    else
                    {
                        Node node = expression();
                        ArrayList parameters = new ArrayList();
                        parameters.Add(node);
                        while (lookAhead.type == TokenType.COMMA)
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
            if (lookAhead.type != TokenType.IMPORT)
                return null;
            Token importToken = read();
            match(TokenType.ID);
            ArrayList imports = new ArrayList();
            imports.Add(lookAhead.lexeme);
            move();
            while (lookAhead.type == TokenType.COMMA)
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
            if (lookAhead.type != TokenType.NEW)
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
                throw new Exception("Line " + newToken.line + ": " + "expected a constructor after 'new' token");
            }
            return new ClassInitStatement(stat, newToken);
        }

        ReturnStatement return_statement()
        {
            if (lookAhead.type != TokenType.RETURN)
                return null;
            Token ret_token = read();
            Node expr = expression();
            return new ReturnStatement(expr, ret_token);
        }

        RaiseStatement raise_statement()
        {
            if (lookAhead.type != TokenType.RAISE)
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
            while (lookAhead.type == TokenType.AND || lookAhead.type == TokenType.OR)
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
                node = new BinaryOperator(nodeType, node, right, op.lexeme, op);
            }
            return node;
        }

        Node comp_expr(bool MustFind = true)
        {
            Node node = add_expr(MustFind);
            while (lookAhead.type == TokenType.GREATER_EQUAL || lookAhead.type == TokenType.GREATER || lookAhead.type == TokenType.SMALLER || lookAhead.type == TokenType.SMALLER_EQUAL || lookAhead.type == TokenType.EQUAL_TEST || lookAhead.type == TokenType.NOT_EQUAL)
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
                node = new BinaryOperator(nodeType, node, right, op.lexeme, op);
            }
            return node;
        }

        Node add_expr(bool MustFind = true)
        {
            Node node = mul_expr(MustFind);
            while (lookAhead.type == TokenType.MINUS || lookAhead.type == TokenType.PLUS)
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
                node = new BinaryOperator(nodeType, node, right, op.lexeme, op);
            }
            return node;
        }

        Node mul_expr(bool MustFind = true)
        {
            Node node = pow_expr(MustFind);
            while (lookAhead.type == TokenType.MUL || lookAhead.type == TokenType.DIV || lookAhead.type == TokenType.DIV_INT || lookAhead.type == TokenType.MOD)
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
                node = new BinaryOperator(nodeType, node, right, op.lexeme, op);
            }
            return node;
        }

        Node pow_expr(bool MustFind = true)
        {
            Node node = dot_expr(MustFind);
            if (lookAhead.type == TokenType.POW)
            {
                Token powToken = read();
                return new BinaryOperator(NodeType.POW, node, pow_expr(MustFind), "^", powToken);
            }
            return node;
        }

        Node dot_expr(bool MustFind = true)
        {
            Node node = factor(MustFind);
            while (lookAhead.type == TokenType.DOT || lookAhead.type == TokenType.L_BRACK)
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
                    if (lookAhead.type == TokenType.L_PARA)
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
                node = new BinaryOperator(type, node, _right, dotToken.lexeme, dotToken);
            }
            return node;
        }

        Node factor(bool MustFind = true)
        {
            if (lookAhead.type == TokenType.MINUS)
            {
                Token lookedAhead = lookAhead;
                move();
                Node node = new BinaryOperator(NodeType.MINUS, new Number("0", null), factor(), "-", lookedAhead);
                return node;
            }
            else if (lookAhead.type == TokenType.NUMBER)
            {
                Token lookedAhead = lookAhead;
                move();
                return new Number(lookedAhead.lexeme, lookedAhead);
            }
            else if (lookAhead.type == TokenType.ID)
            {
                Statement stat = simple_stat(true);
                if (stat != null)
                {
                    return stat;
                }
                Token lookedAhead = lookAhead;
                move();
                /*
                BinaryOperator main = null;
                while (lookAhead.type == TokenType.L_BRACK)
                {
                    Token brackToken = read();
                    Node node = expression();
                    match(TokenType.R_BRACK);
                    move();
                    if (main == null)
                    {
                        main = new BinaryOperator(NodeType.BRACKETS, new ID(lookedAhead.lexeme, lookedAhead), node, "[]", brackToken);
                    }
                    else
                    {
                        main = new BinaryOperator(NodeType.BRACKETS, main, node, "[]", brackToken);
                    }
                }
                */
                return new ID(lookedAhead.lexeme, lookedAhead);
            }
            else if (lookAhead.type == TokenType.STRING)
            {
                Token lookedAhead = lookAhead;
                move();
                return new StringVal(lookedAhead.lexeme, lookedAhead);
            }
            else if (lookAhead.type == TokenType.L_PARA)
            {
                move();// (
                Node node = expression();
                move();// )
                return node;
            }
            else if (lookAhead.type == TokenType.NEW)
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
                throw new Exception("Line " + lookAhead.line + ": " + "Expected an expression, " + Convert.ToString(lookAhead.type) + " was found instead");
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}
