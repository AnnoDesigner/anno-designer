using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace AnnoDesigner
{
    /// <summary>
    /// Sort <see cref="System.Windows.Media.Color" by Hue, Saturation and Brightness./>
    /// </summary>
    /// <remarks>based on ColorSorter from Extended WPF Toolkit.</remarks>
    public class ColorHueSaturationBrightnessComparer : IComparer<System.Windows.Media.Color>
    {
        public int Compare(Color x, Color y)
        {
            var drawingColor1 = System.Drawing.Color.FromArgb(x.A, x.R, x.G, x.B);
            var drawingColor2 = System.Drawing.Color.FromArgb(y.A, y.R, y.G, y.B);

            // Compare Hue
            double hueColor1 = Math.Round(drawingColor1.GetHue(), 3);
            double hueColor2 = Math.Round(drawingColor2.GetHue(), 3);

            if (hueColor1 > hueColor2)
            {
                return 1;
            }
            else if (hueColor1 < hueColor2)
            {
                return -1;
            }
            else
            {
                // Hue is equal, compare Saturation
                double satColor1 = Math.Round(drawingColor1.GetSaturation(), 3);
                double satColor2 = Math.Round(drawingColor2.GetSaturation(), 3);

                if (satColor1 > satColor2)
                {
                    return 1;
                }
                else if (satColor1 < satColor2)
                {
                    return -1;
                }
                else
                {
                    // Saturation is equal, compare Brightness
                    double brightColor1 = Math.Round(drawingColor1.GetBrightness(), 3);
                    double brightColor2 = Math.Round(drawingColor2.GetBrightness(), 3);

                    if (brightColor1 > brightColor2)
                    {
                        return 1;
                    }
                    else if (brightColor1 < brightColor2)
                    {
                        return -1;
                    }
                }
            }

            return 0;
        }
    }
}

