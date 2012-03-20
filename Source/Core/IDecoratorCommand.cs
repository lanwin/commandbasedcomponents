namespace CommandBasedComponents.Core
{
    public interface IDecoratorCommand : ICommand
    {
        ICommand Inner { get; }
    }
}