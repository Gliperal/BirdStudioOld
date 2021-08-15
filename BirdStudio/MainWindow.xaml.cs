using System;
using System.Collections.Generic;
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

using System.IO;
using System.Reflection;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace BirdStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Assembly asm = Assembly.GetExecutingAssembly();
            using (Stream s = asm.GetManifestResourceStream("BirdStudio.SyntaxHighlighting.xshd"))
            {
                if (s == null)
                    throw new Exception();
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    inputEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            SetColorScheme(ColorScheme.LightMode());
        }

        private void SetColorScheme(ColorScheme cs)
        {
            inputEditor.Background = cs.background;
            inputEditor.Foreground = cs.defaultText;

            var highlighting = inputEditor.SyntaxHighlighting;
            var commentHighlighting = highlighting.NamedHighlightingColors.First(c => c.Name == "Comment");
            commentHighlighting.Foreground = cs.comment;
            var frameHighlighting = highlighting.NamedHighlightingColors.First(c => c.Name == "Frame");
            frameHighlighting.Foreground = cs.frame;
            var inputHighlighting = highlighting.NamedHighlightingColors.First(c => c.Name == "Input");
            inputHighlighting.Foreground = cs.input;
            inputEditor.SyntaxHighlighting = highlighting;

            inputEditor.TextArea.TextView.BackgroundRenderers.Clear();
            inputEditor.TextArea.TextView.BackgroundRenderers.Add(new LineHighlighter(inputEditor, cs.activeLine));
        }

        private void Menu_LightMode(object sender, RoutedEventArgs e)
        {
            SetColorScheme(ColorScheme.LightMode());
        }

        private void Menu_DarkMode(object sender, RoutedEventArgs e)
        {
            SetColorScheme(ColorScheme.DarkMode());
        }

        private void OpenCommand_Execute(object sender, RoutedEventArgs e)
        {
            Replay replay = new Replay(@"C:\Program Files (x86)\Steam\steamapps\common\The King's Bird\Replays\Twin Tree Village.txt");
            List<Press> presses = replay.toPresses();
            TAS tas = new TAS(presses);
            inputEditor.Text = string.Join('\n', tas.lines);
        }

        private void SaveCommand_Execute(object sender, RoutedEventArgs e)
        {
            ;
        }
    }
}
