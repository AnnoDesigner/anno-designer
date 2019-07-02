using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace AnnoDesigner.Behaviors
{
    public class FixCaretIndexBehavior : Behavior<UIElement>
    {
        private TextBox _textBox;

        protected override void OnAttached()
        {
            base.OnAttached();

            _textBox = AssociatedObject as TextBox;

            if (_textBox == null)
            {
                return;
            }

            //_textBox.TextChanged += TextBox_TextChanged;
            _textBox.KeyUp += TextBox_KeyUp;
        }



        protected override void OnDetaching()
        {
            if (_textBox == null)
            {
                return;
            }

            //_textBox.TextChanged -= TextBox_TextChanged;
            _textBox.KeyUp -= TextBox_KeyUp;

            base.OnDetaching();
        }

        //private void TextBox_TextChanged(object sender, RoutedEventArgs routedEventArgs)
        //{
        //    if (string.IsNullOrWhiteSpace(_textBox.Text))
        //    {
        //        Debug.WriteLine("########## UpdateLayout");

        //        Thread.Sleep(10);

        //        _textBox.Clear();
        //        //_textBox.ScrollToHome();
        //        //_textBox.CaretIndex = _textBox.Text.Length;
        //        _textBox.UpdateLayout();
        //    }

        //}

        private void TextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (!string.IsNullOrWhiteSpace(_textBox.Text))
                {
                    _textBox.Clear();
                }

                _textBox.CaretIndex= _textBox.CaretIndex+1;
                _textBox.CaretIndex = _textBox.Text.Length;
                _textBox.UpdateLayout();
            }
        }


    }
}
