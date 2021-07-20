using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.Win32;
using Python.Runtime;
using Pythonnet.Repl.Wpf;

namespace Pythonnet.Wpf.Editor.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
		{
            Initialized += new EventHandler(MainWindow_Initialized);

            // Load our custom highlighting definition
            IHighlightingDefinition pythonHighlighting;
            using (Stream s = typeof(ConsoleControl).Assembly.GetManifestResourceStream("Pythonnet.Repl.Wpf.Resources.Python.xshd"))
            {
	            if (s == null)
		            throw new InvalidOperationException("Could not find embedded resource");
	            using (XmlReader reader = new XmlTextReader(s))
	            {
		            pythonHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
			            HighlightingLoader.Load(reader, HighlightingManager.Instance);
	            }
            }
            // and register it in the HighlightingManager
            HighlightingManager.Instance.RegisterHighlighting("Python Highlighting", new string[] { ".cool" }, pythonHighlighting);
            
            	
			InitializeComponent();

            textEditor.SyntaxHighlighting = pythonHighlighting;

            textEditor.PreviewKeyDown += new KeyEventHandler(textEditor_PreviewKeyDown);
            
            console.Pad.Host.ConsoleCreated += Host_ConsoleCreated;
		}

		string currentFileName;

        void Host_ConsoleCreated(object sender, EventArgs e)
        {
            console.Pad.Console.ConsoleInitialized += Console_ConsoleInitialized;
        }

        void Console_ConsoleInitialized(object sender, EventArgs e)
        {
	        var foo = new Foo(42, "vogons");

	        using (Py.GIL())
	        {
		        console.Pad.Console.ScriptScope.Set("foo", foo.ToPython());
	        }
	        


	        // string startupScipt = "import IronPythonConsole";
	        // ScriptSource scriptSource = console.Pad.Console.ScriptScope.Engine.CreateScriptSourceFromString(startupScipt, SourceCodeKind.Statements);
	        // try
	        // {
	        //     scriptSource.Execute();
	        // }
	        // catch {}
	        //double[] test = new double[] { 1.2, 4.6 };
	        //console.Pad.Console.ScriptScope.SetVariable("test", test);
        }

        void MainWindow_Initialized(object sender, EventArgs e)
        {
            //propertyGridComboBox.SelectedIndex = 1;
        }
		
		void openFileClick(object sender, RoutedEventArgs e)
		{   
            OpenFileDialog dlg = new OpenFileDialog();
			dlg.CheckFileExists = true;
			if (dlg.ShowDialog() ?? false) {
				currentFileName = dlg.FileName;
				textEditor.Load(currentFileName);
				//textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(currentFileName));
			}
		}
		
		void saveFileClick(object sender, EventArgs e)
		{
			if (currentFileName == null) {
				SaveFileDialog dlg = new SaveFileDialog();
				dlg.DefaultExt = ".txt";
				if (dlg.ShowDialog() ?? false) {
					currentFileName = dlg.FileName;
				} else {
					return;
				}
			}
			textEditor.Save(currentFileName);
		}

        void runClick(object sender, EventArgs e)
        {
            RunStatements();
        }

        void textEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5) RunStatements();
        }

        void RunStatements()
        {
            string statementsToRun = "";
            if (textEditor.TextArea.Selection.Length > 0)
                statementsToRun = textEditor.TextArea.Selection.GetText();
            else
                statementsToRun = textEditor.TextArea.Document.Text;
            console.Pad.Console.RunStatements(statementsToRun);
        }
        
    }
}