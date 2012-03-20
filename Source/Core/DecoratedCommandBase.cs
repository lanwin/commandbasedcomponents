using System;
using System.Diagnostics;

namespace CommandBasedComponents.Core
{
    [DebuggerNonUserCode]
    public abstract class DecoratedCommandBase
    {
        readonly Func<ICommand, ICommand> _decoratorCommand;

        protected DecoratedCommandBase(Func<ICommand, ICommand> decoratorCommand = null)
        {
            _decoratorCommand = decoratorCommand;
        }

        public void ExecuteDecorated(IContext context, ICommand command)
        {
            if(context == null)
            {
                throw new ArgumentNullException("context");
            }

            if(_decoratorCommand != null)
            {
                _decoratorCommand(command).Execute(context);
            }
            else
            {
                command.Execute(context);
            }
        }
    }
}