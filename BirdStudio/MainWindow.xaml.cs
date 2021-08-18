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
using System.Windows.Forms;
using System.Diagnostics;

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
            inputEditor.Text = string.Join('\n', tas.lines);
            // TODO this is sloppy
            // it will cause TextChanged to fire, creating a 2nd TAS object for no reason
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
            AttemptProcessConnection();
            e.CanExecute =
                (gameDirectory != null) &&
                (tas != null) &&
                (tas.stage != null);
        }

        private void _watch(int breakpoint)
        {
            List<Press> presses = tas.toPresses();
            Replay replay = new Replay(presses, breakpoint);
            try
            {
                Directory.CreateDirectory(gameDirectory + @"Replays\");
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Unable to create directory '" + gameDirectory + @"Replays\'");
            }
            replay.writeFile(gameDirectory + @"Replays\" + tas.stage + ".txt");
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
