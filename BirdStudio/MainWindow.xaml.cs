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

            inputEditor.Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180));
            inputEditor.Background = new SolidColorBrush(Color.FromRgb(26, 26, 26));
            inputEditor.Text = @"#a
console load 1
   1

#Start
#lvl_1
  88
  10,R,X
   7,R,J
   1,R
   4,R,K,G
   1,R
  16,R,U,X
   3,R,J,G
   7,R
   4,D,X
   1,R,J
   6,R
  13,R,J
   3,R,K,G
  15,U,X
   3,L,J,G
  40";
        }

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            inputEditor.TextArea.TextView.BackgroundRenderers.Add(new LineHighlighter(inputEditor));

            var highlighting = inputEditor.SyntaxHighlighting;
            var commentHighlighting = highlighting.NamedHighlightingColors.First(c => c.Name == "Comment");
            commentHighlighting.Foreground = new SimpleHighlightingBrush(Color.FromRgb(0xC0, 0xC0, 0xC0));
            inputEditor.SyntaxHighlighting = highlighting;
        }
    }
}
