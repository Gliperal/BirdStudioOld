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

public class LineHighlighter : ICSharpCode.AvalonEdit.Rendering.IBackgroundRenderer
{
    private ICSharpCode.AvalonEdit.TextEditor _editor;

    public LineHighlighter(ICSharpCode.AvalonEdit.TextEditor editor)
    {
        _editor = editor;
    }

    public ICSharpCode.AvalonEdit.Rendering.KnownLayer Layer
    {
        get
        {
            // draw behind selection
            return ICSharpCode.AvalonEdit.Rendering.KnownLayer.Selection;
        }
    }

    public void Draw(ICSharpCode.AvalonEdit.Rendering.TextView textView, DrawingContext drawingContext)
    {
        if (textView == null)
            throw new ArgumentNullException("textView");
        if (drawingContext == null)
            throw new ArgumentNullException("drawingContext");
        if (_editor.Document == null)
            return;

        textView.EnsureVisualLines();

        // TODO if multiple lines selected?
        var currentLine = _editor.Document.GetLineByOffset(_editor.CaretOffset);
        foreach (var rect in ICSharpCode.AvalonEdit.Rendering.BackgroundGeometryBuilder.GetRectsForSegment(textView, currentLine))
        {
            drawingContext.DrawRectangle(
                new SolidColorBrush(Color.FromArgb(0x40, 0, 0, 0xFF)), null,
                new Rect(rect.Location, new Size(textView.ActualWidth - 32, rect.Height)));
        }

        //ICSharpCode.AvalonEdit.Rendering.VisualLine line = textView.VisualLines[9];
        //Rect r = new Rect();
        //r.X = 0;
        //r.Y = line.VisualTop;
        //r.Width = 100;
        //r.Height = line.Height;
        //drawingContext.DrawRectangle(Brushes.Red, null, r);
    }
}

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
        }

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
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
            inputEditor.TextArea.TextView.BackgroundRenderers.Add(new LineHighlighter(inputEditor));
        }
    }
}
