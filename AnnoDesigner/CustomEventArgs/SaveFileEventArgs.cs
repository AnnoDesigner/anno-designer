using System;

namespace AnnoDesigner.CustomEventArgs;

public class SaveFileEventArgs : EventArgs
{
    public SaveFileEventArgs(string filePathToUse)
    {
        FilePath = filePathToUse;
    }

    public string FilePath { get; private set; }
}
