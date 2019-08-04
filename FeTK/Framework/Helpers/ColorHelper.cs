using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Color = Microsoft.Xna.Framework.Color;
using DColor = System.Drawing.Color;

namespace FelixDev.StardewMods.FeTK.Framework.Helpers
{
    /// <summary>
    /// Provides an API for common tasks involving objects of the <see cref="Color"/> structure.
    /// </summary>
    public static class ColorHelper
    {
        /// <summary>
        /// Translate a string representation of a color to a corresponding <see cref="Color"/> structure.
        /// </summary>
        /// <param name="sColor">The string representation of a color.</param>
        /// <returns>The <see cref="Color"/> structure that represents the translated string representation of a color.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="sColor"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The specified <paramref name="sColor"/> is not a valid HTML color name.</exception>
        /// <exception cref="FormatException">The specified <paramref name="sColor"/> is not in the correct format.</exception>
        /// <remarks>
        /// The only supported string representations of a color are the following: 
        ///     (1) The hexadecimal color format: #RRGGBB or #AARRGGBB, 
        ///     where AA, RR, GG, BB are hexadecimal integers between 00 - FF, respectively, following a number sign ('#').
        ///     (2) A valid HTML color name. See https://htmlcolorcodes.com/color-names/ for a list of all supported names.
        ///     Names are case-insensitive.
        /// </remarks>
        public static Color GetColorFromString(string sColor)
        {
            if (sColor == null)
            {
                throw new ArgumentNullException(nameof(sColor));
            }

            if (sColor.StartsWith("#"))
            {
                var tColor = GetColorFromHexCode(sColor);
                if (!tColor.HasValue)
                {
                    throw new FormatException("The given string is not in a valid hex color format!");
                }

                return tColor.Value;

            }
            else
            {
                var tColor = GetColorFromName(sColor);
                if (!tColor.HasValue)
                {
                    throw new ArgumentException("The given string is not a valid HTML color name!", nameof(sColor));
                }

                return tColor.Value;
            }

        }

        /// <summary>
        /// Translate a string representation of a color to a corresponding <see cref="Color"/> structure.
        /// </summary>
        /// <param name="sColor">The string representation of a color.</param>
        /// <param name="color">
        /// When this method returns, contains the <see cref="Color"/> structure that represents the translated string representation 
        /// of a color, or <c>null</c> if the translation failed. The translation fails if 
        ///     a) the specified <paramref name="sColor"/> is not in the correct format -or-
        ///     b) the specified <paramref name="sColor"/> is not a valid HTML color name.
        /// This parameter is passed uninitialized; 
        /// any value originally supplied in <paramref name="color"/> will be overwritten.
        /// </param>
        /// <returns><c>true</c> if <paramref name="sColor"/> was converted successfully; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// The only supported string representations of a color are the following: 
        ///     (1) The hexadecimal color format: #RRGGBB or #AARRGGBB, 
        ///     where AA, RR, GG, BB are hexadecimal integers between 00 - FF, respectively, following a number sign ('#').
        ///     (2) A valid HTML color name. See https://htmlcolorcodes.com/color-names/ for a list of all supported color names.
        ///     Names are case-insensitive.
        /// </remarks>
        public static bool TryGetColorFromString(string sColor, out Color? color)
        {
            if (sColor == null)
            {
                color = null;
                return false;
            }

            if (sColor.StartsWith("#"))
            {
                color = GetColorFromHexCode(sColor);
            }
            else
            {
                color = GetColorFromName(sColor);
            }

            return color != null;
        }

        /// <summary>
        /// Translate a hex-code color representation to a corresponding <see cref="Color"/> structure.
        /// </summary>
        /// <param name="hexCode">The streng representation of the hex-code to translate.</param>
        /// <returns>
        /// The <see cref="Color"/> structure that represents the translated hex-code color representation on success; 
        /// otherwise, <c>null</c>.
        /// </returns>
        /// <remarks>
        /// Valid hex-code color formats are the following: #RRGGBB or #AARRGGBB, 
        /// where AA, RR, GG, BB are hexadecimal integers between 00 - FF, respectively, following a number sign ('#').
        /// </remarks>
        private static Color? GetColorFromHexCode(string hexCode)
        {
            if (hexCode.Length != 7 && hexCode.Length != 9)
            {
                return null;
            }

            hexCode = hexCode.Replace("#", string.Empty);

            try
            {
                byte a;
                byte r;
                byte g;
                byte b;
                if (hexCode.Length == 8)
                {
                    a = (byte)(Convert.ToUInt32(hexCode.Substring(0, 2), 16));
                    r = (byte)(Convert.ToUInt32(hexCode.Substring(2, 2), 16));
                    g = (byte)(Convert.ToUInt32(hexCode.Substring(4, 2), 16));
                    b = (byte)(Convert.ToUInt32(hexCode.Substring(6, 2), 16));

                }
                else
                {
                    a = 0xFF;
                    r = (byte)(Convert.ToUInt32(hexCode.Substring(0, 2), 16));
                    g = (byte)(Convert.ToUInt32(hexCode.Substring(2, 2), 16));
                    b = (byte)(Convert.ToUInt32(hexCode.Substring(4, 2), 16));
                }

                return new Color(r, g, b, a);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        /// <summary>
        /// Translate a HTML color representation to a corresponding <see cref="Color"/> structure.
        /// </summary>
        /// <param name="colorName">The string representation of the HTML color to translate.</param>
        /// <returns>
        /// The <see cref="Color"/> structure that represents the translated HTML color representation on success; 
        /// otherwise, <c>null</c>.
        /// </returns>
        /// <remarks>
        /// For a list of supported color names, see: https://htmlcolorcodes.com/color-names/
        /// </remarks>
        private static Color? GetColorFromName(string htmlColor)
        {
            try
            {
                DColor tColor = ColorTranslator.FromHtml(htmlColor);
                return new Color(tColor.R, tColor.G, tColor.B, tColor.A);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
