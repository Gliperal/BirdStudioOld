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

        public static RoutedUICommand Comment = new RoutedUICommand(
            "Comment/Uncomment Lines",
            "ToggleComment",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.OemQuestion, ModifierKeys.Control)
            }
        );

        public static RoutedUICommand AddTimestamp = new RoutedUICommand(
            "Add Timestamp",
            "Timestamp",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.T, ModifierKeys.Control)
            }
        );

        public static RoutedUICommand StepFrame = new RoutedUICommand(
            "Frame Advance",
            "StepFrame",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.OemOpenBrackets)
            }
        );

        public static RoutedUICommand PlayPause = new RoutedUICommand(
            "Play / Pause",
            "PlayPause",
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.OemCloseBrackets)
            }
        );
    }
}
