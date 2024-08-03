using System;

namespace AnnoDesigner.CustomEventArgs;

public class OpenFileEventArgs : EventArgs
{
    public OpenFileEventArgs(string filePathToUse)
    {
        FilePath = filePathToUse;
    }

    public string FilePath { get; private set; }
}
