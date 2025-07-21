using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Threading.Tasks;

namespace AnnoDesigner.CommandLine;

public abstract class ArgsBinderBase<T> : BinderBase<T>
{
    protected Command command;

    public Command ConfigureCommand(Action<T> handler)
    {
        command.SetHandler(handler, this);

        return command;
    }

    public Command ConfigureCommand(Func<T, Task> handler)
    {
        command.SetHandler(handler, this);

        return command;
    }
}
