using System.CommandLine;
using System.CommandLine.Binding;

namespace AnnoDesigner.CommandLine.Arguments
{
    public class AdminRestartArgs : IProgramArgs
    {
        public class Binder : ArgsBinderBase<AdminRestartArgs>
        {
            public Binder()
            {
                command = new Command(Arguments)
                {
                    IsHidden = true
                };
            }

            protected override AdminRestartArgs GetBoundValue(BindingContext bindingContext)
            {
                return new AdminRestartArgs();
            }
        }

        public const string Arguments = "askAdmin";
    }
}
