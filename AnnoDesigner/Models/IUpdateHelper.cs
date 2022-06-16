using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnnoDesigner.Models
{
    public interface IUpdateHelper
    {
        Task<List<AvailableRelease>> GetAvailableReleasesAsync();

        Task<AvailableRelease> GetAvailableReleasesAsync(ReleaseType releaseType);

        Task<string> DownloadReleaseAsync(AvailableRelease releaseToDownload);

        Task ReplaceUpdatedPresetsFilesAsync();

        Task<bool> AreUpdatedPresetsFilesPresentAsync();

        Task<(bool, Version)> IsNewAppVersionAvailableAsync();
    }
}