using AnnoDesigner.Presets;
using ColorPresetsDesigner.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorPresetsDesigner.ViewModels
{
    public class ColorSchemeViewModel : BaseModel
    {
        private string _name;
        private ObservableCollection<PredefinedColorViewModel> _colors;

        public ColorSchemeViewModel()
        {
            Name = string.Empty;
            Colors = new ObservableCollection<PredefinedColorViewModel>();
        }

        public ColorSchemeViewModel(ColorScheme colorScheme) : this()
        {
            Name = colorScheme.Name;

            foreach (var curColor in colorScheme.Colors)
            {
                Colors.Add(new PredefinedColorViewModel(curColor));
            }
        }

        public string Name
        {
            get { return _name; }
            set { SetPropertyAndNotify(ref _name, value); }
        }

        public ObservableCollection<PredefinedColorViewModel> Colors
        {
            get { return _colors; }
            set { SetPropertyAndNotify(ref _colors, value); }
        }

    }
}
