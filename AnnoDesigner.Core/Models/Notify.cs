using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AnnoDesigner.Core.Models;


/// <summary>
/// Holds the base INotifyPropertyChanged implementation plus helper methods
/// //https://stackoverflow.com/questions/1315621/implementing-inotifypropertychanged-does-a-better-way-exist
/// </summary>
public class Notify : INotifyPropertyChanged, INotifyPropertyChangedWithValues<object>
{
    public event PropertyChangedEventHandler PropertyChanged;
    public event EventHandler<PropertyChangedWithValuesEventArgs<object>> PropertyChangedWithValues;

    protected void OnPropertyChanged(string propertyName)
    {
        //Invoke event if not null
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void OnPropertyChangedWithValues(string propertyName, object oldValue, object newValue)
    {
        //Invoke event if not null
        PropertyChangedWithValues?.Invoke(this, new PropertyChangedWithValuesEventArgs<object>(propertyName, oldValue, newValue));
    }

    protected bool UpdateProperty<T>(ref T field, T value, [CallerMemberName] string name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        OnPropertyChangedWithValues(name, field, value);
        field = value;
        OnPropertyChanged(name);
        return true;
    }
}


