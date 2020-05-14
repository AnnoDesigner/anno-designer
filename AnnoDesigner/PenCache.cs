using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using AnnoDesigner.model;

namespace AnnoDesigner
{
    public class PenCache : IPenCache
    {
        private static readonly Dictionary<(Brush brush, double thickness), Pen> _cachedPens;

        static PenCache()
        {
            _cachedPens = new Dictionary<(Brush brush, double thickness), Pen>();
        }

        public Pen GetPen(Brush brush, double thickness)
        {
            if (!_cachedPens.TryGetValue((brush, thickness), out var foundPen))
            {
                foundPen = new Pen(brush, thickness);
                if (foundPen.CanFreeze)
                {
                    foundPen.Freeze();
                }

                _cachedPens.Add((brush, thickness), foundPen);
            }

            return foundPen;
        }
    }
}
