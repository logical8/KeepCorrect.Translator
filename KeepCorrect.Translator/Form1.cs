using System;
using System.Windows.Forms;
using KeepCorrect.Translator.Properties;

namespace KeepCorrect.Translator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;

            // Initialize Tray Icon
            _trayIcon = new NotifyIcon
            {
                Icon = Resources.Icon1,
                ContextMenu = new ContextMenu(new[]
                {
                    new MenuItem("Show", Show),
                    new MenuItem("Exit", Exit)
                }),
                Visible = true
            };

            GlobalHotKey.RegisterHotKey(Modifiers.Alt | Modifiers.Shift, Keys.T, ShowFormAndTranslate);
        }

        private readonly NotifyIcon _trayIcon;

        private void Show(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
        }

        private void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            _trayIcon.Visible = false;

            Application.Exit();
        }

        private void ShowFormAndTranslate()
        {
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
        }
    }
}