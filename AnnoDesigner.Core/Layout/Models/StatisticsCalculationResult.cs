using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Core.Layout.Models
{
    public class StatisticsCalculationResult
    {
        public double UsedAreaX { get; set; }

        public double UsedAreaY { get; set; }

        public double UsedTiles { get; set; }

        public double MinTiles { get; set; }

        /// <summary>
        /// The efficiency of the layout in percent.
        /// </summary>
        public double Efficiency { get; set; }
    }
}
