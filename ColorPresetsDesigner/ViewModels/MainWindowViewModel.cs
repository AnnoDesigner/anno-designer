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
using AnnoDesigner;
using AnnoDesigner.Presets;
using AnnoDesigner.PresetsLoader;
using ColorPresetsDesigner.Models;

namespace ColorPresetsDesigner.ViewModels
{
    public class MainWindowViewModel : BaseModel
    {
        //        private const string DEFAULT_BUSY_CONTENT = "Prüfung läuft ...";

        private readonly SelectFileViewModel vmPresets;
        private readonly SelectFileViewModel vmColors;
        private ObservableCollection<ColorSchemeViewModel> _availableColorSchemes;
        private ColorSchemeViewModel _selectedColorScheme;
        private string _colorPresetsVersion;
        private string _colorPresetsVersionUpdated;
        private string _presetsVersion;
        private PredefinedColorViewModel _selectedPredefinedColor;
        private ObservableCollection<PredefinedColorViewModel> _availablePredefinedColors;
        private ObservableCollection<string> _availableTemplates;
        private ObservableCollection<string> _availableIdentifiers;
        //        private string busyContent;        
        private string _title = "Color Presets Designer";
        private string _newColorSchemeName;

        public MainWindowViewModel()
        {
            vmPresets = new SelectFileViewModel("Please select the presets.json file");
            vmPresets.PropertyChanged += vmPresets_PropertyChanged;
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastSelectedPresetsFilePath) && File.Exists(Properties.Settings.Default.LastSelectedPresetsFilePath))
            {
                vmPresets.SelectedFile = Properties.Settings.Default.LastSelectedPresetsFilePath;
            }

            vmColors = new SelectFileViewModel("Please select the colors.json file");
            vmColors.PropertyChanged += vmColors_PropertyChanged;
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastSelectedColorsFilePath) && File.Exists(Properties.Settings.Default.LastSelectedColorsFilePath))
            {
                vmColors.SelectedFile = Properties.Settings.Default.LastSelectedColorsFilePath;
            }

            //            BusyContent = DEFAULT_BUSY_CONTENT;

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
            PresetsVersion = String.Empty;
            ColorPresetsVersion = String.Empty;
            ColorPresetsVersionUpdated = String.Empty;
            NewColorSchemeName = String.Empty;

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

        private void vmColors_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(vmColors.SelectedFile)))
            {
                Properties.Settings.Default.LastSelectedColorsFilePath = vmColors.SelectedFile;
            }
        }

        public SelectFileViewModel PresetsVM { get { return vmPresets; } }

        public SelectFileViewModel ColorsVM { get { return vmColors; } }

        #region commands

        public ICommand ClosingWindowCommand { get; private set; }

        private void ClosingWindow(object param)
        {
            var userConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            Trace.WriteLine($"saving settings: \"{userConfig}\"");
            Properties.Settings.Default.Save();
        }

        public ICommand LoadPresetDataCommand { get; private set; }

        private void LoadPresetData(object param)
        {
            //IsBusy = true;

            var buildingPresets = DataIO.LoadFromFile<BuildingPresets>(PresetsVM.SelectedFile);

            PresetsVersion = buildingPresets.Version;

            fillAvailableTemplates(buildingPresets);
            fillAvailableIdentifiers(buildingPresets);

            AvailableColorSchemes.Clear();

            ColorPresetsLoader loader = new ColorPresetsLoader();
            var colorPresets = loader.Load(ColorsVM.SelectedFile);
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

            SelectedPredefinedColor = AvailablePredefinedColors.First();
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

            File.Copy(vmColors.SelectedFile, vmColors.SelectedFile + ".bak", true);
            //DataIO.SaveToFile<ColorPresets>(colorPresets, "test.json");
            DataIO.SaveToFile<ColorPresets>(colorPresets, vmColors.SelectedFile);

            MessageBox.Show("New colors.json was saved.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanSave(object param)
        {
            //detect changes
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
                }
            }
        }

        private void updateAvailablePredefinedColors()
        {
            AvailablePredefinedColors.Clear();
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
                if (SetPropertyAndNotify(ref _selectedPredefinedColor, value))
                {
                    OnPropertyChanged(nameof(ShowColorEdit));
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
                if (String.IsNullOrWhiteSpace(curBuilding.Template))
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
                if (String.IsNullOrWhiteSpace(curBuilding.Identifier))
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
