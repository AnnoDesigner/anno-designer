using System;

namespace AnnoDesigner.Models;

public class AvailableRelease
{
    public long Id { get; set; }

    public ReleaseType Type { get; set; }

    public Version Version { get; set; }
}
