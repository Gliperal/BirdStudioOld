using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirdStudio
{
    public class TAS
    {
        private List<string> lines;
        public string stage { get; }

        public TAS(List<string> _lines)
        {
            lines = _lines;
            foreach (string l in lines)
            {
                string line = l.Trim();
                if (line.StartsWith("stage "))
                    stage = line.Substring(6);
            }
        }

        public TAS(List<Press> presses, string _stage)
        {
            lines = new List<string>();
            stage = _stage;
            lines.Add("stage " + _stage);
            lines.Add("");
            presses.Sort(Press.compareFrames);
            HashSet<char> state = new HashSet<char>();
            int frame = 0;
            foreach (Press press in presses)
            {
                if (press.frame > frame)
                {
                    lines.Add(_tasLine(press.frame - frame, state));
                    frame = press.frame;
                }
                if (press.on)
                    state.Add(press.button);
                else
                    state.Remove(press.button);
            }
            lines.Add(_tasLine(1, state));
        }

        private static string _tasLine(int frames, HashSet<char> buttons)
        {
            if (buttons.Count == 0)
                return String.Format("{0,4}", frames);
            const string order = "RLUDJXGCQ";
            List<char> orderedButtons = new List<char>();
            foreach (char c in order)
                if (buttons.Contains(c))
                    orderedButtons.Add(c);
            string buttonsStr = string.Join(",", orderedButtons);
            return String.Format("{0,4},{1}", frames, buttonsStr);
        }

        public string toText()
        {
            return string.Join('\n', lines);
        }

        private static bool _isInputLine(string line)
        {
            if (line == "" || line.StartsWith('#') || line.StartsWith("stage "))
                return false;
            return true;
        }

        public List<Press> toPresses()
        {
            List<Press> presses = new List<Press>();
            HashSet<char> state = new HashSet<char>();
            int frame = 0;
            foreach (string l in lines)
            {
                string line = l.Trim();
                if (!_isInputLine(line))
                    continue;
                int frames;
                string buttons;
                if (line.Contains(','))
                {
                    string[] s = line.Split(',', 2);
                    frames = Int32.Parse(s[0]);
                    buttons = string.Join("", s[1].ToUpper().Split(','));
                }
                else
                {
                    frames = Int32.Parse(line);
                    buttons = "";
                }
                foreach (char button in buttons)
                    if (!state.Contains(button))
                    {
                        presses.Add(new Press
                        {
                            frame = frame,
                            button = button,
                            on = true
                        });
                        state.Add(button);
                    }
                foreach (char button in state)
                    if (!buttons.Contains(button))
                    {
                        presses.Add(new Press
                        {
                            frame = frame,
                            button = button,
                            on = false
                        });
                        state.Remove(button);
                    }
                frame += frames;
            }
            return presses;
        }

        public void updateInputs(TAS newInputs)
        {
            List<string> newLines = new List<string>();
            List<string> newInputLines = newInputs.lines.Where(line => _isInputLine(line)).ToList();
            int oldIndex = 0;
            int newIndex = 0;
            for (; oldIndex < lines.Count && newIndex < newInputLines.Count ; oldIndex++)
            {
                string oldLine = lines[oldIndex];
                string newLine = newInputLines[newIndex];
                if (!_isInputLine(oldLine)) // copy comments from old tas
                    newLines.Add(oldLine);
                else if (oldLine == newLine) // copy inputs as long as they're the same
                {
                    newLines.Add(oldLine);
                    newIndex++;
                }
                else
                    break;
            }
            // as soon as the inputs diverge:
            // finish copying new inputs
            for (; newIndex < newInputLines.Count; newIndex++)
                newLines.Add(newInputLines[newIndex]);
            // finish copying old comments
            for (; oldIndex < lines.Count; oldIndex++)
                if (!_isInputLine(lines[oldIndex]))
                    newLines.Add(lines[oldIndex]);
            lines = newLines;
        }

        public int startingFrameForLine(int lineNumber)
        {
            if (lineNumber >= lines.Count())
                return -1;
            int frame = 0;
            for (int i = 0; i < lineNumber; i++)
            {
                string line = lines[i].Trim();
                if (!_isInputLine(line))
                    continue;
                string[] s = line.Split(',');
                frame += Int32.Parse(s[0]);
            }
            return frame;
        }
    }
}
