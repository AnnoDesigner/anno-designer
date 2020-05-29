﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AnnoDesigner.Models;
using AnnoDesigner.ViewModels;

namespace AnnoDesigner.PreferencesPages
{
    /// <summary>
    /// Interaction logic for ManageKeybindings.xaml
    /// </summary>
    public partial class ManageKeybindings : Page, INavigatedTo
    {
        public void NavigatedTo(object parameter)
        {
            DataContext = new ManageKeybindingsViewModel((HotkeyCommandManager)parameter);
        }
        public ViewModels.ManageKeybindingsViewModel ViewModel;
    }
}
