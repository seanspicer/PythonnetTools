// Copyright (c) 2010 Joe Moorhouse

using System.Windows.Media;
using ICSharpCode.AvalonEdit;

namespace Pythonnet.Repl.Wpf
{   
    public class PythonConsolePad 
    {
        PythonTextEditor pythonTextEditor;
        TextEditor textEditor;
        PythonConsoleHost host;

        public PythonConsolePad()
        {
            textEditor = new TextEditor();
            pythonTextEditor = new PythonTextEditor(textEditor);
            host = new PythonConsoleHost(pythonTextEditor);
            host.Run();
            textEditor.FontFamily = new FontFamily("Consolas");
            textEditor.FontSize = 12;
        }

        public TextEditor Control
        {
            get { return textEditor; }
        }
        
        public PythonConsoleHost Host
        {
            get { return host; }
        }

        public PythonConsole Console
        {
            get { return host.Console; }
        }

        public void Dispose()
        {
            host.Dispose();
        }
    }
}
