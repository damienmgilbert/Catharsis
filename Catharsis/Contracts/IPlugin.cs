namespace Catharsis.Contracts;

///<summary>
///The contract a plugin implements so a host can start it uniformly. Discover implementations with ///<see
///cref="PluginLoader.Discover{TContract}(System.Reflection.Assembly[])"/> and start them with ///<see
///cref="PluginLoader.InitializeAllAsync"/>.
///</summary>
public interface IPlugin
{
    #region Public methods

    ///<summary>
    ///Called once after the plugin is created, before it is used.
    ///</summary>
    ///<param name="services">The host's services, for resolving whatever the plugin needs.</param>
    ///<param name="cancellationToken">A token that can abandon initialization.</param>
    ///<returns>A task that completes when the plugin is ready.</returns>
    Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default);
    #endregion
}
