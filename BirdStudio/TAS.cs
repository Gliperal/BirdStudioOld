using System;
using System.Collections.Generic;
using System.Linq;

namespace BirdStudio
{
    public class TAS
    {
        public class InputLine
        {
            public int frames;
            public string buttons;

            public InputLine(int frames, string buttons)
            {
                this.frames = frames;
                this.buttons = buttons;
            }
        }

        private List<string> lines;
        public string stage { get; }

        public TAS(List<string> lines)
        {
            this.lines = lines;
            foreach (string l in lines)
            {
                string line = l.Trim();
                if (line.StartsWith("stage "))
                    stage = line.Substring(6);
            }
        }

        public TAS(List<Press> presses, string stage)
        {
            lines = new List<string>();
            this.stage = stage;
            lines.Add("stage " + stage);
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

        // TODO kinda inefficient to reparse these lines for every call to locateFrame
        // would be better to just save the parsed lines
        private InputLine _toInputLine(string line)
        {
            line = line.Trim();
            if (line == "" || line.StartsWith('#'))
                return null;
            string frames;
            string buttons;
            if (line.Contains(','))
            {
                string[] s = line.Split(',', 2);
                frames = s[0];
                buttons = string.Join("", s[1].ToUpper().Split(','));
            }
            else
            {
                // frame with no inputs
                frames = line;
                buttons = "";
            }
            // inputs with no frame
            if (frames == "")
                return new InputLine(0, buttons);
            try
            {
                return new InputLine(Int32.Parse(frames), buttons);
            }
            catch { }
            return null;
        }

        private bool _isInputLine(string line)
        {
            return _toInputLine(line) != null;
        }

        public List<Press> toPresses()
        {
            List<Press> presses = new List<Press>();
            HashSet<char> state = new HashSet<char>();
            int frame = 0;
            foreach (string line in lines)
            {
                InputLine inputLine = _toInputLine(line);
                if (inputLine == null)
                    continue;
                foreach (char button in inputLine.buttons)
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
                    if (!inputLine.buttons.Contains(button))
                    {
                        presses.Add(new Press
                        {
                            frame = frame,
                            button = button,
                            on = false
                        });
                        state.Remove(button);
                    }
                frame += inputLine.frames;
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
                InputLine inputLine = _toInputLine(line);
                if (inputLine == null)
                    continue;
                frame += inputLine.frames;
            }
            return frame;
        }

        public int[] locateFrame(int frame)
        {
            int lineStartFrame = 0;
            int lineEndFrame = 0;
            for (int i = 0; i < lines.Count(); i++)
            {
                lineStartFrame = lineEndFrame;
                string line = lines[i].Trim();
                InputLine inputLine = _toInputLine(line);
                if (inputLine == null)
                    continue;
                lineEndFrame += inputLine.frames;
                if (frame <= lineEndFrame)
                    return new int[] { i, frame - lineStartFrame };
            }
            return new int[] { lines.Count() - 1, frame - lineStartFrame };
        }
    }
}
