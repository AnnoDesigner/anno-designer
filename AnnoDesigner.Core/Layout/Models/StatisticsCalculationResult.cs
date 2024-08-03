using System;
using System.Windows;

namespace AnnoDesigner.Core.Layout.Models;

/// <summary>
/// Contains various statistics of a layout.
/// </summary>
public class StatisticsCalculationResult : IEquatable<StatisticsCalculationResult>
{
    /// <summary>
    /// A static instance with empty statistics for easier use.
    /// </summary>
    public static StatisticsCalculationResult Empty { get; } = new StatisticsCalculationResult();

    private StatisticsCalculationResult() : this(0, 0, 0, 0, 0, 0, 0, 0, 0)
    { }

    public StatisticsCalculationResult(double minX,
        double minY,
        double maxX,
        double maxY,
        double usedAreaWidth,
        double usedAreaHeight,
        double usedTiles,
        double minTiles,
        double efficiency)
    {
        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;
        UsedAreaWidth = usedAreaWidth;
        UsedAreaHeight = usedAreaHeight;
        UsedTiles = usedTiles;
        MinTiles = minTiles;
        Efficiency = efficiency;
    }

    public double MinX { get; }

    public double MinY { get; }

    public double MaxX { get; }

    public double MaxY { get; }

    public double UsedAreaWidth { get; }

    public double UsedAreaHeight { get; }

    /// <summary>
    /// The number of tiles which are used by the layout.
    /// </summary>
    public double UsedTiles { get; }

    /// <summary>
    /// The number of tiles that are necessary by the layout.
    /// </summary>
    /// <remarks>This ignores all empty tiles.<para>Depending on the calculation used also all roads are ignored.</para></remarks>
    public double MinTiles { get; }

    /// <summary>
    /// The efficiency of the layout in percent.
    /// </summary>
    public double Efficiency { get; }

    /// <summary>
    /// Used to get the bounds of the layout.
    /// </summary>
    /// <param name="result">The <see cref="StatisticsCalculationResult"/> to get the bounds from.</param>
    public static explicit operator Rect(StatisticsCalculationResult result)
    {
        return new Rect(result.MinX, result.MinY, result.UsedAreaWidth, result.UsedAreaHeight);
    }

    #region IEquatable<StatisticsCalculationResult> Members

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <returns><c>true</c> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <c>false</c>.</returns>
    /// <param name="other">An object to compare with this object.</param>
    public bool Equals(StatisticsCalculationResult other)
    {
        //check for null
        if (other is null)
        {
            return false;
        }

        //check for same reference
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        //check for same values
        return MinX.Equals(other.MinX) &&
            MinY.Equals(other.MinY) &&
            MaxX.Equals(other.MaxX) &&
            MaxY.Equals(other.MaxY) &&
            UsedAreaWidth.Equals(other.UsedAreaWidth) &&
            UsedAreaHeight.Equals(other.UsedAreaHeight) &&
            UsedTiles.Equals(other.UsedTiles) &&
            MinTiles.Equals(other.MinTiles) &&
            Efficiency.Equals(other.Efficiency);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as StatisticsCalculationResult);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(MinX);
        hash.Add(MinY);
        hash.Add(MaxX);
        hash.Add(MaxY);
        hash.Add(UsedAreaWidth);
        hash.Add(UsedAreaHeight);
        hash.Add(UsedTiles);
        hash.Add(MinTiles);
        hash.Add(Efficiency);

        return hash.ToHashCode();
    }

    #endregion
}
