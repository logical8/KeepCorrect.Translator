using System.Windows.Forms;

namespace KeepCorrect.Translator.Extensions
{
    public static class ControlExtensions
    {
        public static void AppendLine(this TextBox source, string value)
        {
            if (source.Text.Length==0)
                source.Text = value;
            else
                source.AppendText("\r\n"+value);
        }

    }
}