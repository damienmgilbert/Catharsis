using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Catharsis.Contracts;

///<summary>
///Discovers classes marked with <see cref="PluginAttribute"/> that implement a contract interface (or derive from a
///contract base class), and creates them on demand.
///</summary>
public static class PluginLoader
{
    #region Public methods
    ///<summary>
    ///Finds every plugin in <paramref name="assemblies"/> that is assignable to <typeparamref name="TContract"/>.
    ///</summary>
    ///<typeparam name="TContract">The contract each plugin must satisfy.</typeparam>
    ///<param name="assemblies">The assemblies to search.</param>
    ///<returns>The plugins, ordered by name.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="assemblies"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException">Two plugins share the same name.</exception>
    public static IReadOnlyList<PluginDescriptor> Discover<TContract>(params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        return Discover<TContract>(assemblies.SelectMany(static a => SafeGetTypes(a)));
    }

    ///<summary>
    ///Finds every plugin among <paramref name="types"/> that is assignable to <typeparamref name="TContract"/>.
    ///</summary>
    ///<typeparam name="TContract">The contract each plugin must satisfy.</typeparam>
    ///<param name="types">The candidate types.</param>
    ///<returns>The plugins, ordered by name.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="types"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException">Two plugins share the same name.</exception>
    public static IReadOnlyList<PluginDescriptor> Discover<TContract>(IEnumerable<Type> types)
    {
        ArgumentNullException.ThrowIfNull(types);

        Dictionary<string, PluginDescriptor> byName = new(StringComparer.OrdinalIgnoreCase);

        foreach(Type type in types)
        {
            if(type is not { IsClass: true, IsAbstract: false } || !typeof(TContract).IsAssignableFrom(type))
            {
                continue;
            }

            PluginAttribute? attribute = type.GetCustomAttribute<PluginAttribute>(inherit: false);

            if(attribute is null)
            {
                continue;
            }

            if(!byName.TryAdd(attribute.Name, new PluginDescriptor(attribute.Name, attribute.Version, type)))
            {
                throw new InvalidOperationException($"Plugin name '{attribute.Name}' is declared by both {byName[attribute.Name].ImplementationType.FullName} and {type.FullName}.");
            }
        }

        return [.. byName.Values.OrderBy(static d => d.Name, StringComparer.OrdinalIgnoreCase)];
    }

    ///<summary>
    ///Creates an instance of a discovered plugin.
    ///</summary>
    ///<typeparam name="TContract">The contract type to return.</typeparam>
    ///<param name="descriptor">The plugin to create.</param>
    ///<param name="services">
    ///An optional container used to supply constructor arguments. When <c>null</c> the plugin needs a public
    ///parameterless constructor.
    ///</param>
    ///<returns>The new plugin instance.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="descriptor"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidCastException">The plugin does not implement <typeparamref name="TContract"/>.</exception>
    public static TContract Create<TContract>(PluginDescriptor descriptor, IServiceProvider? services = null)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        object instance = services is null
            ? Activator.CreateInstance(descriptor.ImplementationType)!
            : ActivatorUtilities.CreateInstance(services, descriptor.ImplementationType);

        return (TContract)instance;
    }
    #endregion

    ///<summary>
    ///Creates every plugin in <paramref name="descriptors"/> as an <see cref="IPlugin"/> and initializes each in turn.
    ///</summary>
    ///<param name="descriptors">The plugins to start, typically from <c>Discover&lt;IPlugin&gt;</c>.</param>
    ///<param name="services">Supplies constructor arguments and is passed to each plugin's initializer.</param>
    ///<param name="cancellationToken">A token that can abandon startup between plugins.</param>
    ///<returns>The started plugins, in the order of <paramref name="descriptors"/>.</returns>
    ///<exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    ///<exception cref="InvalidCastException">A descriptor's type does not implement <see cref="IPlugin"/>.</exception>
    public static async Task<IReadOnlyList<IPlugin>> InitializeAllAsync(IEnumerable<PluginDescriptor> descriptors, IServiceProvider services, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(descriptors);
        ArgumentNullException.ThrowIfNull(services);

        List<IPlugin> started = [];

        foreach(PluginDescriptor descriptor in descriptors)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IPlugin plugin = Create<IPlugin>(descriptor, services);
            await plugin.InitializeAsync(services, cancellationToken).ConfigureAwait(false);
            started.Add(plugin);
        }

        return started;
    }

    ///<summary>
    ///Starts plugins like <see cref="InitializeAllAsync"/>, returning a <see cref="ValueTask{TResult}"/>.
    ///</summary>
    ///<param name="descriptors">The plugins to start.</param>
    ///<param name="services">Supplies constructor arguments and is passed to each plugin's initializer.</param>
    ///<param name="cancellationToken">A token that can abandon startup between plugins.</param>
    ///<returns>The started plugins, in the order of <paramref name="descriptors"/>.</returns>
    ///<exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    public static ValueTask<IReadOnlyList<IPlugin>> InitializeAllValueAsync(IEnumerable<PluginDescriptor> descriptors, IServiceProvider services, CancellationToken cancellationToken = default)
    {
        return new ValueTask<IReadOnlyList<IPlugin>>(InitializeAllAsync(descriptors, services, cancellationToken));
    }

    #region Private methods
    static Type[] SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch(ReflectionTypeLoadException ex)
        {
            return [.. ex.Types.OfType<Type>()];
        }
    }
    #endregion
}
