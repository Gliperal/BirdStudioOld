using System;
using System.Windows;
using System.Windows.Media;
using System.Globalization;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Document;

namespace BirdStudio
{
    public class LineHighlighter : IBackgroundRenderer
    {
        private TextEditor _editor;
        private Brush _currentLineBrush;
        private Brush _playbackLineBrush;
        private Brush _playbackFrameBrush;
        private int line = -1;
        private int frame;

        public LineHighlighter(TextEditor editor, Brush currentLine, Brush playbackLine, Brush playbackFrame)
        {
            _editor = editor;
            _currentLineBrush = currentLine;
            _playbackLineBrush = playbackLine;
            _playbackFrameBrush = playbackFrame;
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
                    _currentLineBrush, null,
                    new Rect(rect.Location, new Size(textView.ActualWidth, rect.Height)));
            }

            if (line > -1 && line < _editor.Document.LineCount)
            {
                DocumentLine docLine = _editor.Document.GetLineByNumber(line + 1);
                foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, docLine))
                {
                    drawingContext.DrawRectangle(
                        _playbackLineBrush, null,
                        new Rect(rect.Location, new Size(textView.ActualWidth, rect.Height))
                    );
                    // This is probably the wrong way to do this
                    FormattedText text = new FormattedText(
                        frame.ToString(),
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Consolas"),
                        19,
                        _playbackFrameBrush,
                        1 //TODO dafuq is pixels per DIP
                    );
                    Point origin = new Point(textView.ActualWidth - text.Width - 5, rect.Top);
                    drawingContext.DrawText(text, origin);
                }
            }
        }

        public void ShowActiveFrame(int line, int frame)
        {
            this.line = line;
            this.frame = frame;
        }
    }
}
