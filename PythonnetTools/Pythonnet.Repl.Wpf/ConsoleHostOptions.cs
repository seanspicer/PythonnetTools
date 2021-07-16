

namespace Pythonnet.Repl.Wpf
{
    public class ConsoleHostOptions
    {
        public enum Action {
            None,
            RunConsole,
            RunFile,
            DisplayHelp
        }
        
        public Action RunAction { get; set; }

        public ConsoleHostOptions()
        {
            RunAction = Action.RunConsole;
        }
    }
}