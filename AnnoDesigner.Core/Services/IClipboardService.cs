using System.Collections.Generic;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Core.Services;

public interface IClipboardService
{
    void Copy(IEnumerable<AnnoObject> objects);

    ICollection<AnnoObject> Paste();
}
