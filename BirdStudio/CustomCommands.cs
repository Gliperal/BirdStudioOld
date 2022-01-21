using System.Windows.Input;

namespace BirdStudio.Commands
{
    public class CustomCommands
    {
        private static RoutedUICommand makeCommand(string text, string name, KeyGesture defaultInput)
        {
            KeyGesture input = UserPreferences.getKeyBinding(name, defaultInput);
            return new RoutedUICommand(
                text,
                name,
                typeof(CustomCommands),
                new InputGestureCollection()
                {
                    input
                }
            );
        }

        public static RoutedUICommand WatchFromStart = makeCommand(
            "Watch from Start",
            "WatchFromStart",
            new KeyGesture(Key.W, ModifierKeys.Control)
        );

        public static RoutedUICommand WatchToCursor = makeCommand(
            "Watch to Cursor",
            "WatchToCursor",
            new KeyGesture(Key.Q, ModifierKeys.Control)
        );

//        public static RoutedUICommand ToggleDarkMode = makeCommand(
//            "Toggle Dark Mode",
//            "ToggleDarkMode",
//            new KeyGesture(Key.D, ModifierKeys.Control)
//        );

        public static RoutedUICommand Comment = makeCommand(
            "Comment/Uncomment Lines",
            "ToggleComment",
            new KeyGesture(Key.OemQuestion, ModifierKeys.Control)
        );

        public static RoutedUICommand AddTimestamp = makeCommand(
            "Add Timestamp",
            "Timestamp",
            new KeyGesture(Key.T, ModifierKeys.Control)
        );

        public static RoutedUICommand StepFrame = makeCommand(
            "Frame Advance",
            "StepFrame",
            new KeyGesture(Key.OemOpenBrackets)
        );

        public static RoutedUICommand PlayPause = makeCommand(
            "Play / Pause",
            "PlayPause",
            new KeyGesture(Key.OemCloseBrackets)
        );
    }
}
