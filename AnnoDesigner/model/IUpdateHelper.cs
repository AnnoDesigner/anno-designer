using System;
using System.Threading.Tasks;

namespace AnnoDesigner.model
{
    public interface IUpdateHelper
    {
        string PathToUpdatedPresetsAndIconsFile { get; }
        string PathToUpdatedPresetsFile { get; }

        Task<string> DownloadLatestPresetFile();
        Task<bool> IsNewPresetFileAvailableAsync(Version currentPresetVersion);
        Task ReplaceUpdatedPresetFile();
    }
}