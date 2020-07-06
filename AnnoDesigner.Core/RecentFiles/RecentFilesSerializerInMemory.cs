using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Core.RecentFiles
{
    public class RecentFilesSerializerInMemory : IRecentFilesSerializer
    {
        private List<RecentFile> _recentFiles;

        public RecentFilesSerializerInMemory()
        {
            _recentFiles = new List<RecentFile>();
        }

        public List<RecentFile> Deserialize()
        {
            return _recentFiles;
        }

        public void Serialize(List<RecentFile> recentFiles)
        {
            _recentFiles = new List<RecentFile>(recentFiles);
        }
    }
}
