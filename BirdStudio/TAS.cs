using System;
using System.Collections.Generic;
using System.Drawing;
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
        private string _stage;
        public string stage { get => _stage; }

        public TAS(List<string> lines)
        {
            this.lines = lines;
            _obtainStage();
        }

        public TAS(List<Press> presses, string stage)
        {
            lines = new List<string>();
            _stage = stage;
            lines.Add(">stage " + stage);
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

        private void _obtainStage()
        {
            _stage = null;
            foreach (string l in lines)
            {
                string line = l.Trim();
                if (line.StartsWith(">stage "))
                    _stage = line.Substring(7);
            }
        }

        private static string _tasLine(int frames, HashSet<char> buttons)
        {
            if (buttons.Count == 0)
                return String.Format("{0,4}", frames);
            const string order = "RLUDJXGCQMN";
            List<char> orderedButtons = new List<char>();
            foreach (char c in order)
                if (buttons.Contains(c))
                    orderedButtons.Add(c);
            string buttonsStr = string.Join(",", orderedButtons);
            if (buttonsStr == "")
                return String.Format("{0,4}", frames);
            else
                return String.Format("{0,4},{1}", frames, buttonsStr);
        }

        public string toText()
        {
            return string.Join('\n', lines);
        }

        // TODO kinda inefficient to reparse these lines for every call to locateFrame
        // would be better to just save the parsed lines
        private static InputLine _toInputLine(string line)
        {
            line = line.Trim();
            if (line == "" || line.StartsWith('#') || line.StartsWith('>'))
                return null;
            int split = line.LastIndexOfAny("0123456789".ToCharArray()) + 1;
            // If no frame number is found, split will be at 0.
            string frames = line.Substring(0, split);
            string buttons = line.Substring(split).ToUpper();
            buttons = string.Join("", buttons.Split(','));
            foreach (char c in buttons)
                if (!Char.IsLetter(c))
                    return null;
            if (split == 0)
                return new InputLine(0, buttons);
            try
            {
                return new InputLine(Int32.Parse(frames), buttons);
            }
            catch { }
            return null;
        }

        private static bool _isInputLine(string line)
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

        public void incrementRerecords()
        {
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith(">rerecords "))
                {
                    int rerecords;
                    if (int.TryParse(line.Substring(10).Trim(), out rerecords))
                        lines[i] = ">rerecords " + (rerecords + 1);
                }
            }
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

        public int endingFrameForLine(int lineNumber)
        {
            if (lineNumber >= lines.Count())
                return -1;
            int frame = 0;
            for (int i = 0; i <= lineNumber; i++)
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

        private Point _replaceLine(int lineNumber, string oldText, string newText, int caret)
        {
            InputLine inputLine = _toInputLine(newText);
            if (inputLine == null)
            {
                lines[lineNumber] = newText;
                if (oldText.StartsWith(">stage ") || newText.StartsWith(">stage "))
                    _obtainStage();
                return new Point(lineNumber, caret);
            }
            HashSet<char> buttons = new HashSet<char>();
            foreach (char c in inputLine.buttons)
            {
                if (buttons.Contains(c))
                    buttons.Remove(c);
                else
                    buttons.Add(c);
            }
            string reformattedText = _tasLine(inputLine.frames, buttons);
            lines[lineNumber] = reformattedText;

            // Calculate new caret position, based on where caret appeared relative to the frame number
            int newCaret = 0;
            while (newCaret < reformattedText.Length && " 0".Contains(reformattedText[newCaret]))
                newCaret++;
            int i = 0;
            while (i < newText.Length && " 0".Contains(newText[i]))
                i++;
            for (; i < caret; i++)
                if (Char.IsDigit(newText[i]))
                    newCaret++;
                else
                    break;
            return new Point(lineNumber, newCaret);
        }

        // update the text, and return a Point describing where to place the caret
        public Point insertText(int lineNumber, int column, string textToInsert)
        {
            string oldText = lines[lineNumber];
            if (textToInsert.Contains("\n"))
            {
                if (_isInputLine(oldText))
                {
                    if (oldText.Substring(0, column).Trim() == "")
                        lines.Insert(lineNumber, "");
                    else
                        lines.Insert(lineNumber + 1, "");
                    return new Point(lineNumber + 1, 0);
                }
                else
                {
                    lines.Insert(lineNumber + 1, oldText.Substring(column));
                    lines[lineNumber] = oldText.Substring(0, column);
                    return new Point(lineNumber + 1, 0);
                }
            }
            if (_isInputLine(oldText))
            {
                if (textToInsert[0] != '#')
                {
                    // Unless we're typing numbers in the middle of the number, default cursor to position 4
                    int endOfNumbers = 0;
                    for (; endOfNumbers < oldText.Length; endOfNumbers++)
                        if (!" \t0123456789".Contains(oldText[endOfNumbers]))
                            break;
                    if (!Char.IsDigit(textToInsert[0]) || column > endOfNumbers)
                        column = endOfNumbers;
                }
            }
            string newText = oldText.Insert(column, textToInsert);
            return _replaceLine(lineNumber, oldText, newText, column + textToInsert.Length);
        }

        public Point removeText(int startLine, int startCol, int endLine, int endCol, bool reformat)
        {
            string oldText = lines[startLine];
            string newText = lines[startLine].Substring(0, startCol) + lines[endLine].Substring(endCol);
            while (endLine > startLine && startLine + 1 < lines.Count)
            {
                lines.RemoveAt(startLine + 1);
                // TODO If removing any stage lines, _obtainStage()
                endLine--;
            }
            if (reformat)
                return _replaceLine(startLine, oldText, newText, startCol);
            else
            {
                lines[startLine] = newText;
                return new Point(startLine, startCol);
            }
        }

        public Point reformatLine(int lineNumber, int caret)
        {
            string oldText = lines[lineNumber];
            if (_isInputLine(oldText))
                return _replaceLine(lineNumber, oldText, oldText, caret);
            return new Point(-1, -1);
        }

        public void commentBlock(int startLine, int endLine)
        {
            bool uncomment = true;
            for (int i = startLine; i <= endLine; i++)
                if (!lines[i].Trim().StartsWith('#'))
                    uncomment = false;
            if (uncomment)
            {
                for (int i = startLine; i <= endLine; i++)
                {
                    int commentStart = lines[i].IndexOf('#');
                    lines[i] = lines[i].Substring(commentStart + 1);
                }
            }
            else
            {
                for (int i = startLine; i <= endLine; i++)
                    lines[i] = "#" + lines[i];
            }
        }
    }
}
