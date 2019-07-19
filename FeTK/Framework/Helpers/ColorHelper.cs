using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Framework.Helpers
{
    /// <summary>
    /// Provides an API for common tasks involving objects of the <see cref="Color"/> structure.
    /// </summary>
    public static class ColorHelper
    {
        /// <summary>
        /// Convert a given string into a corresponding <see cref="Color"/> object.
        /// </summary>
        /// <param name="s">The string representation of a color.</param>
        /// <returns>The <see cref="Color"/> value equivalent to the string representation of a color in <paramref name="s"/>.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="s"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">The specified <paramref name="s"/> is not in the correct format.</exception>
        /// <remarks>
        /// The only supported string representation of a color is the hexadecimal color format: #RRGGBB or #AARRGGBB, 
        /// where AA, RR, GG, BB are hexadecimal integers between 00 - FF, respectively, following a number sign ('#').
        /// </remarks>
        public static Color GetColorFromString(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            if (!s.StartsWith("#") || (s.Length != 7 && s.Length != 9))
            {
                throw new FormatException("The given string is not in a valid hex color format!");
            }

            s = s.Replace("#", string.Empty);

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

        /// <summary>
        /// Convert a given string into a corresponding <see cref="Color"/> object.
        /// </summary>
        /// <param name="s">The string representation of a color.</param>
        /// <param name="color">
        /// When this method returns, contains the <see cref="Color"/> value equivalent to the string representation
        /// of a color in <paramref name="s"/>, or <c>null</c> if the conversion failed. The conversion fails if 
        /// the specified <paramref name="s"/> is not of the correct format. This parameter is passed uninitialized; 
        /// any value originally supplied in <paramref name="color"/> will be overwritten.
        /// </param>
        /// <returns><c>true</c> if <paramref name="s"/> was converted successfully; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// The only supported string representation of a color is the hexadecimal color format: #RRGGBB or #AARRGGBB, 
        /// where AA, RR, GG, BB are hexadecimal integers between 00 - FF, respectively, following a number sign ('#').
        /// </remarks>
        public static bool TryGetColorFromString(string s, out Color? color)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            if (!s.StartsWith("#") || (s.Length != 7 && s.Length != 9))
            {
                throw new FormatException("The given string is not in a valid hex color format!");
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

                color = new Color(r, g, b, a);
                return true;
            }
            catch (FormatException)
            {
                color = null;
                return false;
            }
        }
    }
}
