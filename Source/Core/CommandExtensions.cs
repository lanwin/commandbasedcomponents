using System;
using System.Diagnostics;

namespace CommandBasedComponents.Core
{
    [DebuggerNonUserCode]
    public static class CommandExtensions
    {
        public static ICommand IfHasKey(this ICommand command, object key)
        {
            if(key == null)
            {
                throw new ArgumentNullException("key");
            }

            return new IfCommand(command, context => context.HasValue(key));
        }

        public static ICommand If(this ICommand command, Predicate<IContext> action)
        {
            if(action == null)
            {
                throw new ArgumentNullException("action");
            }

            return new IfCommand(command, action);
        }

        public static ICommand IgnoreExceptions(this ICommand command)
        {
            return new IgnoreExceptionsCommand(command);
        }

        public static ICommand GetDecorated(this IDecoratorCommand command)
        {
            ICommand baseCommand = command;
            while(baseCommand is IDecoratorCommand)
            {
                baseCommand = ( (IDecoratorCommand)baseCommand ).Inner;
            }
            return baseCommand;
        }

        class IfCommand : IDecoratorCommand
        {
            readonly Predicate<IContext> _action;

            public IfCommand(ICommand inner, Predicate<IContext> action)
            {
                if(inner == null)
                {
                    throw new ArgumentNullException("inner");
                }
                if(action == null)
                {
                    throw new ArgumentNullException("action");
                }
                Inner = inner;
                _action = action;
            }

            public void Execute(IContext context)
            {
                if(_action(context))
                {
                    Inner.Execute(context);
                }
            }

            public ICommand Inner { get; private set; }
        }

        class IgnoreExceptionsCommand : IDecoratorCommand
        {
            public IgnoreExceptionsCommand(ICommand inner)
            {
                if(inner == null)
                {
                    throw new ArgumentNullException("inner");
                }
                Inner = inner;
            }

            public void Execute(IContext context)
            {
                try
                {
                    Inner.Execute(context);
                }
                catch(Exception)
                {
                }
            }

            public ICommand Inner { get; private set; }
        }
    }
}