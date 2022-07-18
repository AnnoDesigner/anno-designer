using Xunit.Sdk;

namespace Xunit
{
    [XunitTestCaseDiscoverer("PresetParser.Tests.CultureAware.CulturedFactAttributeDiscoverer", "PresetParser.Tests")]
    public sealed class CulturedFactAttribute : FactAttribute
    {
        public CulturedFactAttribute(params string[] cultures) { }
    }
}