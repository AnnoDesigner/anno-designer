using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AnnoDesigner.Core;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Presets.Loader;
using AnnoDesigner.Core.Presets.Models;
using FandomTemplateExporter.Models;
using FandomTemplateExporter.ViewModels;

namespace FandomTemplateExporter.ViewModels
{
    public class MainWindowViewModel : BaseModel
    {
        //private const string DEFAULT_BUSY_CONTENT = "Please wait ...";

        private readonly SelectFileViewModel vmPresets;
        private readonly SelectFileViewModel vmWikiBuildingsInfo;
        private readonly SelectFileViewModel vmLayout;
        private string _presetsVersion;
        private string _statusMessage;
        private string _template;
        //private string busyContent;
        private string _title = "Fandom Template Exporter";

        public MainWindowViewModel()
        {
            var applicationDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            vmPresets = new SelectFileViewModel("Please select the presets.json file");
            vmPresets.PropertyChanged += vmPresets_PropertyChanged;
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastSelectedPresetsFilePath) && File.Exists(Properties.Settings.Default.LastSelectedPresetsFilePath))
            {
                vmPresets.SelectedFile = Properties.Settings.Default.LastSelectedPresetsFilePath;
            }
            else
            {
                var presetsFileInApplicationDirectory = Path.Combine(applicationDirectory, CoreConstants.BuildingPresetsFile);
                if (File.Exists(presetsFileInApplicationDirectory))
                {
                    vmPresets.SelectedFile = presetsFileInApplicationDirectory;
                }
            }

            vmWikiBuildingsInfo = new SelectFileViewModel("Please select the wikiBuildingInfo.json file");
            vmWikiBuildingsInfo.PropertyChanged += vmWikiBuildingsInfo_PropertyChanged;
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastSelectedWikiBuildingsInfoFilePath) && File.Exists(Properties.Settings.Default.LastSelectedWikiBuildingsInfoFilePath))
            {
                vmWikiBuildingsInfo.SelectedFile = Properties.Settings.Default.LastSelectedWikiBuildingsInfoFilePath;
            }
            else
            {
                var wikiBuildingsInfoFileInApplicationDirectory = Path.Combine(applicationDirectory, FandomParser.Core.CoreConstants.WikiBuildingInfoPresetsFile);
                if (File.Exists(wikiBuildingsInfoFileInApplicationDirectory))
                {
                    vmWikiBuildingsInfo.SelectedFile = wikiBuildingsInfoFileInApplicationDirectory;
                }
            }

            vmLayout = new SelectFileViewModel("Please select the layout file (*.ad)");
            vmLayout.PropertyChanged += vmLayout_PropertyChanged;
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastSelectedLayoutFilePath) && File.Exists(Properties.Settings.Default.LastSelectedLayoutFilePath))
            {
                vmLayout.SelectedFile = Properties.Settings.Default.LastSelectedLayoutFilePath;
            }

            //BusyContent = DEFAULT_BUSY_CONTENT;

            ClosingWindowCommand = new RelayCommand(ClosingWindow, null);
            GenerateTemplateCommand = new RelayCommand(GenerateTemplate, CanGenerateTemplate);
            //SaveCommand = new RelayCommand(Save, CanSave);
            StatusMessage = string.Empty;
            Template = string.Empty;

            var oldTitle = Title;
            try
            {
                //http://stackoverflow.com/a/1605873
                Title = $"{oldTitle} - {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion}";
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error adjusting the title.{Environment.NewLine}{ex}");
                Title = oldTitle;
            }
        }

        private void vmPresets_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(vmPresets.SelectedFile)))
            {
                Properties.Settings.Default.LastSelectedPresetsFilePath = vmPresets.SelectedFile;
            }
        }

        private void vmWikiBuildingsInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(vmWikiBuildingsInfo.SelectedFile)))
            {
                Properties.Settings.Default.LastSelectedWikiBuildingsInfoFilePath = vmWikiBuildingsInfo.SelectedFile;
            }
        }

        private void vmLayout_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(vmLayout.SelectedFile)))
            {
                Properties.Settings.Default.LastSelectedLayoutFilePath = vmLayout.SelectedFile;
            }
        }

        public SelectFileViewModel PresetsVM { get { return vmPresets; } }

        public SelectFileViewModel WikiBuildingsInfoVM { get { return vmWikiBuildingsInfo; } }

        public SelectFileViewModel LayoutVM { get { return vmLayout; } }

        #region commands

        public ICommand ClosingWindowCommand { get; private set; }

        private void ClosingWindow(object param)
        {
            var userConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            Trace.WriteLine($"saving settings: \"{userConfig}\"");
            Properties.Settings.Default.Save();
        }

        public ICommand GenerateTemplateCommand { get; private set; }

        private void GenerateTemplate(object param)
        {
            //IsBusy = true;

            StatusMessage = string.Empty;

            BuildingPresets buildingPresets = null;
            try
            {
                buildingPresets = SerializationHelper.LoadFromFile<BuildingPresets>(PresetsVM.SelectedFile);
            }
            catch (Exception ex)
            {
                var message = $"Error parsing {nameof(BuildingPresets)}.";
                Trace.WriteLine($"{message}{Environment.NewLine}{ex}");
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                StatusMessage = $"{message} -> Maybe wrong selected file?";
                return;
            }

            PresetsVersion = buildingPresets.Version;

            //IsBusy = false;
        }

        private bool CanGenerateTemplate(object param)
        {
            return //!IsBusy &&
                           !string.IsNullOrWhiteSpace(PresetsVM.SelectedFile) &&
                           !string.IsNullOrWhiteSpace(WikiBuildingsInfoVM.SelectedFile) &&
                           !string.IsNullOrWhiteSpace(LayoutVM.SelectedFile);
        }

        //public ICommand SaveCommand { get; private set; }

        //private void Save(object param)
        //{
        //    try
        //    {
        //        StatusMessage = string.Empty;

        //        var colorPresets = new ColorPresets
        //        {
        //            Version = ColorPresetsVersionUpdated
        //        };

        //        colorPresets.AvailableSchemes.AddRange(AvailableColorSchemes.Select(x => new ColorScheme
        //        {
        //            Name = x.Name,
        //            Colors = x.Colors.Select(color => new PredefinedColor
        //            {
        //                TargetTemplate = color.TargetTemplate,
        //                TargetIdentifiers = color.TargetIdentifiers.ToList(),
        //                Color = color.SelectedColor.Value
        //            }).ToList()
        //        }));

        //        var backupFilePath = vmColors.SelectedFile + ".bak";
        //        File.Copy(vmColors.SelectedFile, backupFilePath, true);
        //        SerializationHelper.SaveToFile<ColorPresets>(colorPresets, vmColors.SelectedFile);

        //        MessageBox.Show("New colors.json was saved.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

        //        StatusMessage = $"Backup created \"{backupFilePath}\".";
        //    }
        //    catch (Exception ex)
        //    {
        //        var message = "Error saving colors.json.";
        //        Trace.WriteLine($"{message}{Environment.NewLine}{ex}");
        //        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        //        StatusMessage = $"{message} -> Could not save to \"{vmColors.SelectedFile}\"";
        //        return;
        //    }
        //}

        //private bool CanSave(object param)
        //{
        //    //TODO detect changes
        //    return true;
        //}

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

        public string Template
        {
            get { return _template; }
            set { SetPropertyAndNotify(ref _template, value); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetPropertyAndNotify(ref _statusMessage, value); }
        }

        //        public bool IsBusy
        //        {
        //            get { return isBusy; }
        //            set
        //            {
        //                //if (isBusy != value)
        //                //{
        //                SetPropertyAndNotify(ref isBusy, value);
        //                CommandManager.InvalidateRequerySuggested();
        //                //}
        //            }
        //        }

    }
}
