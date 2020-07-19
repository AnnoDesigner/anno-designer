using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AnnoDesigner.Core;
using AnnoDesigner.Core.Extensions;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Services;
using AnnoDesigner.Models;
using NLog;
using Octokit;

namespace AnnoDesigner
{
    public class UpdateHelper : IUpdateHelper
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string GITHUB_USERNAME = "AnnoDesigner";
        private const string GITHUB_PROJECTNAME = "anno-designer";

        private const string TAG_PRESETS = "Presetsv";
        private const string TAG_PRESETS_ICONS = "PresetsIconsv";
        private const string TAG_PRESETS_COLORS = "PresetsColorsv";
        private const string TAG_PRESETS_WIKIBUILDINGINFO = "PresetsWikiBuildingInfov";

        private GitHubClient _apiClient;
        private HttpClient _httpClient;
        private readonly string _basePath;
        private readonly IAppSettings _appSettings;
        private readonly IMessageBoxService _messageBoxService;
        private readonly ILocalizationHelper _localizationHelper;

        /// <summary>
        /// Initializes a new instance of <see cref="AnnoDesigner.UpdateHelper"./>
        /// </summary>
        /// <param name="basePathToUse">The path the directory of the application.</param>
        /// <remarks>
        /// example to get the basePath: <c>string basePath = AppDomain.CurrentDomain.BaseDirectory;</c>
        /// </remarks>
        public UpdateHelper(string basePathToUse,
            IAppSettings appSettingsToUse,
            IMessageBoxService messageBoxServiceToUse,
            ILocalizationHelper localizationHelperToUse)
        {
            _basePath = basePathToUse;
            _appSettings = appSettingsToUse;
            _messageBoxService = messageBoxServiceToUse;
            _localizationHelper = localizationHelperToUse;
        }

        private GitHubClient ApiClient
        {
            get
            {
                if (_apiClient == null)
                {
                    _apiClient = new GitHubClient(new Octokit.ProductHeaderValue($"anno-designer-{Constants.Version.ToString("0.0#", CultureInfo.InvariantCulture)}", "1.0"));
                }

                return _apiClient;
            }
        }

        private HttpClient LocalHttpClient
        {
            get
            {
                if (_httpClient == null)
                {
                    var handler = new HttpClientHandler();
                    if (handler.SupportsAutomaticDecompression)
                    {
                        handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                    }

                    _httpClient = new HttpClient(handler, true);
                    _httpClient.Timeout = TimeSpan.FromSeconds(30);
                    _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue($"anno-designer-{Constants.Version.ToString("0.0#", CultureInfo.InvariantCulture)}", "1.0"));

                    //detect DNS changes (default is infinite)
                    //ServicePointManager.FindServicePoint(new Uri(BASE_URI)).ConnectionLeaseTimeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
                    //defaut is 2 minutes
                    ServicePointManager.DnsRefreshTimeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
                    //increases the concurrent outbound connections
                    if (ServicePointManager.DefaultConnectionLimit < 1024)
                    {
                        ServicePointManager.DefaultConnectionLimit = 1024;
                    }
                    //only allow secure protocols
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                }

                return _httpClient;
            }
        }

        private IReadOnlyList<Release> AllReleases { get; set; }


        #region IUpdateHelper members        

        public async Task<List<AvailableRelease>> GetAvailableReleasesAsync()
        {
            var result = new List<AvailableRelease>();

            if (AllReleases == null)
            {
                AllReleases = await GetAllAvailableReleases().ConfigureAwait(false);
            }

            foreach (ReleaseType curReleaseType in Enum.GetValues(typeof(ReleaseType)))
            {
                var foundRelease = CheckForAvailableRelease(curReleaseType);
                if (foundRelease != null)
                {
                    result.Add(foundRelease);
                }
            }

            return result;
        }

        public async Task<AvailableRelease> GetAvailableReleasesAsync(ReleaseType releaseType)
        {
            if (AllReleases == null)
            {
                AllReleases = await GetAllAvailableReleases().ConfigureAwait(false);
            }

            var foundRelease = CheckForAvailableRelease(releaseType);
            if (foundRelease == null)
            {
                return null;
            }

            return foundRelease;
        }

        public async Task<string> DownloadReleaseAsync(AvailableRelease releaseToDownload)
        {
            try
            {
                if (AllReleases == null)
                {
                    return null;
                }

                logger.Debug($"Download asset for release {releaseToDownload.Version}.");

                var release = AllReleases.FirstOrDefault(x => x.Id == releaseToDownload.Id);
                if (release == null)
                {
                    logger.Warn($"No release found for {nameof(releaseToDownload.Id)}: {nameof(releaseToDownload.Id)}");
                    return null;
                }

                var assetName = GetAssetNameForReleaseType(releaseToDownload.Type);
                if (string.IsNullOrWhiteSpace(assetName))
                {
                    logger.Warn($"No asset name found for type: {releaseToDownload.Type}");
                    return null;
                }

                var foundAsset = release.Assets.FirstOrDefault(x => x.Name.StartsWith(assetName, StringComparison.OrdinalIgnoreCase));
                if (foundAsset == null && releaseToDownload.Type == ReleaseType.Presets)
                {
                    //check for release of presets with icons
                    releaseToDownload.Type = ReleaseType.PresetsAndIcons;
                    assetName = GetAssetNameForReleaseType(releaseToDownload.Type);
                    foundAsset = release.Assets.FirstOrDefault(x => x.Name.StartsWith(assetName, StringComparison.OrdinalIgnoreCase));
                }

                if (foundAsset == null)
                {
                    logger.Warn($"No asset found with name: {assetName}");
                    return null;
                }

                //download file to temp directory            
                var pathToDownloadedFile = await DownloadFileAsync(foundAsset.BrowserDownloadUrl, Path.GetTempFileName()).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(pathToDownloadedFile))
                {
                    logger.Warn("Could not get path to downloaded file.");
                    return null;
                }

                //move file to app directory
                var tempFileInfo = new FileInfo(pathToDownloadedFile);

                var pathToUpdatedPresetsFile = GetPathToUpdatedPresetsFile(releaseToDownload.Type);
                if (string.IsNullOrWhiteSpace(pathToUpdatedPresetsFile))
                {
                    logger.Warn($"Could not get path to updated presets file for type: {releaseToDownload.Type}");
                    return null;
                }

                if (File.Exists(pathToUpdatedPresetsFile))
                {
                    FileHelper.ResetFileAttributes(pathToUpdatedPresetsFile);
                    File.Delete(pathToUpdatedPresetsFile);
                }

                tempFileInfo.MoveTo(pathToUpdatedPresetsFile);

                return pathToUpdatedPresetsFile;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error downloading release.");
                App.ShowMessageWithUnexpectedErrorAndExit();
                return null;
            }
        }

        public async Task ReplaceUpdatedPresetsFilesAsync()
        {
            try
            {
                await Task.Run(async () =>
                {
                    foreach (ReleaseType curReleaseType in Enum.GetValues(typeof(ReleaseType)))
                    {
                        var pathToUpdatedPresetsFile = GetPathToUpdatedPresetsFile(curReleaseType);
                        if (string.IsNullOrWhiteSpace(pathToUpdatedPresetsFile) || !File.Exists(pathToUpdatedPresetsFile))
                        {
                            continue;
                        }

                        logger.Debug($"Start replacing presets with update: {pathToUpdatedPresetsFile}");

                        if (curReleaseType == ReleaseType.PresetsAndIcons)
                        {
                            using (var archive = ZipFile.OpenRead(pathToUpdatedPresetsFile))
                            {
                                foreach (var curEntry in archive.Entries)
                                {
                                    var destinationPath = Path.Combine(Path.GetDirectoryName(pathToUpdatedPresetsFile), curEntry.FullName);
                                    var destinationDirectory = Path.GetDirectoryName(destinationPath);

                                    if (!Directory.Exists(destinationDirectory))
                                    {
                                        Directory.CreateDirectory(destinationDirectory);
                                    }

                                    if (File.Exists(destinationPath))
                                    {
                                        FileHelper.ResetFileAttributes(destinationPath);
                                    }

                                    //Sometimes an entry has no name. (Temporary file? Directory?)
                                    if (!string.IsNullOrWhiteSpace(curEntry.Name))
                                    {
                                        curEntry.ExtractToFile(destinationPath, true);
                                    }
                                }
                            }

                            //wait extra time for extraction to finish (sometimes the disk needs extra time)
                            await Task.Delay(TimeSpan.FromMilliseconds(200));

                            File.Delete(pathToUpdatedPresetsFile);

                            logger.Debug("Finished extracting updated presets file.");

                            continue;
                        }

                        var originalPresetsFileName = Path.GetFileName(pathToUpdatedPresetsFile).Replace(CoreConstants.PrefixUpdatedPresetsFile, string.Empty);
                        var pathToOriginalPresetsFile = Path.Combine(Path.GetDirectoryName(pathToUpdatedPresetsFile), originalPresetsFileName);
                        if (File.Exists(pathToOriginalPresetsFile))
                        {
                            FileHelper.ResetFileAttributes(pathToOriginalPresetsFile);
                            File.Delete(pathToOriginalPresetsFile);
                        }

                        File.Move(pathToUpdatedPresetsFile, pathToOriginalPresetsFile);

                        logger.Debug("Finished replacing presets with update.");
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error replacing updated presets file.");

                _messageBoxService.ShowError(_localizationHelper.GetLocalization("UpdateErrorPresetMessage"),
                    _localizationHelper.GetLocalization("Error"));
            }
        }

        #endregion

        private async Task<IReadOnlyList<Release>> GetAllAvailableReleases()
        {
            try
            {
                if (!await ConnectivityHelper.IsConnected())
                {
                    logger.Info("Could not establish a connection to the internet.");

                    _messageBoxService.ShowError(_localizationHelper.GetLocalization("UpdateNoConnectionMessage"),
                            _localizationHelper.GetLocalization("Error"));

                    return null;
                }

                var releases = await ApiClient.Repository.Release.GetAll(GITHUB_USERNAME, GITHUB_PROJECTNAME).ConfigureAwait(false);
                if (releases == null || releases.Count < 1)
                {
                    return null;
                }

                return releases;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error getting info about preset updates.");
                return null;
            }
        }

        private AvailableRelease CheckForAvailableRelease(ReleaseType releaseType)
        {
            AvailableRelease result = null;

            var tagToCheck = GetTagForReleaseType(releaseType);
            if (string.IsNullOrWhiteSpace(tagToCheck))
            {
                return result;
            }

            logger.Debug($"Check for updates with tag: \"{tagToCheck}\"");

            var supportPrerelease = _appSettings.UpdateSupportsPrerelease;
            logger.Debug($"Update supports prereleases: {supportPrerelease}");

            Release foundPrerelease = null;
            if (supportPrerelease)
            {
                foundPrerelease = AllReleases.FirstOrDefault(x => !x.Draft && x.Prerelease && x.TagName.StartsWith(tagToCheck, StringComparison.OrdinalIgnoreCase));
            }

            Version versionPrerelease = default;
            if (foundPrerelease != null)
            {
                versionPrerelease = ParseVersionFromTag(foundPrerelease, tagToCheck);
                logger.Debug($"Found version (prerelease): {versionPrerelease}");
            }

            var foundRelease = AllReleases.FirstOrDefault(x => !x.Draft && !x.Prerelease && x.TagName.StartsWith(tagToCheck, StringComparison.OrdinalIgnoreCase));
            Version versionRelease = default;
            if (foundRelease != null)
            {
                versionRelease = ParseVersionFromTag(foundRelease, tagToCheck);
                logger.Debug($"Found version: {versionRelease}");
            }

            if (versionPrerelease == default && versionRelease == default)
            {
                return result;
            }

            if (versionPrerelease != default && versionPrerelease > versionRelease)
            {
                result = new AvailableRelease
                {
                    Id = foundPrerelease.Id,
                    Type = releaseType,
                    Version = versionPrerelease
                };
            }
            else
            {
                result = new AvailableRelease
                {
                    Id = foundRelease.Id,
                    Type = releaseType,
                    Version = versionRelease
                };
            }

            return result;
        }

        private Version ParseVersionFromTag(Release release, string tagToCheck)
        {
            var result = default(Version);

            var versionString = release.TagName.Replace(tagToCheck, string.Empty).Trim();
            if (!Version.TryParse(versionString, out var parsedVersion))
            {
                logger.Warn($"Could not parse version of release. ({nameof(release.TagName)}: {release.TagName})");
                return result;
            }

            result = parsedVersion;

            return result;
        }

        private string GetTagForReleaseType(ReleaseType releaseType)
        {
            var result = string.Empty;

            switch (releaseType)
            {
                case ReleaseType.Presets:
                case ReleaseType.PresetsAndIcons:
                    result = TAG_PRESETS;
                    break;
                case ReleaseType.PresetsIcons:
                    result = TAG_PRESETS_ICONS;
                    break;
                case ReleaseType.PresetsColors:
                    result = TAG_PRESETS_COLORS;
                    break;
                case ReleaseType.PresetsWikiBuildingInfo:
                    result = TAG_PRESETS_WIKIBUILDINGINFO;
                    break;
                case ReleaseType.Unknown:
                default:
                    break;
            }

            return result;
        }

        private string GetAssetNameForReleaseType(ReleaseType releaseType)
        {
            var result = string.Empty;

            switch (releaseType)
            {
                case ReleaseType.Presets:
                    result = CoreConstants.PresetsFiles.BuildingPresetsFile;
                    break;
                case ReleaseType.PresetsAndIcons:
                    result = "Presets.and.Icons.Update";
                    break;
                case ReleaseType.PresetsIcons:
                    result = CoreConstants.PresetsFiles.IconNameFile;
                    break;
                case ReleaseType.PresetsColors:
                    result = CoreConstants.PresetsFiles.ColorPresetsFile;
                    break;
                case ReleaseType.PresetsWikiBuildingInfo:
                    result = CoreConstants.PresetsFiles.WikiBuildingInfoPresetsFile;
                    break;
                case ReleaseType.Unknown:
                default:
                    break;
            }

            return result;
        }

        private string GetPathToUpdatedPresetsFile(ReleaseType releaseType)
        {
            var result = string.Empty;

            //maybe this will change for each presets type in the future

            switch (releaseType)
            {
                case ReleaseType.Presets:
                    result = Path.Combine(_basePath, CoreConstants.PrefixUpdatedPresetsFile + GetAssetNameForReleaseType(releaseType));
                    break;
                case ReleaseType.PresetsAndIcons:
                    result = Path.Combine(_basePath, CoreConstants.PrefixUpdatedPresetsFile + GetAssetNameForReleaseType(releaseType));
                    break;
                case ReleaseType.PresetsIcons:
                    result = Path.Combine(_basePath, CoreConstants.PrefixUpdatedPresetsFile + GetAssetNameForReleaseType(releaseType));
                    break;
                case ReleaseType.PresetsColors:
                    result = Path.Combine(_basePath, CoreConstants.PrefixUpdatedPresetsFile + GetAssetNameForReleaseType(releaseType));
                    break;
                case ReleaseType.PresetsWikiBuildingInfo:
                    result = Path.Combine(_basePath, CoreConstants.PrefixUpdatedPresetsFile + GetAssetNameForReleaseType(releaseType));
                    break;
                case ReleaseType.Unknown:
                default:
                    break;
            }

            return result;
        }

        private async Task<string> DownloadFileAsync(string url, string pathToSavedFile)
        {
            try
            {
                logger.Debug($"Start downloading file: {url}");

                var stream = await LocalHttpClient.GetStreamAsync(url).ConfigureAwait(false);
                using (var fileStream = new FileStream(pathToSavedFile, System.IO.FileMode.Create))
                {
                    await stream.CopyToAsync(fileStream).ConfigureAwait(false);
                }

                logger.Debug($"Finished downloading file to \"{pathToSavedFile}\"");

                return pathToSavedFile;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error downloading file ({url}).");
                return string.Empty;
            }
        }
    }
}
