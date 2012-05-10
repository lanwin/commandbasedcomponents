using System.Threading.Tasks;

namespace CommandBasedComponents.Infrastructure
{
    public interface IWindowsServiceFacade
    {
        bool IsStarted(string name);
        bool IsStopped(string name);
        bool Exists(string name);
        Task<bool> Kill(string name);
        Task<bool> Start(string name);
        Task<bool> Stop(string name);
    }
}