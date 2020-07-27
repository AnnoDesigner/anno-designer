using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Core.Layout.Models
{
    public interface ILayoutLoader
    {
        void SaveLayout(SavedLayout layout, string pathToLayoutFile);

        void SaveLayout(SavedLayout layout, Stream streamWithLayout);

        SavedLayout LoadLayout(string pathToLayoutFile, bool forceLoad = false);

        SavedLayout LoadLayout(Stream streamWithLayout, bool forceLoad = false);
    }
}
