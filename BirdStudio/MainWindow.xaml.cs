﻿using System;
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
using System.Windows.Forms;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

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
        private string gameDirectory;
        private string replayFile;
        private string tasFile;
        private TAS tas;

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
            SetColorScheme(ColorScheme.LightMode());
            new Thread(new ThreadStart(TalkWithGame)).Start();
        }

        private void TalkWithGame()
        {
            while (true)
            {
                Message message = TcpManager.listenForMessage();
                switch(message.type)
                {
                    case "SaveReplay":
                        string levelName = (string) message.args[0];
                        string replayBuffer = (string)message.args[1];
                        int breakpoint = (int) message.args[2]; // TODO do I actually care about this?
                        OnReplaySaved(levelName, replayBuffer, breakpoint);
                        break;
                    case "Frame":
                        int frame = (int) message.args[0];
                        float pos_x = (float) message.args[1];
                        float pos_y = (float) message.args[2];
                        float vel_x = (float) message.args[3];
                        float vel_y = (float) message.args[4];
                        // TODO
                        break;
                }
            }
        }

        private void OnReplaySaved(string levelName, string replayBuffer, int breakpoint)
        {
            if (levelName != tas.stage) // TODO fails if tas is null
            { } // TODO update to a different level: would you like to open?
                // no // yes (save current file) // yes (discard changes to current file)
            Replay replay = new Replay(replayBuffer, breakpoint);
            List<Press> presses = replay.toPresses();
            TAS newInputs = new TAS(presses, levelName);
            App.Current.Dispatcher.Invoke((Action)delegate // need to update on main thread
            {
                if (levelName == tas.stage)
                    tas.updateInputs(newInputs);
                else
                    tas = newInputs;
                inputEditor.Text = tas.toText();
            });
        }

        private void AttemptProcessConnection()
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("TheKingsBird");
                string path = processes.First().MainModule.FileName;
                int i = path.LastIndexOf('\\');
                gameDirectory = path.Substring(0, i + 1);
            }
            catch
            {
                gameDirectory = null;
            }
            UpdateReplayFile();
        }

        private void UpdateReplayFile()
        {
            replayFile = null;
            if ((gameDirectory == null) ||
                (tas == null) ||
                (tas.stage == null))
            {
                return;
            }
            string replayFolder = gameDirectory + @"Replays\";
            try
            {
                Directory.CreateDirectory(replayFolder);
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Unable to create directory '" + replayFolder + "'");
            }
            replayFile = gameDirectory + @"Replays\" + tas.stage + ".txt";
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
            inputEditor.TextArea.TextView.BackgroundRenderers.Add(new LineHighlighter(inputEditor, cs.activeLine));
        }

        private void Menu_LightMode(object sender, RoutedEventArgs e)
        {
            SetColorScheme(ColorScheme.LightMode());
        }

        private void Menu_DarkMode(object sender, RoutedEventArgs e)
        {
            SetColorScheme(ColorScheme.DarkMode());
        }

        private void Editor_TextChanged(object sender, System.EventArgs e)
        {
            tas = new TAS(inputEditor.Text.Split('\n').ToList());
            // inputEditor.Document.Text = string.Join('\n', tas.lines);
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

        private void OpenCommand_Execute(object sender, RoutedEventArgs e)
        {
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

            if (file.EndsWith(".tas"))
            {
                // tas file
                string[] lines = System.IO.File.ReadAllLines(file);
                tas = new TAS(lines.ToList());
                tasFile = file;
            }
            else
            {
                // replay file
                Replay replay = new Replay(file);
                List<Press> presses = replay.toPresses();
                string stage = filePathToNameOnly(file);
                tas = new TAS(presses, stage);
                tasFile = null;
            }
            inputEditor.Text = string.Join('\n', tas.toText());
            // TODO this is sloppy
            // it will cause TextChanged to fire, creating a 2nd TAS object for no reason
            UpdateReplayFile();
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
            System.IO.File.WriteAllText(file, inputEditor.Text);
            tasFile = file;
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
            if (gameDirectory == null)
                AttemptProcessConnection();
            e.CanExecute = (replayFile != null);
        }

        private void _watch(int breakpoint)
        {
            List<Press> presses = tas.toPresses();
            Replay replay = new Replay(presses, breakpoint);
            string replayBuffer = replay.writeString();
            TcpManager.sendLoadReplayCommand(tas.stage, replayBuffer, breakpoint);
        }

        private void WatchFromStart_Execute(object sender, RoutedEventArgs e)
        {
            _watch(0);
        }

        private void WatchToCursor_Execute(object sender, RoutedEventArgs e)
        {
            _watch(tas.startingFrameForLine(inputEditor.TextArea.Caret.Line - 1));
        }
    }
}
