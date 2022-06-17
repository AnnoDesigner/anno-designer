using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Presets.Loader;
using AnnoDesigner.Core.Presets.Models;
using ColorPresetsDesigner.Models;
using NLog;

namespace ColorPresetsDesigner.ViewModels
{
    public class MainWindowViewModel : BaseModel
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        //private const string DEFAULT_BUSY_CONTENT = "Please wait ...";
        private const string NO_TEMPLATE_NAME = "NO_TEMPLATE";
        private ObservableCollection<ColorSchemeViewModel> _availableColorSchemes;
        private ColorSchemeViewModel _selectedColorScheme;
        private string _colorPresetsVersion;
        private string _colorPresetsVersionUpdated;
        private string _presetsVersion;
        private string _statusMessage;
        private PredefinedColorViewModel _selectedPredefinedColor;
        private ObservableCollection<PredefinedColorViewModel> _availablePredefinedColors;
        private ObservableCollection<string> _availableTemplates;
        private ObservableCollection<string> _availableIdentifiers;
        private Dictionary<string, List<string>> _templateIdentifierMapping;
        //private string busyContent;
        private string _title = "Color Presets Designer";
        private string _newColorSchemeName;

        public MainWindowViewModel()
        {
            PresetsVM = new SelectFileViewModel("Please select the presets.json file");
            PresetsVM.PropertyChanged += PresetsVM_PropertyChanged;
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastSelectedPresetsFilePath) && File.Exists(Properties.Settings.Default.LastSelectedPresetsFilePath))
            {
                PresetsVM.SelectedFile = Properties.Settings.Default.LastSelectedPresetsFilePath;
            }

            ColorsVM = new SelectFileViewModel("Please select the colors.json file");
            ColorsVM.PropertyChanged += ColorsVM_PropertyChanged;
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastSelectedColorsFilePath) && File.Exists(Properties.Settings.Default.LastSelectedColorsFilePath))
            {
                ColorsVM.SelectedFile = Properties.Settings.Default.LastSelectedColorsFilePath;
            }

            //BusyContent = DEFAULT_BUSY_CONTENT;

            _templateIdentifierMapping = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            ClosingWindowCommand = new RelayCommand(ClosingWindow, null);
            LoadPresetDataCommand = new RelayCommand(LoadPresetData, CanLoadPresetData);
            AddColorCommand = new RelayCommand(AddColor, CanAddColor);
            SaveCommand = new RelayCommand(Save, CanSave);
            AddColorSchemeCommand = new RelayCommand(AddColorScheme, CanAddColorScheme);
            DeleteColorSchemeCommand = new RelayCommand(DeleteColorScheme, CanDeleteColorScheme);
            DeleteColorCommand = new RelayCommand(DeleteColor, CanDeleteColor);
            CopyColorCommand = new RelayCommand(CopyColor, CanCopyColor);

            AvailableColorSchemes = new ObservableCollection<ColorSchemeViewModel>();
            AvailablePredefinedColors = new ObservableCollection<PredefinedColorViewModel>();
            AvailableTemplates = new ObservableCollection<string>();
            AvailableIdentifiers = new ObservableCollection<string>();
            PresetsVersion = string.Empty;
            ColorPresetsVersion = string.Empty;
            ColorPresetsVersionUpdated = string.Empty;
            NewColorSchemeName = string.Empty;
            StatusMessage = string.Empty;

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
            }
        }

        private void ColorsVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(ColorsVM.SelectedFile)))
            {
                Properties.Settings.Default.LastSelectedColorsFilePath = ColorsVM.SelectedFile;
                logger.Info($"selected color presets file: \"{ColorsVM.SelectedFile}\"");
            }
        }

        public SelectFileViewModel PresetsVM { get; }

        public SelectFileViewModel ColorsVM { get; }

        #region commands

        public ICommand ClosingWindowCommand { get; private set; }

        private void ClosingWindow(object param)
        {
            var userConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            logger.Trace($"saving settings: \"{userConfig}\"");
            Properties.Settings.Default.Save();
        }

        public ICommand LoadPresetDataCommand { get; private set; }

        private void LoadPresetData(object param)
        {
            //IsBusy = true;

            StatusMessage = string.Empty;

            BuildingPresets buildingPresets = null;
            try
            {
                buildingPresets = SerializationHelper.LoadFromFile<BuildingPresets>(PresetsVM.SelectedFile);
                if (buildingPresets == null || buildingPresets.Buildings == null)
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
                return;
            }

            PresetsVersion = buildingPresets.Version;
            logger.Info($"loaded presets version: {PresetsVersion}");

            fillAvailableTemplates(buildingPresets);
            fillAvailableIdentifiers(buildingPresets);
            fillTemplateIdentifierMapping(buildingPresets);

            AvailableColorSchemes.Clear();

            ColorPresets colorPresets = null;
            try
            {
                ColorPresetsLoader loader = new ColorPresetsLoader();
                colorPresets = loader.Load(ColorsVM.SelectedFile);
                if (colorPresets == null || colorPresets.AvailableSchemes == null)
                {
                    throw new ArgumentException();
                }
            }
            catch (Exception ex)
            {
                var message = $"Error parsing {nameof(ColorPresets)}.";
                logger.Error(ex, message);
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                StatusMessage = $"{message} -> Maybe wrong selected file?";
                return;
            }

            logger.Info($"loaded color presets version: {colorPresets.Version}");

            foreach (var curScheme in colorPresets.AvailableSchemes)
            {
                AvailableColorSchemes.Add(new ColorSchemeViewModel(curScheme));
            }
            //var defaultScheme = loader.LoadDefaultScheme(vmColors.SelectedFile);

            ColorPresetsVersion = colorPresets.Version;
            ColorPresetsVersionUpdated = ColorPresetsVersion;

            SelectedColorScheme = AvailableColorSchemes.First();

            //IsBusy = false;
        }

        private bool CanLoadPresetData(object param)
        {
            return //!IsBusy &&
                           !string.IsNullOrWhiteSpace(PresetsVM.SelectedFile) &&
                           !string.IsNullOrWhiteSpace(ColorsVM.SelectedFile);
        }

        public ICommand AddColorCommand { get; private set; }

        private void AddColor(object param)
        {
            var newColor = new PredefinedColorViewModel(new PredefinedColor
            {
                Color = Colors.Red,
                TargetTemplate = AvailableTemplates.First()
            });

            SelectedColorScheme.Colors.Add(newColor);
            AvailablePredefinedColors.Add(newColor);
        }

        private bool CanAddColor(object param)
        {
            return SelectedColorScheme != null && AvailableTemplates.Any();
        }

        public ICommand DeleteColorCommand { get; private set; }

        private void DeleteColor(object param)
        {
            SelectedColorScheme.Colors.Remove(SelectedPredefinedColor);
            updateAvailablePredefinedColors();

            SelectedPredefinedColor = AvailablePredefinedColors.FirstOrDefault();
        }

        private bool CanDeleteColor(object param)
        {
            return SelectedPredefinedColor != null;
        }

        public ICommand CopyColorCommand { get; private set; }

        private void CopyColor(object param)
        {
            var copiedColor = new PredefinedColorViewModel
            {
                TargetTemplate = SelectedPredefinedColor.TargetTemplate,
                TargetIdentifiers = new ObservableCollection<string>(SelectedPredefinedColor.TargetIdentifiers),
                SelectedColor = SelectedPredefinedColor.SelectedColor
            };

            SelectedColorScheme.Colors.Add(copiedColor);
            updateAvailablePredefinedColors();

            SelectedPredefinedColor = copiedColor;
        }

        private bool CanCopyColor(object param)
        {
            return SelectedPredefinedColor != null;
        }

        public ICommand AddColorSchemeCommand { get; private set; }

        private void AddColorScheme(object param)
        {
            var newScheme = new ColorSchemeViewModel(new ColorScheme
            {
                Name = NewColorSchemeName
            });

            AvailableColorSchemes.Add(newScheme);
            SelectedColorScheme = newScheme;
        }

        private bool CanAddColorScheme(object param)
        {
            return !String.IsNullOrWhiteSpace(NewColorSchemeName) &&
                !AvailableColorSchemes.Any(x => x.Name.Equals(NewColorSchemeName, StringComparison.OrdinalIgnoreCase));
        }

        public ICommand DeleteColorSchemeCommand { get; private set; }

        private void DeleteColorScheme(object param)
        {
            AvailableColorSchemes.Remove(SelectedColorScheme);
            SelectedColorScheme = AvailableColorSchemes.First();
        }

        private bool CanDeleteColorScheme(object param)
        {
            return SelectedColorScheme != null &&
                !SelectedColorScheme.Name.Equals("Default", StringComparison.OrdinalIgnoreCase);
        }

        public ICommand SaveCommand { get; private set; }

        private void Save(object param)
        {
            try
            {
                StatusMessage = string.Empty;

                var colorPresets = new ColorPresets
                {
                    Version = ColorPresetsVersionUpdated
                };

                colorPresets.AvailableSchemes.AddRange(AvailableColorSchemes.Select(x => new ColorScheme
                {
                    Name = x.Name,
                    Colors = x.Colors.Select(color => new PredefinedColor
                    {
                        TargetTemplate = color.TargetTemplate,
                        TargetIdentifiers = color.TargetIdentifiers.ToList(),
                        Color = color.SelectedColor.Value
                    }).ToList()
                }));

                var backupFilePath = ColorsVM.SelectedFile + ".bak";
                logger.Trace($"create backup of \"{ColorsVM.SelectedFile}\" at \"{backupFilePath}\"");
                File.Copy(ColorsVM.SelectedFile, backupFilePath, true);
                StatusMessage = $"Backup created \"{backupFilePath}\".";

                logger.Info($"save color presets file: \"{ColorsVM.SelectedFile}\" ({colorPresets.Version})");
                SerializationHelper.SaveToFile<ColorPresets>(colorPresets, ColorsVM.SelectedFile);

                MessageBox.Show("New colors.json was saved.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                var message = "Error saving colors.json.";
                logger.Error(ex, message);
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                StatusMessage = $"{message} -> Could not save to \"{ColorsVM.SelectedFile}\"";
                return;
            }
        }

        private bool CanSave(object param)
        {
            //TODO detect changes
            return true;
        }

        #endregion

        public string Title
        {
            get { return _title; }
            set { SetPropertyAndNotify(ref _title, value); }
        }

        public ObservableCollection<ColorSchemeViewModel> AvailableColorSchemes
        {
            get { return _availableColorSchemes; }
            set { SetPropertyAndNotify(ref _availableColorSchemes, value); }
        }

        public ColorSchemeViewModel SelectedColorScheme
        {
            get { return _selectedColorScheme; }
            set
            {
                if (SetPropertyAndNotify(ref _selectedColorScheme, value))
                {
                    updateAvailablePredefinedColors();
                    logger.Trace($"selected color scheme: {_selectedColorScheme.Name}");
                }
            }
        }

        private void updateAvailablePredefinedColors()
        {
            AvailablePredefinedColors.Clear();

            if (SelectedColorScheme == null)
            {
                return;
            }

            foreach (var curColor in SelectedColorScheme.Colors)
            {
                AvailablePredefinedColors.Add(curColor);
            }
        }

        public ObservableCollection<PredefinedColorViewModel> AvailablePredefinedColors
        {
            get { return _availablePredefinedColors; }
            set { SetPropertyAndNotify(ref _availablePredefinedColors, value); }
        }

        public PredefinedColorViewModel SelectedPredefinedColor
        {
            get { return _selectedPredefinedColor; }
            set
            {
                if (_selectedPredefinedColor != null)
                {
                    _selectedPredefinedColor.OnTargetTemplateChanged -= _selectedPredefinedColor_OnTargetTemplateChanged;
                    _selectedPredefinedColor.OnTargetIdentifiersChanged -= _selectedPredefinedColor_OnTargetIdentifiersChanged;
                }

                if (SetPropertyAndNotify(ref _selectedPredefinedColor, value))
                {
                    OnPropertyChanged(nameof(ShowColorEdit));
                    OnPropertyChanged(nameof(AvailableIdentifiersForTemplate));
                }

                if (_selectedPredefinedColor != null)
                {
                    _selectedPredefinedColor.OnTargetTemplateChanged += _selectedPredefinedColor_OnTargetTemplateChanged;
                    _selectedPredefinedColor.OnTargetIdentifiersChanged += _selectedPredefinedColor_OnTargetIdentifiersChanged;
                }
            }
        }

        public string PresetsVersion
        {
            get { return _presetsVersion; }
            set { SetPropertyAndNotify(ref _presetsVersion, value); }
        }

        public string ColorPresetsVersion
        {
            get { return _colorPresetsVersion; }
            set { SetPropertyAndNotify(ref _colorPresetsVersion, value); }
        }

        public string ColorPresetsVersionUpdated
        {
            get { return _colorPresetsVersionUpdated; }
            set { SetPropertyAndNotify(ref _colorPresetsVersionUpdated, value); }
        }

        public bool ShowColorEdit
        {
            get { return SelectedPredefinedColor != null; }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetPropertyAndNotify(ref _statusMessage, value); }
        }

        public ObservableCollection<string> AvailableTemplates
        {
            get { return _availableTemplates; }
            set { SetPropertyAndNotify(ref _availableTemplates, value); }
        }

        public ObservableCollection<string> AvailableIdentifiers
        {
            get { return _availableIdentifiers; }
            set { SetPropertyAndNotify(ref _availableIdentifiers, value); }
        }

        public ObservableCollection<string> AvailableIdentifiersForTemplate
        {
            get
            {
                var result = new ObservableCollection<string>();

                if (SelectedPredefinedColor == null ||
                    string.IsNullOrWhiteSpace(SelectedPredefinedColor.TargetTemplate) ||
                    !_templateIdentifierMapping.ContainsKey(SelectedPredefinedColor.TargetTemplate))
                {
                    return result;
                }

                //add identifiers for template
                var filteredIdentifiers = _templateIdentifierMapping[SelectedPredefinedColor.TargetTemplate];
                foreach (var curIdentifierName in filteredIdentifiers)
                {
                    result.Add(curIdentifierName);
                }

                //add identifiers without template
                if (_templateIdentifierMapping.ContainsKey(NO_TEMPLATE_NAME))
                {
                    var identifiersWithoutTemplate = _templateIdentifierMapping[NO_TEMPLATE_NAME];
                    foreach (var curIdentifierName in identifiersWithoutTemplate)
                    {
                        result.Add(curIdentifierName);
                    }
                }

                //remove already added identifiers (present in this template)
                foreach (var alreadyAddedIdentifier in SelectedPredefinedColor.TargetIdentifiers)
                {
                    result.Remove(alreadyAddedIdentifier);
                }

                //remove already added identifiers (present in all templates)
                foreach (var curPredefinedColor in SelectedColorScheme.Colors)
                {
                    foreach (var curIdentifier in curPredefinedColor.TargetIdentifiers)
                    {
                        result.Remove(curIdentifier);
                    }
                }

                return result;
            }
        }

        public string NewColorSchemeName
        {
            get { return _newColorSchemeName; }
            set { SetPropertyAndNotify(ref _newColorSchemeName, value); }
        }

        private void fillAvailableTemplates(BuildingPresets buildingPresets)
        {
            AvailableTemplates.Clear();

            var allTemplates = new Dictionary<string, int>();
            foreach (var curBuilding in buildingPresets.Buildings)
            {
                if (string.IsNullOrWhiteSpace(curBuilding.Template))
                {
                    continue;
                }

                if (!allTemplates.ContainsKey(curBuilding.Template))
                {
                    allTemplates.Add(curBuilding.Template, 1);
                }
                else
                {
                    allTemplates[curBuilding.Template] = ++allTemplates[curBuilding.Template];
                }
            }

            var templateListOrderedByOccurrence = allTemplates.OrderByDescending(x => x.Value).ToList();
            var templateNameList = allTemplates.OrderBy(x => x.Key).Select(x => x.Key).ToList();

            foreach (var curTemplateName in templateNameList)
            {
                AvailableTemplates.Add(curTemplateName);
            }
        }

        private void fillAvailableIdentifiers(BuildingPresets buildingPresets)
        {
            AvailableIdentifiers.Clear();

            var allIdentifiers = new Dictionary<string, int>();
            foreach (var curBuilding in buildingPresets.Buildings)
            {
                if (string.IsNullOrWhiteSpace(curBuilding.Identifier))
                {
                    continue;
                }

                if (!allIdentifiers.ContainsKey(curBuilding.Identifier))
                {
                    allIdentifiers.Add(curBuilding.Identifier, 1);
                }
                else
                {
                    allIdentifiers[curBuilding.Identifier] = ++allIdentifiers[curBuilding.Identifier];
                }
            }

            var identifierListOrderedByOccurrence = allIdentifiers.OrderByDescending(x => x.Value).ToList();
            var identifierNameList = allIdentifiers.OrderBy(x => x.Key).Select(x => x.Key).ToList();

            foreach (var curIdentifierName in identifierNameList)
            {
                AvailableIdentifiers.Add(curIdentifierName);
            }
        }

        private void fillTemplateIdentifierMapping(BuildingPresets buildingPresets)
        {
            _templateIdentifierMapping.Clear();

            foreach (var curBuilding in buildingPresets.Buildings)
            {
                if (string.IsNullOrWhiteSpace(curBuilding.Identifier))
                {
                    continue;
                }

                var templateName = curBuilding.Template;
                if (string.IsNullOrWhiteSpace(templateName))
                {
                    templateName = NO_TEMPLATE_NAME;
                }

                if (!_templateIdentifierMapping.ContainsKey(templateName))
                {
                    _templateIdentifierMapping.Add(templateName, new List<string> { curBuilding.Identifier });
                }
                else
                {
                    _templateIdentifierMapping[templateName].Add(curBuilding.Identifier);
                }
            }
        }

        private void _selectedPredefinedColor_OnTargetTemplateChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(AvailableIdentifiersForTemplate));
        }

        private void _selectedPredefinedColor_OnTargetIdentifiersChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(AvailableIdentifiersForTemplate));
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
