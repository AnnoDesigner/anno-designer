using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;
using Newtonsoft.Json;
using NLog;

namespace AnnoDesigner.Core.RecentFiles
{
    public class RecentFilesAppSettingsSerializer : IRecentFilesSerializer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IAppSettings _appSettings;

        public RecentFilesAppSettingsSerializer(IAppSettings appSettingsToUse)
        {
            _appSettings = appSettingsToUse;
        }

        public List<RecentFile> Deserialize()
        {
            if (string.IsNullOrWhiteSpace(_appSettings.RecentFiles))
            {
                return new List<RecentFile>();
            }

            var savedList = new List<RecentFile>();

            try
            {
                var deserializedList = JsonConvert.DeserializeObject<List<RecentFile>>(_appSettings.RecentFiles);
                if (deserializedList is null)
                {
                    return savedList;
                }

                savedList = deserializedList.Where(x => !string.IsNullOrWhiteSpace(x.Path)).ToList();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error deserializing saved list of recent files.{Environment.NewLine}{nameof(_appSettings.RecentFiles)}: \"{_appSettings.RecentFiles}\"");
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
