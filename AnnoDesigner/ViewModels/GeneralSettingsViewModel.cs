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

        private bool _hideInfluenceOnSelection;
        private bool _useZoomToPoint;
        private UserDefinedColor _selectedGridLineColor;
        private ObservableCollection<UserDefinedColor> _gridLineColors;
        private bool _isGridLineColorPickerVisible;
        private Color? _selectedCustomGridLineColor;

        public GeneralSettingsViewModel(IAppSettings appSettingsToUse)
        {
            _appSettings = appSettingsToUse;

            GridLineColors = new ObservableCollection<UserDefinedColor>();
            InitGridLineColors();
            var savedGridLineColor = SerializationHelper.LoadFromJsonString<UserDefinedColor>(_appSettings.ColorGridLines);
            SelectedGridLineColor = GridLineColors.SingleOrDefault(x => x.Type == savedGridLineColor.Type);
            SelectedCustomGridLineColor = savedGridLineColor.Color;
        }

        private void InitGridLineColors()
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
                    SaveSelectedColor();
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
                        SaveSelectedColor();
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

        private void SaveSelectedColor()
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

        public bool HideInfluenceOnSelection
        {
            get { return _hideInfluenceOnSelection; }
            set
            {
                if (UpdateProperty(ref _hideInfluenceOnSelection, value))
                {
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
                    _appSettings.Save();
                }
            }
        }
    }
}
