using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Layout.Models;

namespace AnnoDesigner.CustomEventArgs
{
    public class FileLoadedEventArgs : EventArgs
    {
        public FileLoadedEventArgs(string filePathToUse, LayoutFile layoutToUse = null)
        {
            FilePath = filePathToUse;
            Layout = layoutToUse;
        }

        public string FilePath { get; private set; }

        public LayoutFile Layout { get; private set; }
    }
}
