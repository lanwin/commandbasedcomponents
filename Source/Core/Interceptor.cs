using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CommandBasedComponents.Core
{
    [DebuggerNonUserCode]
    public static class Interceptor
    {
        static readonly Dictionary<Type, TypeHandler> Handlers = new Dictionary<Type, TypeHandler>();

        public static void RunBefore<TCommandType>(ICommand command)
        {
            GetOrAddHandler(typeof(TCommandType)).Before.Add(command);
        }

        public static void RunAfter<TCommandType>(ICommand command)
        {
            GetOrAddHandler(typeof(TCommandType)).After.Add(command);
        }

        static TypeHandler GetOrAddHandler(Type commandType)
        {
            TypeHandler handler;
            if(Handlers.TryGetValue(commandType, out handler) == false)
                Handlers.Add(commandType,
                    handler = new TypeHandler
                    {
                        CommandType = commandType
                    });
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
                foreach(var interceptingCommand in handler.Before)
                    interceptingCommand.Execute(context);

            command.Inner.Execute(context);

            if(handler != null)
                foreach(var interceptingCommand in handler.After)
                    interceptingCommand.Execute(context);
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
            public readonly List<ICommand> After = new List<ICommand>();
            public readonly List<ICommand> Before = new List<ICommand>();
            public Type CommandType { get; set; }
        }
    }
}