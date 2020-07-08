using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Core.RecentFiles
{
    public class RecentFilesInMemorySerializer : IRecentFilesSerializer
    {
        private List<RecentFile> _recentFiles;

        public RecentFilesInMemorySerializer()
        {
            _recentFiles = new List<RecentFile>();
            _recentFiles.Add(new RecentFile(@"C:\test\sub\file_01.ad", DateTime.UtcNow));
            _recentFiles.Add(new RecentFile(@"C:\test\sub\file_02.ad", DateTime.UtcNow));
            _recentFiles.Add(new RecentFile(@"C:\test\sub\file_03.ad", DateTime.UtcNow));
            _recentFiles.Add(new RecentFile(@"C:\test\sub\file_04.ad", DateTime.UtcNow));
            _recentFiles.Add(new RecentFile(@"C:\test\sub\file_05.ad", DateTime.UtcNow));
            _recentFiles.Add(new RecentFile(@"C:\test\sub\file_06.ad", DateTime.UtcNow));
            _recentFiles.Add(new RecentFile(@"C:\test\sub\file_07.ad", DateTime.UtcNow));
            _recentFiles.Add(new RecentFile(@"C:\test\sub\file_08.ad", DateTime.UtcNow));
            _recentFiles.Add(new RecentFile(@"C:\test\sub\file_09.ad", DateTime.UtcNow));
            _recentFiles.Add(new RecentFile(@"C:\test\sub\file_10.ad", DateTime.UtcNow));
            _recentFiles.Add(new RecentFile(@"C:\test\sub\file_11.ad", DateTime.UtcNow));
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
