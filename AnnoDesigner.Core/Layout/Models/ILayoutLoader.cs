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
        void SaveLayout(List<AnnoObject> objects, string pathToLayoutFile);

        void SaveLayout(List<AnnoObject> objects, Stream streamWithLayout);

        List<AnnoObject> LoadLayout(string pathToLayoutFile, bool forceLoad = false);

        List<AnnoObject> LoadLayout(Stream streamWithLayout, bool forceLoad = false);
    }
}
