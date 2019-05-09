using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner
{
    public class UpdateHelper
    {
        public enum AssetType
        {
            None,
            Presets,
            PresetsAndIcons
        }

        private const string GITHUB_USERNAME = "AgmasGold";
        private const string GITHUB_PROJECTNAME = "anno-designer";
        private const string RELEASE_PRESET_TAG = "Presetsv";
        private const string ASSET_NAME_PRESETS_AND_ICONS = "Presets.and.Icons.Update";

        private GitHubClient _gitHubClient;

        public UpdateHelper()
        {
            LatestPresetReleaseType = AssetType.None;
        }

        private GitHubClient Client
        {
            get
            {
                if (_gitHubClient == null)
                {
                    _gitHubClient = new GitHubClient(new ProductHeaderValue($"anno-designer-{Constants.Version.ToString("0.0#", CultureInfo.InvariantCulture)}"));
                }

                return _gitHubClient;
            }
        }

        public string PathToUpdatedPresetsFile
        {
            get { return Path.Combine(App.ApplicationPath, Constants.PrefixTempBuildingPresetsFile + Constants.BuildingPresetsFile); }
        }

        public string PathToUpdatedPresetsAndIconsFile
        {
            get { return Path.Combine(App.ApplicationPath, Constants.PrefixTempBuildingPresetsFile + "PresetsAndIcons.zip"); }
        }

        private IReadOnlyList<Release> AllReleases { get; set; }

        private Release LatestPresetRelease { get; set; }

        private AssetType LatestPresetReleaseType { get; set; }

        public async Task<bool> IsNewPresetFileAvailableAsync(Version currentPresetVersion)
        {
            try
            {
                var result = false;

                var releases = await Client.Repository.Release.GetAll(GITHUB_USERNAME, GITHUB_PROJECTNAME).ConfigureAwait(false);
                if (releases == null || releases.Count < 1)
                {
                    AllReleases = null;
                    return result;
                }

                AllReleases = releases;

                //var latestPresetRelease = releases.FirstOrDefault(x => !x.Draft && !x.Prerelease && x.TagName.StartsWith(RELEASE_PRESET_TAG, StringComparison.OrdinalIgnoreCase));
                //for testing - latest preset and icons release
                var latestPresetRelease = releases.FirstOrDefault(x => !x.Draft && !x.Prerelease && x.TagName.StartsWith("Presetsv3.0.0", StringComparison.OrdinalIgnoreCase));
                if (latestPresetRelease == null)
                {
                    LatestPresetRelease = null;
                    return result;
                }

                LatestPresetRelease = latestPresetRelease;

                var latestPresetVersionString = latestPresetRelease.TagName.Replace(RELEASE_PRESET_TAG, string.Empty).Trim();
                if (String.IsNullOrWhiteSpace(latestPresetVersionString))
                {
                    Debug.WriteLine($"Could not get version of latest preset release. {nameof(latestPresetRelease.TagName)}: {latestPresetRelease.TagName}");
                    return result;
                }

                var latestPresetVersion = new Version(latestPresetVersionString);
                if (latestPresetVersion > currentPresetVersion)
                {
                    result = true;
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting info about preset updates.{Environment.NewLine}{ex}");
                return false;
            }
        }

        public async Task<string> DownloadLatestPresetFile()
        {
            try
            {
                var result = string.Empty;

                if (LatestPresetRelease == null)
                {
                    return result;
                }

                //check presets.json
                var latestPresetAsset = LatestPresetRelease.Assets.FirstOrDefault(x => x.Name.Equals(Constants.BuildingPresetsFile, StringComparison.OrdinalIgnoreCase));
                if (latestPresetAsset == null)
                {
                    Debug.WriteLine($"No asset found for latest preset update. ({Constants.BuildingPresetsFile})");
                }
                else
                {
                    LatestPresetReleaseType = AssetType.Presets;
                }

                //check presets with icons
                if (latestPresetAsset == null)
                {
                    latestPresetAsset = LatestPresetRelease.Assets.FirstOrDefault(x => x.Name.StartsWith(ASSET_NAME_PRESETS_AND_ICONS, StringComparison.OrdinalIgnoreCase));
                    if (latestPresetAsset == null)
                    {
                        Debug.WriteLine($"No asset found for latest preset update. ({ASSET_NAME_PRESETS_AND_ICONS})");
                    }
                    else
                    {
                        LatestPresetReleaseType = AssetType.PresetsAndIcons;
                    }
                }

                //still no supported asset found
                if (latestPresetAsset == null)
                {
                    Debug.WriteLine("No supported asset found for latest preset update.");
                    return result;
                }

                //download file to temp directory
                var tempFileName = Path.GetTempFileName();
                var pathToDownloadedFile = await DownloadFile(latestPresetAsset.BrowserDownloadUrl, tempFileName).ConfigureAwait(false);
                if (String.IsNullOrWhiteSpace(pathToDownloadedFile))
                {
                    return result;
                }

                tempFileName = pathToDownloadedFile;

                //move file to app directory
                var tempFileInfo = new FileInfo(tempFileName);

                if (LatestPresetReleaseType == AssetType.Presets)
                {
                    if (File.Exists(PathToUpdatedPresetsFile))
                    {
                        File.Delete(PathToUpdatedPresetsFile);
                    }

                    tempFileInfo.MoveTo(PathToUpdatedPresetsFile);
                    result = PathToUpdatedPresetsFile;
                }
                else if (LatestPresetReleaseType == AssetType.PresetsAndIcons)
                {
                    if (File.Exists(PathToUpdatedPresetsAndIconsFile))
                    {
                        File.Delete(PathToUpdatedPresetsAndIconsFile);
                    }

                    tempFileInfo.MoveTo(PathToUpdatedPresetsAndIconsFile);
                    result = PathToUpdatedPresetsAndIconsFile;
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error downloading latest preset file.{Environment.NewLine}{ex}");
                return string.Empty;
            }
        }

        private async Task<string> DownloadFile(string url, string pathToSavedFile)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    await webClient.DownloadFileTaskAsync(url, pathToSavedFile).ConfigureAwait(false);
                }

                return pathToSavedFile;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error downloading file ({url}).{Environment.NewLine}{ex}");
                return string.Empty;
            }
        }
    }
}
