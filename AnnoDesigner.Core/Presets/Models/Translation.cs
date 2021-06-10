using System.Diagnostics;

namespace AnnoDesigner.Core.Presets.Models
{
    [DebuggerDisplay("{" + nameof(Key) + "} - {" + nameof(Value) + "}")]
    public class Translation
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}