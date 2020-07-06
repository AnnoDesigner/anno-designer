using System;
using System.Collections.Generic;
using System.Text;

namespace InfoboxParser
{
    public interface IInfoboxExtractor
    {
        List<(string title, string infobox)> ExtractInfobox(string content);
    }
}
