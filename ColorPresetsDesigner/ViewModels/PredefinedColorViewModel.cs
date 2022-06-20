using AnnoDesigner.Core.Presets.Models;
using ColorPresetsDesigner.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace ColorPresetsDesigner.ViewModels
{
    public class PredefinedColorViewModel : BaseModel
    {
        public event EventHandler OnTargetTemplateChanged;
        public event EventHandler OnTargetIdentifiersChanged;

        private string _targetTemplate;
        private ObservableCollection<string> _targetIdentifiers;
        private Color? _selectedColor;
        private string _selectedIdentifier;
        private string _newIdentifier;

        public PredefinedColorViewModel()
        {
            AddIdentifierCommand = new RelayCommand(AddIdentifier, CanAddIdentifier);
            DeleteIdentifierCommand = new RelayCommand(DeleteIdentifier, CanDeleteIdentifier);

            SelectedColor = Colors.Red;
            TargetTemplate = String.Empty;
            TargetIdentifiers = new ObservableCollection<string>();
            SelectedIdentifier = String.Empty;
            NewIdentifier = String.Empty;
        }

        public PredefinedColorViewModel(PredefinedColor color) : this()
        {
            TargetTemplate = color.TargetTemplate;
            SelectedColor = color.Color;
            foreach (var curIdentifier in color.TargetIdentifiers)
            {
                TargetIdentifiers.Add(curIdentifier);
            }
        }

        public string TargetTemplate
        {
            get { return _targetTemplate; }
            set
            {
                if (SetPropertyAndNotify(ref _targetTemplate, value))
                {
                    OnTargetTemplateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public ObservableCollection<string> TargetIdentifiers
        {
            get { return _targetIdentifiers; }
            set { SetPropertyAndNotify(ref _targetIdentifiers, value); }
        }

        public Color? SelectedColor
        {
            get { return _selectedColor; }
            set { SetPropertyAndNotify(ref _selectedColor, value); }
        }

        public string SelectedIdentifier
        {
            get { return _selectedIdentifier; }
            set { SetPropertyAndNotify(ref _selectedIdentifier, value); }
        }

        public string NewIdentifier
        {
            get { return _newIdentifier; }
            set { SetPropertyAndNotify(ref _newIdentifier, value); }
        }

        public ICommand AddIdentifierCommand { get; private set; }

        private void AddIdentifier(object param)
        {
            TargetIdentifiers.Add(NewIdentifier);
            OnTargetIdentifiersChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool CanAddIdentifier(object param)
        {
            return !string.IsNullOrWhiteSpace(NewIdentifier) &&
                !TargetIdentifiers.Contains(NewIdentifier, StringComparer.OrdinalIgnoreCase);
        }

        public ICommand DeleteIdentifierCommand { get; private set; }

        private void DeleteIdentifier(object param)
        {
            TargetIdentifiers.Remove(SelectedIdentifier);
            OnTargetIdentifiersChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool CanDeleteIdentifier(object param)
        {
            return !string.IsNullOrWhiteSpace(SelectedIdentifier);
        }
    }
}
