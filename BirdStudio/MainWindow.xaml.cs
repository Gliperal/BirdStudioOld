﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

using System.IO;
using System.Reflection;
using System.Xml;

using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Document;

namespace BirdStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string DEFAULT_FILE_TEXT = ">stage Twin Tree Village\n>rerecords 0\n\n  29";

        private LineHighlighter bgRenderer;
        private string tasFile;
        private TAS tas;
        private int currentFrame = -1;
        private bool tasEditedSinceLastWatch = true;

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
            if (UserPreferences.get("dark mode", "false") == "true")
            {
                SetColorScheme(ColorScheme.DarkMode());
                darkModeMenuItem.IsChecked = true;
            }
            else
            {
                SetColorScheme(ColorScheme.LightMode());
            }
            if (UserPreferences.get("show help", "false") == "true")
                buttonHelper.Visibility = Visibility.Visible;
            new Thread(new ThreadStart(TalkWithGame)).Start();
            inputEditor.TextArea.TextEntering += Editor_TextEntering;
            inputEditor.TextArea.PreviewKeyDown += Editor_KeyDown;
            inputEditor.Options.AllowScrollBelowDocument = true;
            NewCommand_Execute(null, null);

            // TODO custom keybindings
            // playPauseCommandBinding.Command = new RoutedUICommand(
            //     "Play / Pause",
            //     "PlayPause",
            //     typeof(Commands.CustomCommands),
            //     new InputGestureCollection()
            //     {
            //         new KeyGesture(Key.OemMinus)
            //     }
            // );
        }

        private void TalkWithGame()
        {
            while (true)
            {
                Message message = TcpManager.listenForMessage();
                if (message == null)
                    continue;
                switch(message.type)
                {
                    case "SaveReplay":
                        string levelName = (string) message.args[0];
                        string replayBuffer = (string) message.args[1];
                        int breakpoint = (int) message.args[2]; // TODO do I actually care about this?
                        OnReplaySaved(levelName, replayBuffer, breakpoint);
                        break;
                    case "Frame":
                        currentFrame = (int) message.args[0];
                        ShowPlaybackFrame(); // TODO when to set playback frame back to -1?
                        float pos_x = (float) message.args[1];
                        float pos_y = (float) message.args[2];
                        float vel_x = (float) message.args[3];
                        float vel_y = (float) message.args[4];
                        // TODO
                        break;
                    default:
                        inputEditor.Text = message.type;
                        break;
                }
            }
        }

        private void OnReplaySaved(string levelName, string replayBuffer, int breakpoint)
        {
            if (levelName != tas.stage)
            { } // TODO update to a different level: would you like to open?
                // no // yes (save current file) // yes (discard changes to current file)
            Replay replay;
            try
            {
                replay = new Replay(replayBuffer, false);
            }
            catch (FormatException e) { return; }
            List<Press> presses = replay.toPresses();
            TAS newInputs = new TAS(presses, levelName);
            App.Current.Dispatcher.Invoke((Action)delegate // need to update on main thread
            {
                if (levelName == tas.stage)
                    tas.updateInputs(newInputs);
                else
                    tas = newInputs;
                tas.incrementRerecords();
                OnTasEdited();
            });
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
            bgRenderer = new LineHighlighter(inputEditor, cs.activeLine, cs.playbackLine, cs.playbackFrame);
            inputEditor.TextArea.TextView.BackgroundRenderers.Add(bgRenderer);

            foreach (KeyValuePair<string, SolidColorBrush> kvp in cs.resources)
                Resources[kvp.Key] = kvp.Value;

            ShowPlaybackFrame();
        }

        private void ShowPlaybackFrame()
        {
            if (currentFrame != -1)
            {
                int[] frameLocation = tas.locateFrame(currentFrame);
                bgRenderer.ShowActiveFrame(frameLocation[0], frameLocation[1]);
                App.Current.Dispatcher.Invoke((Action)delegate // need to update on main thread
                {
                    inputEditor.TextArea.TextView.Redraw();
                });
            }
        }

        private void Menu_ToggleHelp(object sender, RoutedEventArgs e)
        {
            if (buttonHelper.Visibility == Visibility.Visible)
            {
                buttonHelper.Visibility = Visibility.Collapsed;
                UserPreferences.set("show help", "false");
            }
            else
            {
                buttonHelper.Visibility = Visibility.Visible;
                UserPreferences.set("show help", "true");
            }
        }

        private void Menu_LightMode(object sender, RoutedEventArgs e)
        {
            UserPreferences.set("dark mode", "false");
            SetColorScheme(ColorScheme.LightMode());
        }

        private void Menu_DarkMode(object sender, RoutedEventArgs e)
        {
            UserPreferences.set("dark mode", "true");
            SetColorScheme(ColorScheme.DarkMode());
        }

        private void OnTasEdited()
        {
            if (!tasEditedSinceLastWatch)
            {
                tas.incrementRerecords();
                tasEditedSinceLastWatch = true;
            }
            inputEditor.Document.Replace(
                new TextSegment { StartOffset = 0, EndOffset = inputEditor.Text.Length },
                tas.toText()
            );
            ShowPlaybackFrame();
        }

        private void OnTasEdited(System.Drawing.Point caretPos)
        {
            OnTasEdited();
            DocumentLine caretLine = inputEditor.Document.GetLineByNumber(caretPos.X + 1);
            inputEditor.CaretOffset = caretLine.Offset + caretPos.Y;
            inputEditor.TextArea.Caret.BringCaretToView();
        }

        private void Editor_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                int deletePos = inputEditor.SelectionStart;
                int deleteLength = inputEditor.SelectionLength;
                if (deleteLength == 0)
                {
                    deleteLength = 1;
                    if (e.Key == Key.Back)
                        deletePos--;
                }
                // Ingore backspace at beginning of text or delete at end of text
                if (
                    deletePos < 0 ||
                    deletePos + deleteLength > inputEditor.Document.TextLength
                )
                {
                    e.Handled = true;
                    return;
                }
                TextLocation deleteStart = inputEditor.Document.GetLocation(deletePos);
                TextLocation deleteEnd = inputEditor.Document.GetLocation(deletePos + deleteLength);
                System.Drawing.Point caretPos = tas.removeText(deleteStart.Line - 1, deleteStart.Column - 1, deleteEnd.Line - 1, deleteEnd.Column - 1, true);
                OnTasEdited(caretPos);
                e.Handled = true;
            }
        }

        private void Editor_TextEntering(object sender, TextCompositionEventArgs e)
        {
            DocumentLine line = inputEditor.Document.GetLineByOffset(inputEditor.CaretOffset);
            int insertAt = inputEditor.Document.GetLocation(inputEditor.CaretOffset).Column - 1;
            if (inputEditor.SelectionLength > 0)
            {
                int deletePos = inputEditor.SelectionStart;
                int deleteLength = inputEditor.SelectionLength;
                TextLocation deleteStart = inputEditor.Document.GetLocation(deletePos);
                TextLocation deleteEnd = inputEditor.Document.GetLocation(deletePos + deleteLength);
                tas.removeText(deleteStart.Line - 1, deleteStart.Column - 1, deleteEnd.Line - 1, deleteEnd.Column - 1, false);
                line = inputEditor.Document.GetLineByOffset(deletePos);
                insertAt = inputEditor.Document.GetLocation(deletePos).Column - 1;
            }
            System.Drawing.Point caretPos = tas.insertText(line.LineNumber - 1, insertAt, e.Text);
            OnTasEdited(caretPos);
            e.Handled = true;
        }

        private void Editor_TextChanged(object sender, System.EventArgs e)
        {
            // any text changes that aren't caught by the above:
            //     undo/redo events
            //     opening tas files
            //     pasting from clipboard
            // should force a reload of the tas file

            // Kind of annoying that this also triggers for every regular
            // modification of the tas file as well, but there's no real good
            // way that I can think to fix that. :/
            tas = new TAS(inputEditor.Text.Split('\n').ToList());
            ShowPlaybackFrame();
        }

        private string filePathToFileName(string path)
        {
            return path.Split('\\').Last();
        }

        private string filePathToNameOnly(string path)
        {
            string name = path.Split('\\').Last();
            int i = name.LastIndexOf('.');
            if (i == -1)
                return name;
            else
                return name.Substring(0, i);
        }

        private void _setTasFile(string filepath)
        {
            if (filepath == null)
                Title = "Bird Studio";
            else
                Title = "Bird Studio - " + filePathToFileName(filepath);
            tasFile = filepath;
        }

        private void NewCommand_Execute(object sender, RoutedEventArgs e)
        {
            // TODO do you want to save your edits to the current file?
            tas = new TAS(DEFAULT_FILE_TEXT.Split('\n').ToList());
            _setTasFile(null);
            tasEditedSinceLastWatch = true;
            // set text and clear the undo stack
            inputEditor.Text = DEFAULT_FILE_TEXT;
        }

        private void OpenCommand_Execute(object sender, RoutedEventArgs e)
        {
            string gameDirectory = null;
            try
            {
                Process[] processes = Process.GetProcessesByName("TheKingsBird");
                string path = processes.First().MainModule.FileName;
                int i = path.LastIndexOf('\\');
                gameDirectory = path.Substring(0, i + 1);
            }
            catch {}

            string file;
            using (OpenFileDialog openFileDialogue = new OpenFileDialog())
            {
                if (gameDirectory != null && File.Exists(gameDirectory + @"Replays\"))
                    openFileDialogue.InitialDirectory = gameDirectory + @"Replays\";
                else if (gameDirectory != null)
                    openFileDialogue.InitialDirectory = gameDirectory;
                openFileDialogue.Filter = "TAS files (*.tas)|*.tas|Replay files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialogue.RestoreDirectory = true;
                if (openFileDialogue.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    file = openFileDialogue.FileName;
                else
                    return;
            }

            // TODO handle file IO exceptions
            bool fileIsReplay = false;
            if (!file.EndsWith(".tas"))
            {
                // replay file
                try
                {
                    Replay replay = new Replay(file);
                    List<Press> presses = replay.toPresses();
                    string stage = filePathToNameOnly(file);
                    tas = new TAS(presses, stage);
                    _setTasFile(null);
                    fileIsReplay = true;
                }
                catch (FormatException ex) {}
            }
            if (!fileIsReplay)
            {
                // tas file
                string[] lines = System.IO.File.ReadAllLines(file);
                tas = new TAS(lines.ToList());
                _setTasFile(file);
            }
            inputEditor.Text = string.Join('\n', tas.toText());
        }

        private void _saveAs(string file)
        {
            if (file == null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "TAS file (*.tas)|*.tas";
                saveFileDialog.ShowDialog();
                if (saveFileDialog.FileName == "")
                    return;
                file = saveFileDialog.FileName;
            }
            // TODO handle fle IO errors
            File.WriteAllText(file, inputEditor.Text);
            _setTasFile(file);
        }

        private void SaveAsCommand_Execute(object sender, RoutedEventArgs e)
        {
            _saveAs(null);
        }

        private void SaveCommand_Execute(object sender, RoutedEventArgs e)
        {
            _saveAs(tasFile);
        }

        private void WatchFromStart_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = TcpManager.isConnected() && tas.stage != null;
        }

        private void _watch(int breakpoint)
        {
            List<Press> presses = tas.toPresses();
            Replay replay = new Replay(presses);
            string replayBuffer = replay.writeString();
            TcpManager.sendLoadReplayCommand(tas.stage, replayBuffer, breakpoint);
            tasEditedSinceLastWatch = false;
        }

        private void WatchFromStart_Execute(object sender, RoutedEventArgs e)
        {
            _watch(-1);
        }

        private void WatchToCursor_Execute(object sender, RoutedEventArgs e)
        {
            _watch(tas.endingFrameForLine(inputEditor.TextArea.Caret.Line - 1));
        }

        private void PlayPause_Execute(object sender, RoutedEventArgs e)
        {
            TcpManager.sendCommand("TogglePause");
        }

        private void StepFrame_Execute(object sender, RoutedEventArgs e)
        {
            TcpManager.sendCommand("StepFrame");
        }

        private void CommentCommand_Execute(object sender, RoutedEventArgs e)
        {
            int startLine, endLine;
            if (inputEditor.SelectionLength > 0)
            {
                int pos = inputEditor.SelectionStart;
                int length = inputEditor.SelectionLength;
                TextLocation start = inputEditor.Document.GetLocation(pos);
                TextLocation end = inputEditor.Document.GetLocation(pos + length);
                startLine = start.Line - 1;
                endLine = end.Line - 1;
            }
            else
            {
                DocumentLine line = inputEditor.Document.GetLineByOffset(inputEditor.CaretOffset);
                startLine = line.LineNumber - 1;
                endLine = startLine;
            }
            tas.commentBlock(startLine, endLine);
            OnTasEdited();
        }

        private void TimestampCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = currentFrame != -1;
        }

        private void TimestampCommand_Execute(object sender, RoutedEventArgs e)
        {
            DocumentLine line = inputEditor.Document.GetLineByOffset(inputEditor.CaretOffset);
            int lineNumber = line.LineNumber - 1;
            int end = line.Length;
            string lineText = inputEditor.Text.Substring(line.Offset, line.Length);
            if (lineText.Trim() != "")
            {
                tas.insertText(lineNumber, end, "\n");
                lineNumber++;
                end = 0;
            }
            int milliseconds = (int) Math.Round((currentFrame % 48) * 1000.0 / 48.0);
            int seconds = (currentFrame / 48) % 60;
            int minutes = currentFrame / (48 * 60);
            string timestamp = String.Format("# {0} ({1}:{2:D2}.{3:D3})", currentFrame, minutes, seconds, milliseconds);
            System.Drawing.Point caretPos = tas.insertText(lineNumber, end, timestamp);
            OnTasEdited(caretPos);
        }
    }
}
