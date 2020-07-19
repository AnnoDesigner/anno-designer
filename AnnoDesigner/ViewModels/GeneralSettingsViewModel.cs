using System;
using System.Collections.ObjectModel;
using System.Linq;
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

        public GeneralSettingsViewModel(IAppSettings appSettingsToUse, ICommons commonsToUse)
        {
            _appSettings = appSettingsToUse;
            _commons = commonsToUse;
            _commons.SelectedLanguageChanged += Commons_SelectedLanguageChanged;

            UseZoomToPoint = _appSettings.UseZoomToPoint;
            ZoomSensitivityPercentage = _appSettings.ZoomSensitivityPercentage;

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
                    UpdateGridLineColorVisibility();
                    SaveSelectedGridLineColor();
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

            switch (SelectedGridLineColor.Type)
            {
                case UserDefinedColorType.Custom:
                    IsGridLineColorPickerVisible = true;
                    break;
                case UserDefinedColorType.Default:
                case UserDefinedColorType.Light:
                default:
                    IsGridLineColorPickerVisible = false;
                    break;
            }
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
                    UpdateObjectBorderLineVisibility();
                    SaveSelectedObjectBorderLine();
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

            switch (SelectedObjectBorderLineColor.Type)
            {
                case UserDefinedColorType.Custom:
                    IsObjectBorderLineColorPickerVisible = true;
                    break;
                case UserDefinedColorType.Default:
                case UserDefinedColorType.Light:
                default:
                    IsObjectBorderLineColorPickerVisible = false;
                    break;
            }
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
    }
}
