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

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;

namespace BirdStudio
{
    public class LineHighlighter : IBackgroundRenderer
    {
        private TextEditor _editor;
        private Brush _brush;

        public LineHighlighter(TextEditor editor, Brush brush)
        {
            _editor = editor;
            _brush = brush;
        }

        public KnownLayer Layer
        {
            get
            {
                // draw behind selection
                return KnownLayer.Selection;
            }
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
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
            foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, currentLine))
            {
                drawingContext.DrawRectangle(
                    _brush, null,
                    new Rect(rect.Location, new Size(textView.ActualWidth, rect.Height)));
            }

            //VisualLine line = textView.VisualLines[9];
            //Rect r = new Rect();
            //r.X = 0;
            //r.Y = line.VisualTop;
            //r.Width = 100;
            //r.Height = line.Height;
            //drawingContext.DrawRectangle(Brushes.Red, null, r);
        }
    }
}
