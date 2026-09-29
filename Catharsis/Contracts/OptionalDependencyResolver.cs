using System.Reflection;

namespace Catharsis.Contracts;

///<summary>
///Creates an instance by calling its widest public constructor, resolving each parameter from an
///<see cref="IServiceProvider"/> and honouring <see cref="OptionalDependencyAttribute"/> for parameters the container
///cannot supply.
///</summary>
public static class OptionalDependencyResolver
{
    #region Public methods
    ///<summary>
    ///Determines whether any public constructor parameter of <paramref name="type"/> carries
    ///<see cref="OptionalDependencyAttribute"/>.
    ///</summary>
    ///<param name="type">The type to inspect.</param>
    ///<exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
    public static bool HasOptionalDependencies(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type.GetConstructors().Any(static ctor => ctor.GetParameters().Any(static p => p.IsDefined(typeof(OptionalDependencyAttribute), inherit: false)));
    }

    ///<summary>
    ///Creates an instance of <paramref name="type"/>.
    ///</summary>
    ///<param name="services">Supplies the constructor arguments.</param>
    ///<param name="type">The concrete type to create.</param>
    ///<returns>The new instance.</returns>
    ///<exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException">
    ///<paramref name="type"/> has no public constructor, or a required parameter cannot be resolved.
    ///</exception>
    public static object Create(IServiceProvider services, Type type)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(type);

        ConstructorInfo? constructor = type.GetConstructors().OrderByDescending(static c => c.GetParameters().Length).FirstOrDefault()
            ?? throw new InvalidOperationException($"{type.FullName} has no public constructor.");

        ParameterInfo[] parameters = constructor.GetParameters();
        object?[] arguments = new object?[parameters.Length];

        for(int i = 0; i < parameters.Length; i++)
        {
            ParameterInfo parameter = parameters[i];
            object? value = services.GetService(parameter.ParameterType);

            if(value is null)
            {
                if(parameter.IsDefined(typeof(OptionalDependencyAttribute), inherit: false) || parameter.HasDefaultValue)
                {
                    value = parameter.HasDefaultValue ? parameter.DefaultValue : DefaultOf(parameter.ParameterType);
                }
                else
                {
                    throw new InvalidOperationException($"Cannot resolve required parameter '{parameter.Name}' ({parameter.ParameterType.Name}) of {type.FullName}.");
                }
            }

            arguments[i] = value;
        }

        return constructor.Invoke(arguments);
    }
    #endregion

    #region Private methods
    static object? DefaultOf(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;
    #endregion
}
