namespace Catharsis.Configuration;

///<summary>
///Resolves a setting's value by checking a fixed, precedence-ordered list of layers (e.g. environment variables,
///then a config file, then hard-coded defaults) and returning the first layer that defines the key. This is
///deliberately source-agnostic: callers supply each layer as a plain dictionary, so no config-file parser or other
///dependency is required.
///</summary>
///<param name="layersHighestPrecedenceFirst">The setting layers, in precedence order (the first layer wins).</param>
///<exception cref="ArgumentNullException"><paramref name="layersHighestPrecedenceFirst"/> is <c>null</c>.</exception>
public sealed class LayeredSettingsResolver(params IReadOnlyDictionary<string, string?>[] layersHighestPrecedenceFirst)
{
    #region Fields
    readonly IReadOnlyList<IReadOnlyDictionary<string, string?>> _layers = layersHighestPrecedenceFirst ?? throw new ArgumentNullException(nameof(layersHighestPrecedenceFirst));
    #endregion

    #region Public methods
    ///<summary>
    ///Creates a two-layer resolver: the current process's environment variables take precedence over the supplied
    ///defaults.
    ///</summary>
    ///<param name="defaults">The fallback values used when a key is not set as an environment variable.</param>
    ///<returns>A configured <see cref="LayeredSettingsResolver"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="defaults"/> is <c>null</c>.</exception>
    public static LayeredSettingsResolver FromEnvironmentAndDefaults(IReadOnlyDictionary<string, string?> defaults)
    {
        ArgumentNullException.ThrowIfNull(defaults);

        Dictionary<string, string?> environment = [];

        foreach(System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
        {
            environment[(string)entry.Key] = (string?)entry.Value;
        }

        return new LayeredSettingsResolver(environment, defaults);
    }

    ///<summary>
    ///Resolves a setting's value, or <paramref name="defaultValue"/> if no layer defines it.
    ///</summary>
    ///<param name="key">The setting key.</param>
    ///<param name="defaultValue">The value to return if no layer defines <paramref name="key"/>.</param>
    ///<returns>The resolved value, or <paramref name="defaultValue"/>.</returns>
    ///<exception cref="ArgumentException"><paramref name="key"/> is <c>null</c>, empty, or whitespace.</exception>
    public string? GetValue(string key, string? defaultValue = null) => TryGetValue(key, out string? value) ? value : defaultValue;

    ///<summary>
    ///Attempts to resolve a setting's value from the highest-precedence layer that defines it.
    ///</summary>
    ///<param name="key">The setting key.</param>
    ///<param name="value">The resolved value, if any layer defines <paramref name="key"/>.</param>
    ///<returns><c>true</c> if some layer defines <paramref name="key"/>; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentException"><paramref name="key"/> is <c>null</c>, empty, or whitespace.</exception>
    public bool TryGetValue(string key, out string? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        foreach(IReadOnlyDictionary<string, string?> layer in _layers)
        {
            if(layer.TryGetValue(key, out string? found))
            {
                value = found;
                return true;
            }
        }

        value = null;
        return false;
    }
    #endregion
}
