using System;
using System.Collections.Generic;
using System.Text;

namespace InfoboxParser
{
    public interface IInfoboxExtractor
    {
        string ExtractInfobox(string content);
    }
}
