using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Core.Helper;

public static class MathHelper
{
    static readonly double sqrt2 = Math.Sqrt(2);

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

    /// <summary>
    /// Computes the diagonal tile amount from a length. 
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public static double GetDiagonalTiles(double size) => Math.Round(size * sqrt2);

    /// <summary>
    /// computes the real length of size diagonal tiles. 
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public static double GetRealLengthOfDiagonalTiles(double size) => size / sqrt2;

    public static double RoundSnap(double dx, double snap)
    {
        return Math.Floor(dx) + (Math.Round((dx - Math.Floor(dx)) * (1f / snap)) * snap);
    }

}
