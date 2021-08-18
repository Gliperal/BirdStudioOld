using System.Windows.Input;

namespace BirdStudio.Commands
{
    public class CustomCommands
    {
        public static RoutedUICommand WatchFromStart = new RoutedUICommand(
            "Watch from Start",
            "WatchFromStart",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.W, ModifierKeys.Control)
            }
        );

        public static RoutedUICommand WatchToCursor = new RoutedUICommand(
            "Watch to Cursor",
            "WatchToCursor",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Q, ModifierKeys.Control)
            }
        );

        public static RoutedUICommand ToggleDarkMode = new RoutedUICommand(
            "Toggle Dark Mode",
            "ToggleDarkMode",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.D, ModifierKeys.Control)
            }
        );
    }
}
