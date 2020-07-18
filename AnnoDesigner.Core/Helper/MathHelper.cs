using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Core.Helper
{
    public static class MathHelper
    {
        /// <summary>
        /// Return the fractional value of a <see cref="double"/>.
        /// This value will always be between -0.99 recurring and 0.99 recurring.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double GetFractionalValue(double value) => value - Math.Truncate(value);
    }
}
