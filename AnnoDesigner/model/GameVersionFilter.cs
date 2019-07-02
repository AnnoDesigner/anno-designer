using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;
using static AnnoDesigner.Core.CoreConstants;

namespace AnnoDesigner.model
{
    public class GameVersionFilter : Notify
    {
        private GameVersion _gameVersion;
        private string _name;
        private bool _isSelected;

        public GameVersion Type
        {
            get { return _gameVersion; }
            set { UpdateProperty(ref _gameVersion, value); }
        }

        public string Name
        {
            get { return _name; }
            set { UpdateProperty(ref _name, value); }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { UpdateProperty(ref _isSelected, value); }
        }

    }
}
