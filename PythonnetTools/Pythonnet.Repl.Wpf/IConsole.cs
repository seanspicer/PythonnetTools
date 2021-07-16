using System.IO;

namespace Pythonnet.Repl.Wpf
{
    public interface IConsole
    {
        string ReadLine(int autoIndentSize);

        void Write(string text, Style style);

        void WriteLine(string text, Style style);

        void WriteLine();

        TextWriter Output { get; set; }

        TextWriter ErrorOutput { get; set; }
    }
}