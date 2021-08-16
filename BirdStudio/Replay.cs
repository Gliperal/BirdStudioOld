using System;
using System.Collections.Generic;

namespace BirdStudio
{
    public class Replay
    {
        public struct ReplayInput
        {
            public int frame;
            public int buttonID;
        }

        private Dictionary<string, List<ReplayInput>> inputsByType;
        private int breakpoint;

        public Replay(string file)
        {
            inputsByType = new Dictionary<string, List<ReplayInput>>();
            string[] lines = System.IO.File.ReadAllLines(file);
            if (lines[0] != "0:") throw new FormatException();
            _loadInputs(" J", lines[1]);
            if (lines[2] != "1:") throw new FormatException();
            _loadInputs(" X", lines[3]);
            if (lines[4] != "2:") throw new FormatException();
            _loadInputs(" G", lines[5]);
            if (lines[6] != "3:") throw new FormatException();
            _loadInputs(" C", lines[7]);
            if (lines[8] != "4:") throw new FormatException();
            _loadInputs(" Q", lines[9]);
            if (lines[10] != "") throw new FormatException();
            if (lines[11] != "0:") throw new FormatException();
            _loadInputs(" RL", lines[12]);
            if (lines[13] != "1:") throw new FormatException();
            _loadInputs(" UD", lines[14]);
            if (lines[15] != "2:") throw new FormatException();
            // TODO what if user doesn't record replay with dash axis
            if (lines[16] != lines[14]) throw new FormatException();
            if (lines[17] != "") throw new FormatException();
            breakpoint = Int32.Parse(lines[18]);
        }

        private void _loadInputs(string type, string inputsString)
        {
            List<ReplayInput> inputs = new List<ReplayInput>();
            foreach (string input in inputsString.Split('|'))
            {
                if (input == "")
                    continue;
                string[] s = input.Split(',');
                inputs.Add(new ReplayInput
                {
                    frame = Int32.Parse(s[0]),
                    buttonID = Int32.Parse(s[1])
                });
            }
            inputsByType[type] = inputs;
        }

        private string[] TYPES = {" J", " X", " G", " C", " Q", " RL", " UD"};
        public Replay(List<Press> presses)
        {
            inputsByType = new Dictionary<string, List<ReplayInput>>();
            foreach (string type in TYPES)
            {
                List<ReplayInput> inputs = new List<ReplayInput>();
                foreach (Press press in presses)
                {
                    string buttons = type;
                    int buttonID = buttons.IndexOf(press.button);
                    if (buttonID >= 0)
                        inputs.Add(new ReplayInput
                        {
                            frame = press.frame,
                            buttonID = buttonID
                        });
                }
                inputsByType[type] = inputs;
            }
        }

        public List<Press> toPresses()
        {
            List<Press> presses = new List<Press>();
            foreach (string type in inputsByType.Keys)
            {
                string buttons = type;
                foreach (ReplayInput input in inputsByType[type])
                    foreach (char button in buttons.Substring(1))
                        presses.Add(new Press
                        {
                            frame = input.frame,
                            button = button,
                            on = button == buttons[input.buttonID]
                        });
            }
            return presses;
        }

        private string _writeInputs(string type)
        {
            string result = "";
            foreach (ReplayInput input in inputsByType[type])
                result += input.frame + "," + input.buttonID + "|";
            return result;
        }

        public void writeFile(string file)
        {
            List<string> lines = new List<string>();
            lines.Add("0:");
            lines.Add(_writeInputs(" J"));
            lines.Add("1:");
            lines.Add(_writeInputs(" X"));
            lines.Add("2:");
            lines.Add(_writeInputs(" G"));
            lines.Add("3:");
            lines.Add(_writeInputs(" C"));
            lines.Add("4:");
            lines.Add(_writeInputs(" Q"));
            lines.Add("");
            lines.Add("0:");
            lines.Add(_writeInputs(" RL"));
            lines.Add("1:");
            lines.Add(_writeInputs(" UD"));
            lines.Add("2:");
            // TODO should probably just not use dash axis for tas
            lines.Add(_writeInputs(" UD"));
            lines.Add("");
            lines.Add(breakpoint.ToString());
            System.IO.File.WriteAllLines(file, lines.ToArray());
        }
    }
}
