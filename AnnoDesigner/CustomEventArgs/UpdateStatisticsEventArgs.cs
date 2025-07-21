using AnnoDesigner.Models;
using System;

namespace AnnoDesigner.CustomEventArgs;

public class UpdateStatisticsEventArgs : EventArgs
{
    public static new readonly UpdateStatisticsEventArgs Empty = new();

    public static readonly UpdateStatisticsEventArgs All = new(UpdateMode.All);

    private UpdateStatisticsEventArgs()
    {
    }

    public UpdateStatisticsEventArgs(UpdateMode _updateMode)
    {
        Mode = _updateMode;
    }

    public UpdateMode Mode { get; private set; }
}
