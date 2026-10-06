using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Catharsis.Contracts;

///<summary>
///<see cref="IServiceCollection"/> extensions for attribute-driven registration, decoration and factory delegates.
///</summary>
public static class ContractServiceCollectionExtensions
{
    #region Public methods
    ///<summary>
    ///Registers every <see cref="ServiceAttribute"/> class in <paramref name="assemblies"/>, then wraps services with
    ///every <see cref="DecoratorForAttribute"/> class in ascending <see cref="DecoratorForAttribute.Order"/>.
    ///</summary>
    ///<param name="services">The service collection.</param>
    ///<param name="assemblies">The assemblies to scan.</param>
    ///<returns>The service collection for chaining.</returns>
    ///<exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException">A decorator targets a service that has no registration.</exception>
    public static IServiceCollection AddAttributedServices(this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        return Register(
            services,
            assemblies.SelectMany(static a => AttributeServiceScanner.ScanServices(a)),
            assemblies.SelectMany(static a => AttributeServiceScanner.ScanDecorators(a)));
    }

    ///<summary>
    ///Registers every <see cref="ServiceAttribute"/> class among <paramref name="types"/>, then decorates services with
    ///every <see cref="DecoratorForAttribute"/> class among them. Services with an
    ///<see cref="OptionalDependencyAttribute"/> constructor parameter are created through
    ///<see cref="OptionalDependencyResolver"/>.
    ///</summary>
    ///<param name="services">The service collection.</param>
    ///<param name="types">The candidate types.</param>
    ///<returns>The service collection for chaining.</returns>
    ///<exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException">A decorator targets a service that has no registration.</exception>
    public static IServiceCollection AddAttributedServices(this IServiceCollection services, IEnumerable<Type> types)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(types);

        Type[] candidates = [.. types];

        return Register(services, AttributeServiceScanner.ScanServices(candidates), AttributeServiceScanner.ScanDecorators(candidates));
    }

    ///<summary>
    ///Wraps the most recent registration of <typeparamref name="TService"/> with <typeparamref name="TDecorator"/>.
    ///</summary>
    ///<typeparam name="TService">The service to decorate.</typeparam>
    ///<typeparam name="TDecorator">The decorator, whose constructor accepts a <typeparamref name="TService"/>.</typeparam>
    ///<param name="services">The service collection.</param>
    ///<returns>The service collection for chaining.</returns>
    ///<inheritdoc cref="AddDecorator(IServiceCollection, Type, Type)"/>
    public static IServiceCollection AddDecorator<TService, TDecorator>(this IServiceCollection services)
        where TService : class
        where TDecorator : class, TService
    {
        return services.AddDecorator(typeof(TService), typeof(TDecorator));
    }

    ///<summary>
    ///Wraps the most recent registration of <paramref name="serviceType"/> with <paramref name="decoratorType"/>. The
    ///decorated registration keeps its position and lifetime; the decorator receives the original implementation
    ///through its constructor, with any other constructor arguments resolved from the container.
    ///</summary>
    ///<remarks>
    ///The container only tracks the outermost object for disposal, so an inner implementation that needs disposing must
    ///be disposed by the decorator.
    ///</remarks>
    ///<param name="services">The service collection.</param>
    ///<param name="serviceType">The service to decorate.</param>
    ///<param name="decoratorType">The decorator class.</param>
    ///<returns>The service collection for chaining.</returns>
    ///<exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="decoratorType"/> is abstract or does not implement <paramref name="serviceType"/>.</exception>
    ///<exception cref="InvalidOperationException">There is no registration of <paramref name="serviceType"/>, or it is keyed.</exception>
    public static IServiceCollection AddDecorator(this IServiceCollection services, Type serviceType, Type decoratorType)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(decoratorType);

        if(decoratorType.IsAbstract || !serviceType.IsAssignableFrom(decoratorType))
        {
            throw new ArgumentException($"{decoratorType.FullName} must be a concrete implementation of {serviceType.FullName}.", nameof(decoratorType));
        }

        int index = -1;

        for(int i = services.Count - 1; i >= 0; i--)
        {
            if(services[i].ServiceType == serviceType)
            {
                index = i;
                break;
            }
        }

        if(index < 0)
        {
            throw new InvalidOperationException($"No registration of {serviceType.FullName} exists to decorate.");
        }

        ServiceDescriptor original = services[index];

        if(original.IsKeyedService)
        {
            throw new InvalidOperationException("Keyed services cannot be decorated.");
        }

        services[index] = new ServiceDescriptor(
            serviceType,
            provider => ActivatorUtilities.CreateInstance(provider, decoratorType, CreateInner(provider, original)),
            original.Lifetime);

        return services;
    }

    ///<summary>
    ///Registers a <see cref="Func{T}"/> that resolves a new <typeparamref name="T"/> from the container on each call.
    ///This lets a class create instances on demand without taking a dependency on <see cref="IServiceProvider"/>.
    ///</summary>
    ///<typeparam name="T">The service the factory resolves.</typeparam>
    ///<param name="services">The service collection.</param>
    ///<returns>The service collection for chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddFactory<T>(this IServiceCollection services)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<Func<T>>(static provider => () => provider.GetRequiredService<T>());
        return services;
    }
    #endregion

    #region Private methods
    static IServiceCollection Register(IServiceCollection services, IEnumerable<ServiceRegistration> registrations, IEnumerable<DecoratorRegistration> decorators)
    {
        foreach(ServiceRegistration registration in registrations)
        {
            Type implementation = registration.ImplementationType;

            services.Add(OptionalDependencyResolver.HasOptionalDependencies(implementation)
                ? new ServiceDescriptor(registration.ServiceType, provider => OptionalDependencyResolver.Create(provider, implementation), registration.Lifetime)
                : new ServiceDescriptor(registration.ServiceType, implementation, registration.Lifetime));
        }

        foreach(DecoratorRegistration decorator in decorators.OrderBy(static d => d.Order))
        {
            services.AddDecorator(decorator.ServiceType, decorator.DecoratorType);
        }

        return services;
    }

    static object CreateInner(IServiceProvider provider, ServiceDescriptor original)
    {
        if(original.ImplementationInstance is not null)
        {
            return original.ImplementationInstance;
        }

        if(original.ImplementationFactory is not null)
        {
            return original.ImplementationFactory(provider);
        }

        return ActivatorUtilities.CreateInstance(provider, original.ImplementationType!);
    }
    #endregion
}
