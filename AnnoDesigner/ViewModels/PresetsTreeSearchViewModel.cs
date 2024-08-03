using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using static AnnoDesigner.Core.CoreConstants;

namespace AnnoDesigner.ViewModels;

public class PresetsTreeSearchViewModel : Notify
{
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
        GameVersionFilters = [];
        InitGameVersionFilters();
    }

    private void InitGameVersionFilters()
    {
        int order = 0;
        foreach (GameVersion curGameVersion in Enum.GetValues<GameVersion>())
        {
            if (curGameVersion is GameVersion.Unknown or GameVersion.All)
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
        get => _searchText;
        set => UpdateProperty(ref _searchText, value);
    }

    public bool HasFocus
    {
        get => _hasFocus;
        set => UpdateProperty(ref _hasFocus, value);
    }

    public ObservableCollection<GameVersionFilter> GameVersionFilters
    {
        get => _gameVersionFilters;
        set => UpdateProperty(ref _gameVersionFilters, value);
    }

    public ObservableCollection<GameVersionFilter> SelectedGameVersionFilters => new(GameVersionFilters.Where(x => x.IsSelected));

    public GameVersion SelectedGameVersions
    {
        set
        {
            try
            {
                _isUpdatingGameVersionFilter = true;

                foreach (GameVersionFilter curFilter in GameVersionFilters)
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

            _ = textBox.Focus();
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
}
