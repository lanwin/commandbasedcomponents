using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CommandBasedComponents.Core
{
    [DebuggerNonUserCode]
    public static class Interceptor
    {
        static readonly Dictionary<Type, TypeHandler> Handlers = new Dictionary<Type, TypeHandler>();

        public static void RunBefore<TCommandType>(Action<IContext, ICommand> command)
        {
            GetOrAddHandler(typeof(TCommandType)).Before.Add(command);
        }

        public static void RunAfter<TCommandType>(Action<IContext, ICommand> command)
        {
            GetOrAddHandler(typeof(TCommandType)).After.Add(command);
        }

        static TypeHandler GetOrAddHandler(Type commandType)
        {
            TypeHandler handler;
            if(Handlers.TryGetValue(commandType, out handler) == false)
            {
                Handlers.Add(commandType,
                             handler = new TypeHandler
                             {
                                 CommandType = commandType
                             });
            }
            return handler;
        }

        public static ICommand Intercept(ICommand command)
        {
            return new IntercepterCommand {Inner = command};
        }

        static void Execute(IDecoratorCommand command, IContext context)
        {
            var decorated = command.GetDecorated();

            TypeHandler handler;

            Handlers.TryGetValue(decorated.GetType(), out handler);

            if(handler != null)
            {
                foreach(var action in handler.Before)
                {
                    action(context, decorated);
                }
            }

            command.Inner.Execute(context);

            if(handler != null)
            {
                foreach(var action in handler.After)
                {
                    action(context, decorated);
                }
            }
        }

        [DebuggerNonUserCode]
        class IntercepterCommand : IDecoratorCommand
        {
            public void Execute(IContext context)
            {
                Interceptor.Execute(this, context);
            }

            public ICommand Inner { get; set; }
        }

        class TypeHandler
        {
            public readonly List<Action<IContext, ICommand>> After = new List<Action<IContext, ICommand>>();
            public readonly List<Action<IContext, ICommand>> Before = new List<Action<IContext, ICommand>>();
            public Type CommandType { get; set; }
        }
    }
}