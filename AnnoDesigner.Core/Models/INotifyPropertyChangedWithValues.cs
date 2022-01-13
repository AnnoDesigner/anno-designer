using System;
using System.ComponentModel;

namespace AnnoDesigner.Core.Models
{
    /// <inheritdoc/>
    public class PropertyChangedWithValuesEventArgs<T> : PropertyChangedEventArgs
    {
        public T OldValue { get; set; }

        public T NewValue { get; set; }

        public PropertyChangedWithValuesEventArgs(string propertyName, T oldValue, T newValue) : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    public interface INotifyPropertyChangedWithValues<T>
    {
        event EventHandler<PropertyChangedWithValuesEventArgs<T>> PropertyChangedWithValues;
    }
}
