using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;

namespace BirdStudio
{
    public class ColorScheme
    {
        public Brush background { get; set; }
        public Brush defaultText { get; set; }
        public HighlightingBrush comment { get; set; }
        public HighlightingBrush frame { get; set; }
        public HighlightingBrush input { get; set; }
        public Brush activeLine { get; set; }
        public Brush playbackLine { get; set; }
        public Brush playbackFrame { get; set; }

        public static ColorScheme LightMode()
        {
            return new ColorScheme
            {
                background = Brushes.White,
                defaultText = Brushes.Gray,
                comment = new SimpleHighlightingBrush(Color.FromRgb(0, 0x80, 0)),
                frame = new SimpleHighlightingBrush(Color.FromRgb(0xFF, 0, 0)),
                input = new SimpleHighlightingBrush(Color.FromRgb(0, 0, 0xFF)),
                activeLine = Brushes.Gainsboro,
                playbackLine = new SolidColorBrush(Color.FromRgb(0xFD, 0xD8, 0x98)),
                playbackFrame = Brushes.Black
            };
        }

        public static ColorScheme DarkMode()
        {
            return new ColorScheme
            {
                background = new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x1A)),
                defaultText = new SolidColorBrush(Color.FromRgb(0xB0, 0xB0, 0xB0)),
                comment = new SimpleHighlightingBrush(Color.FromRgb(0x18, 0xA0, 0x30)),
                frame = new SimpleHighlightingBrush(Color.FromRgb(0xE0, 0x60, 0x40)),
                input = new SimpleHighlightingBrush(Color.FromRgb(0x0A, 0xA0, 0xE0)),
                activeLine = new SolidColorBrush(Color.FromRgb(0x38, 0x38, 0x38)),
                playbackLine = new SolidColorBrush(Color.FromRgb(0x55, 0x48, 0)),
                playbackFrame = Brushes.Orange
            };
        }
    }
}
