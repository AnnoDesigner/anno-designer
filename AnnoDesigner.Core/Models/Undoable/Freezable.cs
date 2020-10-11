using System.Collections.Generic;

namespace AnnoDesigner.Core.Models.Undoable
{
    internal interface IFreezable
    {
        bool FreezeUpdates { get; set; }
    }

    /// <summary>
    /// Class which can suspend INotifyPropertyChanged events when
    /// some action is done on this item which should produce only one event.
    /// </summary>
    public abstract class Freezable : ModelBase, IFreezable
    {
        private class FreezedProperty
        {
            public object OldValue { get; set; }

            public object NewValue { get; set; }

            public bool Undoable { get; set; }
        }

        private bool freezeUpdates;

        public bool FreezeUpdates
        {
            get => freezeUpdates;
            set
            {
                var flag = freezeUpdates;
                freezeUpdates = value;
                if (!flag && value)
                {
                    Freeze();
                }
                else if (flag && !value)
                {
                    AtomicAction(Unfreeze);
                }
            }
        }

        private Dictionary<string, FreezedProperty> ChangedProperties { get; } = new Dictionary<string, FreezedProperty>();

        protected abstract HashSet<string> TrackedProperties { get; }

        protected override void OnUndoableAction<T>(T oldValue, T newValue, string propertyName)
        {
            if (!FreezeUpdates)
            {
                base.OnUndoableAction(oldValue, newValue, propertyName);
            }
            else if (TrackedProperties == null || TrackedProperties.Contains(propertyName))
            {
                lock (ChangedProperties)
                {
                    if (ChangedProperties.ContainsKey(propertyName))
                    {
                        ChangedProperties[propertyName].NewValue = newValue;
                        ChangedProperties[propertyName].Undoable = true;
                    }
                    else
                    {
                        ChangedProperties[propertyName] = new FreezedProperty
                        {
                            OldValue = oldValue,
                            NewValue = newValue,
                            Undoable = true
                        };
                    }
                }
            }
        }

        protected override void OnPropertyChanged<T>(T oldValue, T newValue, string propertyName)
        {
            if (!FreezeUpdates)
            {
                base.OnPropertyChanged(oldValue, newValue, propertyName);
            }
            else if (TrackedProperties == null || TrackedProperties.Contains(propertyName))
            {
                lock (ChangedProperties)
                {
                    if (ChangedProperties.ContainsKey(propertyName))
                    {
                        ChangedProperties[propertyName].NewValue = newValue;
                    }
                    else
                    {
                        ChangedProperties[propertyName] = new FreezedProperty
                        {
                            OldValue = oldValue,
                            NewValue = newValue,
                            Undoable = false
                        };
                    }
                }
            }
        }

        protected virtual void Freeze()
        {
            lock (ChangedProperties)
            {
                ChangedProperties.Clear();
            }
        }

        protected virtual void Unfreeze()
        {
            lock (ChangedProperties)
            {
                foreach (var changedProperty in ChangedProperties)
                {
                    if (changedProperty.Value.Undoable)
                    {
                        OnUndoableAction(changedProperty.Value.OldValue, changedProperty.Value.NewValue, changedProperty.Key);
                    }
                    if (changedProperty.Value.OldValue != null && changedProperty.Value.NewValue != null)
                    {
                        OnPropertyChanged(changedProperty.Value.OldValue, changedProperty.Value.NewValue, changedProperty.Key);
                    }
                }
                ChangedProperties.Clear();
            }
        }
    }
}
