using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using AnnoDesigner.Core.Models;
using AnnoDesigner.model;
using static AnnoDesigner.Core.CoreConstants;

namespace AnnoDesigner.viewmodel
{
    public class PresetsTreeSearchViewModel : Notify
    {
        private string _textSearch;
        private string _textSearchToolTip;

        private string _searchText;
        private bool _hasFocus;
        private ObservableCollection<GameVersionFilter> _gameVersionFilters;
        private ObservableCollection<GameVersionFilter> _selectedGameVersionFilters;
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
            SelectedGameVersionFilters = new ObservableCollection<GameVersionFilter>();
            InitGameVersionFilters();
            SelectedGameVersionFilters.Add(GameVersionFilters.Single(x => x.Type == GameVersion.All));
        }

        private void InitGameVersionFilters()
        {
            //add "All" at first position
            GameVersionFilters.Add(new GameVersionFilter
            {
                Name = GameVersion.All.ToString(),
                Type = GameVersion.All,
                IsSelected = true
            });

            foreach (GameVersion curGameVersion in Enum.GetValues(typeof(GameVersion)))
            {
                if (curGameVersion == GameVersion.Unknown || curGameVersion == GameVersion.All)
                {
                    continue;
                }

                GameVersionFilters.Add(new GameVersionFilter
                {
                    Name = curGameVersion.ToString().Replace("Anno", "Anno "),
                    Type = curGameVersion
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
            get { return _selectedGameVersionFilters; }
            set { UpdateProperty(ref _selectedGameVersionFilters, value); }
        }

        #region commands

        public ICommand ClearSearchTextCommand { get; private set; }

        //TODO: refactor to use interface ICanUpdateLayout -> currently TextBox doe not implement it (create own control?)
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

                if (!(param is GameVersionFilter clickedFilter))
                {
                    return;
                }

                ////at least "All" has to be selected
                //if (SelectedGameVersionFilters.Count == 0 && clickedFilter.Type == GameVersion.All)
                //{                   
                //    //allFilter.IsSelected = true;
                //    //SelectedGameVersionFilters.Clear();
                //    SelectedGameVersionFilters.Add(GameVersionFilters.Single(x => x.Type == GameVersion.All));
                //    return;
                //}

                var allFilter = GameVersionFilters.Single(x => x.Type == GameVersion.All);

                if (clickedFilter.Type != GameVersion.All)
                {
                    allFilter.IsSelected = false;
                    clickedFilter.IsSelected = true;
                }
                else
                {
                    foreach (var curGameFilter in GameVersionFilters.Where(x => x.Type != GameVersion.All))
                    {
                        curGameFilter.IsSelected = false;
                    }

                    allFilter.IsSelected = true;
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

        #endregion       
    }
}
