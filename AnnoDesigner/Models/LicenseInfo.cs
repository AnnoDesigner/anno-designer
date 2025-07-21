using System;
using System.Collections.Generic;
using System.Linq;

namespace AnnoDesigner.Models;

public class LicenseInfo
{
    public string ProjectName { get; set; }
    public string License { get; set; }
    public string LicenseURL { get; set; }
    public string ProjectWebsite { get; set; }
    public IEnumerable<string> Assets { get; set; }
    public bool HasAssets => Assets != null && Assets.Count() > 0;
}
