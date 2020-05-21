﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AnnoDesigner.Core.Models;
using System.Collections.ObjectModel;
using AnnoDesigner.ViewModels;
using AnnoDesigner.PreferencesPages;

namespace AnnoDesigner
{
    /// <summary>
    /// Interaction logic for Preferences.xaml
    /// </summary>
    public partial class Preferences : Window
    {
        public Preferences()
        {
            InitializeComponent();
            preferencesViewModel = DataContext as PreferencesViewModel;

            //For testing only
            //TODO: Remove this before full PR
            this.CurrentPage.Navigate(new ManageKeybindings());
        }

        private PreferencesViewModel preferencesViewModel;
    }
}
