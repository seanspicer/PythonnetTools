// Copyright (c) 2010 Joe Moorhouse

using System;
using System.Printing;
using System.Text;
using System.Threading;
using Python.Runtime;

namespace Pythonnet.Repl.Wpf
{
    public delegate void ConsoleCreatedEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Hosts the python console.
    /// </summary>
    public class PythonConsoleHost : IDisposable
    {
        Thread thread;
        PythonTextEditor textEditor;
        PythonConsole pythonConsole;

        private PythonEngine _engine;
        private ConsoleOptions _consoleOptions;
        private IConsole _console;
        private CommandLine _commandLine;
        private PythonOutputStream _stream;
        
        private ConsoleHostOptions Options { get; }
        
        public event ConsoleCreatedEventHandler ConsoleCreated;

        public PythonConsoleHost(PythonTextEditor textEditor)
        {
            this.textEditor = textEditor;
            Options = new ConsoleHostOptions();
        }

        public PythonConsole Console
        {
            get { return pythonConsole; }
        }

        protected virtual Type Provider
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Runs the console host in its own thread.
        /// </summary>
        public void Run()
        {
            thread = new Thread(RunInternal);
            thread.IsBackground = true;
            thread.Start();
        }

        public void Dispose()
        {
            if (pythonConsole != null)
            {
                pythonConsole.Dispose();
            }

            if (thread != null)
            {
                thread.Join();
            }
        }

        protected virtual CommandLine CreateCommandLine()
        {
            return new PythonCommandLine();
        }

        protected virtual ConsoleOptions CreateOptionsParser()
        {
            throw new NotImplementedException();
        }

        /// <remarks>
        /// After the engine is created the standard output is replaced with our custom Stream class so we
        /// can redirect the stdout to the text editor window.
        /// This can be done in this method since the Runtime object will have been created before this method
        /// is called.
        /// </remarks>
        protected virtual IConsole CreateConsole(PythonEngine engine, CommandLine commandLine, ConsoleOptions options)
        {
            SetOutput(new PythonOutputStream(textEditor));
            pythonConsole = new PythonConsole(textEditor, commandLine);
            if (ConsoleCreated != null) ConsoleCreated(this, EventArgs.Empty);
            return pythonConsole;
        }

        protected virtual void SetOutput(PythonOutputStream stream)
        {
            _stream = stream;
        }

        private int _exitCode;
        public virtual void RunInternal()
        {
            _engine = new PythonEngine();
            _consoleOptions = new ConsoleOptions();
            Execute();
        }

        private void Execute()
        {
            ExecuteInternal();
        }

        
        protected virtual void ExecuteInternal()
        {
            switch (Options.RunAction) {
                case ConsoleHostOptions.Action.None:
                case ConsoleHostOptions.Action.RunConsole:
                    _exitCode = RunCommandLine();
                    break;

                case ConsoleHostOptions.Action.RunFile:
                    _exitCode = RunFile();
                    break;

                default:
                    throw new NotImplementedException("Unreachable");
            }
        }

        private int RunFile()
        {
            throw new NotImplementedException();
        }

        private int RunCommandLine()
        {
            _commandLine = CreateCommandLine();
            
            if (_console == null) {
                _console = CreateConsole(_engine, _commandLine, _consoleOptions);
            }

            _commandLine.Run(_engine, _stream, _console, _consoleOptions);

            return _commandLine.ExitCode;
        }
    }
}
