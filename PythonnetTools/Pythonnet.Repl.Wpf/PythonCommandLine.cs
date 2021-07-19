

namespace Pythonnet.Repl.Wpf
{
    public class PythonCommandLine : CommandLine
    {
        protected override string Prompt
        {
            get
            {
                return ">>> ";
            }
        }

        public override string PromptContinuation
        {
            get
            {
                return "... ";
            }
        }
    }
}