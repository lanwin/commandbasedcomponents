using System.Diagnostics;

namespace CommandBasedComponents.Core
{
    [DebuggerNonUserCode]
    public static class AfterCommand<TCommandType>
    {
        public static void Run(ICommand command)
        {
            Interceptor.RunAfter<TCommandType>(command);
        }
    }
}