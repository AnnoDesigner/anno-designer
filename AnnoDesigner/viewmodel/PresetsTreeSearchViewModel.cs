using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using AnnoDesigner.Core.Models;
using AnnoDesigner.model;

namespace AnnoDesigner.viewmodel
{
    public class PresetsTreeSearchViewModel : Notify
    {
        private string _textSearch;
        private string _textSearchToolTip;

        private string _searchText;
        private bool _hasFocus;

        public PresetsTreeSearchViewModel()
        {
            ClearSearchTextCommand = new RelayCommand(ClearSearchText);
            GotFocusCommand = new RelayCommand(GotFocus);
            LostFocusCommand = new RelayCommand(LostFocus);

            HasFocus = false;
            SearchText = string.Empty;
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
