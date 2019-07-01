using AnnoDesigner.Core.Models;
using AnnoDesigner.model;
using AnnoDesigner.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace AnnoDesigner
{
    /// <summary>
    /// Interaction logic for Welcome.xaml
    /// </summary>
    public partial class Welcome : Window, ICloseable
    {
        public Welcome()
        {
            InitializeComponent();
        }
    }
}
