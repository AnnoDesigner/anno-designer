using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.model
{
    [DebuggerDisplay("{" + nameof(Header) + ",nq}")]
    public class GenericTreeItem : Notify
    {
        private string _header;
        private AnnoObject _annoObject;
        private ObservableCollection<GenericTreeItem> _children;

        public GenericTreeItem()
        {
            Header = string.Empty;
            Children = new ObservableCollection<GenericTreeItem>();
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
    }
}
