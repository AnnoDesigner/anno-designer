using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Core.Layout.Models
{
    //TODO: PR: make this a struct to avoid an allocation?
    public class StatisticsCalculationResult
    {
        public double MinX { get; set; }

        public double MinY { get; set; }

        public double UsedAreaWidth { get; set; }

        public double UsedAreaHeight { get; set; }

        public double UsedTiles { get; set; }

        public double MinTiles { get; set; }

        /// <summary>
        /// The efficiency of the layout in percent.
        /// </summary>
        public double Efficiency { get; set; }
    }
}
