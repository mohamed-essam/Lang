using Lang.language;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Lang.Language
{
    public partial class CodeGUI : Form
    {
        Thread caller;
        Interpreter handler;

        public CodeGUI(Thread _caller, Interpreter _handler)
        {
            InitializeComponent();
            caller = _caller;
            handler = _handler;
        }

        internal void drawImage(Bitmap _image)
        {
            Canvas.Image = _image;
        }

        private void Canvas_MouseClick(object sender, MouseEventArgs e)
        {
            ArrayList parameters = new ArrayList();
            parameters.Add(e.X);
            parameters.Add(e.Y);
            try
            {
                handler.EventHandler("CanvasMouseClick", parameters);
            }
            catch (Exception ex)
            {
                handler.consoleUI.Invoke((MethodInvoker)delegate()
                {
                    handler.consoleUI.HandleExceptions(ex, handler.langManager.lastErrorToken, handler.StackTrace);
                });
            }
        }

        private void CodeGUI_KeyDown(object sender, KeyEventArgs e)
        {
            ArrayList parameters = new ArrayList();
            parameters.Add(e.KeyValue);
            try
            {
                handler.EventHandler("CanvasKeyPress", parameters);
            }
            catch (Exception ex)
            {

                handler.consoleUI.Invoke((MethodInvoker)delegate()
                {
                    handler.consoleUI.HandleExceptions(ex, handler.langManager.lastErrorToken, handler.StackTrace);
                });
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            ArrayList parameters = new ArrayList();
            parameters.Add(e.X);
            parameters.Add(e.Y);
            try
            {
                handler.EventHandler("CanvasMouseMove", parameters);
            }
            catch (Exception ex)
            {
                handler.consoleUI.Invoke((MethodInvoker)delegate()
                {
                    handler.consoleUI.HandleExceptions(ex, handler.langManager.lastErrorToken, handler.StackTrace);
                });
            }
        }

        internal double GetLeftButtonState()
        {
            return Convert.ToDouble(MouseButtons == MouseButtons.Left || MouseButtons == (MouseButtons.Left | MouseButtons.Right) || MouseButtons == (MouseButtons.Left | MouseButtons.Middle) || MouseButtons == (MouseButtons.Left | MouseButtons.Middle | MouseButtons.Right));
        }

        internal double GetMiddleButtonState()
        {
            return Convert.ToDouble(MouseButtons == MouseButtons.Middle || MouseButtons == (MouseButtons.Middle | MouseButtons.Left) || MouseButtons == (MouseButtons.Middle | MouseButtons.Right) || MouseButtons == (MouseButtons.Middle | MouseButtons.Left | MouseButtons.Right));
        }

        internal double GetRightButtonState()
        {
            return Convert.ToDouble(MouseButtons == MouseButtons.Right || MouseButtons == (MouseButtons.Right | MouseButtons.Middle) || MouseButtons == (MouseButtons.Right | MouseButtons.Left) || MouseButtons == (MouseButtons.Right | MouseButtons.Middle | MouseButtons.Left));
        }
    }
}
