using System;
using System.Collections;
using System.Collections.Generic;

namespace CommandBasedComponents.Core
{
    public abstract class ChainCommand : DecoratedCommandBase, IEnumerable<ICommand>, ICommand
    {
        readonly List<ICommand> _chain = new List<ICommand>();

        protected ChainCommand(Func<ICommand, ICommand> decoratorCommand)
            : base(decoratorCommand)
        {
        }

        public virtual void Execute(IContext context)
        {
            if(context == null)
            {
                throw new ArgumentNullException("context");
            }

            using(var scope = new Context(context))
            {
                foreach(var command in _chain)
                {
                    ExecuteDecorated(scope, command);
                }
            }
        }

        public IEnumerator<ICommand> GetEnumerator()
        {
            return _chain.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(ICommand command)
        {
            _chain.Add(command);
        }
    }
}