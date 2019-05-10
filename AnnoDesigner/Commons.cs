using AnnoDesigner.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner
{
    public class Commons : ICommons
    {
        #region ctor

        private static readonly Lazy<Commons> lazy = new Lazy<Commons>(() => new Commons());

        public static Commons Instance
        {
            get { return lazy.Value; }
        }

        private Commons()
        {
        }

        #endregion

        private static readonly UpdateHelper _updateHelper;

        static Commons()
        {
            _updateHelper = new UpdateHelper();
        }

        public IUpdateHelper UpdateHelper
        {
            get { return _updateHelper; }
        }
    }
}
