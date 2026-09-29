namespace Catharsis.Contracts;

///<summary>
///A plugin found by <see cref="PluginLoader"/>.
///</summary>
///<param name="Name">The name from <see cref="PluginAttribute.Name"/>.</param>
///<param name="Version">The version from <see cref="PluginAttribute.Version"/>.</param>
///<param name="ImplementationType">The concrete plugin class.</param>
public sealed record PluginDescriptor(string Name, string Version, Type ImplementationType);
