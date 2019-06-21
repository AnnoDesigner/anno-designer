using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AnnoDesigner.Core.CoreConstants;

namespace AnnoDesigner.model
{
    [DebuggerDisplay("{" + nameof(Header) + ",nq}")]
    public class GameHeaderTreeItem : GenericTreeItem
    {
        private GameVersion _gameVersion;

        public GameHeaderTreeItem()
        {
            GameVersion = GameVersion.Unknown;
        }

        public GameVersion GameVersion
        {
            get { return _gameVersion; }
            set { UpdateProperty(ref _gameVersion, value); }
        }
    }
}
