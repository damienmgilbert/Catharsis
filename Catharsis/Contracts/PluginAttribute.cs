namespace Catharsis.Contracts;

///<summary>
///Declares a class as a named plugin that <see cref="PluginLoader"/> can discover and instantiate.
///</summary>
///<param name="name">The unique plugin name.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PluginAttribute(string name) : Attribute
{
    #region Public properties
    ///<summary>Gets the plugin name.</summary>
    public string Name { get; } = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Plugin name must not be empty.", nameof(name)) : name;

    ///<summary>Gets or sets a free-form version string for the plugin.</summary>
    public string Version { get; set; } = "1.0";
    #endregion
}
