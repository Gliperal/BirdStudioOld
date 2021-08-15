﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirdStudio
{
    public struct Press
    {
        public int frame;
        public char button;
        public bool on;

        public static int compareFrames(Press x, Press y)
        {
            return x.frame - y.frame;
        }
    }
}
