

using System;
using System.Windows.Input;

namespace Pythonnet.Repl.Wpf
{
    public class ConsoleControlViewModel
    {
        public bool AllowFullAutocompletion => true;
        
        public void HandleTextEntering(TextCompositionEventArgs e)
        {
            Console.WriteLine(e.Text);
        }
    }
}