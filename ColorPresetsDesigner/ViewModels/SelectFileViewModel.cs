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
using ColorPresetsDesigner.Models;

namespace ColorPresetsDesigner.ViewModels
{
    public class SelectFileViewModel : BaseModel
    {
        private string selectedFile;
        private string selectCaption;
        private bool canExecute;

        public SelectFileViewModel()
        {
            canExecute = true;
        }

        public SelectFileViewModel(string _selectCaption)
            : this()
        {
            SelectCaption = _selectCaption;
        }

        public string SelectCaption
        {
            get { return selectCaption; }
            set { SetPropertyAndNotify(ref selectCaption, value); }
        }

        public string SelectedFile
        {
            get { return selectedFile; }
            set { SetPropertyAndNotify(ref selectedFile, value); }
        }

        public ICommand SelectFileCommand
        {
            get { return new RelayCommand(SelectFileCommandExecute, CanSelectFileCommandExecute); }
        }

        private void SelectFileCommandExecute(object param)
        {
            canExecute = false;

            if (CommonFileDialog.IsPlatformSupported)
            {
                var ofd = new CommonOpenFileDialog
                {
                    IsFolderPicker = false,
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
                    var localSelectedFile = ofd.FileName;
                    if (File.Exists(localSelectedFile))
                    {
                        SelectedFile = localSelectedFile;
                    }
                }
            }
            else
            {
                MessageBox.Show("Platform is not supported");
            }

            canExecute = true;
        }

        private bool CanSelectFileCommandExecute(object param)
        {
            return canExecute;
        }


    }
}
