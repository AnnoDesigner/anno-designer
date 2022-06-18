using System.CommandLine;
using System.CommandLine.Binding;

namespace AnnoDesigner.CommandLine.Arguments
{
    public class OpenArgs : IProgramArgs
    {
        public class Binder : ArgsBinderBase<OpenArgs>
        {
            private readonly Argument<string> filename;

            public Binder()
            {
                filename = new("layoutPath", "Path to layout file (*.ad)");

                command = new Command("open", "Starts AnnoDesigner with specified layout file opened")
                {
                    filename
                };
            }

            protected override OpenArgs GetBoundValue(BindingContext bindingContext)
            {
                return new OpenArgs()
                {
                    Filename = bindingContext.ParseResult.GetValueForArgument(filename)
                };
            }
        }

        public string Filename { get; set; }
    }
}
