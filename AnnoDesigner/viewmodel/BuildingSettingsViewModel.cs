using AnnoDesigner.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace AnnoDesigner.viewmodel
{
    public class BuildingSettingsViewModel : Notify
    {
        private string _textApplyColorToSelection;
        private string _textApplyColorToSelectionToolTip;
        private string _textApplyPredefinedColorToSelection;
        private string _textApplyPredefinedColorToSelectionToolTip;
        private string _textAvailableColors;
        private string _textStandardColors;
        private string _textRecentColors;
        private string _textStandard;
        private string _textAdvanced;

        private Color? _selectedColor;

        /// <summary>
        /// only used for databinding
        /// </summary>
        public BuildingSettingsViewModel()
        {
            ApplyColorToSelectionCommand = new RelayCommand(ApplyColorToSelection, CanApplyColorToSelection);
            ApplyPredefinedColorToSelectionCommand = new RelayCommand(ApplyPredefinedColorToSelection, CanApplyPredefinedColorToSelection);

            TextApplyColorToSelection = "Apply color";
            TextApplyColorToSelectionToolTip = "Apply color to all buildings in current selection";
            TextApplyPredefinedColorToSelection = "Apply predefined color";
            TextApplyPredefinedColorToSelectionToolTip = "Apply predefined color (if found) to all buildings in current selection";
            TextAvailableColors = "Available Colors";
            TextStandardColors = "Predefined Colors";
            TextRecentColors = "Recent Colors";
            TextStandard = "Standard";
            TextAdvanced = "Advanced";

            SelectedColor = Colors.Red;
        }

        #region localization

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

        #endregion

        public Color? SelectedColor
        {
            get { return _selectedColor; }
            set { UpdateProperty(ref _selectedColor, value); }
        }

        public AnnoCanvas AnnoCanvasToUse { get; set; }

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
        }

        private bool CanApplyColorToSelection(object param)
        {
            return AnnoCanvasToUse?.SelectedObjects.Count > 0;
        }

        public ICommand ApplyPredefinedColorToSelectionCommand { get; private set; }

        private void ApplyPredefinedColorToSelection(object param)
        {

        }

        private bool CanApplyPredefinedColorToSelection(object param)
        {
            return false;
        }



        #endregion
    }
}
