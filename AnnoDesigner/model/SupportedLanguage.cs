using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.model
{
    [DebuggerDisplay("{" + nameof(Name) + ",nq} ({" + nameof(IsSelected) + "})")]
    public class SupportedLanguage : Notify
    {
        private string _name;
        private string _flagPath;
        private bool _isSelected;

        public SupportedLanguage(string nameToUse)
        {
            Name = nameToUse;
        }

        public string Name
        {
            get { return _name; }
            private set { UpdateProperty(ref _name, value); }
        }

        public string FlagPath
        {
            get { return _flagPath; }
            set { UpdateProperty(ref _flagPath, value); }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { UpdateProperty(ref _isSelected, value); }
        }
    }
}


