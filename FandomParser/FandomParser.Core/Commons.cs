using System;
using System.Collections.Generic;
using System.Text;

namespace FandomParser.Core
{
    public class Commons : ICommons
    {
        private const string INFOBOX_TEMPLATE_START = "{{Infobox Buildings";
        private const string INFOBOX_TEMPLATE_BOTH_WORLDS_START = "{{Infobox Buildings Old and New World";
        private const string INFOBOX_TEMPLATE_END = "}}";

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

        public string InfoboxTemplateStart => INFOBOX_TEMPLATE_START;

        public string InfoboxTemplateStartBothWorlds => INFOBOX_TEMPLATE_BOTH_WORLDS_START;

        public string InfoboxTemplateEnd => INFOBOX_TEMPLATE_END;

    }
}
