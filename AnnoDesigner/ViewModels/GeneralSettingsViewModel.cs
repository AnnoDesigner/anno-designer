using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public GeneralSettingsViewModel(IAppSettings appSettingsToUse)
        {
            _appSettings = appSettingsToUse;

            GridLineColors = new ObservableCollection<UserDefinedColor>();
            InitGridLineColors();
            SelectedGridLineColor = GridLineColors.SingleOrDefault(x => x.Type == UserDefinedColorType.Default);
        }

        private void InitGridLineColors()
        {
            foreach (UserDefinedColorType curColorType in Enum.GetValues(typeof(UserDefinedColorType)))
            {
                GridLineColors.Add(new UserDefinedColor
                {
                    Name = Localization.Localization.Translations["ColorType" + curColorType.ToString()],
                    Type = curColorType
                });
            }
        }

        public void UpdateLanguageUserDefinedColorType()
        {
            foreach (var curColorType in GridLineColors)
            {
                curColorType.Name = Localization.Localization.Translations["ColorType" + curColorType.Type.ToString()];
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
                    UpdateGridLineColorVisibility(_selectedGridLineColor.Type);
                }
            }
        }

        public bool IsGridLineColorPickerVisible
        {
            get { return _isGridLineColorPickerVisible; }
            set { UpdateProperty(ref _isGridLineColorPickerVisible, value); }
        }

        private void UpdateGridLineColorVisibility(UserDefinedColorType type)
        {
            switch (type)
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
