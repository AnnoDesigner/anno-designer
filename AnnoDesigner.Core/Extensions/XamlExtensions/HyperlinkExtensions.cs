using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace AnnoDesigner.Core.Extensions.XamlExtensions;

//modified from https://stackoverflow.com/a/11433814
public static class HyperlinkExtensions
{
    [AttachedPropertyBrowsableForType(typeof(Hyperlink))]
    public static bool GetIsExternal(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsExternalProperty);
    }

    public static void SetIsExternal(DependencyObject obj, bool value)
    {
        obj.SetValue(IsExternalProperty, value);
    }

    [AttachedPropertyBrowsableForType(typeof(Hyperlink))]
    public static bool GetConfirmNavigation(DependencyObject obj)
    {
        return (bool)obj.GetValue(ConfirmNavigationProperty);
    }

    public static void SetConfirmNavigation(DependencyObject obj, bool value)
    {
        obj.SetValue(ConfirmNavigationProperty, value);
    }

    public static string GetNavigationMessage(DependencyObject obj)
    {
        return (string)obj.GetValue(NavigationMessageProperty);
    }

    public static void SetNavigationMessage(DependencyObject obj, string value)
    {
        obj.SetValue(NavigationMessageProperty, value);
    }



    public static string GetNavigationTitle(DependencyObject obj)
    {
        return (string)obj.GetValue(NavigationTitleProperty);
    }

    public static void SetNavigationTitle(DependencyObject obj, string value)
    {
        obj.SetValue(NavigationTitleProperty, value);
    }

    public static readonly DependencyProperty NavigationTitleProperty =
        DependencyProperty.RegisterAttached("NavigationTitle", typeof(string), typeof(HyperlinkExtensions), new PropertyMetadata("Opening an external link"));

    public static readonly DependencyProperty NavigationMessageProperty =
        DependencyProperty.RegisterAttached("NavigationMessage", typeof(string), typeof(HyperlinkExtensions), new PropertyMetadata("This will open a new tab in your default web browser. Continue?"));

    //defaults to true
    public static readonly DependencyProperty ConfirmNavigationProperty =
        DependencyProperty.RegisterAttached("ConfirmNavigation", typeof(bool), typeof(HyperlinkExtensions), new PropertyMetadata(true));

    public static readonly DependencyProperty IsExternalProperty =
        DependencyProperty.RegisterAttached("IsExternal", typeof(bool), typeof(HyperlinkExtensions), new UIPropertyMetadata(false, OnIsExternalChanged));

    

    private static void OnIsExternalChanged(object sender, DependencyPropertyChangedEventArgs args)
    {
        var hyperlink = sender as Hyperlink;
        if ((bool)args.NewValue)
            hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
        else
            hyperlink.RequestNavigate -= Hyperlink_RequestNavigate;
    }

    private static void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        var hyperlink = sender as Hyperlink;

        var confirmNavigation = GetConfirmNavigation(hyperlink);
        if (!confirmNavigation || 
            MessageBox.Show(
                GetNavigationMessage(hyperlink), GetNavigationTitle(hyperlink), 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
