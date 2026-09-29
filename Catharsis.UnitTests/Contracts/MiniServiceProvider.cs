using Microsoft.Extensions.DependencyInjection;

namespace Catharsis.UnitTests.Contracts;

///<summary>
///A minimal <see cref="IServiceProvider"/> over a service collection, used so tests can resolve registrations without
///referencing the full DI container package. Scoped services behave as singletons.
///</summary>
internal sealed class MiniServiceProvider(IServiceCollection services) : IServiceProvider
{
    readonly Dictionary<ServiceDescriptor, object> _singletons = [];

    public object? GetService(Type serviceType)
    {
        if(serviceType == typeof(IServiceProvider))
        {
            return this;
        }

        ServiceDescriptor? descriptor = services.LastOrDefault(d => d.ServiceType == serviceType);

        if(descriptor is null)
        {
            return null;
        }

        if(descriptor.Lifetime == ServiceLifetime.Transient)
        {
            return Create(descriptor);
        }

        if(!_singletons.TryGetValue(descriptor, out object? instance))
        {
            instance = Create(descriptor);
            _singletons[descriptor] = instance;
        }

        return instance;
    }

    object Create(ServiceDescriptor descriptor)
    {
        if(descriptor.ImplementationInstance is not null)
        {
            return descriptor.ImplementationInstance;
        }

        if(descriptor.ImplementationFactory is not null)
        {
            return descriptor.ImplementationFactory(this);
        }

        return ActivatorUtilities.CreateInstance(this, descriptor.ImplementationType!);
    }
}
