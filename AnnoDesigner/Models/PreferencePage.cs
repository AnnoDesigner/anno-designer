using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Models
{
    public class PreferencePage : Notify
    {
        private string _header;
        private string _name;
        private Notify _viewModel;

        public string Header
        {
            get { return _header; }
            set { UpdateProperty(ref _header, value); }
        }

        public string Name
        {
            get { return _name; }
            set { UpdateProperty(ref _name, value); }
        }

        public Notify ViewModel
        {
            get { return _viewModel; }
            set { UpdateProperty(ref _viewModel, value); }
        }
    }
}
