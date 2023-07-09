﻿using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeepCorrect.Translator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
        
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}