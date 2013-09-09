using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;

namespace Lang
{
    public class LangConsole
    {
        ArrayList inputSeg;
        int ind;
        internal string output_;

        public void processInput(string input){
            ind = 0;
            inputSeg = new ArrayList();
            string[] ss = input.Split();
            foreach (string str in ss)
            {
                inputSeg.Add(str);
            }
        }
        public void print(string inp){
            output_ += inp + '\n';
            //output.Text += inp + '\r'+'\n';
        }
        public string scan()
        {
            if (ind >= inputSeg.Count)
            {
                return "";
            }
            return (string)(inputSeg[ind++]);
        }
    }
}
