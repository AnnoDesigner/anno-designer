using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AnnoDesigner.Core.Controls;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Models;
using AnnoDesigner.ViewModels;

namespace AnnoDesigner
{
    /// <summary>
    /// Interaction logic for KeyRecorderWindow.xaml
    /// </summary>
    public partial class HotkeyRecorderWindow : Window
    {
        //TODO: localize title and buttons. 
        public  HotkeyRecorderWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
            ViewModel = new HotkeyRecorderViewModel
            {
                ActionRecorder = ActionRecorder
            };
            DataContext = ViewModel;
            KeyDown += HotkeyRecorderWindow_KeyDown;
        }

        private void HotkeyRecorderWindow_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = false;
        }

        public (Key key, ModifierKeys modifiers, MouseAction action, ActionRecorder.ActionType result, bool userCancelled) RecordNewAction()
        {
            ViewModel.Reset();
            ShowDialog();
            return (ViewModel.Key, ViewModel.Modifiers, ViewModel.MouseAction, ViewModel.Result, !DialogResult.Value);
        }

        private HotkeyRecorderViewModel ViewModel { get; set; }

        private void SaveRecordingClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            e.Handled = true; //prevent click from bubbling up to window.
            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            e.Handled = true; //prevent click from bubbling up to window.
            Close();
        }
    }
}
