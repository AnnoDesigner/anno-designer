using System;
using System.Collections.Generic;
using System.Reflection;

namespace AnnoDesigner.Undo.Operations;

public class ModifyObjectPropertiesOperation<TObject, TProperty> : BaseOperation
{
    private string propertyName;
    private PropertyInfo propertyInfo;

    public string PropertyName
    {
        get => propertyName;
        set
        {
            propertyName = value;
            propertyInfo = typeof(TObject).GetProperty(value);
#if DEBUG
            if (!propertyInfo.PropertyType.IsAssignableFrom(typeof(TProperty)))
            {
                throw new InvalidCastException($"Type \"{typeof(TProperty)}\" can't be assigned to property \"{value}\" of type \"{typeof(TObject)}\".");
            }
#endif
        }
    }

    public IEnumerable<(TObject obj, TProperty oldValue, TProperty newValue)> ObjectPropertyValues { get; set; }

    protected override void UndoOperation()
    {
        foreach ((TObject obj, TProperty oldValue, TProperty _) in ObjectPropertyValues)
        {
            propertyInfo.SetValue(obj, oldValue);
        }
    }

    protected override void RedoOperation()
    {
        foreach ((TObject obj, TProperty _, TProperty newValue) in ObjectPropertyValues)
        {
            propertyInfo.SetValue(obj, newValue);
        }
    }
}
