using System;
using System.Diagnostics;

namespace CommandBasedComponents.Core
{
    [DebuggerNonUserCode]
    public static class TransformerDecorators
    {
        public static void Execute(this Func<ICommand, ICommand> decorator, ICommand command, IContext context)
        {
            if(decorator == null)
            {
                decorator = c => c;
            }

            decorator(command).Execute(context);
        }

        public static ICommand All(ICommand command)
        {
            return new LoggingCommand(Interceptor.Intercept(command));
        }
    }
}