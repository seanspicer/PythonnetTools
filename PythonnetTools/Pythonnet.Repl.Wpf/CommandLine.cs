

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Python.Runtime;

namespace Pythonnet.Repl.Wpf
{
    public class CommandLine
    {
        public PyScope ScriptScope { get; internal set; }
        public int ExitCode { get; set; }

        private PythonEngine _engine;
        private PythonOutputStream _stream;
        private ConsoleOptions _consoleOptions;
        private IConsole _console;
        
        protected virtual string Prompt { get { return ">>> "; } }
        public virtual string PromptContinuation { get { return "... "; } }
        protected virtual string Logo { get { return null; } }

        protected string LastPrompt = string.Empty;
        
        private static readonly char[] newLineChar = new char[] { '\n' };
        private static readonly char[] whiteSpace = { ' ', '\t' };
        
        private int? _terminatingExitCode;
        
        public CommandLine()
        {
            ExitCode = 0;
        }

        public void Run(PythonEngine engine, PythonOutputStream stream, IConsole console, ConsoleOptions consoleOptions)
        {
            _engine = engine;
            _stream = stream;
            _console = console;
            _consoleOptions = consoleOptions;

            using (Py.GIL())
            {

            }
            
            Run();
        }

        protected virtual int Run()
        {
            return RunInteractive();
        }
        
        protected virtual int RunInteractive() 
        {
            //PrintLogo();
            return RunInteractiveLoop();
        }

        protected virtual int RunInteractiveLoop()
        {
            int? res = null;
            do
            {
                res = TryInteractiveAction();
                
            } while (res == null);

            return res.Value;
        }

        private int? TryInteractiveAction()
        {
            int? result = null;

            result = RunOneInteraction();

            return result;
        }

        private int? RunOneInteraction()
        {
            string s = ReadStatement(out bool continueInteraction);
            
            if (continueInteraction == false) {
                return _terminatingExitCode ?? 0;
            }
            
            if (String.IsNullOrEmpty(s)) {
                // Is it an empty line?
                _console.Write(String.Empty, Style.Out);
                return null;
            }

            ExecuteCommand(s);
            return null;
        }

        protected virtual void ExecuteCommand(string command) 
        {
            using (Py.GIL())
            {
                // Redirect stdout to text box
                dynamic sys = PythonEngine.ImportModule("sys");

                string codeToRedirectOutput =
                    "import sys\n" +
                    "from io import StringIO\n" +
                    "sys.stdout = mystdout = StringIO()\n" +
                    "sys.stdout.flush()\n" +
                    "sys.stderr = mystderr = StringIO()\n" +
                    "sys.stderr.flush()\n";
                
                PythonEngine.RunString(codeToRedirectOutput);
                
                var result = PythonEngine.RunSimpleString(command);
                if (0 == result)
                {
                    string pyStdout = sys.stdout.getvalue(); // Get stdout
                    using (var sw = new StreamWriter(_stream))
                    {
                        sw.Write(pyStdout);
                    }
                }
                else
                {
                    string pyStderr = sys.stderr.getvalue(); // Get stderr
                    using (var sw = new StreamWriter(_stream))
                    {
                        sw.Write(pyStderr);
                    }
                }
                
            }
        }
        
        public virtual void Terminate(int exitCode) {
            // The default implementation just sets a flag. Derived types can support better termination
            _terminatingExitCode = exitCode;
        }
        
        protected string ReadStatement(out bool continueInteraction) {
            StringBuilder b = new StringBuilder();
            int autoIndentSize = 0;

            _console.Write(Prompt, Style.Prompt);
            LastPrompt = Prompt;
            
            while (true) {
                string line = ReadLine(autoIndentSize);
                continueInteraction = true;

                if (line == null || (_terminatingExitCode != null)) {
                    continueInteraction = false;
                    return null;
                }

                //bool allowIncompleteStatement = TreatAsBlankLine(line, autoIndentSize);
                b.Append(line);
                // Note that this does not use Environment.NewLine because some languages (eg. Python) only
                // recognize \n as a line terminator.
                b.Append("\n");

                string code = b.ToString();

                var codeCompiles = false;
                using (Py.GIL())
                {
                    try
                    {
                        var src = PythonEngine.Compile(code, "<stdin>", RunFlagType.Single);

                        if (LastPrompt == Prompt || code[^2] == '\n')
                        {
                            codeCompiles = true;
                        }
                    } 
                    catch (PythonException e)
                    {
                        if (e.Message.Contains("unexpected EOF while parsing"))
                        {
                            codeCompiles = false;
                        }
                        else
                        {
                            if (LastPrompt == Prompt || code[^2] == '\n')
                            {
                                codeCompiles = true;
                            }
                        }
                    }
                }

                if (codeCompiles)
                {
                    return code;
                }
                
                // var props = GetCommandProperties(code);
                // if (SourceCodePropertiesUtils.IsCompleteOrInvalid(props, allowIncompleteStatement)) {
                //     return props != ScriptCodeParseResult.Empty ? code : null;
                // }
                //
                if (_consoleOptions.AutoIndent && _consoleOptions.AutoIndentSize != 0) {
                    autoIndentSize = GetNextAutoIndentSize(code);
                }
                
                // Keep on reading input
                _console.Write(PromptContinuation, Style.Prompt);
                LastPrompt = PromptContinuation;
            }
        }
        
        // protected virtual ScriptCodeParseResult GetCommandProperties(string code) {
        //     ScriptSource command = _engine.CreateScriptSourceFromString(code, SourceCodeKind.InteractiveCode);
        //     return command.GetCodeProperties(_engine.GetCompilerOptions(_scope));
        // }
        
        protected virtual string ReadLine(int autoIndentSize) {
            return _console.ReadLine(autoIndentSize);
        }
        
        protected virtual int GetNextAutoIndentSize(string text)
        {
            Debug.Assert(text[text.Length - 1] == '\n');
            string[] lines = text.Split(newLineChar);
            if (lines.Length <= 1) return 0;
            string lastLine = lines[lines.Length - 2];

            // Figure out the number of white-spaces at the start of the last line
            int startingSpaces = 0;
            while (startingSpaces < lastLine.Length && lastLine[startingSpaces] == ' ')
                startingSpaces++;

            // Assume the same indent as the previous line
            int autoIndentSize = startingSpaces;
            // Increase the indent if this looks like the start of a compounds statement.
            // Ideally, we would ask the parser to tell us the exact indentation level
            if (lastLine.TrimEnd(whiteSpace).EndsWith(":"))
                autoIndentSize += _consoleOptions.AutoIndentSize;

            return autoIndentSize;
        }
    }
}