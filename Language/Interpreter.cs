using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Diagnostics;

namespace Lang.language
{
    public class Interpreter
    {
        StatementList root;
        internal ArrayList table;
        internal int level;
        internal Hashtable functions;
        internal Hashtable classes;
        internal bool keepWorking = true;
        internal ArrayList breakpoints;
        internal bool isStopped = false;
        internal int stoppedLine;
        internal string FileName;
        LangConsole console;
        LangManager langManager;

        public Interpreter(LangConsole _console, LangManager _langManager)
        {
            table = new ArrayList();
            functions = new Hashtable();
            classes = new Hashtable();
            console = _console;
            breakpoints = new ArrayList();
            langManager = _langManager;
        }

        public void updateRoot(StatementList _root)
        {
            root = _root;
            table.Clear();
            functions.Clear();
            classes.Clear();
            level = 0;
        }

        // Executer

        public LangObject interpret()
        {
            functions.Clear();
            classes.Clear();
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
                        cs.methods[dicc.Key] = dicc.Value;
                    }
                }
                cs.constructors = ((ClassStatement)dic.Value).constructors;
                newClasses[dic.Key] = cs;
            }
            classes = newClasses;
            level = 0;
            table.Clear();
            table.Add(new Hashtable());
            keepWorking = true;
            LangObject val = statListDecider(root);
            return val;
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
                if (langManager.INTERPRET)
                {
                    langManager.lastErrorToken = ((Node)callstat.parameters[0]).token;
                }
                else
                {
                    langManager.lastLiveErrorToken = ((Node)callstat.parameters[0]).token;
                }
                throw new ArgumentException("Line " + ((Node)callstat.parameters[0]).token.line + ": " + "Function '" + functionName + "' expectes parameter 1 to be an ID");
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
                return new LangState("stop");
            foreach (Statement stat in node.statements)
            {
                if (breakpoints.Contains(stat.token.line) && stat.token.file == FileName)
                {
                    isStopped = true;
                    stoppedLine = stat.token.startIndex;
                    Thread.CurrentThread.Suspend();
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
            return new LangNumber(0);
        }

        LangObject statDecider(Statement node)
        {
            if (!keepWorking)
                return new LangState("stop");
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
                    return new LangState("break");
                case StatementType.CONTINUE:
                    return new LangState("continue");
                case StatementType.STOP:
                    return new LangState("stop");
                case StatementType.RETURN:
                    return new LangState("return", decider(((ReturnStatement)node).expr));
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
            return new LangNumber(0);
        }

        LangObject function_decide(Node node)
        {
            if (!keepWorking)
                return new LangState("stop");
            FunctionCallStatement stat = (FunctionCallStatement)node;
            if (level > 5000)
            {
                keepWorking = false;
                if (langManager.INTERPRET)
                {
                    langManager.lastErrorToken = node.token;
                }
                else
                {
                    langManager.lastLiveErrorToken = node.token;
                }
                throw new Exception("Maximum recursion limit exceeded");
            }
            #region built in
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
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = node_.token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = node_.token;
                    }
                    throw new Exception("Line " + node_.token.line + ": Function 'count' Expects parameter 1 to be 'map', '" + node_.nodeType + "' Given");
                }
                if (ret == null || ret.objectType != ObjectType.MAP)
                {
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = node_.token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = node_.token;
                    }
                    throw new Exception("Line " + node_.token.line + ": Function 'count' Expects parameter 1 to be 'map'");
                }
                return new LangNumber(((LangMap)ret).arrayValue.Count);
            }
            #endregion
            #region int
            else if (stat.name == "int")
            {
                checkParameterNumber("int", 1, stat);
                LangObject expr = decider((Node)stat.parameters[0]);
                if (expr.objectType == ObjectType.NUMBER)
                {
                    return new LangNumber((int)((LangNumber)expr).numberValue);
                }
                if (expr.objectType == ObjectType.STRING)
                {
                    string val = ((LangString)expr).stringValue;
                    double val_;
                    if (double.TryParse(val, out val_))
                    {
                        return new LangNumber((int)val_);
                    }
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = ((Node)stat.parameters[0]).token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = ((Node)stat.parameters[0]).token;
                    }
                    throw new InvalidCastException("Line " + ((Node)stat.parameters[0]).token.line + ": " + "Invalid number format");
                }
                if (expr.objectType == ObjectType.MAP)
                {
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = ((Node)stat.parameters[0]).token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = ((Node)stat.parameters[0]).token;
                    }
                    throw new InvalidCastException("Line " + ((Node)stat.parameters[0]).token.line + ": " + "Cannot convert from type 'map' to type 'number'");
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
                        return new LangString(Convert.ToString(((LangNumber)expr).numberValue));
                    case ObjectType.ARRAY:
                        throw new NotImplementedException();
                    case ObjectType.MAP:
                        if (langManager.INTERPRET)
                        {
                            langManager.lastErrorToken = ((Node)stat.parameters[0]).token;
                        }
                        else
                        {
                            langManager.lastLiveErrorToken = ((Node)stat.parameters[0]).token;
                        }
                        throw new InvalidCastException("Line " + ((Node)stat.parameters[0]).token.line + ": " + "Cannot convert from type 'map' to type 'string'");
                    default:
                        throw new NotImplementedException();
                }
            }
            #endregion
            #region map
            else if (stat.name == "map")
            {
                checkParameterNumber("map", 0, stat);
                return new LangMap(new Hashtable());
            }
            #endregion
            #region strlen
            else if (stat.name == "strlen")
            {
                checkParameterNumber("strlen", 1, stat);
                LangObject obj = decider(((Node)stat.parameters[0]));
                if (obj.objectType != ObjectType.STRING)
                {
                    throw new Exception("Line " + stat.token.line + ": " + "Function 'strlen' expects parameter 1 to be 'string', " + Convert.ToString(obj.objectType) + " Found");
                }
                return new LangNumber(((LangString)obj).stringValue.Length);
            }
            #endregion
            #region getPage
            else if (stat.name == "getPage")
            {
                checkParameterNumber("getPage", 1, stat);
                LangObject param = decider(((Node)stat.parameters[0]));
                if (param.objectType != ObjectType.STRING)
                {
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = node.token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = node.token;
                    }
                    throw new Exception("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'string', '" + Convert.ToString(param.objectType) + "' Found");
                }
                WebClient client = new WebClient();
                string ret = client.DownloadString(((LangString)param).stringValue);
                client.Dispose();
                return new LangString(ret);
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
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = node.token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = node.token;
                    }
                    throw new Exception("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'string', '" + Convert.ToString(param1.objectType) + "' Found");
                }
                if (param2.objectType != ObjectType.STRING)
                {
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = node.token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = node.token;
                    }
                    throw new Exception("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'string', '" + Convert.ToString(param2.objectType) + "' Found");
                }
                string regexp = ((LangString)param1).stringValue;
                string searchIn = ((LangString)param2).stringValue;
                Hashtable tbl = new Hashtable();
                MatchCollection col = Regex.Matches(searchIn, regexp);
                foreach (Match mt in col)
                {
                    tbl[(double)tbl.Keys.Count] = new LangNumber(mt.Index);
                    tbl[(double)tbl.Keys.Count] = new LangNumber(mt.Length);
                }
                return new LangMap(tbl);
            }
            #endregion
            #region cmd
            else if (stat.name == "cmd")
            {
                checkParameterNumber("cmd", 1, stat);
                LangObject param1 = decider(((Node)stat.parameters[0]));
                if (param1.objectType != ObjectType.STRING)
                {
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = node.token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = node.token;
                    }
                    throw new Exception("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'string', '" + Convert.ToString(param1.objectType) + "' Found");
                }
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C " + ((LangString)param1).stringValue;
                process.StartInfo = startInfo;
                return new LangNumber(Convert.ToInt32(process.Start()));
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
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = node.token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = node.token;
                    }
                    throw new Exception("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'string', '" + Convert.ToString(param1.objectType) + "' Found");
                }
                if (param2.objectType != ObjectType.STRING)
                {
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = node.token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = node.token;
                    }
                    throw new Exception("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'string', '" + Convert.ToString(param2.objectType) + "' Found");
                }
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = Directory.GetCurrentDirectory() + ((LangString)param1).stringValue;
                startInfo.Arguments = ((LangString)param2).stringValue;
                process.StartInfo = startInfo;
                return new LangNumber(Convert.ToInt32(process.Start()));
            }
            #endregion
            #region getFile
            else if (stat.name == "getFile")
            {
                checkParameterNumber("getFile", 1, stat);
                LangObject param1 = decider(((Node)stat.parameters[0]));
                if (param1.objectType != ObjectType.STRING)
                {
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = node.token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = node.token;
                    }
                    throw new Exception("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'string', '" + Convert.ToString(param1.objectType) + "' Found");
                }
                string filePath = ((LangString)param1).stringValue;
                if (File.Exists(filePath))
                {
                    return new LangString(File.ReadAllText(filePath));
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
                        return new LangString(File.ReadAllText(filePath));
                    }
                    else
                    {
                        throw new Exception("Line " + node.token.line + ": " + "File path '" + filePath + "doesn't exist!");
                    }
                }
                throw new Exception("Line " + node.token.line + ": " + "File path '" + filePath + "doesn't exist!");
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
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = node.token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = node.token;
                    }
                    throw new Exception("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'string', '" + Convert.ToString(param1.objectType) + "' Found");
                }
                if (param2.objectType != ObjectType.STRING)
                {
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = node.token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = node.token;
                    }
                    throw new Exception("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'string', '" + Convert.ToString(param2.objectType) + "' Found");
                }
                try
                {
                    File.WriteAllText(((LangString)param1).stringValue, ((LangString)param2).stringValue);
                }
                catch (Exception)
                {
                    return new LangNumber(0);
                }
                return new LangNumber(1);
            }
            #endregion
            #region getImage
            else if (stat.name == "getImage")
            {
                checkParameterNumber("getImage", 1, stat);
                LangObject param1 = decider(((Node)stat.parameters[0]));
                if (param1.objectType != ObjectType.STRING)
                {
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = node.token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = node.token;
                    }
                    throw new Exception("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'string', '" + Convert.ToString(param1.objectType) + "' Found");
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
                        throw new Exception("Line " + node.token.line + ": " + "File path '" + filePath + "' doesn't exist");
                    }
                }
                Bitmap img = new Bitmap(filePath);
                LangMap ret = new LangMap(new Hashtable());
                for (int i = 0; i < img.Width; i++)
                {
                    LangMap retret = new LangMap(new Hashtable());
                    for (int j = 0; j < img.Height; j++)
                    {
                        LangMap retretret = new LangMap(new Hashtable());
                        retretret.arrayValue[0.0] = new LangNumber(img.GetPixel(i, j).R);
                        retretret.arrayValue[1.0] = new LangNumber(img.GetPixel(i, j).G);
                        retretret.arrayValue[2.0] = new LangNumber(img.GetPixel(i, j).B);
                        retret.arrayValue[(double)j] = retretret;
                    }
                    ret.arrayValue[(double)i] = retret;
                }
                img.Dispose();
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
                if (param1.objectType != ObjectType.MAP)
                {
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = node.token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = node.token;
                    }
                    throw new Exception("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'map', '" + Convert.ToString(param1.objectType) + "' Found");
                }
                if (param2.objectType != ObjectType.STRING)
                {
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = node.token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = node.token;
                    }
                    throw new Exception("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 2 to be 'string', '" + Convert.ToString(param2.objectType) + "' Found");
                }
                #region check Map Validity
                LangMap bm = (LangMap)param1;
                int width = bm.arrayValue.Count;
                int height = 0;
                if (bm.arrayValue.Count == 0)
                {
                    width = height = 0;
                }
                else
                {
                    foreach (DictionaryEntry dic in bm.arrayValue)
                    {
                        LangObject lo = (LangObject)dic.Value;
                        if (lo.objectType != ObjectType.MAP)
                        {
                            throw new Exception("Line " + node.token.line + ": " + "saveImage: Invalid Map Format!");
                        }
                    }
                    height = -1;
                    foreach (DictionaryEntry dic in bm.arrayValue)
                    {
                        if (height == -1)
                        {
                            height = ((LangMap)dic.Value).arrayValue.Count;
                        }
                        else
                        {
                            if (((LangMap)dic.Value).arrayValue.Count != height)
                            {
                                throw new Exception("Line " + node.token.line + ": " + "saveImage: Invalid Map Format!");
                            }
                        }
                        foreach (DictionaryEntry dicc in ((LangMap)dic.Value).arrayValue)
                        {
                            if (((LangObject)dicc.Value).objectType != ObjectType.MAP)
                            {
                                throw new Exception("Line " + node.token.line + ": " + "saveImage: Invalid Map Format!");
                            }
                            LangMap lo = (LangMap)dicc.Value;
                            if (lo.arrayValue.Count != 3)
                                throw new Exception("Line " + node.token.line + ": " + "saveImage: Invalid Map Format!");
                            if (((LangObject)lo.arrayValue[0.0]).objectType != ObjectType.NUMBER)
                            {
                                throw new Exception("Line " + node.token.line + ": " + "saveImage: Invalid Map Format!");
                            }
                            if (((LangObject)lo.arrayValue[1.0]).objectType != ObjectType.NUMBER)
                            {
                                throw new Exception("Line " + node.token.line + ": " + "saveImage: Invalid Map Format!");
                            }
                            if (((LangObject)lo.arrayValue[2.0]).objectType != ObjectType.NUMBER)
                            {
                                throw new Exception("Line " + node.token.line + ": " + "saveImage: Invalid Map Format!");
                            }
                        }
                    }

                }
                #endregion
                if (height == -1)
                    height = 0;
                Bitmap bitmap = new Bitmap(width, height);
                foreach (DictionaryEntry dic in bm.arrayValue)
                {
                    LangMap bmm = (LangMap)dic.Value;
                    foreach (DictionaryEntry dicc in bmm.arrayValue)
                    {
                        LangMap bmmm = (LangMap)dicc.Value;
                        int R, G, B;
                        R = (int)((LangNumber)bmmm.arrayValue[0.0]).numberValue;
                        G = (int)((LangNumber)bmmm.arrayValue[1.0]).numberValue;
                        B = (int)((LangNumber)bmmm.arrayValue[2.0]).numberValue;
                        bitmap.SetPixel((int)((double)dic.Key), (int)((double)dicc.Key), Color.FromArgb(R, G, B));
                    }
                }
                try
                {
                    bitmap.Save(((LangString)param2).stringValue);
                }
                catch (Exception)
                {
                    bitmap.Dispose();
                    throw new Exception("Line " + node.token.line + ": " + "saveImage: Permission Denied!");
                }
                bitmap.Dispose();
                return new LangNumber(1);
            }
            #endregion
            #region getTypeName
            else if (stat.name == "getTypeName")
            {
                checkParameterNumber("getTypeNumber", 1, stat);
                LangObject param1 = decider(((Node)stat.parameters[0]));
                if (param1.objectType != ObjectType.CLASS)
                {
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = node.token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = node.token;
                    }
                    throw new Exception("Line " + node.token.line + ": " + "Function " + stat.name + " expects parameter 1 to be 'class', '" + Convert.ToString(param1.objectType) + "' Found");
                }
                return new LangString(((LangClass)param1).name);
            }
            #endregion
            #region getCurrentDirectory
            else if (stat.name == "getCurrentDirectory")
            {
                checkParameterNumber("getCurrentDirectory", 0, stat);
                return new LangString(Directory.GetCurrentDirectory());
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
                    foreach (FunctionStatement st in ((ArrayList)functions[stat.name]))
                    {
                        if (st.parameters.Count != stat.parameters.Count)
                        {
                            continue;
                        }
                        else
                        {
                            bool works = true;
                            for (int i = 0; i < stat.parameters.Count; i++)
                            {
                                Parameter param = (Parameter)st.parameters[i];
                                LangObject paramC = (LangObject)paramsCalcd[i];
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
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = stat.token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = stat.token;
                    }
                    throw new Exception("Line " + stat.token.line + ": No overloaded instance of function '" + stat.name + "' that accepts types" + types);
                }
                for (int i = 0; i < func.parameters.Count; i++)
                {
                    ((Hashtable)table[level])[((Parameter)func.parameters[i]).name] = ((LangObject)paramsCalcd[i]);
                }
                LangObject obj2 = statListDecider(func.stats);
                decreaseLevel();
                foreach (string name in func.globals)
                {
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
                        return new LangNumber(0);
                    }
                }
                return obj2;
            }
            #endregion
        }

        LangObject decider(Node node)
        {
            if (!keepWorking)
                return new LangState("stop");
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
            return new LangNumber(0);
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
                return new LangNumber(Convert.ToInt32(obj1.Greater(obj2)));
            }
            catch (Exception)
            {
                if (langManager.INTERPRET)
                {
                    langManager.lastErrorToken = node.token;
                }
                else
                {
                    langManager.lastLiveErrorToken = node.token;
                }
                throw;
            }
        }
        LangObject smallerInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            try
            {
                return new LangNumber(Convert.ToInt32(decider(_node.left).Smaller(decider(_node.right))));
            }
            catch (Exception)
            {
                if (langManager.INTERPRET)
                {
                    langManager.lastErrorToken = node.token;
                }
                else
                {
                    langManager.lastLiveErrorToken = node.token;
                }
                throw;
            }
        }
        LangObject equalInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            try
            {
                return new LangNumber(Convert.ToInt32(decider(_node.left).Equal(decider(_node.right))));
            }
            catch (Exception)
            {
                if (langManager.INTERPRET)
                {
                    langManager.lastErrorToken = node.token;
                }
                else
                {
                    langManager.lastLiveErrorToken = node.token;
                }
                throw;
            }
        }
        LangObject greaterEqualInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            try
            {
                return new LangNumber(Convert.ToInt32(decider(_node.left).GreaterEqual(decider(_node.right))));
            }
            catch (Exception)
            {
                if (langManager.INTERPRET)
                {
                    langManager.lastErrorToken = node.token;
                }
                else
                {
                    langManager.lastLiveErrorToken = node.token;
                }
                throw;
            }
        }
        LangObject smallerEqualInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            try
            {
                return new LangNumber(Convert.ToInt32(decider(_node.left).Smaller(decider(_node.right))));
            }
            catch (Exception)
            {
                if (langManager.INTERPRET)
                {
                    langManager.lastErrorToken = node.token;
                }
                else
                {
                    langManager.lastLiveErrorToken = node.token;
                }
                throw;
            }
        }
        LangObject notEqualInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            try
            {
                return new LangNumber(Convert.ToInt32(decider(_node.left).NotEqual(decider(_node.right))));
            }
            catch (Exception)
            {
                if (langManager.INTERPRET)
                {
                    langManager.lastErrorToken = node.token;
                }
                else
                {
                    langManager.lastLiveErrorToken = node.token;
                }
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
                return new LangNumber(Convert.ToInt32(Convert.ToBoolean(((LangNumber)_left).numberValue) && Convert.ToBoolean(((LangNumber)_right).numberValue)));
            }
            if (langManager.INTERPRET)
            {
                langManager.lastErrorToken = node.token;
            }
            else
            {
                langManager.lastLiveErrorToken = node.token;
            }
            throw new InvalidOperationException("Invalid operation '" + Convert.ToString(_left.objectType) + "' & '" + Convert.ToString(_right.objectType) + "'");
        }
        LangObject orInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            LangObject _left, _right;
            _left = decider(_node.left);
            _right = decider(_node.right);
            if (_left.objectType == ObjectType.NUMBER && _right.objectType == ObjectType.NUMBER)
            {
                return new LangNumber(Convert.ToInt32(Convert.ToBoolean(((LangNumber)_left).numberValue) || Convert.ToBoolean(((LangNumber)_right).numberValue)));
            }
            if (langManager.INTERPRET)
            {
                langManager.lastErrorToken = node.token;
            }
            else
            {
                langManager.lastLiveErrorToken = node.token;
            }
            throw new InvalidOperationException("Invalid operation '" + Convert.ToString(_left.objectType) + "' | '" + Convert.ToString(_right.objectType) + "'");
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
                if (langManager.INTERPRET)
                {
                    langManager.lastErrorToken = node.token;
                }
                else
                {
                    langManager.lastLiveErrorToken = node.token;
                }
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
                if (langManager.INTERPRET)
                {
                    langManager.lastErrorToken = node.token;
                }
                else
                {
                    langManager.lastLiveErrorToken = node.token;
                }
                throw;
            }
        }
        LangObject divintInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            LangNumber val = (LangNumber)decider(_node.right);
            try
            {
                return new LangNumber(Math.Floor(((LangNumber)decider(_node.left).Divide(val)).numberValue));
            }
            catch (Exception)
            {
                if (langManager.INTERPRET)
                {
                    langManager.lastErrorToken = node.token;
                }
                else
                {
                    langManager.lastLiveErrorToken = node.token;
                }
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
                if (langManager.INTERPRET)
                {
                    langManager.lastErrorToken = node.token;
                }
                else
                {
                    langManager.lastLiveErrorToken = node.token;
                }
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
                if (langManager.INTERPRET)
                {
                    langManager.lastErrorToken = node.token;
                }
                else
                {
                    langManager.lastLiveErrorToken = node.token;
                }
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
                if (langManager.INTERPRET)
                {
                    langManager.lastErrorToken = node.token;
                }
                else
                {
                    langManager.lastLiveErrorToken = node.token;
                }
                throw;
            }
        }
        LangObject minusInterpret(Node node)
        {
            BinaryOperator _node = (BinaryOperator)node;
            return decider(_node.left).Minus(decider(_node.right));
        }
        #endregion

        LangObject BracketsInterpret(Node node, LangObject _val, bool onlyOnNotFoundState = false)
        {
            if (node.nodeType == NodeType.ID)
            {
                ID node_ = (ID)node;
                LangObject obj = (LangObject)((Hashtable)table[level])[((ID)node).name];
                if (obj == null)
                    return (LangObject)(((Hashtable)table[level])[node_.name] = new LangMap(new Hashtable()));
                if (obj.objectType != ObjectType.MAP && obj.objectType != ObjectType.STRING)
                    return (LangObject)(((Hashtable)table[level])[node_.name] = new LangMap(new Hashtable()));
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
                    retobj = (LangObject)(new LangString("" + ((LangString)_left).stringValue[(int)_right_num.numberValue]));
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
                    throw new Exception("Line " + node.token.line + ": " + "Cannot implicitly convert from type 'string' to type 'number'");
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
                        return ((LangObject)(((LangMap)_left).arrayValue[((LangNumber)_right).numberValue] = new LangMap(new Hashtable())));
                    }
                    else if (_right.objectType == ObjectType.STRING)
                    {
                        return ((LangObject)(((LangMap)_left).arrayValue[((LangString)_right).stringValue] = new LangMap(new Hashtable())));
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
                    return new LangString("" + ((LangString)_val).stringValue[0]);
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
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = node.token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = node.token;
                    }
                    throw new Exception("Line " + node.token.line + ": Expected class on the left handside");
                }
                if (obj.objectType != ObjectType.CLASS)
                {
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = node.token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = node.token;
                    }
                    throw new Exception("Line " + node.token.line + ": Expected class on left handside");
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
                if (langManager.INTERPRET)
                {
                    langManager.lastErrorToken = node.token;
                }
                else
                {
                    langManager.lastLiveErrorToken = node.token;
                }
                throw new Exception("Line " + node.token.line + ": Expected class on left handside");
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
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = node.token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = node.token;
                    }
                    throw new Exception("Class '" + _left_ret_class.name + "' doesn't contain the member '" + _right_ID.name + "'");
                }
                return (LangObject)(_left_ret_class.vars[_right_ID.name]);
            }
            if (_right.nodeType == NodeType.STATEMENT)
            {
                FunctionCallStatement _right_stat = (FunctionCallStatement)_right;
                LangClass _left_ret_class = (LangClass)_left_ret;
                if (!(_left_ret_class.methods.ContainsKey(_right_stat.name)))
                {
                    if (langManager.INTERPRET)
                    {
                        langManager.lastErrorToken = _right.token;
                    }
                    else
                    {
                        langManager.lastLiveErrorToken = _right.token;
                    }
                    throw new Exception("Line " + _right.token.line + ": type '" + _left_ret_class.name + "' doesn't contain the method '" + _right_stat.name + "'");
                }
                return RunClassFunction((FunctionStatement)(_left_ret_class.methods[_right_stat.name]), _right_stat, (LangClass)_left_ret);
            }


            throw new Exception("Something not handled here");
        }
        LangObject DotOperatorInterpret(Node node)
        {
            return DotInterpret(node, null);
        }

        LangObject RunClassFunction(FunctionStatement _function, FunctionCallStatement _call, LangClass _class, ArrayList alreadyCalcd = null)
        {
            if (!keepWorking)
                return new LangState("stop");
            if (_function.parameters.Count != _call.parameters.Count)
            {
                switch (_function.parameters.Count)
                {
                    case 0:
                        if (langManager.INTERPRET)
                        {
                            langManager.lastErrorToken = _call.token;
                        }
                        else
                        {
                            langManager.lastLiveErrorToken = _call.token;
                        }
                        throw new Exception("Line " + _call.token.line + ": function '" + _function.name + "' expectes no parameters, " + _call.parameters.Count + " Given");
                    case 1:
                        if (langManager.INTERPRET)
                        {
                            langManager.lastErrorToken = _call.token;
                        }
                        else
                        {
                            langManager.lastLiveErrorToken = _call.token;
                        }
                        throw new Exception("Line " + _call.token.line + ": function '" + _function.name + "' expectes 1 parameter, " + _call.parameters.Count + " Given");
                    default:
                        if (langManager.INTERPRET)
                        {
                            langManager.lastErrorToken = _call.token;
                        }
                        else
                        {
                            langManager.lastLiveErrorToken = _call.token;
                        }
                        throw new Exception("Line " + _call.token.line + ": function '" + _function.name + "' expectes " + _function.parameters.Count + " parameter, " + _call.parameters.Count + " Given");
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
                            if (langManager.INTERPRET)
                            {
                                langManager.lastErrorToken = ((Node)_call.parameters[i]).token;
                            }
                            else
                            {
                                langManager.lastLiveErrorToken = ((Node)_call.parameters[i]).token;
                            }
                            throw new ArgumentException("Line " + ((Node)_call.parameters[i]).token.line + ": " + "Function '" + _call.name + "' expects parameter " + i + " to be " + param.type + ", " + Convert.ToString(ret.objectType) + " Given");
                        }
                        break;
                    case ObjectType.NUMBER:
                        if (param.type != "number" && param.type != "integer")
                        {
                            if (langManager.INTERPRET)
                            {
                                langManager.lastErrorToken = ((Node)_call.parameters[i]).token;
                            }
                            else
                            {
                                langManager.lastLiveErrorToken = ((Node)_call.parameters[i]).token;
                            }
                            throw new ArgumentException("Line " + ((Node)_call.parameters[i]).token.line + ": " + "Function '" + _call.name + "' expects parameter " + i + " to be " + param.type + ", " + Convert.ToString(ret.objectType) + " Given");
                        }
                        break;
                    case ObjectType.MAP:
                        if (param.type != "map")
                        {
                            if (langManager.INTERPRET)
                            {
                                langManager.lastErrorToken = ((Node)_call.parameters[i]).token;
                            }
                            else
                            {
                                langManager.lastLiveErrorToken = ((Node)_call.parameters[i]).token;
                            }
                            throw new ArgumentException("Line " + ((Node)_call.parameters[i]).token.line + ": " + "Function '" + _call.name + "' expects parameter " + i + " to be " + param.type + ", " + Convert.ToString(ret.objectType) + " Given");
                        }
                        break;
                    default:
                        break;
                }
            }
            ((Hashtable)table[level])["this"] = _class;
            LangObject obj = statListDecider(_function.stats);
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
        public LangObject RunClassOperator(FunctionStatement _function, LangClass _first, LangClass _second)
        {
            if (!keepWorking)
                return new LangState("stop");
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
            return new LangNumber(Convert.ToDouble(_node.value));
        }
        LangObject stringInterpret(Node node)
        {
            return new LangString(((StringVal)(node)).val);
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
                throw new Exception("Line " + node.token.line + ": " + "The variable '" + _node.name + "' doesn't exist!");
            }
        }
        #endregion

        #region statements
        #region simpleStatements
        LangObject bindStatInterpret(Node node)
        {
            if (!keepWorking)
                return new LangState("stop");
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
                return new LangState("stop");
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
            console.print(output);
            foreach (PrintStatement stat in _node.extras)
            {
                printStatInterpret(stat);
            }
            return val;
        }
        LangObject scanStatInterpret(Statement node)
        {
            if (!keepWorking)
                return new LangState("stop");
            ScanStatement _node = (ScanStatement)node;
            string sc = console.scan();
            ObjectType type = ObjectType.STRING;
            bool Successful = true;
            if (sc.Length == 0)
            {
                Successful = false;
                sc = "0";
            }
            else if (_node.scanType != null)
            {
                if (_node.scanType.name == "number")
                {
                    type = ObjectType.NUMBER;
                    double o;
                    if (!double.TryParse(sc, out o))
                    {
                        if (langManager.INTERPRET)
                        {
                            langManager.lastErrorToken = node.token;
                        }
                        else
                        {
                            langManager.lastLiveErrorToken = node.token;
                        }
                        throw new Exception("Line " + node.token.line + ": " + "Invalid number format!");
                    }
                }
                else if (_node.scanType.name == "integer")
                {
                    type = ObjectType.NUMBER;
                    double o;
                    if (!double.TryParse(sc, out o))
                    {
                        if (langManager.INTERPRET)
                        {
                            langManager.lastErrorToken = node.token;
                        }
                        else
                        {
                            langManager.lastLiveErrorToken = node.token;
                        }
                        throw new Exception("Line " + node.token.line + ": " + "Invalid number format!");
                    }
                    sc = Convert.ToString(Convert.ToInt32(o));
                }
            }
            else
            {
                double o;
                if (double.TryParse(sc, out o))
                {
                    type = ObjectType.NUMBER;
                }
            }
            if (type == ObjectType.NUMBER)
            {
                ((Hashtable)table[level])[((ID)(_node.expr)).name] = new LangNumber(Convert.ToDouble(sc));
            }
            else
            {
                ((Hashtable)table[level])[((ID)(_node.expr)).name] = new LangString(sc);
            }
            foreach (ScanStatement stat in _node.extras)
            {
                Successful = Successful && (((LangNumber)scanStatInterpret(stat)).numberValue) != 0;
            }
            return new LangNumber(Convert.ToInt32(Successful));
        }
        LangObject importStatInterpret(Node node)
        {
            if (!keepWorking)
                return new LangState("stop");
            ImportStatement _node = (ImportStatement)node;
            foreach (string file in _node.imports)
            {
                string code = File.ReadAllText(Directory.GetCurrentDirectory() + "\\" + file + ".lan");
                Lexer lexer = new Lexer(new LangManager(null, null), code);
                lexer.FileName = file;
                Parser parser = new Parser(new LangManager(null, null));
                parser.updateTokens(lexer.lex());
                Node stats = parser.parse();
                Interpreter inter = new Interpreter(console, new LangManager(console, null));
                inter.updateRoot((StatementList)stats);
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
            return new LangNumber(0);
        }
        LangObject newStatInterpret(Node node)
        {
            if (!keepWorking)
                return new LangState("stop");
            ClassInitStatement stat = (ClassInitStatement)node;
            if (!classes.ContainsKey(stat.constructor.name))
            {
                if (langManager.INTERPRET)
                {
                    langManager.lastErrorToken = stat.constructor.token;
                }
                else
                {
                    langManager.lastLiveErrorToken = stat.constructor.token;
                }
                throw new Exception("Line " + stat.constructor.token.line + ": There is no such class!");
            }
            ClassStatement _class = (ClassStatement)classes[stat.constructor.name];
            LangClass _ret_class = new LangClass(_class, stat);
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
                throw new Exception("Line " + node.token.line + ": " + ((LangString)raise).stringValue);
            }
            else
            {
                throw new Exception("Cannot throw any type other than string, " + Convert.ToString(raise.objectType) + " Found");
            }
        }
        #endregion

        #region compoundStatements

        LangObject ifStatInterpret(Node node_)
        {
            if (!keepWorking)
                return new LangState("stop");
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
            return new LangNumber(0);
        }

        LangObject forStatInterpret(Node node_)
        {
            if (!keepWorking)
                return new LangState("stop");
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
            return new LangNumber(0);
        }

        LangObject whileStatInterpret(Node node_)
        {
            if (!keepWorking)
                return new LangState("stop");
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
            return new LangNumber(0);
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
                    return new LangNumber(0);
                }
                string risen = e.Message;
                increaseLevel();
                foreach (DictionaryEntry dic in ((Hashtable)table[level - 1]))
                {
                    ((Hashtable)table[level])[dic.Key] = dic.Key;
                }
                ((Hashtable)table[level])[node.catchID.name] = new LangString(risen);
                return statListDecider(node.catchStats);
            }
        }

        #endregion
        #endregion

        #endregion
    }
}
