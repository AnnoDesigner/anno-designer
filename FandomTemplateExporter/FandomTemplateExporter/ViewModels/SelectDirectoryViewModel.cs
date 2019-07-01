using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FandomTemplateExporter.Models;
using AnnoDesigner.Core.Models;

namespace FandomTemplateExporter.ViewModels
{
    public class SelectDirectoryViewModel : BaseModel
    {
        private string selectedDirectory;
        private string selectCaption;
        private bool canExecute;

        public SelectDirectoryViewModel()
        {
            canExecute = true;
        }

        public SelectDirectoryViewModel(string _selectCaption)
            : this()
        {
            SelectCaption = _selectCaption;
        }

        public string SelectCaption
        {
            get { return selectCaption; }
            set { SetPropertyAndNotify(ref selectCaption, value); }
        }

        public string SelectedDirectory
        {
            get { return selectedDirectory; }
            set { SetPropertyAndNotify(ref selectedDirectory, value); }
        }

        public ICommand SelectDirectoryCommand
        {
            get { return new RelayCommand(SelectDirectoryCommandExecute, CanSelectDirectoryCommandExecute); }
        }

        private void SelectDirectoryCommandExecute(object param)
        {
            canExecute = false;

            if (CommonFileDialog.IsPlatformSupported)
            {
                var ofd = new CommonOpenFileDialog
                {
                    IsFolderPicker = true,
                    Multiselect = false,
                    Title = SelectCaption,
                    EnsurePathExists = true,
                    EnsureFileExists = true,
                    ShowPlacesList = true,
                    EnsureValidNames = true,
                    AddToMostRecentlyUsedList = false,
                    AllowNonFileSystemItems = false,
                    EnsureReadOnly = false
                };

                if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var localSelectedDirectory = ofd.FileName;
                    if (Directory.Exists(localSelectedDirectory))
                    {
                        SelectedDirectory = localSelectedDirectory;
                    }
                }
            }
            else
            {
                MessageBox.Show("Platform is not supported");
            }

            canExecute = true;
        }

        private bool CanSelectDirectoryCommandExecute(object param)
        {
            return canExecute;
        }


    }
}
