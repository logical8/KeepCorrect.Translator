using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using KeepCorrect.Translator.Extensions;
using KeepCorrect.Translator.Properties;

namespace KeepCorrect.Translator
{
    public partial class Form1 : Form
    {
        private const int padding = 10;

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
            var ptr = GlobalHotKey.GetForegroundWindow();
            Thread.Sleep(200);
            GlobalHotKey.SendCtrlC(ptr);
            Thread.Sleep(100);
            // programatically copy selected text into clipboard
            //await System.Threading.Tasks.Task.Factory.StartNew(fetchSelectionToClipboard);

            // access clipboard which now contains selected text in foreground window (active application)
            var text = await Task.Factory.StartNew(getClipBoardValue);

            //TODO: if (it is not text) return;
            if (ItIsText(text))
            {
                ShowTranslateOfText(text);
            }
            else
            {
                if (text.Length > 100) return;
                ShowTranslates(await Search.GetSearchResult(text));
            }    
            
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
            SetDesktopLocation(Cursor.Position.X, Cursor.Position.Y);
            Activate();
        }

        private static bool ItIsText(string text)
        {
            return text.Trim().Any(ch => ch == ' ');
        }

        private void ShowTranslateOfText(string text)
        {
            CleanForm();

            text = "test text for testing!";
            
            var textBoxWord = new TextBox();
            textBoxWord.Text = text;
            textBoxWord.ReadOnly = true;
            textBoxWord.BorderStyle = 0;
            textBoxWord.BackColor = BackColor;
            textBoxWord.TabStop = false;
            textBoxWord.Font = new Font(textBoxWord.Font.FontFamily, 16, FontStyle.Regular);
            textBoxWord.Size = new Size(500, 500);
            textBoxWord.Multiline = true;
            textBoxWord.ScrollBars = ScrollBars.Vertical;
            textBoxWord.Location = new Point(padding, padding);
            Controls.Add(textBoxWord);
        }
        

        private void ShowTranslates(SearchResult searchResult)
        {
            CleanForm();
                    
            var count = 0;
            var height = 250;
            
            var partsOfSpeech = searchResult.Word.PartsOfSpeech?.GetType()
                .GetProperties()
                .Where(p => p.GetValue(searchResult.Word.PartsOfSpeech, null) != null)
                .Select(p => (Adjective)p.GetValue(searchResult.Word.PartsOfSpeech, null)) ?? new List<Adjective>();
            foreach (var partOfSpeech in partsOfSpeech)
            {
                var textBoxWord = new TextBox();
                textBoxWord.Text = partOfSpeech.Word;
                textBoxWord.ReadOnly = true;
                textBoxWord.BorderStyle = 0;
                textBoxWord.BackColor = BackColor;
                textBoxWord.TabStop = false;
                textBoxWord.Font = new Font(textBoxWord.Font.FontFamily, 16, FontStyle.Bold);
                textBoxWord.Size = new Size(500, 25);
                textBoxWord.Location = new Point(padding, (10 + height) * count);
                Controls.Add(textBoxWord);
                
                var textBox = new TextBox();
                textBox.Text = partOfSpeech.PartOfSpeechRu;
                textBox.ReadOnly = true;
                textBox.BorderStyle = 0;
                textBox.BackColor = BackColor;
                textBox.TabStop = false;
                textBox.Font = new Font(textBox.Font, FontStyle.Italic);
                textBox.Size = new Size(500, 20);
                textBox.Location = new Point(padding, 40 + (10 + height) * count);
                Controls.Add(textBox);
                count++;

                var translationsTextBox = GetTranslateTextBox(partOfSpeech.Values.Select(v => v.ValueValue),
                    new Point(textBox.Location.X, textBox.Location.Y + 25));
                Controls.Add(translationsTextBox);
                
            }
            // var t = searchResult.Word.PartsOfSpeech.Noun.
            // Label[] labels = new Label[n];
            //
            // for (int i = 0; i < n; i++)
            // {
            //     textBoxes[i] = new TextBox();
            //     // Here you can modify the value of the textbox which is at textBoxes[i]
            //
            //     labels[i] = new Label();
            //     // Here you can modify the value of the label which is at labels[i]
            // }
            //
            // // This adds the controls to the form (you will need to specify thier co-ordinates etc. first)
            // for (int i = 0; i < n; i++)
            // {
            //     this.Controls.Add(textBoxes[i]);
            //     this.Controls.Add(labels[i]);
            // }
        }

        private void CleanForm()
        {
            foreach (var control in Controls.OfType<TextBox>().ToList())
            {
                Controls.Remove(control);
                control.Dispose();
            }
        }

        private Control GetTranslateTextBox(IEnumerable<string> translates, Point point)
        {
            var textBox = new TextBox();
            textBox.ReadOnly = true;
            textBox.BorderStyle = 0;
            textBox.BackColor = BackColor;
            textBox.TabStop = false;
            textBox.Multiline = true;
            textBox.Location = point;
            textBox.Size = new Size(500, 190);
            textBox.ScrollBars = ScrollBars.Vertical;
            foreach (var translate in translates)
            {
                textBox.AppendLine($"– {translate}");
            }

            return textBox;
        }

        private async Task<string> GetResult(string text)
        {
            var searchResult = await Search.GetSearchResult(text);
            if (searchResult == null) return "Перевод не найден";

            return $"{searchResult.Word.BaseWord} – {searchResult.Word.Translation}";
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