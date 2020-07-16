using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Models
{
    /// <summary>
    /// Types of (predefined) colors.
    /// </summary>
    /// <remarks>The members are used as a translation key with a prefix of "ColorType" (e.g. ColorTypeLight).</remarks>
    public enum UserDefinedColorType
    {
        Default,
        Light,
        Custom
    }
}
