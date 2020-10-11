using System;
using System.Collections.Generic;

namespace AnnoDesigner.Core.Models.Undoable
{
	public abstract class ModelBase : INotifyPropertyChangedWithValue
    {
		public event PropertyChangedWithValueEventHandler PropertyChanged;

		protected bool Set<T>(ref T oldValue, T newValue, string propertyName)
		{
			if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
			{
				var oldValue2 = oldValue;
				oldValue = newValue;
				OnPropertyChanged(oldValue2, newValue, propertyName);
				return true;
			}
			return false;
		}

		protected virtual void OnPropertyChanged<T>(T oldValue, T newValue, string propertyName)
		{
			if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
			{
				PropertyChanged?.Invoke(this, new PropertyChangedWithValueEventArgs(propertyName, oldValue));
			}
		}
	}
}
