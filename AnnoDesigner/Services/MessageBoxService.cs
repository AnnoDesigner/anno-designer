using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AnnoDesigner.Core.Services;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace AnnoDesigner.Services
{
    public class MessageBoxService : IMessageBoxService
    {
        public void ShowMessage(object owner, string message, string title)
        {
            if (owner is Window ownerWindow)
            {
                MessageBox.Show(ownerWindow,
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        public void ShowWarning(object owner, string message, string title)
        {
            if (owner is Window ownerWindow)
            {
                MessageBox.Show(ownerWindow,
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show(message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        public void ShowError(object owner, string message, string title)
        {
            if (owner is Window ownerWindow)
            {
                MessageBox.Show(ownerWindow,
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show(message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public bool ShowQuestion(object owner, string message, string title)
        {
            MessageBoxResult result;

            if (owner is Window ownerWindow)
            {
                result = MessageBox.Show(ownerWindow,
                    message,
                    title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
            }
            else
            {
                result = MessageBox.Show(message,
                    title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
            }

            return result == MessageBoxResult.Yes;
        }

        public bool? ShowQuestionWithCancel(object owner, string message, string title)
        {
            var result = owner is Window ownerWindow
                ? MessageBox.Show(ownerWindow,
                    message,
                    title,
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question)
                : MessageBox.Show(message,
                    title,
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
            return result switch
            {
                MessageBoxResult.Yes => true,
                MessageBoxResult.No => false,
                _ => null,
            };
        }
    }
}
