using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirdStudio
{
    public class TAS
    {
        public List<string> lines { get; }

        public TAS(List<string> _lines)
        {
            lines = _lines;
        }

        public TAS(List<Press> presses)
        {
            lines = new List<string>();
            presses.Sort(Press.compareFrames);
            HashSet<char> state = new HashSet<char>();
            int frame = 0;
            foreach (Press press in presses)
            {
                if (press.frame > frame)
                {
                    lines.Add(tasLine(press.frame - frame, state));
                    frame = press.frame;
                }
                if (press.on)
                    state.Add(press.button);
                else
                    state.Remove(press.button);
            }
            lines.Add(tasLine(1, state));
        }

        private static string tasLine(int frames, HashSet<char> buttons)
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

        public List<Press> toPresses()
        {
            List<Press> presses = new List<Press>();
            HashSet<char> state = new HashSet<char>();
            int frame = 0;
            foreach (string l in lines)
            {
                string line = l.Trim();
                if (l.StartsWith('#'))
                    continue;
                int frames;
                string buttons;
                if (line.Contains(','))
                {
                    string[] s = line.Split(',', 2);
                    Console.WriteLine(s);
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
    }
}
