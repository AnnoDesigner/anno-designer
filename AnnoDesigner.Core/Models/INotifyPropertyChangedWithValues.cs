using System.ComponentModel;

namespace AnnoDesigner.Core.Models
{
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

    public delegate void PropertyChangedWithValuesEventHandler<T>(object sender, PropertyChangedWithValuesEventArgs<T> e);

    public interface INotifyPropertyChangedWithValues<T>
    {
        event PropertyChangedWithValuesEventHandler<T> PropertyChangedWithValues;
    }
}
