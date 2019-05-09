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
        private const string GITHUB_USERNAME = "AgmasGold";
        private const string GITHUB_PROJECTNAME = "anno-designer";
        private const string RELEASE_PRESET_TAG = "Presetsv";

        private GitHubClient _gitHubClient;

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

        private IReadOnlyList<Release> AllReleases { get; set; }

        private Release LatestPresetRelease { get; set; }

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

                var latestPresetRelease = releases.FirstOrDefault(x => !x.Draft && !x.Prerelease && x.TagName.StartsWith(RELEASE_PRESET_TAG, StringComparison.OrdinalIgnoreCase));
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

                var latestPresetAsset = LatestPresetRelease.Assets.FirstOrDefault(x => x.Name.Equals(Constants.BuildingPresetsFile, StringComparison.OrdinalIgnoreCase));
                if (latestPresetAsset == null)
                {
                    Debug.WriteLine("No asset found for latest preset update.");
                    return result;
                }

                //download file to temp directory
                var tempFileName = Path.GetTempFileName();
                using (WebClient webClient = new WebClient())
                {
                    await webClient.DownloadFileTaskAsync(latestPresetAsset.BrowserDownloadUrl, tempFileName).ConfigureAwait(false);
                }

                //move file to app directory
                var tempFileInfo = new FileInfo(tempFileName);

                tempFileInfo.MoveTo(PathToUpdatedPresetsFile);
                result = PathToUpdatedPresetsFile;

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error downloading latest preset file.{Environment.NewLine}{ex}");
                return string.Empty;
            }
        }
    }
}
