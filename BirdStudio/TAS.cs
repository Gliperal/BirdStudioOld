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
    }
}
