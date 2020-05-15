using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;
using static AnnoDesigner.Core.CoreConstants;

namespace AnnoDesigner.viewmodel
{
    public class PresetsTreeSearchViewModel : Notify
    {
        private string _textSearch;
        private string _textSearchToolTip;
        private string _textSelectAll;

        private string _searchText;
        private bool _hasFocus;
        private ObservableCollection<GameVersionFilter> _gameVersionFilters;
        private bool _isUpdatingGameVersionFilter;

        public PresetsTreeSearchViewModel()
        {
            ClearSearchTextCommand = new RelayCommand(ClearSearchText);
            GotFocusCommand = new RelayCommand(GotFocus);
            LostFocusCommand = new RelayCommand(LostFocus);
            GameVersionFilterChangedCommand = new RelayCommand(GameVersionFilterChanged);

            HasFocus = false;
            SearchText = string.Empty;
            GameVersionFilters = new ObservableCollection<GameVersionFilter>();
            InitGameVersionFilters();
        }

        private void InitGameVersionFilters()
        {
            var order = 0;
            foreach (GameVersion curGameVersion in Enum.GetValues(typeof(GameVersion)))
            {
                if (curGameVersion == GameVersion.Unknown || curGameVersion == GameVersion.All)
                {
                    continue;
                }

                GameVersionFilters.Add(new GameVersionFilter
                {
                    Name = curGameVersion.ToString().Replace("Anno", "Anno "),
                    Type = curGameVersion,
                    Order = ++order
                });
            }
        }

        public string SearchText
        {
            get { return _searchText; }
            set { UpdateProperty(ref _searchText, value); }
        }

        public bool HasFocus
        {
            get { return _hasFocus; }
            set { UpdateProperty(ref _hasFocus, value); }
        }

        public ObservableCollection<GameVersionFilter> GameVersionFilters
        {
            get { return _gameVersionFilters; }
            set { UpdateProperty(ref _gameVersionFilters, value); }
        }

        public ObservableCollection<GameVersionFilter> SelectedGameVersionFilters
        {
            get { return new ObservableCollection<GameVersionFilter>(GameVersionFilters.Where(x => x.IsSelected)); }
        }

        public GameVersion SelectedGameVersions
        {
            set
            {
                try
                {
                    _isUpdatingGameVersionFilter = true;

                    foreach (var curFilter in GameVersionFilters)
                    {
                        curFilter.IsSelected = value.HasFlag(curFilter.Type);
                    }
                }
                finally
                {
                    _isUpdatingGameVersionFilter = false;

                    OnPropertyChanged(nameof(SelectedGameVersionFilters));
                }
            }
        }

        #region commands

        public ICommand ClearSearchTextCommand { get; private set; }

        //TODO: refactor to use interface ICanUpdateLayout -> currently TextBox does not implement it (create own control?)
        private void ClearSearchText(object param)
        {
            SearchText = string.Empty;

            if (param is ICanUpdateLayout updateable)
            {
                updateable.UpdateLayout();
            }
            else if (param is TextBox textBox)
            {
                //Debug.WriteLine($"+ IsFocused: {textBox.IsFocused} | IsKeyboardFocused: {textBox.IsKeyboardFocused} | IsKeyboardFocusWithin: {textBox.IsKeyboardFocusWithin} | CaretIndex: {textBox.CaretIndex}");

                //SearchText = string.Empty;

                //Debug.WriteLine($"++ IsFocused: {textBox.IsFocused} | IsKeyboardFocused: {textBox.IsKeyboardFocused} | IsKeyboardFocusWithin: {textBox.IsKeyboardFocusWithin} | CaretIndex: {textBox.CaretIndex}");

                textBox.Focus();
                textBox.UpdateLayout();

                //Debug.WriteLine($"+++ IsFocused: {textBox.IsFocused} | IsKeyboardFocused: {textBox.IsKeyboardFocused} | IsKeyboardFocusWithin: {textBox.IsKeyboardFocusWithin} | CaretIndex: {textBox.CaretIndex}");
            }
        }

        public ICommand GotFocusCommand { get; private set; }

        private void GotFocus(object param)
        {
            HasFocus = true;
        }

        public ICommand LostFocusCommand { get; private set; }

        private void LostFocus(object param)
        {
            HasFocus = false;
        }

        public ICommand GameVersionFilterChangedCommand { get; private set; }

        private void GameVersionFilterChanged(object param)
        {
            if (_isUpdatingGameVersionFilter)
            {
                return;
            }

            try
            {
                _isUpdatingGameVersionFilter = true;

                if (param is GameVersionFilter x)
                {
                    x.IsSelected = !x.IsSelected;
                }
            }
            finally
            {
                _isUpdatingGameVersionFilter = false;

                OnPropertyChanged(nameof(SelectedGameVersionFilters));
            }
        }

        #endregion

        #region localization

        public string TextSearch
        {
            get { return _textSearch; }
            set { UpdateProperty(ref _textSearch, value); }
        }

        public string TextSearchToolTip
        {
            get { return _textSearchToolTip; }
            set { UpdateProperty(ref _textSearchToolTip, value); }
        }

        public string TextSelectAll
        {
            get { return _textSelectAll; }
            set { UpdateProperty(ref _textSelectAll, value); }
        }

        #endregion       
    }
}
