using System;
using System.Collections.Generic;
using System.Text;

namespace FandomParser.Core
{
    public interface ICommons
    {
        string InfoboxTemplateStart { get; }

        string InfoboxTemplateStartBothWorlds { get; }

        string InfoboxTemplateEnd { get; }

    }
}
