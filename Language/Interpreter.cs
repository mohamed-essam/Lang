using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Drawing;
using Lang.Language;
using System.Windows.Forms;

namespace Lang.language
{
    internal class InterpreterException : LangException
    {

        public InterpreterException()
            : base()
        {

        }
        public InterpreterException(string Message)
            : base(Message)
        {

        }
        public InterpreterException(string Message, Exception innerException)
            : base(Message, innerException)
        {

        }
    }
    internal class StackTraceEntry
    {
        internal string FileName;
        internal string FunctionName;
        internal int LineNumber;

        public StackTraceEntry(string _fileName, string _functionName, int _lineNumber)
        {
            FileName = _fileName;
            FunctionName = _functionName;
            LineNumber = _lineNumber;
        }

    }

    public class Interpreter
    {
        private StatementList root;
        internal ArrayList table;
        internal int level;
        internal Hashtable functions;
        internal Hashtable classes;
        internal bool keepWorking = true;
        internal ArrayList breakpoints;
        internal bool isStopped = false;
        internal string FileName;
        internal LangManager langManager;
        internal ArrayList StackTrace;
        private string lastFunctionCalled = "__MAIN__";
        internal CodeConsoleUI consoleUI;
        internal CodeGUI gui;
        private Hashtable EventHandlers;

        /// <summary>
        /// Creates a new Interpeter object
        /// </summary>
        /// <param name="_console">The LangConsole this code will read/write</param>
        /// <param name="_langManager">The LangManager that created this instance, for error reporting</param>
        public Interpreter(LangManager _langManager)
        {
            table = new ArrayList();
            functions = new Hashtable();
            classes = new Hashtable();
            breakpoints = new ArrayList();
            StackTrace = new ArrayList();
            langManager = _langManager;
        }

        /// <summary>
        /// Updates the tree
        /// </summary>
        /// <param name="_root">The new Tree root</param>
        public void updateRoot(StatementList _root)
        {
            root = _root;
            table.Clear();
            functions.Clear();
            classes.Clear();
            level = 0;
        }

        // Executer

        /// <summary>
        /// Starts the interpreting process
        /// </summary>
        /// <returns></returns>
        public LangObject interpret(bool createGUI = false)
        {
            functions.Clear();
            classes.Clear();
            StackTrace.Clear();
            foreach (Statement statement in root.statements)
            {
                if (statement.type == StatementType.FUNCTION)
                {
                    FunctionDefine((FunctionStatement)statement);
                }
                else if (statement.type == StatementType.CLASS)
                {
                    ClassDefine((ClassStatement)statement);
                }
            }
            #region Inhertiance
            Hashtable newClasses = new Hashtable();
            foreach (DictionaryEntry dic in classes)
            {
                ClassStatement cs = new ClassStatement(((ClassStatement)dic.Value).name, new ArrayList(), new Hashtable(), new ArrayList(), ((ClassStatement)dic.Value).token, ((ClassStatement)dic.Value).parent);
                if (cs.parent == null)
                {
                    newClasses[dic.Key] = dic.Value;
                    continue;
                }
                ArrayList arr = new ArrayList();
                ClassStatement cur = cs;
                arr.Add(dic.Value);
                while (true)
                {
                    ClassStatement parent = (ClassStatement)classes[cur.parent.name];
                    arr.Add(parent);
                    cur = parent;
                    if (cur.parent == null)
                        break;
                }
                for (int i = arr.Count - 1; i >= 0; i--)
                {
                    foreach (string dicc in ((ClassStatement)arr[i]).vars)
                    {
                        cs.vars.Add(dicc);
                    }
                    foreach (DictionaryEntry dicc in ((ClassStatement)arr[i]).methods)
                    {
                        ArrayList methods = (ArrayList)dicc.Value;
                        string name = (string)dicc.Key;
                        if (cs.methods.ContainsKey(name))
                        {
                            ArrayList csMethods = (ArrayList)cs.methods[name];
                            for (int m = 0; m < methods.Count; m++)
                            {
                                bool Found = false;
                                FunctionStatement mm;
                                mm = (FunctionStatement)methods[m];
                                for (int csM = 0; csM < csMethods.Count; csM++)
                                {
                                    FunctionStatement csMm;
                                    csMm = (FunctionStatement)csMethods[csM];
                                    if (mm.parameters.Count != csMm.parameters.Count)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        bool Works = true;
                                        for (int pr = 0; pr < mm.parameters.Count; pr++)
                                        {
                                            Parameter mmp, csMmp;
                                            mmp = (Parameter)mm.parameters[pr];
                                            csMmp = (Parameter)csMm.parameters[pr];
                                            if (!(mmp.type == "any" || csMmp.type == "any" || csMmp.type == mmp.type))
                                            {
                                                Works = false;
                                                break;
                                            }
                                        }
                                        if (Works)
                                        {
                                            Found = true;
                                            csMethods[csM] = mm;
                                            break;
                                        }
                                    }
                                    if (Found)
                                        break;
                                }
                                if (!Found)
                                {
                                    csMethods.Add(mm);
                                }
                            }
                        }
                        else
                        {
                            cs.methods[name] = methods;
                        }
                    }
                }
                cs.constructors = ((ClassStatement)dic.Value).constructors;
                newClasses[dic.Key] = cs;
            }
#endregion
            classes = newClasses;
            level = 0;
            table.Clear();
            table.Add(new Hashtable());
            keepWorking = true;
            //gui = new CodeGUI(Thread.CurrentThread);
            EventHandlers = new Hashtable();
            if (createGUI)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(runConsole));
                consoleUI = null;
                thread.Start(Thread.CurrentThread);
                thread = new Thread(new ParameterizedThreadStart(runGUI));
                gui = null;
                thread.Start(Thread.CurrentThread);
                Thread.Sleep(500);
                while (consoleUI == null || !consoleUI.IsHandleCreated)
                {
                    Thread.Sleep(1);
                }
                while (gui == null || !gui.IsHandleCreated)
                {
                    Thread.Sleep(1);
                }
            }
            LangObject val = null;
            try
            {
                val = statListDecider(root);
            }
            catch (LangException e)
            {
                if (consoleUI == null)
                {
                    throw;
                }
                consoleUI.Invoke((MethodInvoker)delegate() {
                    consoleUI.HandleExceptions(e, langManager.lastErrorToken, StackTrace);
                });
                gui.Invoke((MethodInvoker)delegate(){
                    gui.Close();
                });
                return val;
            }
            if (consoleUI != null)
            {
                try
                {
                    consoleUI.Invoke((MethodInvoker)delegate()
                    {
                        consoleUI.Close();
                    });
                }
                catch (InvalidOperationException) { }
                try
                {
                    gui.Invoke((MethodInvoker)delegate()
                    {
                        gui.Close();
                    });
                }
                catch (InvalidOperationException) { }
            }
            return val;
        }

        private void runConsole(object thread)
        {
            consoleUI = new CodeConsoleUI((Thread)thread, this);
            while (true)
            {
                try
                {
                    Application.Run(consoleUI);
                    break;
                }
                catch (Exception)
                {

                }
            }
        }

        private void runGUI(object thread)
        {
            gui = new CodeGUI((Thread)thread, this);
            while (true)
            {
                try
                {
                    Application.Run(gui);
                    break;
                }
                catch (Exception)
                {

                }
            }
        }

        public void Dispose()
        {
            foreach (Hashtable t in table)
            {
                t.Clear();
            }
            table.Clear();
            functions.Clear();
        }

        #region Tools

        void ShallowCopyToLevel(int from, int to)
        {
            Hashtable table_ = ((Hashtable)table[from]);
            foreach (DictionaryEntry item in table_)
            {
                ((Hashtable)table[to])[item.Key] = ((LangObject)item.Value).Clone();
            }
        }

        void increaseLevel()
        {
            level++;
            if (table.Count > level)
            {
                ((Hashtable)table[level]).Clear();
                ShallowCopyToLevel(level - 1, level);
            }
            else
            {
                table.Add(new Hashtable());
                ShallowCopyToLevel(level - 1, level);
            }
        }

        void decreaseLevel()
        {
            level--;
        }

        void checkParameterNumber(string functionName, int expected, FunctionCallStatement callstat)
        {
            if (callstat.parameters.Count != expected)
            {
                langManager.lastErrorToken = ((Node)callstat.parameters[0]).token;
                throw new InterpreterException("Line " + ((Node)callstat.parameters[0]).token.line + ": " + "Function '" + functionName + "' expectes " + expected + " parameters, " + callstat.parameters.Count + " given");
            }
        }

        void FunctionDefine(FunctionStatement stat)
        {
            if (functions[stat.name] == null)
                functions[stat.name] = new ArrayList();
            ((ArrayList)functions[stat.name]).Add(stat);
        }

        void ClassDefine(ClassStatement stat)
        {
            classes[stat.name] = stat;
        }

        #endregion

        #region deciders
        LangObject statListDecider(StatementList node)
        {
            if (!keepWorking)
                return new LangState("stop", this);
            for(int i = 0; i < node.statements.Count; i++)
            {
                Statement stat = (Statement)node.statements[i];
                if (breakpoints.Contains(stat.token.line) && stat.token.file == FileName)
                {
                    consoleUI.Invoke((MethodInvoker)delegate()
                    {
                        consoleUI.Debug((Hashtable)table[level], stat.token.line);
                    });
                }
                LangObject ret = statDecider(stat);
                if (ret.objectType == ObjectType.STATE)
                {
                    LangState _ret = (LangState)ret;
                    if (_ret.message == "break" || _ret.message == "continue" || _ret.message == "stop" || _ret.message == "return")
                    {
                        return ret;
                    }
                }

            }
            return new LangNumber(0, this);
        }

        LangObject statDecider(Statement node)
        {
            if (!keepWorking)
                return new LangState("stop", this);
            switch (node.type)
            {
                case StatementType.BIND:
                    return bindStatInterpret(node);
                case StatementType.PRINT:
                    return printStatInterpret(node);
                case StatementType.SCAN:
                    return scanStatInterpret(node);
                case StatementType.IF:
                    return ifStatInterpret(node);
                case StatementType.FOR:
                    return forStatInterpret(node);
                case StatementType.WHILE:
                    return whileStatInterpret(node);
                case StatementType.BREAK:
                    return new LangState("break", this);
                case StatementType.CONTINUE:
                    return new LangState("continue", this);
                case StatementType.STOP:
                    return new LangState("stop", this);
                case StatementType.RETURN:
                    return new LangState("return", decider(((ReturnStatement)node).expr), this);
                case StatementType.FUNCTION_CALL:
                    return function_decide(node);
                case StatementType.IMPORT:
                    return importStatInterpret(node);
                case StatementType.CLASS_NEW:
                    return newStatInterpret(node);
                case StatementType.CLASS_FUNCTION:
                    ClassFuncStatement stat = (ClassFuncStatement)node;
                    if (stat.expr.nodeType == NodeType.BRACKETS)
                    {
                        return BracketsOperatorInterpret(stat.expr);
                    }
                    else
                    {
                        return DotOperatorInterpret(stat.expr);
                    }
                case StatementType.RAISE:
                    return raiseStatInterpret(node);
                case StatementType.TRY:
                    return tryStatInterpret(node);
            }
            return new LangNumber(0, this);
        }

        LangObject function_decide(Node node)
        {
            if (!keepWorking)
                return new LangState("stop", this);
            FunctionCallStatement stat = (FunctionCallStatement)node;
            if (level > 5000)
            {
                langManager.lastLiveErrorToken = node.token;
                throw new InterpreterException("Maximum recursion limit exceeded");
            }
            #region built in
            #region Simple
            #region count
            if (stat.name == "count")
            {
                checkParameterNumber("count", 1, stat);
                Node node_ = (Node)stat.parameters[0];
                LangObject ret = null;
                if (node_.nodeType == NodeType.BRACKETS)
                {
                    ret = BracketsOperatorInterpret(node_);
                }
                else if (node_.nodeType == NodeType.DOT)
                {
                    ret = DotOperatorInterpret(node_);
                }
                else if (node_.nodeType == NodeType.ID)
                {
                    ret = idInterpret(node_);
                }
                else
                {
                    langManager.lastErrorToken = node_.token;
                    throw new InterpreterException("Line " + node_.token.line + ": Function 'count' Expects parameter 1 to be 'map', '" + node_.nodeType + "' Given");
                }
                if (ret == null || ret.objectType != ObjectType.MAP)
                {
                    throw new InterpreterException("Line " + node_.token.line + ": Function 'count' Expects parameter 1 to be 'map'");
                }
                return new LangNumber(((LangMap)ret).arrayValue.Count, this);
            }
            #endregion
            #region int
            else if (stat.name == "int")
            {
                checkParameterNumber("int", 1, stat);
                LangObject expr = decider((Node)stat.parameters[0]);
                if (expr.objectType == ObjectType.NUMBER)
                {
                    return new LangNumber((int)((LangNumber)expr).numberValue, this);
                }
                if (expr.objectType == ObjectType.STRING)
                {
                    string val = ((LangString)expr).stringValue;
                    double val_;
                    if (double.TryParse(val, out val_))
                    {
                        return new LangNumber((int)val_, this);
                    }
                    langManager.lastErrorToken = ((Node)stat.parameters[0]).token;
                    throw new InterpreterException("Line " + ((Node)stat.parameters[0]).token.line + ": " + "Invalid number format");
                }
                if (expr.objectType == ObjectType.MAP)
                {
                    langManager.lastErrorToken = ((Node)stat.parameters[0]).token;
                    throw new InterpreterException("Line " + ((Node)stat.parameters[0]).token.line + ": " + "Cannot convert from type 'map' to type 'number'");
                }
                throw new NotImplementedException();
            }
            #endregion
            #region string
            else if (stat.name == "string")
            {
                checkParameterNumber("string", 1, stat);
                LangObject expr = decider((Node)stat.parameters[0]);
                switch (expr.objectType)
                {
                    case ObjectType.STRING:
                        return expr;
                    case ObjectType.NUMBER:
                        return new LangString(Convert.ToString(((LangNumber)expr).numberValue), this);
                    case ObjectType.ARRAY:
                        throw new NotImplementedException();
                    case ObjectType.MAP:
                        langManager.lastErrorToken = ((Node)stat.parameters[0]).token;
                        throw new InterpreterException("Line " + ((Node)stat.parameters[0]).token.line + ": " + "Cannot convert from type 'map' to type 'string'");
                    default:
                        throw new NotImplementedException();
                }
            }
            #endregion
            #region map
            else if (stat.name == "map")
            {
                checkParameterNumber("map", 0, stat);
                return new LangMap(new Hashtable(), this);
            }
            #endregion
            #region strlen
            else if (stat.name == "strlen")
            {
                checkParameterNumber("strlen", 1, stat);
                LangObject obj = decider(((Node)stat.parameters[0]));
                if (obj.objectType != ObjectType.STRING)
                {
                    throw new InterpreterException("Line " + stat.token.line + ": " + "Function 'strlen' expects parameter 1 to be 'string', " + Convert.ToString(obj.objectType) + " Found");
                }
                return new LangNumber(((LangString)obj).stringValue.Length, this);
            }
            #endregion
            #endregion
            #region getPage
            else if (stat.name == "getPage")
            {
                checkParameterNumber("getPage", 1, stat);
                LangObject param = decider(((Node)stat.parameters[0]));
                if (param.objectType != ObjectType.STRING)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'string', '" + Convert.ToString(param.objectType) + "' Found");
                }
                WebClient client = new WebClient();
                string ret = client.DownloadString(((LangString)param).stringValue);
                client.Dispose();
                return new LangString(ret, this);
            }
            #endregion
            #region regexSearch
            else if (stat.name == "regexSearch")
            {
                checkParameterNumber("regexSearch", 2, stat);
                LangObject param1 = decider((Node)stat.parameters[0]);
                LangObject param2 = decider((Node)stat.parameters[1]);
                if (param1.objectType != ObjectType.STRING)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'string', '" + Convert.ToString(param1.objectType) + "' Found");
                }
                if (param2.objectType != ObjectType.STRING)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'string', '" + Convert.ToString(param2.objectType) + "' Found");
                }
                string regexp = ((LangString)param1).stringValue;
                string searchIn = ((LangString)param2).stringValue;
                Hashtable tbl = new Hashtable();
                MatchCollection col = Regex.Matches(searchIn, regexp);
                foreach (Match mt in col)
                {
                    tbl[(double)tbl.Keys.Count] = new LangNumber(mt.Index, this);
                    tbl[(double)tbl.Keys.Count] = new LangNumber(mt.Length, this);
                }
                return new LangMap(tbl, this);
            }
            #endregion
            #region cmd
            else if (stat.name == "cmd")
            {
                checkParameterNumber("cmd", 1, stat);
                LangObject param1 = decider(((Node)stat.parameters[0]));
                if (param1.objectType != ObjectType.STRING)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'string', '" + Convert.ToString(param1.objectType) + "' Found");
                }
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C " + ((LangString)param1).stringValue;
                process.StartInfo = startInfo;
                return new LangNumber(Convert.ToInt32(process.Start()), this);
            }
            #endregion
            #region run
            else if (stat.name == "run")
            {
                checkParameterNumber("run", 2, stat);
                LangObject param1 = decider((Node)stat.parameters[0]);
                LangObject param2 = decider((Node)stat.parameters[1]);
                if (param1.objectType != ObjectType.STRING)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'string', '" + Convert.ToString(param1.objectType) + "' Found");
                }
                if (param2.objectType != ObjectType.STRING)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'string', '" + Convert.ToString(param2.objectType) + "' Found");
                }
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = Directory.GetCurrentDirectory() + ((LangString)param1).stringValue;
                startInfo.Arguments = ((LangString)param2).stringValue;
                process.StartInfo = startInfo;
                return new LangNumber(Convert.ToInt32(process.Start()), this);
            }
            #endregion
            #region Files
            #region getFile
            else if (stat.name == "getFile")
            {
                checkParameterNumber("getFile", 1, stat);
                LangObject param1 = decider(((Node)stat.parameters[0]));
                if (param1.objectType != ObjectType.STRING)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'string', '" + Convert.ToString(param1.objectType) + "' Found");
                }
                string filePath = ((LangString)param1).stringValue;
                if (File.Exists(filePath))
                {
                    return new LangString(File.ReadAllText(filePath), this);
                }
                if (!filePath.Contains(":"))
                {
                    if (filePath.StartsWith("\\"))
                    {
                        filePath = Directory.GetCurrentDirectory() + filePath;
                    }
                    else
                    {
                        filePath = Directory.GetCurrentDirectory() + "\\" + filePath;
                    }
                    if (File.Exists(filePath))
                    {
                        return new LangString(File.ReadAllText(filePath), this);
                    }
                    else
                    {
                        throw new InterpreterException("Line " + node.token.line + ": " + "File path '" + filePath + "doesn't exist!");
                    }
                }
                throw new InterpreterException("Line " + node.token.line + ": " + "File path '" + filePath + "doesn't exist!");
            }
            #endregion
            #region saveFile
            else if (stat.name == "saveFile")
            {
                checkParameterNumber("saveFile", 2, stat);
                LangObject param1 = decider((Node)stat.parameters[0]);
                LangObject param2 = decider((Node)stat.parameters[1]);
                if (param1.objectType != ObjectType.STRING)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'string', '" + Convert.ToString(param1.objectType) + "' Found");
                }
                if (param2.objectType != ObjectType.STRING)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'string', '" + Convert.ToString(param2.objectType) + "' Found");
                }
                try
                {
                    File.WriteAllText(((LangString)param1).stringValue, ((LangString)param2).stringValue);
                }
                catch (Exception)
                {
                    return new LangNumber(0, this);
                }
                return new LangNumber(1, this);
            }
            #endregion
            #endregion
            #region Image Operations
            #region getImage
            else if (stat.name == "getImage")
            {
                checkParameterNumber("getImage", 1, stat);
                LangObject param1 = decider(((Node)stat.parameters[0]));
                if (param1.objectType != ObjectType.STRING)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'string', '" + Convert.ToString(param1.objectType) + "' Found");
                }
                string filePath = ((LangString)param1).stringValue;
                if (!File.Exists(filePath))
                {
                    if (!filePath.Contains(":"))
                    {
                        if (filePath.StartsWith("\\"))
                        {
                            filePath = Directory.GetCurrentDirectory() + filePath;
                        }
                        else
                        {
                            filePath = Directory.GetCurrentDirectory() + "\\" + filePath;
                        }
                    }
                    if (!File.Exists(filePath))
                    {
                        throw new InterpreterException("Line " + node.token.line + ": " + "File path '" + filePath + "' doesn't exist");
                    }
                }
                Bitmap img = null;
                using (var fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open))
                {
                    img = new Bitmap(fs);
                }
                LangImage ret = new LangImage(img, this);
                return ret;
            }
            #endregion
            #region saveImage
            else if (stat.name == "saveImage")
            {
                checkParameterNumber("saveImage", 2, stat);
                LangObject param1, param2;
                param1 = decider((Node)stat.parameters[0]);
                param2 = decider((Node)stat.parameters[1]);
                if (param1.objectType != ObjectType.IMAGE)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'image', '" + Convert.ToString(param1.objectType) + "' Found");
                }
                if (param2.objectType != ObjectType.STRING)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'string', '" + Convert.ToString(param2.objectType) + "' Found");
                }
                try
                {
                    string filePath = ((LangString)param2).stringValue;
                    if (!filePath.Contains(":"))
                    {
                        if (filePath.StartsWith("\\"))
                        {
                            filePath = Directory.GetCurrentDirectory() + filePath;
                        }
                        else
                        {
                            filePath = Directory.GetCurrentDirectory() + "\\" + filePath;
                        }
                    }
                    ((LangImage)param1).imageValue.Save(filePath);
                }
                catch (Exception)
                {
                    return new LangNumber(0, this);
                }
                return new LangNumber(1, this);
            }
            #endregion
            #region newImage
            else if (stat.name == "newImage")
            {
                checkParameterNumber("newImage", 5, stat);
                LangObject _W, _H, _R, _G, _B;
                _W = decider((Node)stat.parameters[0]);
                _H = decider((Node)stat.parameters[1]);
                _R = decider((Node)stat.parameters[2]);
                _G = decider((Node)stat.parameters[3]);
                _B = decider((Node)stat.parameters[4]);
                #region confirm Types
                if (_W.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'number', '" + Convert.ToString(_W.objectType) + "' Found");
                }
                if (_H.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'number', '" + Convert.ToString(_H.objectType) + "' Found");
                }
                if (_R.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 3 to be 'number', '" + Convert.ToString(_R.objectType) + "' Found");
                }
                if (_G.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 4 to be 'number', '" + Convert.ToString(_G.objectType) + "' Found");
                }
                if (_B.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 5 to be 'number', '" + Convert.ToString(_B.objectType) + "' Found");
                }
                #endregion
                LangNumber W = (LangNumber)_W,
                           H = (LangNumber)_H,
                           R = (LangNumber)_R,
                           G = (LangNumber)_G,
                           B = (LangNumber)_B;
                LangImage created = new LangImage(new Bitmap((int)W.numberValue, (int)H.numberValue), this);
                using (Graphics graphics = Graphics.FromImage(created.imageValue))
                {
                    graphics.FillRectangle(new SolidBrush(Color.FromArgb((int)R.numberValue, (int)G.numberValue, (int)B.numberValue)), new Rectangle(0, 0, (int)W.numberValue, (int)H.numberValue));
                }
                return created;
            }
            #endregion
            #region setImagePixel
            else if (stat.name == "setImagePixel")
            {
                checkParameterNumber("setImagePixel", 6, stat);
                LangObject _img, _X, _Y, _R, _G, _B;
                _img = decider((Node)stat.parameters[0]);
                _X = decider((Node)stat.parameters[1]);
                _Y = decider((Node)stat.parameters[2]);
                _R = decider((Node)stat.parameters[3]);
                _G = decider((Node)stat.parameters[4]);
                _B = decider((Node)stat.parameters[5]);
                #region TypeConfirming
                if (_img.objectType != ObjectType.IMAGE)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'image', '" + Convert.ToString(_img.objectType) + "' Found");
                }
                if (_X.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'number', '" + Convert.ToString(_X.objectType) + "' Found");
                }
                if (_Y.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 3 to be 'number', '" + Convert.ToString(_Y.objectType) + "' Found");
                }
                if (_R.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 4 to be 'number', '" + Convert.ToString(_R.objectType) + "' Found");
                }
                if (_G.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 5 to be 'number', '" + Convert.ToString(_G.objectType) + "' Found");
                }
                if (_B.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 6 to be 'number', '" + Convert.ToString(_B.objectType) + "' Found");
                }
                #endregion
                LangImage img = (LangImage)_img;
                LangNumber X = (LangNumber)_X,
                           Y = (LangNumber)_Y,
                           R = (LangNumber)_R,
                           G = (LangNumber)_G,
                           B = (LangNumber)_B;
                img.imageValue.SetPixel((int)X.numberValue, (int)Y.numberValue, Color.FromArgb((int)R.numberValue, (int)G.numberValue, (int)B.numberValue));
                return new LangNumber(1, this);
            }
            #endregion
            #region getImagePixel
            else if (stat.name == "getImagePixel")
            {
                checkParameterNumber("getImagePixel", 3, stat);
                LangObject _img, _X, _Y;
                _img = decider((Node)stat.parameters[0]);
                _X = decider((Node)stat.parameters[1]);
                _Y = decider((Node)stat.parameters[2]);
                if (_img.objectType != ObjectType.IMAGE)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'image', '" + Convert.ToString(_img.objectType) + "' Found");
                }
                if (_X.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'number', '" + Convert.ToString(_X.objectType) + "' Found");
                }
                if (_Y.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 3 to be 'number', '" + Convert.ToString(_Y.objectType) + "' Found");
                }
                LangImage img = (LangImage)_img;
                LangNumber X = (LangNumber)_X,
                           Y = (LangNumber)_Y;
                Hashtable tbl = new Hashtable();
                tbl[0.0] = new LangNumber(img.imageValue.GetPixel((int)X.numberValue, (int)Y.numberValue).R, this);
                tbl[1.0] = new LangNumber(img.imageValue.GetPixel((int)X.numberValue, (int)Y.numberValue).G, this);
                tbl[2.0] = new LangNumber(img.imageValue.GetPixel((int)X.numberValue, (int)Y.numberValue).B, this);
                return new LangMap(tbl, this);
            }
            #endregion
            #region getImageWidth
            else if (stat.name == "getImageWidth")
            {
                checkParameterNumber("getImageWidth", 1, stat);
                LangObject _img = decider((Node)stat.parameters[0]);
                if (_img.objectType != ObjectType.IMAGE)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'image', '" + Convert.ToString(_img.objectType) + "' Found");
                }
                LangImage img = (LangImage)_img;

                return new LangNumber(img.imageValue.Width, this);
            }
            #endregion
            #region getImageHeight
            else if (stat.name == "getImageHeight")
            {
                checkParameterNumber("getImageHeight", 1, stat);
                LangObject _img = decider((Node)stat.parameters[0]);
                if (_img.objectType != ObjectType.IMAGE)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'image', '" + Convert.ToString(_img.objectType) + "' Found");
                }
                LangImage img = (LangImage)_img;

                return new LangNumber(img.imageValue.Height, this);
            }
            #endregion
            #region drawImageLine
            else if (stat.name == "drawImageLine")
            {
                checkParameterNumber("drawImageLine", 9, stat);
                LangObject _img, _X1, _Y1, _X2, _Y2, _R, _G, _B, _T;
                _img = decider((Node)stat.parameters[0]);
                _X1 = decider((Node)stat.parameters[1]);
                _Y1 = decider((Node)stat.parameters[2]);
                _X2 = decider((Node)stat.parameters[3]);
                _Y2 = decider((Node)stat.parameters[4]);
                _R = decider((Node)stat.parameters[5]);
                _G = decider((Node)stat.parameters[6]);
                _B = decider((Node)stat.parameters[7]);
                _T = decider((Node)stat.parameters[8]);
                #region TypeConfirming
                if (_img.objectType != ObjectType.IMAGE)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'image', '" + Convert.ToString(_img.objectType) + "' Found");
                }
                if (_X1.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'number', '" + Convert.ToString(_X1.objectType) + "' Found");
                }
                if (_Y1.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 3 to be 'number', '" + Convert.ToString(_Y1.objectType) + "' Found");
                }
                if (_X2.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 4 to be 'number', '" + Convert.ToString(_X2.objectType) + "' Found");
                }
                if (_Y2.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 5 to be 'number', '" + Convert.ToString(_Y2.objectType) + "' Found");
                }
                if (_R.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 6 to be 'number', '" + Convert.ToString(_R.objectType) + "' Found");
                }
                if (_G.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 7 to be 'number', '" + Convert.ToString(_G.objectType) + "' Found");
                }
                if (_B.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 8 to be 'number', '" + Convert.ToString(_B.objectType) + "' Found");
                }
                if (_T.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 9 to be 'number', '" + Convert.ToString(_B.objectType) + "' Found");
                }
                #endregion
                LangImage img = (LangImage)_img;
                LangNumber X1 = (LangNumber)_X1,
                           Y1 = (LangNumber)_Y1,
                           X2 = (LangNumber)_X2,
                           Y2 = (LangNumber)_Y2,
                           R = (LangNumber)_R,
                           G = (LangNumber)_G,
                           B = (LangNumber)_B,
                           T = (LangNumber)_T;
                using (Graphics graphics = Graphics.FromImage(img.imageValue))
                {
                    Pen pen = new Pen(Color.FromArgb((int)R.numberValue, (int)G.numberValue, (int)B.numberValue), (float)T.numberValue);
                    graphics.DrawLine(pen, new Point((int)X1.numberValue, (int)Y1.numberValue), new Point((int)X2.numberValue, (int)Y2.numberValue));
                }
                return new LangNumber(0, this);
            }
            #endregion
            #region drawImageRectangle
            else if (stat.name == "drawImageRectangle")
            {
                checkParameterNumber("drawImageRectangle", 9, stat);
                LangObject _img, _X1, _Y1, _X2, _Y2, _R, _G, _B, _T;
                _img = decider((Node)stat.parameters[0]);
                _X1 = decider((Node)stat.parameters[1]);
                _Y1 = decider((Node)stat.parameters[2]);
                _X2 = decider((Node)stat.parameters[3]);
                _Y2 = decider((Node)stat.parameters[4]);
                _R = decider((Node)stat.parameters[5]);
                _G = decider((Node)stat.parameters[6]);
                _B = decider((Node)stat.parameters[7]);
                _T = decider((Node)stat.parameters[8]);
                #region TypeConfirming
                if (_img.objectType != ObjectType.IMAGE)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'image', '" + Convert.ToString(_img.objectType) + "' Found");
                }
                if (_X1.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'number', '" + Convert.ToString(_X1.objectType) + "' Found");
                }
                if (_Y1.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 3 to be 'number', '" + Convert.ToString(_Y1.objectType) + "' Found");
                }
                if (_X2.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 4 to be 'number', '" + Convert.ToString(_X2.objectType) + "' Found");
                }
                if (_Y2.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 5 to be 'number', '" + Convert.ToString(_Y2.objectType) + "' Found");
                }
                if (_R.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 6 to be 'number', '" + Convert.ToString(_R.objectType) + "' Found");
                }
                if (_G.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 7 to be 'number', '" + Convert.ToString(_G.objectType) + "' Found");
                }
                if (_B.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 8 to be 'number', '" + Convert.ToString(_B.objectType) + "' Found");
                }
                if (_T.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 9 to be 'number', '" + Convert.ToString(_B.objectType) + "' Found");
                }
                #endregion
                LangImage img = (LangImage)_img;
                LangNumber X1 = (LangNumber)_X1,
                           Y1 = (LangNumber)_Y1,
                           X2 = (LangNumber)_X2,
                           Y2 = (LangNumber)_Y2,
                           R = (LangNumber)_R,
                           G = (LangNumber)_G,
                           B = (LangNumber)_B,
                           T = (LangNumber)_T;
                using (Graphics graphics = Graphics.FromImage(img.imageValue))
                {
                    Pen pen = new Pen(Color.FromArgb((int)R.numberValue, (int)G.numberValue, (int)B.numberValue), (float)T.numberValue);
                    Point start = new Point((int)X1.numberValue, (int)Y1.numberValue);
                    Size size = new Size(new Point((int)(X2.numberValue - X1.numberValue), (int)(Y2.numberValue - Y1.numberValue)));
                    Rectangle rect = new Rectangle(start, size);
                    graphics.DrawRectangle(pen, rect);
                }
                return new LangNumber(0, this);
            }
            #endregion
            #region drawImageFilledRectangle
            else if (stat.name == "drawImageFilledRectangle")
            {
                checkParameterNumber("drawImageFilledRectangle", 12, stat);
                LangObject _img, _X1, _Y1, _X2, _Y2, _R, _G, _B, _T, _R2, _G2, _B2;
                _img = decider((Node)stat.parameters[0]);
                _X1 = decider((Node)stat.parameters[1]);
                _Y1 = decider((Node)stat.parameters[2]);
                _X2 = decider((Node)stat.parameters[3]);
                _Y2 = decider((Node)stat.parameters[4]);
                _R = decider((Node)stat.parameters[5]);
                _G = decider((Node)stat.parameters[6]);
                _B = decider((Node)stat.parameters[7]);
                _T = decider((Node)stat.parameters[8]);
                _R2 = decider((Node)stat.parameters[9]);
                _G2 = decider((Node)stat.parameters[10]);
                _B2 = decider((Node)stat.parameters[11]);
                #region TypeConfirming
                if (_img.objectType != ObjectType.IMAGE)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'image', '" + Convert.ToString(_img.objectType) + "' Found");
                }
                if (_X1.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'number', '" + Convert.ToString(_X1.objectType) + "' Found");
                }
                if (_Y1.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 3 to be 'number', '" + Convert.ToString(_Y1.objectType) + "' Found");
                }
                if (_X2.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 4 to be 'number', '" + Convert.ToString(_X2.objectType) + "' Found");
                }
                if (_Y2.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 5 to be 'number', '" + Convert.ToString(_Y2.objectType) + "' Found");
                }
                if (_R.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 6 to be 'number', '" + Convert.ToString(_R.objectType) + "' Found");
                }
                if (_G.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 7 to be 'number', '" + Convert.ToString(_G.objectType) + "' Found");
                }
                if (_B.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 8 to be 'number', '" + Convert.ToString(_B.objectType) + "' Found");
                }
                if (_T.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 9 to be 'number', '" + Convert.ToString(_T.objectType) + "' Found");
                }
                if (_R2.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 6 to be 'number', '" + Convert.ToString(_R2.objectType) + "' Found");
                }
                if (_G2.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 7 to be 'number', '" + Convert.ToString(_G2.objectType) + "' Found");
                }
                if (_B2.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 8 to be 'number', '" + Convert.ToString(_B2.objectType) + "' Found");
                }
                #endregion
                LangImage img = (LangImage)_img;
                LangNumber X1 = (LangNumber)_X1,
                           Y1 = (LangNumber)_Y1,
                           X2 = (LangNumber)_X2,
                           Y2 = (LangNumber)_Y2,
                           R = (LangNumber)_R,
                           G = (LangNumber)_G,
                           B = (LangNumber)_B,
                           T = (LangNumber)_T,
                           R2 = (LangNumber)_R2,
                           G2 = (LangNumber)_G2,
                           B2 = (LangNumber)_B2;
                using (Graphics graphics = Graphics.FromImage(img.imageValue))
                {
                    Pen pen = new Pen(Color.FromArgb((int)R.numberValue, (int)G.numberValue, (int)B.numberValue), (float)T.numberValue);
                    Point start = new Point((int)X1.numberValue, (int)Y1.numberValue);
                    Size size = new Size(new Point((int)(X2.numberValue - X1.numberValue), (int)(Y2.numberValue - Y1.numberValue)));
                    Rectangle rect = new Rectangle(start, size);
                    graphics.FillRectangle(new SolidBrush(Color.FromArgb((int)R2.numberValue, (int)G2.numberValue, (int)B2.numberValue)), rect);
                    graphics.DrawRectangle(pen, rect);
                }
                return new LangNumber(0, this);
            }
            #endregion
            #region drawImageCircle
            else if (stat.name == "drawImageCircle")
            {
                checkParameterNumber("drawImageCircle", 8, stat);
                LangObject _img, _X, _Y, _RAD, _R, _G, _B, _T;
                _img = decider((Node)stat.parameters[0]);
                _X = decider((Node)stat.parameters[1]);
                _Y = decider((Node)stat.parameters[2]);
                _RAD = decider((Node)stat.parameters[3]);
                _R = decider((Node)stat.parameters[4]);
                _G = decider((Node)stat.parameters[5]);
                _B = decider((Node)stat.parameters[6]);
                _T = decider((Node)stat.parameters[7]);
                #region TypeConfirming
                if (_img.objectType != ObjectType.IMAGE)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'image', '" + Convert.ToString(_img.objectType) + "' Found");
                }
                if (_X.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'number', '" + Convert.ToString(_X.objectType) + "' Found");
                }
                if (_Y.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 3 to be 'number', '" + Convert.ToString(_Y.objectType) + "' Found");
                }
                if (_RAD.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 4 to be 'number', '" + Convert.ToString(_RAD.objectType) + "' Found");
                }
                if (_R.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 5 to be 'number', '" + Convert.ToString(_R.objectType) + "' Found");
                }
                if (_G.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 6 to be 'number', '" + Convert.ToString(_G.objectType) + "' Found");
                }
                if (_B.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 7 to be 'number', '" + Convert.ToString(_B.objectType) + "' Found");
                }
                if (_T.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 8 to be 'number', '" + Convert.ToString(_B.objectType) + "' Found");
                }
                #endregion
                LangImage img = (LangImage)_img;
                LangNumber X = (LangNumber)_X,
                           Y = (LangNumber)_Y,
                           RAD = (LangNumber)_RAD,
                           R = (LangNumber)_R,
                           G = (LangNumber)_G,
                           B = (LangNumber)_B,
                           T = (LangNumber)_T;
                using (Graphics graphics = Graphics.FromImage(img.imageValue))
                {
                    Pen pen = new Pen(Color.FromArgb((int)R.numberValue, (int)G.numberValue, (int)B.numberValue), (float)T.numberValue);
                    Rectangle rect = new Rectangle(new Point((int)(X.numberValue - RAD.numberValue), (int)(Y.numberValue - RAD.numberValue)), new Size((int)RAD.numberValue * 2, (int)RAD.numberValue * 2));
                    graphics.DrawEllipse(pen, rect);
                }
                return new LangNumber(0, this);
            }
            #endregion
            #region drawImageFilledCircle
            else if (stat.name == "drawImageFilledCircle")
            {
                checkParameterNumber("drawImageFilledCircle", 11, stat);
                LangObject _img, _X, _Y, _RAD, _R, _G, _B, _T, _R2, _G2, _B2;
                _img = decider((Node)stat.parameters[0]);
                _X = decider((Node)stat.parameters[1]);
                _Y = decider((Node)stat.parameters[2]);
                _RAD = decider((Node)stat.parameters[3]);
                _R = decider((Node)stat.parameters[4]);
                _G = decider((Node)stat.parameters[5]);
                _B = decider((Node)stat.parameters[6]);
                _T = decider((Node)stat.parameters[7]);
                _R2 = decider((Node)stat.parameters[8]);
                _G2 = decider((Node)stat.parameters[9]);
                _B2 = decider((Node)stat.parameters[10]);
                #region TypeConfirming
                if (_img.objectType != ObjectType.IMAGE)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'image', '" + Convert.ToString(_img.objectType) + "' Found");
                }
                if (_X.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'number', '" + Convert.ToString(_X.objectType) + "' Found");
                }
                if (_Y.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 3 to be 'number', '" + Convert.ToString(_Y.objectType) + "' Found");
                }
                if (_RAD.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 4 to be 'number', '" + Convert.ToString(_RAD.objectType) + "' Found");
                }
                if (_R.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 5 to be 'number', '" + Convert.ToString(_R.objectType) + "' Found");
                }
                if (_G.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 6 to be 'number', '" + Convert.ToString(_G.objectType) + "' Found");
                }
                if (_B.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 7 to be 'number', '" + Convert.ToString(_B.objectType) + "' Found");
                }
                if (_T.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 8 to be 'number', '" + Convert.ToString(_T.objectType) + "' Found");
                }
                if (_R2.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 9 to be 'number', '" + Convert.ToString(_R2.objectType) + "' Found");
                }
                if (_G2.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 10 to be 'number', '" + Convert.ToString(_G2.objectType) + "' Found");
                }
                if (_B2.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 11 to be 'number', '" + Convert.ToString(_B2.objectType) + "' Found");
                }
                #endregion
                LangImage img = (LangImage)_img;
                LangNumber X = (LangNumber)_X,
                           Y = (LangNumber)_Y,
                           RAD = (LangNumber)_RAD,
                           R = (LangNumber)_R,
                           G = (LangNumber)_G,
                           B = (LangNumber)_B,
                           T = (LangNumber)_T,
                           R2 = (LangNumber)_R2,
                           G2 = (LangNumber)_G2,
                           B2 = (LangNumber)_B2;
                using (Graphics graphics = Graphics.FromImage(img.imageValue))
                {
                    Pen pen = new Pen(Color.FromArgb((int)R.numberValue, (int)G.numberValue, (int)B.numberValue), (float)T.numberValue);
                    Brush brush = new SolidBrush(Color.FromArgb((int)R.numberValue, (int)G.numberValue, (int)B.numberValue));
                    Rectangle rect = new Rectangle(new Point((int)(X.numberValue - RAD.numberValue), (int)(Y.numberValue - RAD.numberValue)), new Size((int)RAD.numberValue * 2, (int)RAD.numberValue * 2));
                    graphics.FillEllipse(brush, rect);
                    graphics.DrawEllipse(pen, rect);
                }
                return new LangNumber(0, this);
            }
            #endregion
            #region drawImageText
            else if (stat.name == "drawImageText")
            {
                checkParameterNumber("drawImageText", 9, stat);
                LangObject _img, _text, _X, _Y, _R, _G, _B, _Size, _Font;
                _img = decider((Node)stat.parameters[0]);
                _text = decider((Node)stat.parameters[1]);
                _X = decider((Node)stat.parameters[2]);
                _Y = decider((Node)stat.parameters[3]);
                _R = decider((Node)stat.parameters[4]);
                _G = decider((Node)stat.parameters[5]);
                _B = decider((Node)stat.parameters[6]);
                _Size = decider((Node)stat.parameters[7]);
                _Font = decider((Node)stat.parameters[8]);
                #region TypeConfirming
                if (_img.objectType != ObjectType.IMAGE)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'image', '" + Convert.ToString(_img.objectType) + "' Found");
                }
                if (_text.objectType != ObjectType.STRING)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'string', '" + Convert.ToString(_text.objectType) + "' Found");
                }
                if (_X.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 3 to be 'number', '" + Convert.ToString(_X.objectType) + "' Found");
                }
                if (_Y.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 4 to be 'number', '" + Convert.ToString(_Y.objectType) + "' Found");
                }
                if (_R.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 5 to be 'number', '" + Convert.ToString(_R.objectType) + "' Found");
                }
                if (_G.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 6 to be 'number', '" + Convert.ToString(_G.objectType) + "' Found");
                }
                if (_B.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 7 to be 'number', '" + Convert.ToString(_B.objectType) + "' Found");
                }
                if (_Size.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 8 to be 'number', '" + Convert.ToString(_Size.objectType) + "' Found");
                }
                if (_Font.objectType != ObjectType.STRING)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 9 to be 'string', '" + Convert.ToString(_Font.objectType) + "' Found");
                }

                #endregion
                LangImage img = (LangImage)_img;
                LangString text = (LangString)_text,
                           Font = (LangString)_Font;
                LangNumber X = (LangNumber)_X,
                           Y = (LangNumber)_Y,
                           Size = (LangNumber)_Size,
                           R = (LangNumber)_R,
                           G = (LangNumber)_G,
                           B = (LangNumber)_B;
                using (Graphics graphics = Graphics.FromImage(img.imageValue))
                {
                    Font font = new Font(Font.stringValue, (float)Size.numberValue);
                    Brush brush = new SolidBrush(Color.FromArgb((int)R.numberValue, (int)G.numberValue, (int)B.numberValue));
                    graphics.DrawString(text.stringValue, font, brush, new PointF((float)X.numberValue, (float)Y.numberValue));
                }
                return new LangNumber(0, this);
            }
            #endregion
            #region drawImageImage
            else if (stat.name == "drawImageImage")
            {
                checkParameterNumber("drawImageImage", 4, stat);
                LangObject _img, _img2, _X, _Y;
                _img = decider((Node)stat.parameters[0]);
                _img2 = decider((Node)stat.parameters[1]);
                _X = decider((Node)stat.parameters[2]);
                _Y = decider((Node)stat.parameters[3]);
                #region typeConfirming
                if (_img.objectType != ObjectType.IMAGE)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'image', '" + Convert.ToString(_img.objectType) + "' Found");
                }
                if (_img2.objectType != ObjectType.IMAGE)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'image', '" + Convert.ToString(_img2.objectType) + "' Found");
                }
                if (_X.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 3 to be 'number', '" + Convert.ToString(_X.objectType) + "' Found");
                }
                if (_Y.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 4 to be 'number', '" + Convert.ToString(_Y.objectType) + "' Found");
                }
                #endregion
                LangImage img = (LangImage)_img,
                          img2 = (LangImage)_img2;
                LangNumber X = (LangNumber)_X,
                           Y = (LangNumber)_Y;
                using (Graphics graphics = Graphics.FromImage(img.imageValue))
                {
                    graphics.DrawImage(img2.imageValue, new Point((int)X.numberValue, (int)Y.numberValue));
                }
                return new LangNumber(0, this);
            }
            #endregion
            #region getTextWidth
            else if (stat.name == "getTextWidth")
            {
                checkParameterNumber("getTextWidth", 3, stat);
                LangObject _text, _Font, _Size;
                _text = decider((Node)stat.parameters[0]);
                _Font = decider((Node)stat.parameters[1]);
                _Size = decider((Node)stat.parameters[2]);
                #region typeConfirming
                if (_text.objectType != ObjectType.STRING)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'string', '" + Convert.ToString(_text.objectType) + "' Found");
                }
                if (_Font.objectType != ObjectType.STRING)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'string', '" + Convert.ToString(_Font.objectType) + "' Found");
                }
                if (_Size.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 3 to be 'number', '" + Convert.ToString(_Size.objectType) + "' Found");
                }
                #endregion
                LangString text = (LangString)_text,
                           Font = (LangString)_Font;
                LangNumber Size = (LangNumber)_Size;
                Bitmap fake = new Bitmap(1, 1);
                using (Graphics graphics = Graphics.FromImage(fake))
                {
                    return new LangNumber(graphics.MeasureString(text.stringValue, new Font(Font.stringValue, (int)Size.numberValue)).Width, this);
                }
            }
            #endregion
            #region getTextHeight
            else if (stat.name == "getTextHeight")
            {
                checkParameterNumber("getTextHeight", 3, stat);
                LangObject _text, _Font, _Size;
                _text = decider((Node)stat.parameters[0]);
                _Font = decider((Node)stat.parameters[1]);
                _Size = decider((Node)stat.parameters[2]);
                #region typeConfirming
                if (_text.objectType != ObjectType.STRING)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'string', '" + Convert.ToString(_text.objectType) + "' Found");
                }
                if (_Font.objectType != ObjectType.STRING)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'string', '" + Convert.ToString(_Font.objectType) + "' Found");
                }
                if (_Size.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 3 to be 'number', '" + Convert.ToString(_Size.objectType) + "' Found");
                }
                #endregion
                LangString text = (LangString)_text,
                           Font = (LangString)_Font;
                LangNumber Size = (LangNumber)_Size;
                Bitmap fake = new Bitmap(1, 1);
                using (Graphics graphics = Graphics.FromImage(fake))
                {
                    return new LangNumber(graphics.MeasureString(text.stringValue, new Font(Font.stringValue, (int)Size.numberValue)).Height, this);
                }
            }
            #endregion
            #region cropImage
            else if (stat.name == "cropImage")
            {
                checkParameterNumber("cropImage", 5, stat);
                LangObject _img, _X1, _Y1, _X2, _Y2;
                _img = decider((Node)stat.parameters[0]);
                _X1 = decider((Node)stat.parameters[1]);
                _Y1 = decider((Node)stat.parameters[2]);
                _X2 = decider((Node)stat.parameters[3]);
                _Y2 = decider((Node)stat.parameters[4]);
                #region typeConfirming
                if (_img.objectType != ObjectType.IMAGE)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'image', '" + Convert.ToString(_img.objectType) + "' Found");
                }
                if (_X1.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'number', '" + Convert.ToString(_X1.objectType) + "' Found");
                }
                if (_Y1.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 3 to be 'number', '" + Convert.ToString(_Y1.objectType) + "' Found");
                }
                if (_X2.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 4 to be 'number', '" + Convert.ToString(_X2.objectType) + "' Found");
                }
                if (_Y2.objectType != ObjectType.NUMBER)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 5 to be 'number', '" + Convert.ToString(_Y2.objectType) + "' Found");
                }

                #endregion
                LangImage img = (LangImage)_img;
                LangNumber X1 = (LangNumber)_X1,
                           Y1 = (LangNumber)_Y1,
                           X2 = (LangNumber)_X2,
                           Y2 = (LangNumber)_Y2;
                Rectangle cropRect = new Rectangle(new Point((int)X1.numberValue, (int)Y1.numberValue), new Size((int)(X2.numberValue - X1.numberValue), (int)(Y2.numberValue - Y1.numberValue)));
                Bitmap newB = new Bitmap(cropRect.Width, cropRect.Height);
                using (Graphics graphics = Graphics.FromImage(newB))
                {
                    graphics.DrawImage(img.imageValue, cropRect, cropRect, GraphicsUnit.Pixel);
                    img.imageValue = newB;
                }
                return new LangNumber(0, this);
            }
            #endregion
            #endregion
            #region Canvas Operations
            #region drawOnCanvas
            else if (stat.name == "drawOnCanvas")
            {
                checkParameterNumber("drawOnCanvas", 1, stat);
                LangObject _img;
                _img = decider((Node)stat.parameters[0]);
                if (_img.objectType != ObjectType.IMAGE)
                {
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'image', '" + Convert.ToString(_img.objectType) + "' Found");
                }
                LangImage img = (LangImage)_img;
                gui.Invoke((MethodInvoker)delegate()
                {
                    gui.drawImage(img.imageValue);
                });
                return new LangNumber(0, this);
            }
            #endregion
            #endregion
            #region Events
            #region registerHandler
            else if (stat.name == "registerHandler")
            {
                checkParameterNumber("registerHandler", 2, stat);
                LangObject _event, _handler;
                _event = decider((Node)stat.parameters[0]);
                _handler = decider((Node)stat.parameters[1]);
                if (_event.objectType != ObjectType.STRING)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'string', '" + Convert.ToString(_event.objectType) + "' Found");
                }
                if (_handler.objectType != ObjectType.CLASS)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'class', '" + Convert.ToString(_handler.objectType) + "' Found");
                }
                LangString event_ = (LangString)_event;
                LangClass handler = (LangClass)_handler;
                if (!handler.methods.ContainsKey(event_.stringValue))
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Class '" + handler.name + "' can't handler the event '" + event_.stringValue + "'!");
                }
                EventHandlers[event_.stringValue] = _handler;
                return new LangNumber(0, this);
            }
            #endregion
            #endregion
            #region getTypeName
            else if (stat.name == "getTypeName")
            {
                checkParameterNumber("getTypeNumber", 1, stat);
                LangObject param1 = decider(((Node)stat.parameters[0]));
                if (param1.objectType != ObjectType.CLASS)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'class', '" + Convert.ToString(param1.objectType) + "' Found");
                }
                return new LangString(((LangClass)param1).name, this);
            }
            #endregion
            #region getCurrentDirectory
            else if (stat.name == "getCurrentDirectory")
            {
                checkParameterNumber("getCurrentDirectory", 0, stat);
                return new LangString(Directory.GetCurrentDirectory(), this);
            }
            #endregion
            #endregion
            #region custom functions
            else
            {
                increaseLevel();
                FunctionStatement func = null;
                ArrayList paramsCalcd = new ArrayList();
                foreach (Node nd in stat.parameters)
                {
                    paramsCalcd.Add(decider(nd));
                }
                #region find correct overloading
                if (functions.Contains(stat.name))
                {
                    ArrayList funcs = (ArrayList)functions[stat.name];
                    for (int i = 0; i < funcs.Count; i++)
                    {
                        FunctionStatement st = (FunctionStatement)(funcs)[i];
                        if (st.parameters.Count != stat.parameters.Count)
                        {
                            continue;
                        }
                        else
                        {
                            bool works = true;
                            for (int j = 0; j < stat.parameters.Count; j++)
                            {
                                Parameter param = (Parameter)st.parameters[j];
                                LangObject paramC = (LangObject)paramsCalcd[j];
                                if (param.type == "any")
                                {
                                    continue;
                                }
                                else if (param.type == "integer" || param.type == "number")
                                {
                                    if (paramC.objectType != ObjectType.NUMBER)
                                    {
                                        works = false;
                                        break;
                                    }
                                }
                                else if (param.type == "string")
                                {
                                    if (paramC.objectType != ObjectType.STRING)
                                    {
                                        works = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (!(paramC.objectType == ObjectType.CLASS && ((LangClass)paramC).name == param.type))
                                    {
                                        works = false;
                                        break;
                                    }
                                }
                            }
                            if (works)
                            {
                                func = st;
                                break;
                            }
                        }
                    }
                }
                #endregion
                if (func == null)
                {
                    string types = "";
                    foreach (LangObject obj in paramsCalcd)
                    {
                        types += " " + Convert.ToString(obj.objectType);
                    }
                    langManager.lastErrorToken = stat.token;
                    throw new InterpreterException("Line " + stat.token.line + ": No overloaded instance of function '" + stat.name + "' that accepts types" + types);
                }
                for (int i = 0; i < func.parameters.Count; i++)
                {
                    ((Hashtable)table[level])[((Parameter)func.parameters[i]).name] = ((LangObject)paramsCalcd[i]);
                }
                StackTrace.Add(new StackTraceEntry(node.token.file, lastFunctionCalled, node.token.line));
                lastFunctionCalled = func.name;
                LangObject obj2 = statListDecider(func.stats);
                lastFunctionCalled = ((StackTraceEntry)StackTrace[StackTrace.Count - 1]).FunctionName;
                StackTrace.RemoveAt(StackTrace.Count - 1);
                decreaseLevel();
                for (int i = 0; i < func.globals.Count; i++)
                {
                    string name = (string)func.globals[i];
                    ((Hashtable)table[level])[name] = ((Hashtable)table[level + 1])[name];
                }
                if (obj2.objectType == ObjectType.STATE)
                {
                    LangState obj3 = (LangState)obj2;
                    if (obj3.message == "return")
                    {
                        return obj3.optionalMessage;
                    }
                    else if (obj3.message == "stop")
                    {
                        return obj2;
                    }
                    else
                    {
                        return new LangNumber(0, this);
                    }
                }
                return obj2;
            }
            #endregion
        }

        LangObject decider(Node node)
        {
            if (!keepWorking)
                return new LangState("stop", this);
            switch (node.nodeType)
            {
                case NodeType.DIV:
                    return divInterpret(node);
                case NodeType.DIV_INT:
                    return divintInterpret(node);
                case NodeType.MOD:
                    return modInterpret(node);
                case NodeType.MUL:
                    return mulInterpret(node);
                case NodeType.PLUS:
                    return plusInterpret(node);
                case NodeType.MINUS:
                    return minusInterpret(node);
                case NodeType.POW:
                    return powInterpret(node);
                case NodeType.GREATER:
                    return greaterInterpret(node);
                case NodeType.SMALLER:
                    return smallerInterpret(node);
                case NodeType.EQUAL_TEST:
                    return equalInterpret(node);
                case NodeType.GREATER_EQUAL:
                    return greaterEqualInterpret(node);
                case NodeType.SMALLER_EQUAL:
                    return smallerEqualInterpret(node);
                case NodeType.NOT_EQUAL:
                    return notEqualInterpret(node);
                case NodeType.AND:
                    return andInterpret(node);
                case NodeType.OR:
                    return orInterpret(node);
                case NodeType.NUMBER:
                    return numInterpret(node);
                case NodeType.ID:
                    return idInterpret(node);
                case NodeType.STRING:
                    return stringInterpret(node);
                case NodeType.BRACKETS:
                    return BracketsOperatorInterpret(node);
                case NodeType.DOT:
                    return DotOperatorInterpret(node);
                case NodeType.STATEMENT:
                    return statDecider((Statement)node);
                case NodeType.STATEMENT_LIST:
                    return statListDecider((StatementList)node);
            }
            return new LangNumber(0, this);
        }

        internal void EventHandler(string EventName, ArrayList parameters)
        {
            if (!EventHandlers.ContainsKey(EventName))
            {
                return;
            }
            if (EventHandlers[EventName] == null)
            {
                return;
            }
            LangClass handler = (LangClass)EventHandlers[EventName];
            ArrayList newParameters = new ArrayList();
            foreach (Object obj in parameters)
            {
                if (obj is Int32)
                {
                    newParameters.Add(new LangNumber((int)obj, this));
                }
                else if (obj is String)
                {
                    newParameters.Add(new LangString((string)obj, this));
                }
            }
            FunctionCallStatement stat = new FunctionCallStatement(EventName, newParameters, new Token("", TokenType.AND, -1, "", 0, 0));
            RunClassFunction((ArrayList)handler.methods[EventName], stat, handler, newParameters);
        }
        #endregion

        #region Interpreters

        #region comparisonFunctions
        LangObject greaterInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            try
            {
                LangObject obj1 = decider(_node.left);
                LangObject obj2 = decider(_node.right);
                return new LangNumber(Convert.ToInt32(obj1.Greater(obj2)), this);
            }
            catch (Exception)
            {
                langManager.lastErrorToken = node.token;
                throw;
            }
        }
        LangObject smallerInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            try
            {
                return new LangNumber(Convert.ToInt32(decider(_node.left).Smaller(decider(_node.right))), this);
            }
            catch (Exception)
            {
                langManager.lastErrorToken = node.token;
                throw;
            }
        }
        LangObject equalInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            try
            {
                return new LangNumber(Convert.ToInt32(decider(_node.left).Equal(decider(_node.right))), this);
            }
            catch (Exception)
            {
                langManager.lastErrorToken = node.token;
                throw;
            }
        }
        LangObject greaterEqualInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            try
            {
                return new LangNumber(Convert.ToInt32(decider(_node.left).GreaterEqual(decider(_node.right))), this);
            }
            catch (Exception)
            {
                langManager.lastErrorToken = node.token;
                throw;
            }
        }
        LangObject smallerEqualInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            try
            {
                return new LangNumber(Convert.ToInt32(decider(_node.left).Smaller(decider(_node.right))), this);
            }
            catch (Exception)
            {
                langManager.lastErrorToken = node.token;
                throw;
            }
        }
        LangObject notEqualInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            try
            {
                return new LangNumber(Convert.ToInt32(decider(_node.left).NotEqual(decider(_node.right))), this);
            }
            catch (Exception)
            {
                langManager.lastErrorToken = node.token;
                throw;
            }
        }
        #endregion

        #region logicalFunctions
        LangObject andInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            LangObject _left, _right;
            _left = decider(_node.left);
            _right = decider(_node.right);
            if (_left.objectType == ObjectType.NUMBER && _right.objectType == ObjectType.NUMBER)
            {
                return new LangNumber(Convert.ToInt32(((LangNumber)_left).numberValue != 0.0 && ((LangNumber)_right).numberValue != 0.0), this);
            }
            langManager.lastErrorToken = node.token;
            throw new InterpreterException("Invalid operation '" + Convert.ToString(_left.objectType) + "' & '" + Convert.ToString(_right.objectType) + "'");
        }
        LangObject orInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            LangObject _left, _right;
            _left = decider(_node.left);
            _right = decider(_node.right);
            if (_left.objectType == ObjectType.NUMBER && _right.objectType == ObjectType.NUMBER)
            {
                return new LangNumber(Convert.ToInt32(((LangNumber)_left).numberValue != 0 || ((LangNumber)_right).numberValue != 0), this);
            }
            langManager.lastErrorToken = node.token;
            throw new InterpreterException("Invalid operation '" + Convert.ToString(_left.objectType) + "' | '" + Convert.ToString(_right.objectType) + "'");
        }
        #endregion

        #region mathFunctions
        LangObject powInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            try
            {
                return decider(_node.left).Pow(decider(_node.right));
            }
            catch (Exception)
            {
                langManager.lastErrorToken = node.token;
                throw;
            }
        }
        LangObject divInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            LangObject val = decider(_node.right);
            try
            {
                return decider(_node.left).Divide(val);
            }
            catch (Exception)
            {
                langManager.lastErrorToken = node.token;
                throw;
            }
        }
        LangObject divintInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            LangNumber val = (LangNumber)decider(_node.right);
            try
            {
                return new LangNumber(Math.Floor(((LangNumber)decider(_node.left).Divide(val)).numberValue), this);
            }
            catch (Exception)
            {
                langManager.lastErrorToken = node.token;
                throw;
            }
        }
        LangObject mulInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            try
            {
                return decider(_node.left).Multiply(decider(_node.right));
            }
            catch (Exception)
            {
                langManager.lastErrorToken = node.token;
                throw;
            }
        }
        LangObject modInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            LangNumber val = (LangNumber)decider(_node.right);
            try
            {
                return decider(_node.left).Mod(val);
            }
            catch (Exception)
            {
                langManager.lastErrorToken = node.token;
                throw;
            }
        }
        LangObject plusInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            try
            {
                return decider(_node.left).Plus(decider(_node.right));
            }
            catch (Exception)
            {
                langManager.lastErrorToken = node.token;
                throw;
            }
        }
        LangObject minusInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            try
            {
                return decider(_node.left).Minus(decider(_node.right));
            }
            catch (Exception)
            {
                langManager.lastErrorToken = node.token;
                throw;
            }
        }
        #endregion

        LangObject BracketsInterpret(Node node, LangObject _val, bool onlyOnNotFoundState = false)
        {
            if (node.nodeType != NodeType.BRACKETS)
            {
                LangObject obj = decider(node);
                if (obj == null)
                    return obj = new LangMap(new Hashtable(), this);
                if (obj.objectType != ObjectType.MAP && obj.objectType != ObjectType.STRING)
                    return obj = new LangMap(new Hashtable(), this);
                return obj;
            }
            BinaryOperator _node = (BinaryOperator)node;
            LangObject _left = null;
            if (_node.left.nodeType != NodeType.DOT)
                _left = BracketsInterpret(_node.left, null, onlyOnNotFoundState);
            else if (_node.left.nodeType == NodeType.DOT)
            {
                _left = DotInterpret(_node.left, null);
            }
            LangObject _right = (LangObject)decider(_node.right);
            LangObject retobj = null;
            if (_right.objectType == ObjectType.NUMBER)
            {
                LangNumber _right_num = (LangNumber)_right;
                if (_left.objectType == ObjectType.STRING)
                {
                    if (((LangString)_left).stringValue.Length <= (int)_right_num.numberValue || (int)_right_num.numberValue < 0)
                    {
                        langManager.lastErrorToken = node.token;
                        throw new InterpreterException("Line " + node.token.line + ": " + "String subscript out of bounds!");
                    }
                    retobj = (LangObject)(new LangString("" + ((LangString)_left).stringValue[(int)_right_num.numberValue], this));
                }
                else
                {
                    retobj = (LangObject)((LangMap)_left).arrayValue[_right_num.numberValue];
                }
            }
            else if (_right.objectType == ObjectType.STRING)
            {
                if (_left.objectType == ObjectType.STRING)
                {
                    throw new InterpreterException("Line " + node.token.line + ": " + "Cannot implicitly convert from type 'string' to type 'number'");
                }
                else
                {
                    retobj = (LangObject)((LangMap)_left).arrayValue[((LangString)_right).stringValue];
                }
            }
            if (retobj == null)
            {
                if (_val == null)
                {
                    if (_right.objectType == ObjectType.NUMBER)
                    {
                        return ((LangObject)(((LangMap)_left).arrayValue[((LangNumber)_right).numberValue] = new LangMap(new Hashtable(), this)));
                    }
                    else if (_right.objectType == ObjectType.STRING)
                    {
                        return ((LangObject)(((LangMap)_left).arrayValue[((LangString)_right).stringValue] = new LangMap(new Hashtable(), this)));
                    }
                }
                else
                {
                    if (_left.objectType == ObjectType.MAP)
                    {
                        if (_right.objectType == ObjectType.NUMBER)
                        {
                            return ((LangObject)(((LangMap)_left).arrayValue[((LangNumber)_right).numberValue] = _val));
                        }
                        else if (_right.objectType == ObjectType.STRING)
                        {
                            return ((LangObject)(((LangMap)_left).arrayValue[((LangString)_right).stringValue] = _val));
                        }
                    }
                }
            }
            if (_val != null && !onlyOnNotFoundState)
            {
                if (_left.objectType == ObjectType.MAP)
                {
                    if (_right.objectType == ObjectType.NUMBER)
                    {
                        return ((LangObject)(((LangMap)_left).arrayValue[((LangNumber)_right).numberValue] = _val));
                    }

                    else if (_right.objectType == ObjectType.STRING)
                    {
                        return ((LangObject)(((LangMap)_left).arrayValue[((LangString)_right).stringValue] = _val));
                    }
                }
                else if (_left.objectType == ObjectType.STRING)
                {
                    string inp = ((LangString)_left).stringValue;
                    char[] x = inp.ToCharArray();
                    x[(int)((LangNumber)_right).numberValue] = ((LangString)_val).stringValue[0];
                    inp = new string(x);
                    ((LangString)_left).stringValue = inp;
                    return new LangString("" + ((LangString)_val).stringValue[0], this);
                }
            }
            return retobj;
        }
        LangObject BracketsOperatorInterpret(Node node)
        {
            return BracketsInterpret(node, null, true);
        }

        LangObject DotInterpret(Node node, LangObject _val)
        {
            if (node.nodeType == NodeType.ID)
            {
                LangObject obj = (LangObject)(((Hashtable)table[level])[((ID)node).name]);
                if (obj == null)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": Expected class on the left handside");
                }
                if (obj.objectType != ObjectType.CLASS)
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": Expected class on left handside");
                }
                return obj;
            }
            BinaryOperator op = (BinaryOperator)node;
            Node _left = op.left;
            LangObject _left_ret = null;
            if (_left.nodeType == NodeType.BRACKETS)
            {
                _left_ret = BracketsOperatorInterpret(_left);
            }
            else
            {
                _left_ret = DotInterpret(_left, null);
            }
            if (_left_ret.objectType != ObjectType.CLASS)
            {
                langManager.lastErrorToken = node.token;
                throw new InterpreterException("Line " + node.token.line + ": Expected class on left handside");
            }
            Node _right = op.right;
            if (_right.nodeType == NodeType.ID)
            {
                LangClass _left_ret_class = (LangClass)_left_ret;
                ID _right_ID = (ID)_right;
                if (_val != null)
                {
                    return (LangObject)((_left_ret_class.vars[_right_ID.name]) = _val);
                }
                if (!_left_ret_class.vars.Contains(_right_ID.name))
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Class '" + _left_ret_class.name + "' doesn't contain the member '" + _right_ID.name + "'");
                }
                return (LangObject)(_left_ret_class.vars[_right_ID.name]);
            }
            if (_right.nodeType == NodeType.STATEMENT)
            {
                FunctionCallStatement _right_stat = (FunctionCallStatement)_right;
                LangClass _left_ret_class = (LangClass)_left_ret;
                if (!(_left_ret_class.methods.ContainsKey(_right_stat.name)))
                {
                    langManager.lastErrorToken = _right.token;
                    throw new InterpreterException("Line " + _right.token.line + ": type '" + _left_ret_class.name + "' doesn't contain the method '" + _right_stat.name + "'");
                }
                return RunClassFunction((ArrayList)(_left_ret_class.methods[_right_stat.name]), _right_stat, (LangClass)_left_ret);
            }


            throw new InterpreterException("Something not handled here");
        }
        LangObject DotOperatorInterpret(Node node)
        {
            return DotInterpret(node, null);
        }

        LangObject RunClassFunction(FunctionStatement _function, FunctionCallStatement _call, LangClass _class, ArrayList alreadyCalcd = null)
        {
            if (!keepWorking)
                return new LangState("stop", this);
            if (_function.parameters.Count != _call.parameters.Count)
            {
                switch (_function.parameters.Count)
                {
                    case 0:
                        langManager.lastErrorToken = _call.token;
                        throw new InterpreterException("Line " + _call.token.line + ": function '" + _function.name + "' expectes no parameters, " + _call.parameters.Count + " Given");
                    case 1:
                        langManager.lastErrorToken = _call.token;
                        throw new InterpreterException("Line " + _call.token.line + ": function '" + _function.name + "' expectes 1 parameter, " + _call.parameters.Count + " Given");
                    default:
                        langManager.lastErrorToken = _call.token;
                        throw new InterpreterException("Line " + _call.token.line + ": function '" + _function.name + "' expectes " + _function.parameters.Count + " parameter, " + _call.parameters.Count + " Given");
                }
            }
            level++;
            if (level >= table.Count)
                table.Add(new Hashtable());
            for (int i = 0; i < _call.parameters.Count; i++)
            {
                level--;
                LangObject ret = null;
                if (alreadyCalcd == null)
                {
                    ret = decider(((Node)_call.parameters[i]));
                }
                else
                {
                    ret = (LangObject)alreadyCalcd[i];
                }
                Parameter param = ((Parameter)_function.parameters[i]);
                level++;
                ((Hashtable)table[level])[param.name] = ret;
                if (param.type == "any")
                    continue;
                switch (ret.objectType)
                {
                    case ObjectType.STRING:
                        if (param.type != "string")
                        {
                            langManager.lastErrorToken = ((Node)_call.parameters[i]).token;
                            throw new InterpreterException("Line " + ((Node)_call.parameters[i]).token.line + ": " + "Function '" + _call.name + "' expects parameter " + i + " to be " + param.type + ", " + Convert.ToString(ret.objectType) + " Given");
                        }
                        break;
                    case ObjectType.NUMBER:
                        if (param.type != "number" && param.type != "integer")
                        {
                            langManager.lastErrorToken = ((Node)_call.parameters[i]).token;
                            throw new InterpreterException("Line " + ((Node)_call.parameters[i]).token.line + ": " + "Function '" + _call.name + "' expects parameter " + i + " to be " + param.type + ", " + Convert.ToString(ret.objectType) + " Given");
                        }
                        break;
                    case ObjectType.MAP:
                        if (param.type != "map")
                        {
                            langManager.lastErrorToken = ((Node)_call.parameters[i]).token;
                            throw new InterpreterException("Line " + ((Node)_call.parameters[i]).token.line + ": " + "Function '" + _call.name + "' expects parameter " + i + " to be " + param.type + ", " + Convert.ToString(ret.objectType) + " Given");
                        }
                        break;
                    default:
                        break;
                }
            }
            ((Hashtable)table[level])["this"] = _class;
            StackTrace.Add(new StackTraceEntry(_call.token.file, lastFunctionCalled, _call.token.line));
            lastFunctionCalled = _call.name;
            LangObject obj = statListDecider(_function.stats);
            lastFunctionCalled = ((StackTraceEntry)StackTrace[StackTrace.Count - 1]).FunctionName;
            StackTrace.RemoveAt(StackTrace.Count - 1);
            level--;
            /*
            Hashtable levelplus = (Hashtable)table[level + 1];
            ArrayList varsEdited = new ArrayList();
            foreach (DictionaryEntry dic in _class.vars)
            {
                if (levelplus.Contains(dic.Key))
                {
                    varsEdited.Add(new KeyValuePair<string, LangObject>((string)(dic.Key), ((LangObject)levelplus[dic.Key])));
                }
            }
            foreach (KeyValuePair<string, LangObject> dic in varsEdited)
            {
                _class.vars[dic.Key] = dic.Value;
            }
            */
            if (obj.objectType == ObjectType.STATE)
            {
                LangState obj_state = (LangState)obj;
                if (obj_state.message == "return")
                {
                    return obj_state.optionalMessage;
                }
            }
            return obj;
        }
        LangObject RunClassFunction(ArrayList _functions, FunctionCallStatement _call, LangClass _class, ArrayList alreadyCalcd = null)
        {
            if (!keepWorking)
                return new LangState("stop", this);
            FunctionStatement _function = null;
            if (alreadyCalcd == null)
            {
                alreadyCalcd = new ArrayList();
                for (int i = 0; i < _call.parameters.Count; i++)
                {
                    alreadyCalcd.Add(decider((Node)_call.parameters[i]));
                }
            }
            #region find correct overloading
            ArrayList funcs = _functions;
            for (int i = 0; i < funcs.Count; i++)
            {
                FunctionStatement st = (FunctionStatement)(funcs)[i];
                if (st.parameters.Count != _call.parameters.Count)
                {
                    continue;
                }
                else
                {
                    bool works = true;
                    for (int j = 0; j < _call.parameters.Count; j++)
                    {
                        Parameter param = (Parameter)st.parameters[j];
                        LangObject paramC = (LangObject)alreadyCalcd[j];
                        if (param.type == "any")
                        {
                            continue;
                        }
                        else if (param.type == "integer" || param.type == "number")
                        {
                            if (paramC.objectType != ObjectType.NUMBER)
                            {
                                works = false;
                                break;
                            }
                        }
                        else if (param.type == "string")
                        {
                            if (paramC.objectType != ObjectType.STRING)
                            {
                                works = false;
                                break;
                            }
                        }
                        else
                        {
                            if (!(paramC.objectType == ObjectType.CLASS && ((LangClass)paramC).name == param.type))
                            {
                                works = false;
                                break;
                            }
                        }
                    }
                    if (works)
                    {
                        _function = st;
                        break;
                    }
                }
            }
            #endregion
            if (_function == null)
            {
                langManager.lastErrorToken = _call.token;
                throw new InterpreterException("Method " + _call.name + " not here");
            }
            level++;
            if (level >= table.Count)
                table.Add(new Hashtable());
            if(_function.globals != null)
                foreach (string global in _function.globals)
                {
                    ((Hashtable)table[level])[global] = ((Hashtable)table[level - 1])[global];
                }
            for (int i = 0; i < _call.parameters.Count; i++)
            {
                LangObject ret = (LangObject)alreadyCalcd[i];
                Parameter param = ((Parameter)_function.parameters[i]);
                ((Hashtable)table[level])[param.name] = ret;
            }
            ((Hashtable)table[level])["this"] = _class;
            StackTrace.Add(new StackTraceEntry(_call.token.file, lastFunctionCalled, _call.token.line));
            lastFunctionCalled = _call.name;
            LangObject obj = statListDecider(_function.stats);
            lastFunctionCalled = ((StackTraceEntry)StackTrace[StackTrace.Count - 1]).FunctionName;
            StackTrace.RemoveAt(StackTrace.Count - 1);
            level--;
            if (obj.objectType == ObjectType.STATE)
            {
                LangState obj_state = (LangState)obj;
                if (obj_state.message == "return")
                {
                    return obj_state.optionalMessage;
                }
            }
            return obj;
        }
        public LangObject RunClassOperator(FunctionStatement _function, LangClass _first, LangClass _second)
        {
            if (!keepWorking)
                return new LangState("stop", this);
            Parameter param = (Parameter)_function.parameters[0];
            increaseLevel();
            ((Hashtable)table[level]).Clear();
            Hashtable newTable = new Hashtable();
            foreach (DictionaryEntry dic in _first.vars)
            {
                newTable[dic.Key] = dic.Value;
            }
            newTable[param.name] = _second;
            newTable["this"] = _first;
            table[level] = newTable;
            LangObject retObj = statListDecider(_function.stats);
            decreaseLevel();
            if (retObj.objectType == ObjectType.STATE)
            {
                LangState retObj_state = (LangState)retObj;
                if (retObj_state.message == "return")
                {
                    return retObj_state.optionalMessage;
                }
            }
            return retObj;
        }

        #region factors
        LangObject numInterpret(Node node)
        {
            Number _node = (Number)node;
            return new LangNumber(Convert.ToDouble(_node.value), this);
        }
        LangObject stringInterpret(Node node)
        {
            return new LangString(((StringVal)(node)).val, this);
        }
        LangObject idInterpret(Node node)
        {
            ID _node = (ID)node;
            if (((Hashtable)table[level]).Contains(_node.name))
            {
                return (LangObject)((Hashtable)table[level])[_node.name];
            }
            else
            {
                langManager.lastErrorToken = node.token;
                throw new InterpreterException("Line " + node.token.line + ": " + "The variable '" + _node.name + "' doesn't exist!");
            }
        }
        #endregion

        #region statements
        #region simpleStatements
        LangObject bindStatInterpret(Node node)
        {
            if (!keepWorking)
                return new LangState("stop", this);
            BindStatement _node = (BindStatement)node;
            LangObject val = decider(_node.expr);
            if (_node.Id.nodeType == NodeType.ID)
                ((Hashtable)table[level])[((ID)_node.Id).name] = val.Clone();
            else
            {
                if (_node.Id.nodeType == NodeType.BRACKETS)
                    BracketsInterpret(_node.Id, val.Clone());
                if (_node.Id.nodeType == NodeType.DOT)
                    DotInterpret(_node.Id, val.Clone());
            }
            foreach (BindStatement stat in _node.extras)
            {
                bindStatInterpret(stat);
            }
            return val;
        }
        LangObject printStatInterpret(Node node)
        {
            if (!keepWorking)
                return new LangState("stop", this);
            PrintStatement _node = (PrintStatement)node;
            LangObject val = decider(_node.expr);
            string output = "";
            if (val.objectType == ObjectType.STRING)
            {
                output = ((LangString)val).stringValue;
            }
            else if (val.objectType == ObjectType.NUMBER)
            {
                output = Convert.ToString(((LangNumber)val).numberValue);
            }
            consoleUI.Invoke((MethodInvoker)delegate()
            {
                consoleUI.printLine(output);
            });
            foreach (PrintStatement stat in _node.extras)
            {
                printStatInterpret(stat);
            }
            return val;
        }
        LangObject scanStatInterpret(Statement node)
        {
            if (!keepWorking)
                return new LangState("stop", this);
            ScanStatement _node = (ScanStatement)node;
            if (_node.scanType != null && _node.scanType.name == "line")
            {
                consoleUI.Invoke((MethodInvoker)delegate()
                {
                    consoleUI.getLine();
                });
                Thread.Sleep(10);
            }
            else
            {
                consoleUI.Invoke((MethodInvoker)delegate()
                {
                    consoleUI.getNextToken();
                });
                Thread.Sleep(10);
            }
            string sc = consoleUI.lastRequestedString;
            if (_node.scanType == null)
            {
                double res = 0;
                if (Double.TryParse(sc, out res))
                {
                    return new LangNumber(res, this);
                }
                else
                {
                    return new LangString(sc, this);
                }
            }
            else if (_node.scanType.name == "line")
            {
                if (_node.expr.nodeType == NodeType.ID)
                {
                    ((Hashtable)table[level])[((ID)_node.expr).name] = new LangString(sc, this);
                }
                else if (_node.expr.nodeType == NodeType.BRACKETS)
                {
                    BracketsInterpret(_node.expr, new LangString(sc, this));
                }
                else if (_node.expr.nodeType == NodeType.DOT)
                {
                    DotInterpret(_node.expr, new LangString(sc, this));
                }
                return new LangString(sc, this);
            }
            else if (_node.scanType.name == "string")
            {
                if (_node.expr.nodeType == NodeType.ID)
                {
                    ((Hashtable)table[level])[((ID)_node.expr).name] = new LangString(sc, this);
                }
                else if (_node.expr.nodeType == NodeType.BRACKETS)
                {
                    BracketsInterpret(_node.expr, new LangString(sc, this));
                }
                else if (_node.expr.nodeType == NodeType.DOT)
                {
                    DotInterpret(_node.expr, new LangString(sc, this));
                }

                return new LangString(sc, this);
            }
            else if (_node.scanType.name == "integer")
            {
                double res = 0;
                if (Double.TryParse(sc, out res))
                {
                    if (_node.expr.nodeType == NodeType.ID)
                    {
                        ((Hashtable)table[level])[((ID)_node.expr).name] = new LangNumber((int)res, this);
                    }
                    else if (_node.expr.nodeType == NodeType.BRACKETS)
                    {
                        BracketsInterpret(_node.expr, new LangNumber((int)res, this));
                    }
                    else if (_node.expr.nodeType == NodeType.DOT)
                    {
                        DotInterpret(_node.expr, new LangNumber((int)res, this));
                    }
                    return new LangNumber((int)res, this);
                }
                else
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Input cannot be converted into number!");
                }
            }
            else if (_node.scanType.name == "number")
            {

                double res = 0;
                if (Double.TryParse(sc, out res))
                {
                    if (_node.expr.nodeType == NodeType.ID)
                    {
                        ((Hashtable)table[level])[((ID)_node.expr).name] = new LangNumber((int)res, this);
                    }
                    else if (_node.expr.nodeType == NodeType.BRACKETS)
                    {
                        BracketsInterpret(_node.expr, new LangNumber((int)res, this));
                    }
                    else if (_node.expr.nodeType == NodeType.DOT)
                    {
                        DotInterpret(_node.expr, new LangNumber((int)res, this));
                    }
                    return new LangNumber((int)res, this);
                }
                else
                {
                    langManager.lastErrorToken = node.token;
                    throw new InterpreterException("Line " + node.token.line + ": " + "Input cannot be converted into number!");
                }
            }
            else
            {
                langManager.lastErrorToken = node.token;
                throw new InterpreterException("Line " + node.token.line + ": " + "Unknown scan type!");
            }
        }
        LangObject importStatInterpret(Node node)
        {
            if (!keepWorking)
                return new LangState("stop", this);
            ImportStatement _node = (ImportStatement)node;
            foreach (string file in _node.imports)
            {
                string code = "";
                string path = "";
                try
                {
                    code = File.ReadAllText(path = (FileName.Substring(0, FileName.LastIndexOf('\\') + 1) + file + ".lan"));
                }
                catch (IOException)
                {
                    try
                    {
                        code = File.ReadAllText(path = (Directory.GetCurrentDirectory() + "\\Include\\" + file + ".lan"));
                    }
                    catch (IOException)
                    {
                        throw new InterpreterException("The file " + file + " doesn't exists");
                    }
                }
                Lexer lexer = new Lexer(new LangManager(path), code);
                lexer.FileName = path;
                Parser parser = new Parser(new LangManager(path));
                parser.updateTokens(lexer.lex());
                Node stats = parser.parse();
                Interpreter inter = new Interpreter(new LangManager(path));
                inter.updateRoot((StatementList)stats);
                inter.FileName = path;
                inter.interpret();
                foreach (DictionaryEntry dic in ((Hashtable)inter.table[0]))
                {
                    ((Hashtable)table[level])[dic.Key] = dic.Value;
                }
                foreach (DictionaryEntry dic in inter.functions)
                {
                    functions[dic.Key] = dic.Value;
                }
                foreach (DictionaryEntry dic in inter.classes)
                {
                    classes[dic.Key] = dic.Value;
                }
            }
            return new LangNumber(0, this);
        }
        LangObject newStatInterpret(Node node)
        {
            if (!keepWorking)
                return new LangState("stop", this);
            ClassInitStatement stat = (ClassInitStatement)node;
            if (!classes.ContainsKey(stat.constructor.name))
            {
                langManager.lastErrorToken = stat.constructor.token;
                throw new InterpreterException("Line " + stat.constructor.token.line + ": There is no such class!");
            }
            ClassStatement _class = (ClassStatement)classes[stat.constructor.name];
            LangClass _ret_class = new LangClass(_class, stat, this);
            ArrayList paramsCalculated = new ArrayList();
            foreach (Node parm in stat.constructor.parameters)
            {
                paramsCalculated.Add(decider(parm));
            }
            foreach (FunctionStatement st in _class.constructors)
            {
                if (st.parameters.Count != stat.constructor.parameters.Count)
                {
                    continue;
                }
                bool works = true;
                for (int i = 0; i < st.parameters.Count; i++)
                {
                    Parameter param = (Parameter)st.parameters[i];
                    LangObject calcd = (LangObject)paramsCalculated[i];
                    if (param.type == "any")
                        continue;
                    else if (param.type == "integer" || param.type == "number")
                    {
                        if (calcd.objectType != ObjectType.NUMBER)
                        {
                            works = false;
                            break;
                        }
                    }
                    else if (param.type == "string")
                    {
                        if (calcd.objectType == ObjectType.CLASS)
                        {
                            works = false;
                            break;
                        }
                    }
                    else if (param.type == "map")
                    {
                        if (calcd.objectType != ObjectType.MAP)
                        {
                            works = false;
                            break;
                        }
                    }
                    else
                    {
                        if (!(calcd.objectType == ObjectType.CLASS && ((LangClass)calcd).name == param.type))
                        {
                            works = false;
                            break;
                        }
                    }
                }
                if (works)
                {
                    RunClassFunction(st, stat.constructor, _ret_class, paramsCalculated);
                    break;
                }
            }
            return _ret_class;
        }
        LangObject raiseStatInterpret(Node node)
        {
            RaiseStatement stat = (RaiseStatement)node;
            LangObject raise = decider(stat.expr);
            if (raise.objectType == ObjectType.STRING)
            {
                langManager.lastErrorToken = node.token;
                throw new InterpreterException("Line " + node.token.line + ": " + ((LangString)raise).stringValue);
            }
            else
            {
                langManager.lastErrorToken = node.token;
                throw new InterpreterException("Cannot throw any type other than string, " + Convert.ToString(raise.objectType) + " Found");
            }
        }
        #endregion

        #region compoundStatements

        LangObject ifStatInterpret(Node node_)
        {
            if (!keepWorking)
                return new LangState("stop", this);
            IfStatement node = (IfStatement)node_;
            bool foundInIfs = false;
            foreach (IfData data in node.data)
            {
                double d = ((LangNumber)decider(data.expr)).numberValue;
                if (d != 0)
                {
                    LangObject ret = statListDecider(data.stats);
                    if (ret.objectType == ObjectType.STATE)
                    {
                        return ret;
                    }
                    foundInIfs = true;
                    break;
                }
            }
            if (!foundInIfs)
            {
                if (node.elseData != null)
                {
                    LangObject ret = statListDecider(node.elseData);
                    if (ret.objectType == ObjectType.STATE)
                        return ret;
                }
            }
            return new LangNumber(0, this);
        }

        LangObject forStatInterpret(Node node_)
        {
            if (!keepWorking)
                return new LangState("stop", this);
            ForStatement node = (ForStatement)node_;
            for (statDecider(node.init_stat); ((LangNumber)decider(node.check_expr)).numberValue != 0; statDecider(node.inc_stat))
            {
                LangObject val = statListDecider(node.stats);
                if (val.objectType == ObjectType.STATE)
                {
                    LangState _val = (LangState)val;
                    if (_val.message == "break")
                        break;
                    if (_val.message == "stop")
                        return _val;
                    if (_val.message == "return")
                        return _val;
                }
            }
            return new LangNumber(0, this);
        }

        LangObject whileStatInterpret(Node node_)
        {
            if (!keepWorking)
                return new LangState("stop", this);
            WhileStatement node = (WhileStatement)node_;
            while (((LangNumber)decider(node.check_expr)).numberValue != 0)
            {
                LangObject val = statListDecider(node.stats);
                if (val.objectType == ObjectType.STATE)
                {
                    LangState _val = (LangState)val;
                    if (_val.message == "break")
                        break;
                    if (_val.message == "stop")
                        return _val;
                    if (_val.message == "return")
                        return _val;
                }
            }
            return new LangNumber(0, this);
        }

        LangObject tryStatInterpret(Node node_)
        {
            TryStatement node = (TryStatement)node_;
            try
            {
                return statListDecider(node.stats);
            }
            catch (Exception e)
            {
                if (node.catchID == null)
                {
                    return new LangNumber(0, this);
                }
                string risen = e.Message;
                increaseLevel();
                foreach (DictionaryEntry dic in ((Hashtable)table[level - 1]))
                {
                    ((Hashtable)table[level])[dic.Key] = dic.Key;
                }
                ((Hashtable)table[level])[node.catchID.name] = new LangString(risen, this);
                return statListDecider(node.catchStats);
            }
        }

        #endregion
        #endregion

        #endregion
    }
}
