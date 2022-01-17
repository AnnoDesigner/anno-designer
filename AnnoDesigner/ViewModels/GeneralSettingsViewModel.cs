using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;
using NLog;

namespace AnnoDesigner.ViewModels
{
    public class GeneralSettingsViewModel : Notify
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IAppSettings _appSettings;
        private readonly ICommons _commons;
        private readonly IRecentFilesHelper _recentFilesHelper;

        private bool _hideInfluenceOnSelection;
        private bool _useZoomToPoint;
        private UserDefinedColor _selectedGridLineColor;
        private UserDefinedColor _selectedObjectBorderLineColor;
        private ObservableCollection<UserDefinedColor> _gridLineColors;
        private ObservableCollection<UserDefinedColor> _objectBorderLineColors;
        private bool _isGridLineColorPickerVisible;
        private bool _isObjectBorderLineColorPickerVisible;
        private Color? _selectedCustomGridLineColor;
        private Color? _selectedCustomObjectBorderLineColor;
        private double _zoomSensitivityPercentage;
        private bool _invertPanningDirection;
        private bool _showScrollbars;
        private bool _invertScrollingDirection;
        private bool _includeRoadsInStatisticCalculation;
        private int _maxRecentFiles;

        public GeneralSettingsViewModel(IAppSettings appSettingsToUse, ICommons commonsToUse, IRecentFilesHelper recentFilesHelperToUse)
        {
            _appSettings = appSettingsToUse;
            _commons = commonsToUse;
            _commons.SelectedLanguageChanged += Commons_SelectedLanguageChanged;
            _recentFilesHelper = recentFilesHelperToUse;

            UseZoomToPoint = _appSettings.UseZoomToPoint;
            ZoomSensitivityPercentage = _appSettings.ZoomSensitivityPercentage;
            HideInfluenceOnSelection = _appSettings.HideInfluenceOnSelection;
            ShowScrollbars = _appSettings.ShowScrollbars;
            InvertPanningDirection = _appSettings.InvertPanningDirection;
            InvertScrollingDirection = _appSettings.InvertScrollingDirection;
            MaxRecentFiles = _appSettings.MaxRecentFiles;

            ResetZoomSensitivityCommand = new RelayCommand(ExecuteResetZoomSensitivity, CanExecuteResetZoomSensitivity);
            ResetMaxRecentFilesCommand = new RelayCommand(ExecuteResetMaxRecentFiles, CanExecuteResetMaxRecentFiles);
            ClearRecentFilesCommand = new RelayCommand(ExecuteClearRecentFiles, CanExecuteClearRecentFiles);

            GridLineColors = new ObservableCollection<UserDefinedColor>();
            RefreshGridLineColors();
            var savedGridLineColor = SerializationHelper.LoadFromJsonString<UserDefinedColor>(_appSettings.ColorGridLines);
            if (savedGridLineColor is null)
            {
                SelectedGridLineColor = GridLineColors.First();
                SelectedCustomGridLineColor = SelectedGridLineColor.Color;
            }
            else
            {
                SelectedGridLineColor = GridLineColors.SingleOrDefault(x => x.Type == savedGridLineColor.Type);
                SelectedCustomGridLineColor = savedGridLineColor.Color;
            }

            ObjectBorderLineColors = new ObservableCollection<UserDefinedColor>();
            RefreshObjectBorderLineColors();
            var savedObjectBorderLineColor = SerializationHelper.LoadFromJsonString<UserDefinedColor>(_appSettings.ColorObjectBorderLines);
            if (savedObjectBorderLineColor is null)
            {
                SelectedObjectBorderLineColor = ObjectBorderLineColors.First();
                SelectedCustomObjectBorderLineColor = SelectedObjectBorderLineColor.Color;
            }
            else
            {
                SelectedObjectBorderLineColor = ObjectBorderLineColors.SingleOrDefault(x => x.Type == savedObjectBorderLineColor.Type);
                SelectedCustomObjectBorderLineColor = savedObjectBorderLineColor.Color;
            }
        }

        private void Commons_SelectedLanguageChanged(object sender, EventArgs e)
        {
            var selectedGridLineColorType = SelectedGridLineColor.Type;
            GridLineColors.Clear();
            RefreshGridLineColors();
            SelectedGridLineColor = GridLineColors.SingleOrDefault(x => x.Type == selectedGridLineColorType);

            var selectedObjectBorderLineColorType = SelectedObjectBorderLineColor.Type;
            ObjectBorderLineColors.Clear();
            RefreshObjectBorderLineColors();
            SelectedObjectBorderLineColor = ObjectBorderLineColors.SingleOrDefault(x => x.Type == selectedObjectBorderLineColorType);
        }

        #region Color for grid lines

        private void RefreshGridLineColors()
        {
            foreach (UserDefinedColorType curColorType in Enum.GetValues(typeof(UserDefinedColorType)))
            {
                GridLineColors.Add(new UserDefinedColor
                {
                    Type = curColorType
                });
            }
        }

        public ObservableCollection<UserDefinedColor> GridLineColors
        {
            get { return _gridLineColors; }
            set { UpdateProperty(ref _gridLineColors, value); }
        }

        public UserDefinedColor SelectedGridLineColor
        {
            get { return _selectedGridLineColor; }
            set
            {
                if (UpdateProperty(ref _selectedGridLineColor, value))
                {
                    if (value != null)
                    {
                        UpdateGridLineColorVisibility();
                        SaveSelectedGridLineColor();
                    }
                }
            }
        }

        public Color? SelectedCustomGridLineColor
        {
            get { return _selectedCustomGridLineColor; }
            set
            {
                if (UpdateProperty(ref _selectedCustomGridLineColor, value))
                {
                    if (value != null)
                    {
                        SelectedGridLineColor.Color = value.Value;
                        SaveSelectedGridLineColor();
                    }
                }
            }
        }

        public bool IsGridLineColorPickerVisible
        {
            get { return _isGridLineColorPickerVisible; }
            set { UpdateProperty(ref _isGridLineColorPickerVisible, value); }
        }

        private void UpdateGridLineColorVisibility()
        {
            if (SelectedGridLineColor is null)
            {
                IsGridLineColorPickerVisible = false;
                return;
            }

            IsGridLineColorPickerVisible = SelectedGridLineColor.Type switch
            {
                UserDefinedColorType.Custom => true,
                _ => false,
            };
        }

        private void SaveSelectedGridLineColor()
        {
            switch (SelectedGridLineColor.Type)
            {
                case UserDefinedColorType.Default:
                    SelectedGridLineColor.Color = Colors.Black;
                    break;
                case UserDefinedColorType.Light:
                    SelectedGridLineColor.Color = Colors.LightGray;
                    break;
                case UserDefinedColorType.Custom:
                    SelectedGridLineColor.Color = SelectedCustomGridLineColor ?? Colors.Black;
                    break;
                default:
                    break;
            }

            var json = SerializationHelper.SaveToJsonString(SelectedGridLineColor);
            _appSettings.ColorGridLines = json;
            _appSettings.Save();
        }

        #endregion

        #region Color for object border lines

        private void RefreshObjectBorderLineColors()
        {
            foreach (UserDefinedColorType curColorType in Enum.GetValues(typeof(UserDefinedColorType)))
            {
                ObjectBorderLineColors.Add(new UserDefinedColor
                {
                    Type = curColorType
                });
            }
        }

        public ObservableCollection<UserDefinedColor> ObjectBorderLineColors
        {
            get { return _objectBorderLineColors; }
            set { UpdateProperty(ref _objectBorderLineColors, value); }
        }

        public UserDefinedColor SelectedObjectBorderLineColor
        {
            get { return _selectedObjectBorderLineColor; }
            set
            {
                if (UpdateProperty(ref _selectedObjectBorderLineColor, value))
                {
                    if (value != null)
                    {
                        UpdateObjectBorderLineVisibility();
                        SaveSelectedObjectBorderLine();
                    }
                }
            }
        }

        public Color? SelectedCustomObjectBorderLineColor
        {
            get { return _selectedCustomObjectBorderLineColor; }
            set
            {
                if (UpdateProperty(ref _selectedCustomObjectBorderLineColor, value))
                {
                    if (value != null)
                    {
                        SelectedObjectBorderLineColor.Color = value.Value;
                        SaveSelectedObjectBorderLine();
                    }
                }
            }
        }

        public bool IsObjectBorderLineColorPickerVisible
        {
            get { return _isObjectBorderLineColorPickerVisible; }
            set { UpdateProperty(ref _isObjectBorderLineColorPickerVisible, value); }
        }

        private void UpdateObjectBorderLineVisibility()
        {
            if (SelectedObjectBorderLineColor is null)
            {
                IsObjectBorderLineColorPickerVisible = false;
                return;
            }

            IsObjectBorderLineColorPickerVisible = SelectedObjectBorderLineColor.Type switch
            {
                UserDefinedColorType.Custom => true,
                _ => false,
            };
        }

        private void SaveSelectedObjectBorderLine()
        {
            switch (SelectedObjectBorderLineColor.Type)
            {
                case UserDefinedColorType.Default:
                    SelectedObjectBorderLineColor.Color = Colors.Black;
                    break;
                case UserDefinedColorType.Light:
                    SelectedObjectBorderLineColor.Color = Colors.LightGray;
                    break;
                case UserDefinedColorType.Custom:
                    SelectedObjectBorderLineColor.Color = SelectedCustomObjectBorderLineColor ?? Colors.Black;
                    break;
                default:
                    break;
            }

            var json = SerializationHelper.SaveToJsonString(SelectedObjectBorderLineColor);
            _appSettings.ColorObjectBorderLines = json;
            _appSettings.Save();
        }

        #endregion

        public bool HideInfluenceOnSelection
        {
            get { return _hideInfluenceOnSelection; }
            set
            {
                if (UpdateProperty(ref _hideInfluenceOnSelection, value))
                {
                    _appSettings.HideInfluenceOnSelection = value;
                    _appSettings.Save();
                }
            }
        }

        public double ZoomSensitivityPercentage
        {
            get => _zoomSensitivityPercentage;
            set
            {
                if (UpdateProperty(ref _zoomSensitivityPercentage, value))
                {
                    _appSettings.ZoomSensitivityPercentage = value;
                    _appSettings.Save();
                }
            }
        }

        public bool UseZoomToPoint
        {
            get { return _useZoomToPoint; }
            set
            {
                if (UpdateProperty(ref _useZoomToPoint, value))
                {
                    _appSettings.UseZoomToPoint = value;
                    _appSettings.Save();
                }
            }
        }

        public bool InvertScrollingDirection
        {
            get { return _invertScrollingDirection; }
            set
            {
                if (UpdateProperty(ref _invertScrollingDirection, value))
                {
                    _appSettings.InvertScrollingDirection = value;
                    _appSettings.Save();
                }
            }
        }

        public bool InvertPanningDirection
        {
            get { return _invertPanningDirection; }
            set
            {
                if (UpdateProperty(ref _invertPanningDirection, value))
                {
                    _appSettings.InvertPanningDirection = value;
                    _appSettings.Save();
                }
            }
        }

        public bool ShowScrollbars
        {
            get { return _showScrollbars; }
            set
            {
                if (UpdateProperty(ref _showScrollbars, value))
                {
                    _appSettings.ShowScrollbars = value;
                    _appSettings.Save();
                }
            }
        }

        public bool IncludeRoadsInStatisticCalculation
        {
            get { return _includeRoadsInStatisticCalculation; }
            set
            {
                if (UpdateProperty(ref _includeRoadsInStatisticCalculation, value))
                {
                    _appSettings.IncludeRoadsInStatisticCalculation = value;
                    _appSettings.Save();
                }
            }
        }

        public int MaxRecentFiles
        {
            get { return _maxRecentFiles; }
            set
            {
                if (UpdateProperty(ref _maxRecentFiles, value))
                {
                    _appSettings.MaxRecentFiles = value;
                    _appSettings.Save();

                    _recentFilesHelper.MaximumItemCount = value;
                }
            }
        }

        public ICommand ResetZoomSensitivityCommand { get; private set; }

        private void ExecuteResetZoomSensitivity(object param)
        {
            ZoomSensitivityPercentage = Constants.ZoomSensitivityPercentageDefault;
        }

        private bool CanExecuteResetZoomSensitivity(object param)
        {
            return ZoomSensitivityPercentage != Constants.ZoomSensitivityPercentageDefault;
        }

        public ICommand ResetMaxRecentFilesCommand { get; private set; }

        private void ExecuteResetMaxRecentFiles(object param)
        {
            MaxRecentFiles = Constants.MaxRecentFiles;
        }

        private bool CanExecuteResetMaxRecentFiles(object param)
        {
            return MaxRecentFiles != Constants.MaxRecentFiles;
        }

        public ICommand ClearRecentFilesCommand { get; private set; }

        private void ExecuteClearRecentFiles(object param)
        {
            _recentFilesHelper.ClearRecentFiles();
        }

        private bool CanExecuteClearRecentFiles(object param)
        {
            return _recentFilesHelper.RecentFiles.Count > 0;
        }
    }
}
