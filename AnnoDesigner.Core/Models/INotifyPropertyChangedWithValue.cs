using System;

namespace AnnoDesigner.Core.Models
{
    public class PropertyChangedWithValueEventArgs : EventArgs
    {
        public PropertyChangedWithValueEventArgs(string propertyName, object oldValue)
        {
            PropertyName = propertyName;
            OldValue = oldValue;
        }

        public string PropertyName { get; }
        public object OldValue { get; }
    }

    public delegate void PropertyChangedWithValueEventHandler(object sender, PropertyChangedWithValueEventArgs e);

    public interface INotifyPropertyChangedWithValue
    {
        event PropertyChangedWithValueEventHandler PropertyChanged;
    }

}
