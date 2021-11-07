using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Breakout
{
    public enum Colors
    {
        Blue = 0,
        Green = 1,
        Yellow = 2,
        Orange = 3,
        Red = 4
    }

    class DefinedColors
    {
        public static Color GetColor(Colors color)
        {
            switch (color)
            {
                case Colors.Blue: return new Color(0, 0, 1);
                case Colors.Green: return new Color(0, 1, 0);
                case Colors.Yellow: return new Color(1, 1, 0);
                case Colors.Orange: return new Color(1, 0.5f, 1);
                case Colors.Red: return new Color(1, 0, 0);
            }
            throw new Exception("Undefined color detected");
        }
    }
}
