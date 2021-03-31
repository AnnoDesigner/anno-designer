using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AnnoDesigner.Core.Layout.Models
{
    public class StatisticsCalculationResult
    {
        public double MinX { get; set; }

        public double MinY { get; set; }

        public double MaxX { get; set; }

        public double MaxY { get; set; }

        public double UsedAreaWidth { get; set; }

        public double UsedAreaHeight { get; set; }

        public double UsedTiles { get; set; }

        public double MinTiles { get; set; }

        /// <summary>
        /// The efficiency of the layout in percent.
        /// </summary>
        public double Efficiency { get; set; }

        public static explicit operator Rect(StatisticsCalculationResult a)
        {
            return new Rect(a.MinX, a.MinY, a.UsedAreaWidth, a.UsedAreaHeight);
        }
    }
}
