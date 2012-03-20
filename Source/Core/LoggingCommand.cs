using System;
using System.Collections;
using System.Diagnostics;

namespace CommandBasedComponents.Core
{
    [DebuggerNonUserCode]
    public class LoggingCommand : IDecoratorCommand
    {
        static int _indent;

        public LoggingCommand(ICommand inner)
        {
            Inner = inner;
        }

        public void Execute(IContext context)
        {
            var decorated = this.GetDecorated();
            var name = decorated.GetType().Name;

            Console.WriteLine("{1}> {0}", name, new string(' ', _indent*2));

            if(decorated is IEnumerable)
            {
                _indent++;
            }

            Inner.Execute(context);

            if(decorated is IEnumerable)
            {
                _indent--;
            }
        }

        public ICommand Inner { get; private set; }
    }
}