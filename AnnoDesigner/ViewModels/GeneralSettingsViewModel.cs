using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;
using NLog;

namespace AnnoDesigner.ViewModels
{
    public class GeneralSettingsViewModel : Notify
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IAppSettings _appSettings;

        private bool _useLightGridLines;
        private bool _hideInfluenceOnSelection;
        private bool _useZoomToPoint;

        public GeneralSettingsViewModel(IAppSettings appSettingsToUse)
        {
            _appSettings = appSettingsToUse;
        }

        public bool UseLightGridLines
        {
            get { return _useLightGridLines; }
            set
            {
                if (UpdateProperty(ref _useLightGridLines, value))
                {
                    _appSettings.Save();
                }
            }
        }

        public bool HideInfluenceOnSelection
        {
            get { return _hideInfluenceOnSelection; }
            set
            {
                if (UpdateProperty(ref _hideInfluenceOnSelection, value))
                {
                    _appSettings.Save();
                }
            }
        }

        public bool UseZoomToPoint
        {
            get { return _useZoomToPoint; }
            set
            {
                if (UpdateProperty(ref _useZoomToPoint, value))
                {
                    _appSettings.Save();
                }
            }
        }
    }
}
