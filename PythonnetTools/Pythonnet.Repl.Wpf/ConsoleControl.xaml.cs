using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using ReactiveUI;

namespace Pythonnet.Repl.Wpf
{
    public partial class ConsoleControl : IViewFor<ConsoleControlViewModel>
    {
        private PythonConsolePad _pad;   
        
        public ConsoleControl()
        {
            ViewModel = new ConsoleControlViewModel();
            InitializeComponent();

            _pad = new PythonConsolePad();
            Grid.Children.Add(_pad.Control);

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
            _pad.Control.SyntaxHighlighting = pythonHighlighting;
            IList<IVisualLineTransformer> transformers = _pad.Control.TextArea.TextView.LineTransformers;
            var highlighter = new DocumentHighlighter(_pad.Control.Document, pythonHighlighting);
            for (int i = 0; i < transformers.Count; ++i)
            {
                if (transformers[i] is HighlightingColorizer) transformers[i] 
                    = new PythonConsoleHighlightingColorizer(highlighter, _pad.Control.Document);
            }
            
            // this.WhenActivated(d =>
            // {
            //     Observable.FromEventPattern<TextCompositionEventArgs>(TextEditor.TextArea,
            //             nameof(TextEditor.TextArea.TextEntering))
            //         .Select(x => x.EventArgs as TextCompositionEventArgs)
            //         .Subscribe(e =>
            //         {
            //             if (null == ViewModel)
            //             {
            //                 e.Handled = false;
            //                 return;
            //             }
            //             
            //             if (e.Text.Length > 0)
            //             {
            //                 if (!char.IsLetterOrDigit(e.Text[0]))
            //                 {
            //                     // TODO: Here is where we will request an autocomplete update.
            //
            //                 }
            //             }
            //
            //             if (IsInReadOnlyRegion)
            //             {
            //                 e.Handled = true;
            //             }
            //             else
            //             {
            //                 if (e.Text[0] == '\n')
            //                 {
            //                     OnEnterKeyPressed();
            //                 }
            //
            //                 if (e.Text[0] == '.' && ViewModel.AllowFullAutocompletion)
            //                 {
            //                     // TODO: Fire an interaction to show the completion window.
            //                 }
            //
            //                 if ((e.Text[0] == ' ') && (Keyboard.Modifiers == ModifierKeys.Control))
            //                 {
            //                     e.Handled = true;
            //                     // TODO: Fire an interaction to show the completion window.
            //                 }
            //             }
            //         })
            //         .DisposeWith(d);
            //
            // });
        }
        

    }
}