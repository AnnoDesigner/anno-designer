using System;
using System.Collections.Generic;
using System.Text;

namespace FandomParser.Core
{
    public interface ICommons
    {
        string InfoboxTemplateStart { get; }

        string InfoboxTemplateStart2Regions { get; }

        string InfoboxTemplateStart3Regions { get; }

        string InfoboxTemplateStartOldAndNewWorld { get; }

        string InfoboxTemplateEnd { get; }

    }
}
