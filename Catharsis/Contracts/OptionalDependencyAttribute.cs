namespace Catharsis.Contracts;

///<summary>
///Marks a constructor parameter as optional for <see cref="OptionalDependencyResolver"/>: when the container has no
///registration for the parameter's type, the parameter receives its default value (<c>null</c> for reference types)
///instead of the resolution failing. This works for parameters that have no C# default, such as
///<c>ILogger? logger</c>. Services that carry the attribute are registered through the resolver automatically by
///<see cref="ContractServiceCollectionExtensions.AddAttributedServices(Microsoft.Extensions.DependencyInjection.IServiceCollection, System.Reflection.Assembly[])"/>.
///</summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class OptionalDependencyAttribute : Attribute
{
}
