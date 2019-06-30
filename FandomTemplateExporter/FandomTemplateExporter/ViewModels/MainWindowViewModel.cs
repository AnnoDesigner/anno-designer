using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AnnoDesigner.Core;
using AnnoDesigner.Core.Layout;
using AnnoDesigner.Core.Layout.Models;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Loader;
using AnnoDesigner.Core.Presets.Models;
using AnnoDesigner.Export;
using FandomParser.Core.Presets.Loader;
using FandomParser.Core.Presets.Models;
using FandomTemplateExporter.Models;
using NLog;

namespace FandomTemplateExporter.ViewModels
{
    public class MainWindowViewModel : BaseModel
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        //private const string DEFAULT_BUSY_CONTENT = "Please wait ...";
        private string _presetsVersion;
        private string _wikiBuildingInfoPresetsVersion;
        private string _statusMessage;
        private string _template;
        private string _layoutName;
        private bool _isBusy;
        //private string busyContent;
        private string _title = "Fandom Template Exporter";

        public MainWindowViewModel()
        {
            var applicationDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            PresetsVM = new SelectFileViewModel("Please select the presets.json file");
            PresetsVM.PropertyChanged += PresetsVM_PropertyChanged;
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastSelectedPresetsFilePath) && File.Exists(Properties.Settings.Default.LastSelectedPresetsFilePath))
            {
                PresetsVM.SelectedFile = Properties.Settings.Default.LastSelectedPresetsFilePath;
            }
            else
            {
                var presetsFileInApplicationDirectory = Path.Combine(applicationDirectory, CoreConstants.PresetsFiles.BuildingPresetsFile);
                if (File.Exists(presetsFileInApplicationDirectory))
                {
                    PresetsVM.SelectedFile = presetsFileInApplicationDirectory;
                }
            }

            WikiBuildingsInfoVM = new SelectFileViewModel("Please select the wikiBuildingInfo.json file");
            WikiBuildingsInfoVM.PropertyChanged += WikiBuildingsInfoVM_PropertyChanged;
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastSelectedWikiBuildingsInfoFilePath) && File.Exists(Properties.Settings.Default.LastSelectedWikiBuildingsInfoFilePath))
            {
                WikiBuildingsInfoVM.SelectedFile = Properties.Settings.Default.LastSelectedWikiBuildingsInfoFilePath;
            }
            else
            {
                var wikiBuildingsInfoFileInApplicationDirectory = Path.Combine(applicationDirectory, CoreConstants.PresetsFiles.WikiBuildingInfoPresetsFile);
                if (File.Exists(wikiBuildingsInfoFileInApplicationDirectory))
                {
                    WikiBuildingsInfoVM.SelectedFile = wikiBuildingsInfoFileInApplicationDirectory;
                }
            }

            LayoutVM = new SelectFileViewModel("Please select the layout file (*.ad)");
            LayoutVM.PropertyChanged += LayoutVM_PropertyChanged;
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastSelectedLayoutFilePath) && File.Exists(Properties.Settings.Default.LastSelectedLayoutFilePath))
            {
                LayoutVM.SelectedFile = Properties.Settings.Default.LastSelectedLayoutFilePath;
            }

            //BusyContent = DEFAULT_BUSY_CONTENT;

            ClosingWindowCommand = new RelayCommand(ClosingWindow, null);
            GenerateTemplateCommand = new AsyncDelegateCommand(GenerateTemplate, CanGenerateTemplate);
            CopyToClipboardCommand = new RelayCommand(CopyToClipboard, CanCopyToClipboard);
            StatusMessage = string.Empty;
            Template = string.Empty;
            LayoutName = string.Empty;
            IsBusy = false;
            PresetsVersion = string.Empty;
            WikiBuildingInfoPresetsVersion = string.Empty;

            var oldTitle = Title;
            try
            {
                //http://stackoverflow.com/a/1605873
                Title = $"{oldTitle} - {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion}";
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error adjusting the title.");
                Title = oldTitle;
            }
        }

        private void PresetsVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(PresetsVM.SelectedFile)))
            {
                Properties.Settings.Default.LastSelectedPresetsFilePath = PresetsVM.SelectedFile;

                logger.Info($"selected presets file: \"{PresetsVM.SelectedFile}\"");

                Template = string.Empty;
                LayoutName = string.Empty;
                StatusMessage = string.Empty;
            }
        }

        private void WikiBuildingsInfoVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(WikiBuildingsInfoVM.SelectedFile)))
            {
                Properties.Settings.Default.LastSelectedWikiBuildingsInfoFilePath = WikiBuildingsInfoVM.SelectedFile;

                logger.Info($"selected wiki building info file: \"{WikiBuildingsInfoVM.SelectedFile}\"");

                Template = string.Empty;
                LayoutName = string.Empty;
                StatusMessage = string.Empty;
            }
        }

        private void LayoutVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(LayoutVM.SelectedFile)))
            {
                Properties.Settings.Default.LastSelectedLayoutFilePath = LayoutVM.SelectedFile;

                logger.Info($"selected layout file: \"{LayoutVM.SelectedFile}\"");

                Template = string.Empty;
                LayoutName = string.Empty;
                StatusMessage = string.Empty;
            }
        }

        public SelectFileViewModel PresetsVM { get; }

        public SelectFileViewModel WikiBuildingsInfoVM { get; }

        public SelectFileViewModel LayoutVM { get; }

        #region commands

        public ICommand ClosingWindowCommand { get; private set; }

        private void ClosingWindow(object param)
        {
            var userConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            logger.Trace($"saving settings: \"{userConfig}\"");
            Properties.Settings.Default.Save();
        }

        public IAsyncCommand GenerateTemplateCommand { get; private set; }

        private async Task GenerateTemplate(object param)
        {
            try
            {
                IsBusy = true;
                StatusMessage = string.Empty;

                var buildingPresetsTask = Task.Run(() =>
                {
                    //load building presets
                    BuildingPresets localBuildingPresets = null;
                    try
                    {
                        var loader = new BuildingPresetsLoader();
                        localBuildingPresets = loader.Load(PresetsVM.SelectedFile);
                        if (localBuildingPresets == null || localBuildingPresets.Buildings == null)
                        {
                            throw new ArgumentException();
                        }
                    }
                    catch (Exception ex)
                    {
                        var message = $"Error parsing {nameof(BuildingPresets)}.";
                        logger.Error(ex, message);
                        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                        StatusMessage = $"{message} -> Maybe wrong selected file?";
                        return null;
                    }

                    PresetsVersion = localBuildingPresets.Version;

                    return localBuildingPresets;
                });

                var wikiBuildingInfoPresetsTask = Task.Run(() =>
                {
                    //load wiki buildng info
                    WikiBuildingInfoPresets localWikiBuildingInfoPresets = null;
                    try
                    {
                        var loader = new WikiBuildingInfoPresetsLoader();
                        localWikiBuildingInfoPresets = loader.Load(WikiBuildingsInfoVM.SelectedFile);
                    }
                    catch (Exception ex)
                    {
                        var message = $"Error parsing {nameof(WikiBuildingInfoPresets)}.";
                        logger.Error(ex, message);
                        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                        StatusMessage = $"{message} -> Maybe wrong selected file?";
                        return null;
                    }

                    WikiBuildingInfoPresetsVersion = localWikiBuildingInfoPresets.Version.ToString();

                    return localWikiBuildingInfoPresets;

                });

                var layoutTask = Task.Run(() =>
                {
                    //load layout
                    List<AnnoObject> localLayout = null;
                    try
                    {
                        ILayoutLoader loader = new LayoutLoader();
                        localLayout = loader.LoadLayout(LayoutVM.SelectedFile);
                    }
                    catch (Exception ex)
                    {
                        var message = "Error parsing layout file.";
                        logger.Error(ex, message);
                        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                        StatusMessage = $"{message} -> Maybe wrong selected file?";
                        return null;
                    }

                    LayoutName = Path.GetFileName(LayoutVM.SelectedFile);

                    return localLayout;
                });

                await Task.WhenAll(buildingPresetsTask, wikiBuildingInfoPresetsTask, layoutTask);

                var buildingPresets = buildingPresetsTask.Result;
                var wikiBuildingInfoPresets = wikiBuildingInfoPresetsTask.Result;
                var layout = layoutTask.Result;

                if (buildingPresets == null || wikiBuildingInfoPresets == null || layout == null)
                {
                    return;
                }

                var layoutNameForTemplate = Path.GetFileNameWithoutExtension(LayoutVM.SelectedFile).Replace("_", " ");

                await Task.Run(() =>
                {
                    var exporter = new FandomExporter();
                    Template = exporter.StartExport(layoutNameForTemplate, layout, buildingPresets, wikiBuildingInfoPresets, true);
                });

                StatusMessage = "Template successfully generated.";
            }
            catch (Exception ex)
            {
                var message = "Error generating template.";
                logger.Error(ex, message);
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                StatusMessage = $"{message} -> Details in log file.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanGenerateTemplate(object param)
        {
            return !IsBusy &&
                !string.IsNullOrWhiteSpace(PresetsVM.SelectedFile) &&
                !string.IsNullOrWhiteSpace(WikiBuildingsInfoVM.SelectedFile) &&
                !string.IsNullOrWhiteSpace(LayoutVM.SelectedFile);
        }

        public ICommand CopyToClipboardCommand { get; private set; }

        private void CopyToClipboard(object param)
        {
            logger.Trace($"copy layout to clipboard:{Environment.NewLine}{Template}");
            Clipboard.SetText(Template, TextDataFormat.UnicodeText);
        }

        private bool CanCopyToClipboard(object param)
        {
            return !IsBusy && !string.IsNullOrWhiteSpace(Template);
        }

        #endregion

        public string Title
        {
            get { return _title; }
            set { SetPropertyAndNotify(ref _title, value); }
        }

        public string PresetsVersion
        {
            get { return _presetsVersion; }
            set { SetPropertyAndNotify(ref _presetsVersion, value); }
        }

        public string WikiBuildingInfoPresetsVersion
        {
            get { return _wikiBuildingInfoPresetsVersion; }
            set { SetPropertyAndNotify(ref _wikiBuildingInfoPresetsVersion, value); }
        }

        public string Template
        {
            get { return _template; }
            set { SetPropertyAndNotify(ref _template, value); }
        }

        public string LayoutName
        {
            get { return _layoutName; }
            set { SetPropertyAndNotify(ref _layoutName, value); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetPropertyAndNotify(ref _statusMessage, value); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                //if (_isBusy != value)
                //{
                SetPropertyAndNotify(ref _isBusy, value);
                CommandManager.InvalidateRequerySuggested();
                //}
            }
        }

    }
}
