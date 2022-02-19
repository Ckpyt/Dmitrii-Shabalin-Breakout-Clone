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
            return color switch
            {
                Colors.blue => new Color(0, 0, 1),
                Colors.green => new Color(0, 1, 0),
                Colors.yellow => new Color(1, 1, 0),
                Colors.orange => new Color(1, 0.5f, 1),
                Colors.red => new Color(1, 0, 0),
                _ => throw new Exception("Undefined color detected"),
            };
        }
    }
}
