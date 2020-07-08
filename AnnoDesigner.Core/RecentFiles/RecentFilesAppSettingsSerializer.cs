using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;
using Newtonsoft.Json;

namespace AnnoDesigner.Core.RecentFiles
{
    public class RecentFilesAppSettingsSerializer : IRecentFilesSerializer
    {
        private readonly IAppSettings _appSettings;

        public RecentFilesAppSettingsSerializer(IAppSettings appSettingsToUse)
        {
            _appSettings = appSettingsToUse;
        }

        public List<RecentFile> Deserialize()
        {
            var savedList = JsonConvert.DeserializeObject<List<RecentFile>>(_appSettings.RecentFiles);
            if (savedList is null)
            {
                return new List<RecentFile>();
            }

            return savedList;
        }

        public void Serialize(List<RecentFile> recentFiles)
        {
            if (recentFiles is null)
            {
                return;
            }

            var json = JsonConvert.SerializeObject(recentFiles);
            _appSettings.RecentFiles = json;
            _appSettings.Save();
        }
    }
}
