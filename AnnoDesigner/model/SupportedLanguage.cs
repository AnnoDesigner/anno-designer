using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using AnnoDesigner.viewmodel;

namespace AnnoDesigner.model
{
    public class SupportedLanguage
    {
        public SupportedLanguage(string nameToUse)
        {
            Name = nameToUse;
        }

        public string Name { get; }

        public string FlagPath { get; set; }
    }
}


