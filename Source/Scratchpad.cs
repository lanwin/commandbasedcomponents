using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CommandBasedComponents.Core;
using CommandBasedComponents.Infrastructure;
using CommandBasedComponents.Testing;

namespace CommandBasedComponents
{
    // model

    public class NugetPackage
    {
        public string Id { get; set; }
        public Version Version { get; set; }
    }

    public enum PackageType
    {
        Service,
        App,
        Web
    }

    public class HammerPackage
    {
        public string Id { get; set; }
        public Version Version { get; set; }
        public string ServiceName { get; set; }
        public PackageType Type { get; set; }
        public string ExecutablePath { get; set; }

        public static PackageType TypeFromId(string id)
        {
            return PackageType.Service;
        }

        public bool Equals(NugetPackage package)
        {
            return Id.Equals(package.Id) && Version.CompareTo(package.Version) != 0;
        }
    }

    public class ServiceDescription
    {
        public ServiceDescription(HammerPackage package)
        {
            ServiceName = package.ServiceName;
            ExecutablePath = package.ExecutablePath;

            if(string.IsNullOrWhiteSpace(ServiceName))
                ServiceName = package.Id;
        }

        public string ExecutablePath { get; set; }
        public string ServiceName { get; set; }
    }

    // external service facades

    public class WindowsServiceInstaller
    {
        public void Install(ServiceDescription service)
        {
            //
        }

        public void Uninstall(ServiceDescription service)
        {
            //
        }
    }

    public class NugetLocator
    {
        public HammerPackage FindLocalPackage(string id)
        {
            return new HammerPackage
            {
                Id = id,
                Type = PackageType.Service,
                Version = Version.Parse("1.0")
            };
        }

        public NugetPackage FindRemotePackage(string id)
        {
            return new NugetPackage
            {
                Id = id,
                Version = Version.Parse("2.0")
            };
        }
    }

    public class NugetInstaller
    {
        public void Uninstall(HammerPackage package)
        {
        }

        public void Install(string id)
        {
        }
    }

    // keys

    public static class PackageKeys
    {
        public static readonly ContextKey<string> Id = new ContextKey<string>();
        public static readonly ContextKey<HammerPackage> LocalPackage = new ContextKey<HammerPackage>();
        public static readonly ContextKey<NugetPackage> RemotePackage = new ContextKey<NugetPackage>();
        public static readonly ContextKey<string[]> RequiredPackageIds = new ContextKey<string[]>();
        public static readonly ContextKey<ServiceDescription> ServiceDescription = new ContextKey<ServiceDescription>();
    }

    public static class ServiceKeys
    {
        public static readonly ContextKey<NugetInstaller> NugetInstaller = new ContextKey<NugetInstaller>();
        public static readonly ContextKey<NugetLocator> NugetLocator = new ContextKey<NugetLocator>();
        public static readonly ContextKey<IWindowsServiceFacade> WindowsServiceFacade = new ContextKey<IWindowsServiceFacade>();
        public static readonly ContextKey<WindowsServiceInstaller> WindowsServiceInstaller = new ContextKey<WindowsServiceInstaller>();
    }

    // commands

    public class GetLocalPackageFromId : ICommand
    {
        public void Execute(IContext context)
        {
            var id = context.Get(PackageKeys.Id);
            var locator = context.Get(ServiceKeys.NugetLocator);

            var package = locator.FindLocalPackage(id);
            if(package == null)
                return;

            context.Put(PackageKeys.LocalPackage, package, CleanupStrategy.Scope);
        }
    }

    public class GetRemotePackageFromId : ICommand
    {
        public void Execute(IContext context)
        {
            var id = context.Get(PackageKeys.Id);
            var locator = context.Get(ServiceKeys.NugetLocator);

            var package = locator.FindRemotePackage(id);
            if(package == null)
                return;

            context.Put(PackageKeys.RemotePackage, package, CleanupStrategy.Scope);
        }
    }

    public class GetServiceDescriptionFromPackage : ICommand
    {
        public void Execute(IContext context)
        {
            var package = context.Get(PackageKeys.LocalPackage);

            var description = new ServiceDescription(package);

            context.Put(PackageKeys.ServiceDescription, description, CleanupStrategy.Scope);
        }
    }

    public class UninstallNugetPackage : ICommand
    {
        public void Execute(IContext context)
        {
            var package = context.Get(PackageKeys.LocalPackage);
            var installer = context.Get(ServiceKeys.NugetInstaller);

            installer.Uninstall(package);
        }
    }

    public class InstallNugetPackage : ICommand
    {
        public void Execute(IContext context)
        {
            var id = context.Get(PackageKeys.Id);
            var installer = context.Get(ServiceKeys.NugetInstaller);

            installer.Install(id);
        }
    }

    public class StopService : ICommand
    {
        public void Execute(IContext context)
        {
            var service = context.Get(PackageKeys.ServiceDescription);
            var serviceFacade = context.Get(ServiceKeys.WindowsServiceFacade);

            if(!serviceFacade.Exists(service.ServiceName))
                return;

            serviceFacade.Stop(service.ServiceName)
                .Wait(30);

            if(serviceFacade.IsStopped(service.ServiceName))
                return;

            serviceFacade.Kill(service.ServiceName)
                .Wait(30);
        }
    }

    public class RemoveService : ICommand
    {
        public void Execute(IContext context)
        {
            var service = context.Get(PackageKeys.ServiceDescription);
            var serviceFacade = context.Get(ServiceKeys.WindowsServiceFacade);
            var installer = context.Get(ServiceKeys.WindowsServiceInstaller);

            if(!serviceFacade.Exists(service.ServiceName))
                return;

            if(!serviceFacade.IsStopped(service.ServiceName))
                throw new InvalidOperationException("Service " + service.ServiceName + " need to be stopped to remove it");

            installer.Uninstall(service);
        }
    }

    public class UninstallServicePackage : ChainCommand
    {
        public UninstallServicePackage()
            : base(Decorators.All)
        {
            Add(new GetLocalPackageFromId());
            Add(new GetServiceDescriptionFromPackage());
            Add(new StopService());
            Add(new RemoveService());
            Add(new UninstallNugetPackage().IgnoreExceptions());
        }
    }

    public class InstallSerivce : ICommand
    {
        public void Execute(IContext context)
        {
            var service = context.Get(PackageKeys.ServiceDescription);
            var serviceFacade = context.Get(ServiceKeys.WindowsServiceFacade);
            var installer = context.Get(ServiceKeys.WindowsServiceInstaller);

            if(serviceFacade.Exists(service.ServiceName))
                return;

            installer.Install(service);
        }
    }

    public class StartService : ICommand
    {
        public void Execute(IContext context)
        {
            var service = context.Get(PackageKeys.ServiceDescription);
            var serviceFacade = context.Get(ServiceKeys.WindowsServiceFacade);

            if(!serviceFacade.Exists(service.ServiceName))
                return;

            if(serviceFacade.IsStarted(service.ServiceName))
                return;

            serviceFacade.Stop(service.ServiceName);
        }
    }

    public class InstallServicePackage : ChainCommand
    {
        public InstallServicePackage()
            : base(Decorators.All)
        {
            Add(new GetLocalPackageFromId());
            Add(new GetRemotePackageFromId());
            Add(new UninstallServicePackage().If(IsDifferentVersion));
            Add(new InstallNugetPackage());
            Add(new GetLocalPackageFromId()); // reget new one package
            Add(new GetServiceDescriptionFromPackage());
            Add(new InstallSerivce());
            Add(new StartService());
        }

        static bool IsDifferentVersion(IContext context)
        {
            var remotePackage = context.Get(PackageKeys.RemotePackage);
            return context.HasValue(PackageKeys.LocalPackage) == false ||
                context.Get(PackageKeys.LocalPackage).Equals(remotePackage);
        }
    }

    public class InstallAppPackage : ChainCommand
    {
        public InstallAppPackage()
            : base(Decorators.All)
        {
            //...
            Add(new InstallNugetPackage());
            //...
        }
    }

    public class InstallPackage : ChainCommand
    {
        public InstallPackage()
            : base(Decorators.All)
        {
            Add(new CommandRouter
            {
                new PackageTypeRoute(PackageType.Service, new InstallServicePackage()),
                new PackageTypeRoute(PackageType.App, new InstallAppPackage())
            });
        }
    }

    public class InitializeRequiredPackageIds : ICommand
    {
        public void Execute(IContext context)
        {
            if(context.HasValue(PackageKeys.RequiredPackageIds))
                return;
            context.Put(PackageKeys.RequiredPackageIds, new string[0]);
        }
    }

    public class IterateRequiredPackageIds : ChainCommand
    {
        public IterateRequiredPackageIds()
            : base(Decorators.All)
        {
        }

        public override void Execute(IContext context)
        {
            foreach(var packageId in context.Get(PackageKeys.RequiredPackageIds))
                using(var scope = new Context(context))
                {
                    scope.Put(PackageKeys.Id, packageId, CleanupStrategy.Scope);
                    base.Execute(scope);
                }
        }
    }

    public class InstallRequiredPackages : ChainCommand
    {
        public InstallRequiredPackages()
            : base(Decorators.All)
        {
            Add(new InitializeRequiredPackageIds());
            Add(new IterateRequiredPackageIds
            {
                new InstallPackage()
            });
        }
    }

    public class PackageTypeRoute : IRoute
    {
        public PackageTypeRoute(PackageType type, ICommand command)
        {
            Command = command;
            Condition = c => HammerPackage.TypeFromId(c.Get(PackageKeys.Id)) == type;
        }

        public ICommand Command { get; private set; }
        public Predicate<IContext> Condition { get; private set; }
    }

    // lib

    public interface IRoute
    {
        ICommand Command { get; }
        Predicate<IContext> Condition { get; }
    }

    public class CommandRouter : DecoratedCommandBase, IEnumerable, ICommand
    {
        readonly List<IRoute> _routes = new List<IRoute>();

        public CommandRouter(params IRoute[] routes)
            : base(Decorators.All)
        {
            _routes.AddRange(routes);
        }

        [DebuggerNonUserCode]
        public void Execute(IContext context)
        {
            foreach(var route in _routes.Where(route => route.Condition(context)))
            {
                ExecuteDecorated(context, route.Command);
                return;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return _routes.GetEnumerator();
        }

        public void Add(IRoute item)
        {
            _routes.Add(item);
        }
    }

    public static class ContextExtension
    {
        [DebuggerNonUserCode]
        public static void Run(this IContext context, ICommand command, Func<ICommand, ICommand> decoratorCommand = null)
        {
            decoratorCommand.Execute(command, context);
        }
    }

    public class Runner
    {
        public void Run()
        {
            // an plugin would run this
            Interceptor.RunAfter<InitializeRequiredPackageIds>((context, command) =>
            {
                var ids = new List<string>(context.Get(PackageKeys.RequiredPackageIds))
                {
                    "a1",
                    "a2"
                };
                context.Put(PackageKeys.RequiredPackageIds, ids.ToArray());
            });

            using(var context = Context.Empty)
            {
                context.Put(ServiceKeys.WindowsServiceFacade, new FakeWindowsServiceFacade());
                context.Put(ServiceKeys.WindowsServiceInstaller, new WindowsServiceInstaller());
                context.Put(ServiceKeys.NugetInstaller, new NugetInstaller());
                context.Put(ServiceKeys.NugetLocator, new NugetLocator());
                context.Run(new InstallRequiredPackages(), Decorators.All);
            }
        }
    }
}