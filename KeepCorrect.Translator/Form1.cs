using System;
using System.Threading;
using System.Threading.Tasks;
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
            Deactivate += delegate { WindowState = FormWindowState.Minimized; };
        }

        private readonly NotifyIcon _trayIcon;

        private void Show(object sender, EventArgs e)
        {
            ShowFormAndTranslate();
        }

        private void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            _trayIcon.Visible = false;

            Application.Exit();
        }

        private async void ShowFormAndTranslate()
        {
            // programatically copy selected text into clipboard
            await System.Threading.Tasks.Task.Factory.StartNew(fetchSelectionToClipboard);

            // access clipboard which now contains selected text in foreground window (active application)
            var result = await System.Threading.Tasks.Task.Factory.StartNew(getClipBoardValue);

            label1.Text = result;
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
            SetDesktopLocation(Cursor.Position.X, Cursor.Position.Y);
            Activate();
        }

        static void fetchSelectionToClipboard()
        {
            Thread.Sleep(400);
            SendKeys.SendWait("^c"); // magic line which copies selected text to clipboard
            Thread.Sleep(400);
            
        }

        // depends on the type of your app, you sometimes need to access clipboard from a Single Thread Appartment model..therefore I'm creating a new thread here
        static string getClipBoardValue()
        {
            var text = "";
            var staThread = new Thread(
                delegate()
                {
                    try
                    {
                        text = Clipboard.GetText();
                    }
                    catch (Exception ex)
                    {
                        // ignored
                    }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            return text;
        }
    }
}