using CommandBasedComponents.Core;
using CommandBasedComponents.Testing;

namespace CommandBasedComponents.Infrastructure
{
    public interface IWindowsServiceLocator
    {
        [NotNull]
        FakeWindowsServiceFacade Find(string name);

        FakeWindowsServiceFacade TryFind(string name);
    }
}