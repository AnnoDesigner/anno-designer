using System;
using System.Collections.Generic;
using AnnoDesigner.Core.DataStructures;

namespace AnnoDesigner.Core.Models.Undoable
{
	public abstract class ModelBase : INotifyPropertyChangedWithValue, IUndoable
    {
		public event PropertyChangedWithValueEventHandler PropertyChanged;
		public event UndoEventHandler UndoableAction;

		private bool recordEvents;
		private UndoEventArgs recordedEventArgs;

		public bool Undoing { get; set; }

		public bool RecordEvents
		{
			get => recordEvents;
			set
			{
				recordEvents = value;
				if (!recordEvents && recordedEventArgs != null)
				{
					PropagateUndoableAction(recordedEventArgs);
				}
			}
		}

		protected bool Set<T>(ref T oldValue, T newValue, string propertyName)
		{
			if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
			{
				OnUndoableAction(oldValue, newValue, propertyName);
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

		protected virtual void OnUndoableAction<T>(T oldValue, T newValue, string propertyName)
		{
			if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
			{
				PropagateUndoableAction(new PropertyUndoEventArgs
				{
					RefObject = this,
					OldValue = oldValue,
					NewValue = newValue,
					PropertyName = propertyName
				});
			}
		}

		protected void PropagateUndoableAction(UndoEventArgs e)
		{
			if (!Undoing)
			{
				if (RecordEvents)
				{
					if (recordedEventArgs != null)
					{
                        recordedEventArgs.AddToChain(e);
					}
					else
					{
						recordedEventArgs = e;
					}
				}
				else
				{
					UndoableAction?.Invoke(e);
					recordedEventArgs = null;
				}
			}
		}

		public void AtomicAction(Action action)
		{
			bool flag = RecordEvents;
			RecordEvents = true;
			action();
			RecordEvents = flag;
		}
	}
}
