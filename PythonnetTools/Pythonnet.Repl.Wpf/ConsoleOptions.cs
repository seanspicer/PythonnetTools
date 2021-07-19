

namespace Pythonnet.Repl.Wpf
{
    public class ConsoleOptions
    {
        private bool _autoIndent = false;
        private int _autoIndentSize = 4;
        
        public bool AutoIndent {
            get { return _autoIndent; }
            set { _autoIndent = value; }
        }
        
        public int AutoIndentSize {
            get { return _autoIndentSize; }
            set { _autoIndentSize = value; }
        }
    }
}