using AnnoDesigner.Core.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AnnoDesigner.Models.PresetsTree;

[DebuggerDisplay("{" + nameof(Header) + ",nq}")]
public class GenericTreeItem : Notify
{
    private GenericTreeItem _parent;
    private string _header;
    private AnnoObject _annoObject;
    private ObservableCollection<GenericTreeItem> _children;
    private bool _isExpanded;
    private bool _isVisible;
    private bool _isSelected;
    private int _id;

    public GenericTreeItem(GenericTreeItem parent)
    {
        Parent = parent;
        Header = string.Empty;
        Children = [];
        IsExpanded = false;
        IsVisible = true;
        IsSelected = false;
    }

    public GenericTreeItem Parent
    {
        get => _parent;
        private set => UpdateProperty(ref _parent, value);
    }

    public GenericTreeItem Root => Parent == null ? this : Parent.Root;

    public string Header
    {
        get => _header;
        set => UpdateProperty(ref _header, value);
    }

    public AnnoObject AnnoObject
    {
        get => _annoObject;
        set => UpdateProperty(ref _annoObject, value);
    }

    public ObservableCollection<GenericTreeItem> Children
    {
        get => _children;
        set => UpdateProperty(ref _children, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            _ = UpdateProperty(ref _isExpanded, value);

            //also expand all parent nodes
            if (value && Parent != null)
            {
                Parent.IsExpanded = value;
            }
        }
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => UpdateProperty(ref _isVisible, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => UpdateProperty(ref _isSelected, value);
    }

    /// <summary>
    /// Id of this node is mainly used to save/restore a tree state.
    /// </summary>        
    public int Id
    {
        get => _id;
        set => UpdateProperty(ref _id, value);
    }
}
