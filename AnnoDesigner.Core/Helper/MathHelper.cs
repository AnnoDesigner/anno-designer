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
        public static double FractionalValue(double value) => value - Math.Truncate(value);

        /// <summary>
        /// Computes the <paramref name="N"/>th root of a number.
        /// </summary>
        /// <remarks>
        /// https://stackoverflow.com/questions/18657508/c-sharp-find-nth-root
        /// </remarks>
        /// <param name="A"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        public static double NthRoot(double A, double N) => Math.Pow(A, 1.0 / N);

    }
}
