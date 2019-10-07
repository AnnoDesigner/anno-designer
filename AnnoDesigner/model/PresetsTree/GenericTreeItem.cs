using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.model.PresetsTree
{
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
            Children = new ObservableCollection<GenericTreeItem>();
            IsExpanded = false;
            IsVisible = true;
            IsSelected = false;
        }

        public GenericTreeItem Parent
        {
            get { return _parent; }
            private set { UpdateProperty(ref _parent, value); }
        }

        public GenericTreeItem Root
        {
            get { return Parent == null ? this : Parent.Root; }
        }

        public string Header
        {
            get { return _header; }
            set { UpdateProperty(ref _header, value); }
        }

        public AnnoObject AnnoObject
        {
            get { return _annoObject; }
            set { UpdateProperty(ref _annoObject, value); }
        }

        public ObservableCollection<GenericTreeItem> Children
        {
            get { return _children; }
            set { UpdateProperty(ref _children, value); }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                UpdateProperty(ref _isExpanded, value);

                //also expand all parent nodes
                if (value && Parent != null)
                {
                    Parent.IsExpanded = value;
                }
            }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set { UpdateProperty(ref _isVisible, value); }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { UpdateProperty(ref _isSelected, value); }
        }

        /// <summary>
        /// Id of this node is mainly used to save/restore a tree state.
        /// </summary>        
        public int Id
        {
            get { return _id; }
            set { UpdateProperty(ref _id, value); }
        }
    }
}
