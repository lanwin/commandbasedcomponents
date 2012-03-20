namespace CommandBasedComponents.Core
{
    public interface ICommand
    {
        void Execute(IContext context);
    }
}