using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Framework.Helpers
{
    public static class ColorHelper
    {
        public static Color GetColorFromString(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentException("message", nameof(s));
            }

            if (!s.StartsWith("#") || (s.Length != 7 && s.Length != 9))
            {
                throw new ArgumentException("s is not in a valid hex color format", nameof(s));
            }

            s = s.Replace("#", string.Empty);

            try
            {
                byte a;
                byte r;
                byte g;
                byte b;
                if (s.Length == 8)
                {
                    a = (byte)(Convert.ToUInt32(s.Substring(0, 2), 16));
                    r = (byte)(Convert.ToUInt32(s.Substring(2, 2), 16));
                    g = (byte)(Convert.ToUInt32(s.Substring(4, 2), 16));
                    b = (byte)(Convert.ToUInt32(s.Substring(6, 2), 16));

                }
                else
                {
                    a = 0xFF;
                    r = (byte)(Convert.ToUInt32(s.Substring(0, 2), 16));
                    g = (byte)(Convert.ToUInt32(s.Substring(2, 2), 16));
                    b = (byte)(Convert.ToUInt32(s.Substring(4, 2), 16));
                }

                return new Color(r, g, b, a);
            }
            catch (FormatException)
            {
                throw new ArgumentException(nameof(s));
            }
            
        }
    }
}
