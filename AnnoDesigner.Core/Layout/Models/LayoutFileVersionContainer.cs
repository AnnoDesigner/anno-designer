using System;
using System.Runtime.Serialization;

namespace AnnoDesigner.Core.Layout.Models;

/// <summary>
/// Container with just the version information.
/// </summary>
[DataContract]
public class LayoutFileVersionContainer
{
    public LayoutFileVersionContainer()
    {
        LayoutVersion = new Version(1, 0, 0, 0);
        Modified = DateTime.UtcNow;
    }

    [DataMember(Order = 0)]
    public int FileVersion { get; set; }

    [DataMember(Order = 1)]
    public Version LayoutVersion { get; set; }

    [DataMember(Order = 2)]
    public DateTime Modified { get; set; }
}