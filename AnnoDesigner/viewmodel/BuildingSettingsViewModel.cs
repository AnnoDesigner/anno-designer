using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace AnnoDesigner.viewmodel
{
    public class BuildingSettingsViewModel : Notify
    {
        private string _textHeader;
        private string _textSize;
        private string _textColor;
        private string _textApplyColorToSelection;
        private string _textApplyColorToSelectionToolTip;
        private string _textApplyPredefinedColorToSelection;
        private string _textApplyPredefinedColorToSelectionToolTip;
        private string _textAvailableColors;
        private string _textStandardColors;
        private string _textRecentColors;
        private string _textStandard;
        private string _textAdvanced;
        private string _textColorsInLayout;
        private string _textColorsInLayoutToolTip;
        private string _textBuildingName;
        private string _textIcon;
        private string _textInfluenceType;
        private string _textRadius;
        private string _textDistance;
        private string _textPavedStreet;
        private string _textPavedStreetWarningTitle;
        private string _textPavedStreetToolTip;
        private string _textOptions;
        private string _textEnableLabel;
        private string _textBorderless;
        private string _textRoad;
        private string _textPlaceBuilding;

        private Color? _selectedColor;
        private int _buildingHeight;
        private int _buildingWidth;
        private string _buildingTemplate;
        private string _buildingName;
        private string _buildingIdentifier;
        private double _buildingRadius;
        private double _buildingInfluenceRange;
        private bool _isPavedStreet;
        private bool _isEnableLabelChecked;
        private bool _isBorderlessChecked;
        private bool _isRoadChecked;
        private ObservableCollection<SerializableColor> _colorsInLayout;
        private AnnoCanvas _annoCanvasToUse;
        private ColorHueSaturationBrightnessComparer _colorSorter;

        /// <summary>
        /// only used for databinding
        /// </summary>
        public BuildingSettingsViewModel()
        {
            ApplyColorToSelectionCommand = new RelayCommand(ApplyColorToSelection, CanApplyColorToSelection);
            ApplyPredefinedColorToSelectionCommand = new RelayCommand(ApplyPredefinedColorToSelection, CanApplyPredefinedColorToSelection);
            UseColorInLayoutCommand = new RelayCommand(UseColorInLayout, CanUseColorInLayout);

            //only used for WPF Desinger
            TextHeader = "Building Settings";
            TextSize = "Size";
            TextColor = "Color";
            TextApplyColorToSelection = "Apply color";
            TextApplyColorToSelectionToolTip = "Apply color to all buildings in current selection";
            TextApplyPredefinedColorToSelection = "Apply predefined color";
            TextApplyPredefinedColorToSelectionToolTip = "Apply predefined color (if found) to all buildings in current selection";
            TextAvailableColors = "Available Colors";
            TextStandardColors = "Predefined Colors";
            TextRecentColors = "Recent Colors";
            TextStandard = "Standard";
            TextAdvanced = "Advanced";
            TextColorsInLayout = "Colors in Layout";
            TextColorsInLayoutToolTip = "Double click color to select it";
            TextBuildingName = "Label";
            TextIcon = "Icon";
            TextInfluenceType = "Influence Type";
            TextRadius = "Radius";
            TextDistance = "Distance";
            TextPavedStreet = "Paved Street";
            TextPavedStreetWarningTitle = "Paved Street Selection";
            TextPavedStreetToolTip = "Checking this option will change the Influence Range for buildings,\nrepresenting the increased range they receive when using paved streets.\nUse the 'Place Building' button to place object.";
            TextOptions = "Options";
            TextEnableLabel = "Enable label";
            TextBorderless = "Borderless";
            TextRoad = "Road";
            TextPlaceBuilding = "Place building";

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
        }

        #region localization

        public string TextHeader
        {
            get { return _textHeader; }
            set { UpdateProperty(ref _textHeader, value); }
        }

        public string TextSize
        {
            get { return _textSize; }
            set { UpdateProperty(ref _textSize, value); }
        }

        public string TextColor
        {
            get { return _textColor; }
            set { UpdateProperty(ref _textColor, value); }
        }

        public string TextApplyColorToSelection
        {
            get { return _textApplyColorToSelection; }
            set { UpdateProperty(ref _textApplyColorToSelection, value); }
        }

        public string TextApplyColorToSelectionToolTip
        {
            get { return _textApplyColorToSelectionToolTip; }
            set { UpdateProperty(ref _textApplyColorToSelectionToolTip, value); }
        }

        public string TextApplyPredefinedColorToSelection
        {
            get { return _textApplyPredefinedColorToSelection; }
            set { UpdateProperty(ref _textApplyPredefinedColorToSelection, value); }
        }

        public string TextApplyPredefinedColorToSelectionToolTip
        {
            get { return _textApplyPredefinedColorToSelectionToolTip; }
            set { UpdateProperty(ref _textApplyPredefinedColorToSelectionToolTip, value); }
        }

        public string TextAvailableColors
        {
            get { return _textAvailableColors; }
            set { UpdateProperty(ref _textAvailableColors, value); }
        }

        public string TextStandardColors
        {
            get { return _textStandardColors; }
            set { UpdateProperty(ref _textStandardColors, value); }
        }

        public string TextRecentColors
        {
            get { return _textRecentColors; }
            set { UpdateProperty(ref _textRecentColors, value); }
        }

        public string TextStandard
        {
            get { return _textStandard; }
            set { UpdateProperty(ref _textStandard, value); }
        }

        public string TextAdvanced
        {
            get { return _textAdvanced; }
            set { UpdateProperty(ref _textAdvanced, value); }
        }

        public string TextColorsInLayout
        {
            get { return _textColorsInLayout; }
            set { UpdateProperty(ref _textColorsInLayout, value); }
        }

        public string TextColorsInLayoutToolTip
        {
            get { return _textColorsInLayoutToolTip; }
            set { UpdateProperty(ref _textColorsInLayoutToolTip, value); }
        }

        public string TextBuildingName
        {
            get { return _textBuildingName; }
            set { UpdateProperty(ref _textBuildingName, value); }
        }

        public string TextIcon
        {
            get { return _textIcon; }
            set { UpdateProperty(ref _textIcon, value); }
        }

        public string TextInfluenceType
        {
            get { return _textInfluenceType; }
            set { UpdateProperty(ref _textInfluenceType, value); }
        }

        public string TextRadius
        {
            get { return _textRadius; }
            set { UpdateProperty(ref _textRadius, value); }
        }

        public string TextDistance
        {
            get { return _textDistance; }
            set { UpdateProperty(ref _textDistance, value); }
        }

        public string TextPavedStreet
        {
            get { return _textPavedStreet; }
            set { UpdateProperty(ref _textPavedStreet, value); }
        }

        public string TextPavedStreetWarningTitle
        {
            get { return _textPavedStreetWarningTitle; }
            set { UpdateProperty(ref _textPavedStreetWarningTitle, value); }
        }

        public string TextPavedStreetToolTip
        {
            get { return _textPavedStreetToolTip; }
            set { UpdateProperty(ref _textPavedStreetToolTip, value); }
        }

        public string TextOptions
        {
            get { return _textOptions; }
            set { UpdateProperty(ref _textOptions, value); }
        }

        public string TextEnableLabel
        {
            get { return _textEnableLabel; }
            set { UpdateProperty(ref _textEnableLabel, value); }
        }

        public string TextBorderless
        {
            get { return _textBorderless; }
            set { UpdateProperty(ref _textBorderless, value); }
        }

        public string TextRoad
        {
            get { return _textRoad; }
            set { UpdateProperty(ref _textRoad, value); }
        }

        public string TextPlaceBuilding
        {
            get { return _textPlaceBuilding; }
            set { UpdateProperty(ref _textPlaceBuilding, value); }
        }

        #endregion

        public Color? SelectedColor
        {
            get { return _selectedColor; }
            set { UpdateProperty(ref _selectedColor, value); }
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
        public bool IsPavedStreet
        {
            get { return _isPavedStreet; }
            set { UpdateProperty(ref _isPavedStreet, value); }
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

        public AnnoCanvas AnnoCanvasToUse
        {
            get { return _annoCanvasToUse; }
            set
            {
                if (_annoCanvasToUse != null)
                {
                    _annoCanvasToUse.StatisticsUpdated -= AnnoCanvasToUse_StatisticsUpdated;
                }

                _annoCanvasToUse = value;
                _annoCanvasToUse.StatisticsUpdated += AnnoCanvasToUse_StatisticsUpdated;
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
            get { return _colorSorter ?? (_colorSorter = new ColorHueSaturationBrightnessComparer()); }
        }

        #region commands

        public ICommand ApplyColorToSelectionCommand { get; private set; }

        private void ApplyColorToSelection(object param)
        {
            if (AnnoCanvasToUse == null || SelectedColor == null)
            {
                return;
            }

            foreach (var curSelectedObject in AnnoCanvasToUse.SelectedObjects)
            {
                curSelectedObject.Color = SelectedColor.Value;
            }

            AnnoCanvasToUse.InvalidateVisual();

            AnnoCanvasToUse_StatisticsUpdated(this, EventArgs.Empty);
        }

        private bool CanApplyColorToSelection(object param)
        {
            return AnnoCanvasToUse?.SelectedObjects.Count > 0;
        }

        public ICommand ApplyPredefinedColorToSelectionCommand { get; private set; }

        private void ApplyPredefinedColorToSelection(object param)
        {
            if (AnnoCanvasToUse == null)
            {
                return;
            }

            foreach (var curSelectedObject in AnnoCanvasToUse.SelectedObjects)
            {
                var foundPredefinedColor = ColorPresetsHelper.Instance.GetPredefinedColor(curSelectedObject);
                if (foundPredefinedColor != null && foundPredefinedColor.HasValue)
                {
                    curSelectedObject.Color = foundPredefinedColor.Value;
                }
            }

            AnnoCanvasToUse.InvalidateVisual();
        }

        private bool CanApplyPredefinedColorToSelection(object param)
        {
            return AnnoCanvasToUse?.SelectedObjects.Count > 0;
        }

        public ICommand UseColorInLayoutCommand { get; private set; }

        private void UseColorInLayout(object param)
        {
            var clickedColor = (SerializableColor)param;
            SelectedColor = clickedColor.MediaColor;
        }

        private bool CanUseColorInLayout(object param)
        {
            return true;
        }

        #endregion

        private void AnnoCanvasToUse_StatisticsUpdated(object sender, EventArgs e)
        {
            ColorsInLayout.Clear();

            foreach (var curColor in _annoCanvasToUse.PlacedObjects.Select(x => x.Color)
                .OrderBy(x => x.MediaColor, ColorSorter)
                .Distinct())
            {
                ColorsInLayout.Add(curColor);
            }

            OnPropertyChanged(nameof(ShowColorsInLayout));
        }
    }
}
