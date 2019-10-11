using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.model;

namespace AnnoDesigner.CustomEventArgs
{
    [Serializable]
    public class UpdateStatisticsEventArgs : EventArgs
    {
        public new static readonly UpdateStatisticsEventArgs Empty = new UpdateStatisticsEventArgs();

        public static readonly UpdateStatisticsEventArgs All = new UpdateStatisticsEventArgs(UpdateMode.All);

        private UpdateStatisticsEventArgs()
        {
        }

        public UpdateStatisticsEventArgs(UpdateMode _updateMode)
        {
            Mode = _updateMode;
        }

        public UpdateMode Mode { get; private set; }
    }
}
