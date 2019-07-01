using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.viewmodel
{
    public class PresetsTreeSearchViewModel : Notify
    {

        private string _textSearch;
        private string _textSearchToolTip;
        private string _searchText;

        #region localization

        public string TextSearch
        {
            get { return _textSearch; }
            set { UpdateProperty(ref _textSearch, value); }
        }

        public string TextSearchToolTip
        {
            get { return _textSearchToolTip; }
            set { UpdateProperty(ref _textSearchToolTip, value); }
        }

        #endregion

        public string SearchText
        {
            get { return _searchText; }
            set { UpdateProperty(ref _searchText, value); }
        }
    }
}
