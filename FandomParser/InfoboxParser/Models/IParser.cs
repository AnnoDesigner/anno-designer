using System;
using System.Collections.Generic;
using System.Text;
using FandomParser.Core.Models;

namespace InfoboxParser.Models
{
    internal interface IParser
    {
        List<IInfobox> GetInfobox(string wikiText);
    }
}
