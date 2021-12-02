using System.Windows.Media;
using System.Collections.Generic;
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
        public Dictionary<string, SolidColorBrush> resources { get; set; }

        public static ColorScheme LightMode()
        {
            ColorScheme c = new ColorScheme
            {
                background = Brushes.White,
                defaultText = Brushes.Gray,
                comment = new SimpleHighlightingBrush(Color.FromRgb(0, 0x80, 0)),
                frame = new SimpleHighlightingBrush(Color.FromRgb(0xFF, 0, 0)),
                input = new SimpleHighlightingBrush(Color.FromRgb(0, 0, 0xFF)),
                activeLine = Brushes.Gainsboro,
                playbackLine = new SolidColorBrush(Color.FromRgb(0xFD, 0xD8, 0x98)),
                playbackFrame = Brushes.Black,
                resources = new Dictionary<string, SolidColorBrush>()
            };

            c.resources["Menu.Static.Background"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
            c.resources["Menu.Static.Border"] = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x99, 0x99));
            c.resources["Menu.Static.Foreground"] = new SolidColorBrush(Color.FromArgb(0xFF, 0x21, 0x21, 0x21));
            c.resources["Menu.Static.Separator"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xD7, 0xD7, 0xD7));
            c.resources["Menu.Disabled.Foreground"] = new SolidColorBrush(Color.FromArgb(0xFF, 0x70, 0x70, 0x70));
            c.resources["MenuItem.Selected.Background"] = new SolidColorBrush(Color.FromArgb(0x3D, 0x26, 0xA0, 0xDA));
            c.resources["MenuItem.Selected.Border"] = new SolidColorBrush(Color.FromArgb(0xFF, 0x26, 0xA0, 0xDA));
            c.resources["MenuItem.Highlight.Background"] = c.resources["MenuItem.Selected.Background"];
            c.resources["MenuItem.Highlight.Border"] = c.resources["MenuItem.Selected.Border"];
            c.resources["MenuItem.Highlight.Disabled.Background"] = new SolidColorBrush(Color.FromArgb(0x0A, 0x00, 0x00, 0x00));
            c.resources["MenuItem.Highlight.Disabled.Border"] = new SolidColorBrush(Color.FromArgb(0x21, 0x00, 0x00, 0x00));
            c.resources["TextBlock.Background"] = Brushes.White;
            c.resources["TextBlock.Foreground"] = Brushes.Black;

            return c;
        }

        public static ColorScheme DarkMode()
        {
            ColorScheme c = new ColorScheme
            {
                background = new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x1A)),
                defaultText = new SolidColorBrush(Color.FromRgb(0xB0, 0xB0, 0xB0)),
                comment = new SimpleHighlightingBrush(Color.FromRgb(0x18, 0xA0, 0x30)),
                frame = new SimpleHighlightingBrush(Color.FromRgb(0xE0, 0x60, 0x40)),
                input = new SimpleHighlightingBrush(Color.FromRgb(0x0A, 0xA0, 0xE0)),
                activeLine = new SolidColorBrush(Color.FromRgb(0x38, 0x38, 0x38)),
                playbackLine = new SolidColorBrush(Color.FromRgb(0x55, 0x48, 0)),
                playbackFrame = Brushes.Orange,
                resources = new Dictionary<string, SolidColorBrush>()
            };

            c.resources["Menu.Static.Background"] = new SolidColorBrush(Color.FromArgb(0xFF, 0x1A, 0x1A, 0x1A));
            c.resources["Menu.Static.Border"] = new SolidColorBrush(Color.FromArgb(0xFF, 0x60, 0x60, 0x60));
            c.resources["Menu.Static.Foreground"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xD0, 0xD0, 0xD0));
            c.resources["Menu.Static.Separator"] = new SolidColorBrush(Color.FromArgb(0xFF, 0x40, 0x40, 0x40));
            c.resources["Menu.Disabled.Foreground"] = new SolidColorBrush(Color.FromArgb(0xFF, 0x70, 0x70, 0x70));
            c.resources["MenuItem.Selected.Background"] = new SolidColorBrush(Color.FromArgb(0x3D, 0x80, 0x80, 0x80));
            c.resources["MenuItem.Selected.Border"] = new SolidColorBrush(Color.FromArgb(0xFF, 0x80, 0x80, 0x80));
            c.resources["MenuItem.Highlight.Background"] = c.resources["MenuItem.Selected.Background"];
            c.resources["MenuItem.Highlight.Border"] = c.resources["MenuItem.Selected.Border"];
            c.resources["MenuItem.Highlight.Disabled.Background"] = new SolidColorBrush(Color.FromArgb(0x0A, 0x00, 0x00, 0x00));
            c.resources["MenuItem.Highlight.Disabled.Border"] = new SolidColorBrush(Color.FromArgb(0x21, 0x00, 0x00, 0x00));

            // are these ever used?
            c.resources["MenuItem.Highlight.Disabled.Background"] = new SolidColorBrush(Colors.Magenta);
            c.resources["MenuItem.Highlight.Disabled.Border"] = new SolidColorBrush(Colors.Magenta);

            c.resources["TextBlock.Background"] = new SolidColorBrush(Color.FromArgb(0xFF, 0x1A, 0x1A, 0x1A));
            c.resources["TextBlock.Foreground"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xD0, 0xD0, 0xD0));

            return c;
        }
    }
}
