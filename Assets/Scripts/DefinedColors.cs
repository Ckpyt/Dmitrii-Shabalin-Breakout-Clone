using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Shabalin.Breakout
{
    //Colors, used for bricks
    public enum Colors
    {
        blue = 0,
        green = 1,
        yellow = 2,
        orange = 3,
        red = 4
    }

    /// <summary>
    /// Defined colors for bricks.
    /// </summary>
    class DefinedColors
    {
        /// <summary>
        /// translate enum variable into Color
        /// </summary>
        public static Color GetColor(Colors color)
        {
            switch (color)
            {
                case Colors.blue:   return new Color(0, 0, 1);
                case Colors.green:  return new Color(0, 1, 0);
                case Colors.yellow: return new Color(1, 1, 0);
                case Colors.orange: return new Color(1, 0.5f, 1);
                case Colors.red:    return new Color(1, 0, 0);
            }
            throw new Exception("Undefined color detected");
        }
    }
}
