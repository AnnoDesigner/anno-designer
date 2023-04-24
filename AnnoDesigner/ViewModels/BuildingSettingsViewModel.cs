using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using AnnoDesigner.Core.Extensions;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Helper;
using AnnoDesigner.Core.Presets.Models;
using AnnoDesigner.Core.Services;
using AnnoDesigner.Models;
using AnnoDesigner.Undo.Operations;
using NLog;

namespace AnnoDesigner.ViewModels
{
    [Flags]
    public enum ApplySettings
    {
        None = 0,
        Color = 1 << 0,
        Label = 1 << 1,
        Icon = 1 << 2,
        Influence = 1 << 3,
        Borderless = 1 << 4,
        Road = 1 << 5,
    }

    public interface Aaa
    {
        ApplySettings ApplySettings { get; }

        IOperation SetValueAndGetUndoableOperation(IEnumerable<LayoutObject> objs);
    }

    public class Aaaa<T> : Aaa
    {
        public ApplySettings ApplySettings { get; set; }

        public string PropertyName { get; set; }

        public Func<LayoutObject, T> OldValueGetter { get; set; }

        public Func<T> NewValueGetter { get; set; }

        public IOperation SetValueAndGetUndoableOperation(IEnumerable<LayoutObject> objs)
        {
            var operation = new ModifyObjectPropertiesOperation<LayoutObject, T>()
            {
                PropertyName = PropertyName,
                ObjectPropertyValues = objs
                    .Select(obj => (obj, OldValueGetter(obj), NewValueGetter()))
                    .ToList()
            };

            operation.Redo();

            return operation;
        }
    }

    public class BuildingSettingsViewModel : Notify
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly List<Aaa> applySettingsTemplates;

        private readonly IAppSettings _appSettings;
        private readonly IMessageBoxService _messageBoxService;
        private readonly ILocalizationHelper _localizationHelper;

        private Color? _selectedColor;
        private int _buildingHeight;
        private int _buildingWidth;
        private string _buildingTemplate;
        private string _buildingName;
        private string _buildingRealName;
        private string _buildingIdentifier;
        private double _buildingRadius;
        private double _buildingInfluenceRange;
        private double _buildingBlockedAreaLength;
        private double _buildingBlockedAreaWidth;
        private GridDirection _buildingDirection;
        private bool _isPavedStreet;
        private bool _isEnableLabelChecked;
        private bool _isBorderlessChecked;
        private bool _isRoadChecked;
        private ApplySettings _applySettings = ApplySettings.Color | ApplySettings.Borderless;
        private ObservableCollection<SerializableColor> _colorsInLayout;
        private IAnnoCanvas _annoCanvasToUse;
        private ICollection<IconImage> _availableIcons;
        private IconImage _selectedIcon;
        private ColorHueSaturationBrightnessComparer _colorSorter;
        private ObservableCollection<BuildingInfluence> _buildingInfluences;
        private BuildingInfluence _selectedBuildingInfluence;
        private bool _isBuildingInfluenceInputRadiusVisible;
        private bool _isBuildingInfluenceInputRangeVisible;

        /// <summary>
        /// only used for databinding
        /// </summary>
        public BuildingSettingsViewModel(IAppSettings appSettingsToUse,
            IMessageBoxService messageBoxServiceToUse,
            ILocalizationHelper localizationHelperToUse,
            ICollection<IconImage> availableIcons)
        {
            _appSettings = appSettingsToUse;
            _messageBoxService = messageBoxServiceToUse;
            _localizationHelper = localizationHelperToUse;
            _availableIcons = availableIcons;

            ApplyColorToSelectionCommand = new RelayCommand(ApplyColorToSelection, AreSomeObjectsSelected);
            ApplyPredefinedColorToSelectionCommand = new RelayCommand(ApplyPredefinedColorToSelection, AreSomeObjectsSelected);
            ApplySettingsToSelectionCommand = new RelayCommand(ApplySettingsToSelection, AreSomeObjectsSelected);
            UseColorInLayoutCommand = new RelayCommand(UseColorInLayout, CanUseColorInLayout);

            SelectedColor = Colors.Red;
            BuildingHeight = 4;
            BuildingWidth = 4;
            BuildingTemplate = string.Empty;
            BuildingName = string.Empty;
            BuildingIdentifier = string.Empty;
            BuildingRadius = 0;
            BuildingInfluenceRange = 0;
            IsEnableLabelChecked = false;
            IsBorderlessChecked = false;
            IsRoadChecked = false;
            ColorsInLayout = new ObservableCollection<SerializableColor>();

            BuildingInfluences = new ObservableCollection<BuildingInfluence>();
            InitBuildingInfluences();
            SelectedBuildingInfluence = BuildingInfluences.SingleOrDefault(x => x.Type == BuildingInfluenceType.None);

            applySettingsTemplates = new()
            {
                new Aaaa<SerializableColor>()
                {
                    ApplySettings = ApplySettings.Color,
                    PropertyName = nameof(LayoutObject.Color),
                    OldValueGetter = x => x.Color,
                    NewValueGetter = () => SelectedColor.Value
                },
                new Aaaa<string>()
                {
                    ApplySettings = ApplySettings.Label,
                    PropertyName = nameof(LayoutObject.Label),
                    OldValueGetter = x => x.WrappedAnnoObject.Label,
                    NewValueGetter = () => BuildingName
                },
                new Aaaa<IconImage>()
                {
                    ApplySettings = ApplySettings.Icon,
                    PropertyName = nameof(LayoutObject.Icon),
                    OldValueGetter = x => x.Icon,
                    NewValueGetter = () => SelectedIcon
                },
                new Aaaa<double>()
                {
                    ApplySettings = ApplySettings.Influence,
                    PropertyName = nameof(LayoutObject.InfluenceRange),
                    OldValueGetter = x => x.WrappedAnnoObject.InfluenceRange,
                    NewValueGetter = () => BuildingInfluenceRange
                },
                new Aaaa<double>()
                {
                    ApplySettings = ApplySettings.Influence,
                    PropertyName = nameof(LayoutObject.Radius),
                    OldValueGetter = x => x.WrappedAnnoObject.Radius,
                    NewValueGetter = () => BuildingRadius
                },
                new Aaaa<bool>()
                {
                    ApplySettings = ApplySettings.Borderless,
                    PropertyName = nameof(LayoutObject.Borderless),
                    OldValueGetter = x => x.WrappedAnnoObject.Borderless,
                    NewValueGetter = () => IsBorderlessChecked
                },
                new Aaaa<bool>()
                {
                    ApplySettings = ApplySettings.Road,
                    PropertyName = nameof(LayoutObject.Road),
                    OldValueGetter = x => x.WrappedAnnoObject.Road,
                    NewValueGetter = () => IsRoadChecked
                },
            };
        }

        private void InitBuildingInfluences()
        {
            foreach (BuildingInfluenceType curInfluenceType in Enum.GetValues(typeof(BuildingInfluenceType)))
            {
                BuildingInfluences.Add(new BuildingInfluence
                {
                    Name = _localizationHelper.GetLocalization(curInfluenceType.ToString()),
                    Type = curInfluenceType
                });
            }
        }

        #region localization

        public void UpdateLanguageBuildingInfluenceType()
        {
            foreach (var curBuildingInfluence in BuildingInfluences)
            {
                curBuildingInfluence.Name = _localizationHelper.GetLocalization(curBuildingInfluence.Type.ToString());
            }
        }

        #endregion

        public Color? SelectedColor
        {
            get { return _selectedColor; }
            set { UpdateProperty(ref _selectedColor, value); }
        }

        public ICollection<IconImage> AvailableIcons
        {
            get { return _availableIcons; }
            set { UpdateProperty(ref _availableIcons, value); }
        }

        public IconImage SelectedIcon
        {
            get { return _selectedIcon; }
            set { UpdateProperty(ref _selectedIcon, value); }
        }

        public int BuildingHeight
        {
            get { return _buildingHeight; }
            set { UpdateProperty(ref _buildingHeight, value); }
        }

        public int BuildingWidth
        {
            get { return _buildingWidth; }
            set { UpdateProperty(ref _buildingWidth, value); }
        }

        public string BuildingTemplate
        {
            get { return _buildingTemplate; }
            set { UpdateProperty(ref _buildingTemplate, value); }
        }

        public string BuildingName
        {
            get { return _buildingName; }
            set { UpdateProperty(ref _buildingName, value); }
        }

        public string BuildingRealName
        {
            get { return _buildingRealName; }
            set { UpdateProperty(ref _buildingRealName, value); }
        }

        public string BuildingIdentifier
        {
            get { return _buildingIdentifier; }
            set { UpdateProperty(ref _buildingIdentifier, value); }
        }

        public double BuildingRadius
        {
            get { return _buildingRadius; }
            set { UpdateProperty(ref _buildingRadius, value); }
        }

        public double BuildingInfluenceRange
        {
            get { return _buildingInfluenceRange; }
            set { UpdateProperty(ref _buildingInfluenceRange, value); }
        }

        public double BuildingBlockedAreaLength
        {
            get { return _buildingBlockedAreaLength; }
            set { UpdateProperty(ref _buildingBlockedAreaLength, value); }
        }

        public double BuildingBlockedAreaWidth
        {
            get { return _buildingBlockedAreaWidth; }
            set { UpdateProperty(ref _buildingBlockedAreaWidth, value); }
        }

        public GridDirection BuildingDirection
        {
            get { return _buildingDirection; }
            set { UpdateProperty(ref _buildingDirection, value); }
        }

        public bool IsPavedStreet
        {
            get { return _isPavedStreet; }
            set
            {
                if (UpdateProperty(ref _isPavedStreet, value))
                {
                    HandleIsPavedStreet();
                }
            }
        }

        public bool IsEnableLabelChecked
        {
            get { return _isEnableLabelChecked; }
            set { UpdateProperty(ref _isEnableLabelChecked, value); }
        }

        public bool IsBorderlessChecked
        {
            get { return _isBorderlessChecked; }
            set { UpdateProperty(ref _isBorderlessChecked, value); }
        }

        public bool IsRoadChecked
        {
            get { return _isRoadChecked; }
            set { UpdateProperty(ref _isRoadChecked, value); }
        }

        public ApplySettings ApplySettings
        {
            get { return _applySettings; }
            set { UpdateProperty(ref _applySettings, value); }
        }

        public IAnnoCanvas AnnoCanvasToUse
        {
            get { return _annoCanvasToUse; }
            set
            {
                if (_annoCanvasToUse != null)
                {
                    _annoCanvasToUse.ColorsInLayoutUpdated -= AnnoCanvasToUse_ColorsUpdated;
                }

                _annoCanvasToUse = value;
                _annoCanvasToUse.ColorsInLayoutUpdated += AnnoCanvasToUse_ColorsUpdated;
            }
        }

        public bool ShowColorsInLayout
        {
            get { return ColorsInLayout?.Count > 0; }
        }

        public ObservableCollection<SerializableColor> ColorsInLayout
        {
            get { return _colorsInLayout; }
            set
            {
                UpdateProperty(ref _colorsInLayout, value);
                OnPropertyChanged(nameof(ShowColorsInLayout));
            }
        }

        private ColorHueSaturationBrightnessComparer ColorSorter
        {
            get { return _colorSorter ??= new ColorHueSaturationBrightnessComparer(); }
        }

        public ObservableCollection<BuildingInfluence> BuildingInfluences
        {
            get { return _buildingInfluences; }
            set { UpdateProperty(ref _buildingInfluences, value); }
        }

        public bool IsBuildingInfluenceInputRadiusVisible
        {
            get { return _isBuildingInfluenceInputRadiusVisible; }
            set { UpdateProperty(ref _isBuildingInfluenceInputRadiusVisible, value); }
        }

        public bool IsBuildingInfluenceInputRangeVisible
        {
            get { return _isBuildingInfluenceInputRangeVisible; }
            set { UpdateProperty(ref _isBuildingInfluenceInputRangeVisible, value); }
        }

        public BuildingInfluence SelectedBuildingInfluence
        {
            get { return _selectedBuildingInfluence; }
            set
            {
                if (UpdateProperty(ref _selectedBuildingInfluence, value))
                {
                    UpdateBuildingInfluenceInputVisibility(_selectedBuildingInfluence.Type);
                }
            }
        }

        private void UpdateBuildingInfluenceInputVisibility(BuildingInfluenceType type)
        {
            switch (type)
            {
                case BuildingInfluenceType.Radius:
                    IsBuildingInfluenceInputRadiusVisible = true;
                    IsBuildingInfluenceInputRangeVisible = false;
                    //dockPanelPavedStreet.Visibility = Visibility.Collapsed;
                    break;
                case BuildingInfluenceType.Distance:
                    IsBuildingInfluenceInputRadiusVisible = false;
                    IsBuildingInfluenceInputRangeVisible = true;
                    //dockPanelPavedStreet.Visibility = Visibility.Visible;
                    break;
                case BuildingInfluenceType.Both:
                    IsBuildingInfluenceInputRadiusVisible = true;
                    IsBuildingInfluenceInputRangeVisible = true;
                    //dockPanelPavedStreet.Visibility = Visibility.Visible;
                    break;
                case BuildingInfluenceType.None:
                default:
                    IsBuildingInfluenceInputRadiusVisible = false;
                    IsBuildingInfluenceInputRangeVisible = false;
                    //dockPanelPavedStreet.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void HandleIsPavedStreet()
        {
            if (!_appSettings.ShowPavedRoadsWarning)
            {
                _messageBoxService.ShowMessage(_localizationHelper.GetLocalization("PavedStreetToolTip"),
                    _localizationHelper.GetLocalization("PavedStreetWarningTitle"));
                _appSettings.ShowPavedRoadsWarning = true;
            }

            if (!GetDistanceRange(IsPavedStreet, AnnoCanvasToUse.BuildingPresets.Buildings.FirstOrDefault(_ => _.Identifier == BuildingIdentifier)))
            {
                logger.Trace("$Calculate Paved Street/Dirt Street Error: Can not obtain new Distance Value, value set to 0");
            }
            else
            {
                //I like to have here a isObjectOnMouse routine, to check of the command 'ApplyCurrentObject();' must be excecuted or not: 
                //Check of Mouse has an object or not -> if mouse has Object then Renew Object, withouth placnig. (do ApplyCurrentObject();)
            }
        }

        public bool GetDistanceRange(bool isPavedStreet, IBuildingInfo buildingInfo)
        {
            if (buildingInfo == null || buildingInfo.InfluenceRange <= 0)
            {
                BuildingInfluenceRange = 0;
                return false;
            }

            if (isPavedStreet)
            {
                if (buildingInfo.InfluenceRange <= 1)
                {
                    BuildingInfluenceRange = 0;
                    return false;
                }

                //sum for range on paved street for City Institution Building = n*1.38 (Police, Fire stations and Hospials)
                //to round up, there must be added 0.5 to the numbers after the multiplier
                //WYSWYG : the minus 2 is what gamers see as they count the Dark Green area on Paved Street
                if (string.Equals(buildingInfo.Template, "CityInstitutionBuilding", StringComparison.OrdinalIgnoreCase))
                {
                    BuildingInfluenceRange = Math.Round(((buildingInfo.InfluenceRange * 1.38) + 0.5) - 2);
                }
                //sum for range on paved street for Public Service Building = n*1.43 (Marketplaces, Pubs, Banks, ... (etc))
                //to round up, there must be added 0.5 to the numbers after the multiplier
                //WYSWYG : the minus 2 is what gamers see as they count the Dark Green area on Paved Street
                else
                {
                    BuildingInfluenceRange = Math.Round(((buildingInfo.InfluenceRange * 1.43) + 0.5) - 2);
                }

                return true;
            }
            else
            {
                if (buildingInfo.InfluenceRange <= 2)
                {
                    BuildingInfluenceRange = 0;
                    return false;
                }

                //WYSWYG : the minus 2 is what gamers see as they count the Dark Green area on Dirt Road
                BuildingInfluenceRange = buildingInfo.InfluenceRange - 2;
                return true;
            }
        }

        #region commands

        public ICommand ApplyColorToSelectionCommand { get; private set; }

        private void ApplyColorToSelection(object param)
        {
            if (AnnoCanvasToUse == null || SelectedColor == null)
            {
                return;
            }

            AnnoCanvasToUse.UndoManager.RegisterOperation(new ModifyObjectPropertiesOperation<LayoutObject, SerializableColor>()
            {
                PropertyName = nameof(LayoutObject.Color),
                ObjectPropertyValues = AnnoCanvasToUse.SelectedObjects
                    .Select(obj => (obj, obj.Color, selectedColor: (SerializableColor)SelectedColor.Value))
                    .ToList(),
                AfterAction = RerenderCanvas
            });

            foreach (var curSelectedObject in AnnoCanvasToUse.SelectedObjects)
            {
                curSelectedObject.Color = SelectedColor.Value;
            }

            AnnoCanvasToUse.ForceRendering();

            AnnoCanvasToUse_ColorsUpdated(this, EventArgs.Empty);
        }

        public ICommand ApplyPredefinedColorToSelectionCommand { get; private set; }

        private void ApplyPredefinedColorToSelection(object param)
        {
            if (AnnoCanvasToUse == null)
            {
                return;
            }

            AnnoCanvasToUse.UndoManager.RegisterOperation(new ModifyObjectPropertiesOperation<LayoutObject, SerializableColor>()
            {
                PropertyName = nameof(LayoutObject.Color),
                ObjectPropertyValues = AnnoCanvasToUse.SelectedObjects
                    .Where(obj => ColorPresetsHelper.Instance.GetPredefinedColor(obj.WrappedAnnoObject).HasValue)
                    .Select(obj => (obj, obj.Color, (SerializableColor)ColorPresetsHelper.Instance.GetPredefinedColor(obj.WrappedAnnoObject).Value))
                    .ToList(),
                AfterAction = RerenderCanvas
            });

            foreach (var curSelectedObject in AnnoCanvasToUse.SelectedObjects)
            {
                var predefinedColor = ColorPresetsHelper.Instance.GetPredefinedColor(curSelectedObject.WrappedAnnoObject);
                if (predefinedColor != null && predefinedColor.HasValue)
                {
                    curSelectedObject.Color = predefinedColor.Value;
                }
            }

            RerenderCanvas();
        }

        public ICommand ApplySettingsToSelectionCommand { get; private set; }

        private void ApplySettingsToSelection(object param)
        {
            if (AnnoCanvasToUse == null)
            {
                return;
            }

            var operations = applySettingsTemplates.Where(x => ApplySettings.HasFlag(x.ApplySettings)).Select(x => x.SetValueAndGetUndoableOperation(AnnoCanvasToUse.SelectedObjects)).ToList();

            if (operations.Count > 0)
            {
                AnnoCanvasToUse.UndoManager.RegisterOperation(new CompositeOperation(operations)
                {
                    AfterAction = RerenderCanvas
                });

                RerenderCanvas();
            }
        }

        private bool AreSomeObjectsSelected(object param)
        {
            return AnnoCanvasToUse?.SelectedObjects.Count > 0;
        }

        public ICommand UseColorInLayoutCommand { get; private set; }

        private void UseColorInLayout(object param)
        {
            if (param == null)
            {
                return;
            }

            if (param is SerializableColor clickedSerializableColor)
            {
                SelectedColor = clickedSerializableColor.MediaColor;
            }
            else if (param is Color clickedColor)
            {
                SelectedColor = clickedColor;
            }
        }

        private bool CanUseColorInLayout(object param)
        {
            return true;
        }

        #endregion

        private void AnnoCanvasToUse_ColorsUpdated(object sender, EventArgs e)
        {
            ColorsInLayout.Clear();

            foreach (var curColor in AnnoCanvasToUse.PlacedObjects.Select(_ => _.Color)
                .Distinct()
                .OrderBy(_ => _.MediaColor, ColorSorter))
            {
                ColorsInLayout.Add(curColor);
            }

            OnPropertyChanged(nameof(ShowColorsInLayout));
        }

        private void RerenderCanvas()
        {
            AnnoCanvasToUse.ForceRendering();
            AnnoCanvasToUse_ColorsUpdated(this, EventArgs.Empty);
        }
    }
}
