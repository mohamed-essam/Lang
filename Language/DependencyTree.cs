using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace Lang.language
{
    class DependencyTree
    {
        private Lexer lexer;
        private Parser parser;
        private ArrayList imported;
        private ArrayList tree;
        
        public DependencyTree()
        {
            lexer = new Lexer(new LangManager(null));
            parser = new Parser(new LangManager(null));
            imported = new ArrayList();
            tree = new ArrayList();
        }

        private ArrayList getImports(Node node)
        {
            StatementList stats = (StatementList)node;
            ArrayList imports = new ArrayList();
            foreach (Statement stat in stats.statements)
            {
                switch (stat.type)
                {
                    case StatementType.IF:
                        IfStatement _stat = (IfStatement)stat;
                        foreach (IfData data in _stat.data)
                        {
                            ArrayList arr = getImports(data.stats);
                            foreach (string str in arr)
                            {
                                imports.Add(str);
                            }
                        }
                        if (_stat.elseData != null)
                        {
                            ArrayList arr = getImports(_stat.elseData);
                            foreach (string str in arr)
                            {
                                imports.Add(str);
                            }
                        }
                        break;
                    case StatementType.FOR:
                        ArrayList arr2 = getImports(((ForStatement)stat).stats);
                        foreach (string str in arr2)
                        {
                            imports.Add(str);
                        }

                        break;
                    case StatementType.WHILE:
                        ArrayList arr3 = getImports(((WhileStatement)stat).stats);
                        foreach (string str in arr3)
                        {
                            imports.Add(str);
                        }

                        break;
                    case StatementType.FUNCTION:
                        ArrayList arr4 = getImports(((FunctionStatement)stat).stats);
                        foreach (string str in arr4)
                        {
                            imports.Add(str);
                        }
                        break;
                    case StatementType.IMPORT:
                        ImportStatement stat_ = ((ImportStatement)stat);
                        foreach (string str in stat_.imports)
                        {
                            imports.Add(str);
                        }
                        break;
                    default:
                        break;
                }
            }
            return imports;
        }

        private ArrayList readFile(string fileName, string code = null)
        {
            Node stats = null;
            try
            {
                if (code == null)
                {
                    try
                    {
                        code = File.ReadAllText(Directory.GetCurrentDirectory() + "\\" + fileName + ".lan");
                    }
                    catch (IOException)
                    {
                        try
                        {
                            code = File.ReadAllText(Directory.GetCurrentDirectory() + "\\Include\\" + fileName + ".lan");
                        }
                        catch (IOException)
                        {
                            throw new Exception("The file " + fileName + " doesn't exist!");
                        }
                    }
                }
                lexer.updateCode(code);
                lexer.FileName = fileName;
                ArrayList tokens = lexer.lex();
                parser.updateTokens(tokens);
                stats = parser.parse();
            }
            catch (IOException)
            {
                return null;
            }
            catch (Exception)
            {
                throw;
            }
            return getImports(stats);
        }

        private void BuildTree(string fileName)
        {
            if (imported.IndexOf(fileName) >= 0)
            {
                return;
            }
            ArrayList imports = readFile(fileName);
            imported.Add(fileName);
            foreach (string str in imports)
            {
                tree.Add(new KeyValuePair<string, string>(fileName, str));
                BuildTree(str);
            }
        }

        private bool BellmanFord(string fileName)
        {
            Hashtable table = new Hashtable();
            for (int v = 0; v < imported.Count; v++)
            {
                table[imported[v]] = 1000000000;
            }
            table[fileName] = 0;
            for (int v = 0; v < imported.Count; v++)
            {
                bool relax = false;
                for (int e = 0; e < tree.Count; e++)
                {
                    KeyValuePair<string, string> E = (KeyValuePair<string, string>)tree[e];
                    if (((int)table[E.Value]) > ((int)table[E.Key]) - 1)
                    {
                        table[E.Value] = ((int)table[E.Key]) - 1;
                        relax = true;
                    }
                }
                if (!relax)
                    return false;
                if (v + 1 == imported.Count)
                    return true;
            }
            return false;
        }

        public bool DetectCirularDependency(string fileName, string code)
        {
            ArrayList imports = readFile("", code);
            imported.Add(fileName);
            foreach (string str in imports)
            {
                tree.Add(new KeyValuePair<string, string>(fileName, str));
                BuildTree(str);
            }

            return BellmanFord(fileName);
        }
    }
}
