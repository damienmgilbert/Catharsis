namespace Catharsis.Common;

///<summary>
///Caches an enum type's values and names so callers avoid the repeated allocations that
///<see cref="Enum.GetValues{TEnum}()"/> and <see cref="Enum.GetNames{TEnum}()"/> incur on every call.
///</summary>
///<typeparam name="TEnum">The enum type to cache.</typeparam>
public static class EnumCache<TEnum> where TEnum : struct, Enum
{
    #region Fields
    static readonly Dictionary<string, TEnum> _valuesByName = BuildValuesByName();
    static readonly string[] _names = Enum.GetNames<TEnum>();
    static readonly TEnum[] _values = Enum.GetValues<TEnum>();
    #endregion

    #region Private methods
    static Dictionary<string, TEnum> BuildValuesByName()
    {
        Dictionary<string, TEnum> map = new(StringComparer.Ordinal);

        foreach(TEnum value in Enum.GetValues<TEnum>())
        {
            map[value.ToString()] = value;
        }

        return map;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Attempts to parse a member name into its <typeparamref name="TEnum"/> value using the cached lookup table
    ///instead of reflection-based parsing.
    ///</summary>
    ///<param name="name">The member name to parse.</param>
    ///<param name="value">The parsed value, if found.</param>
    ///<returns><c>true</c> if <paramref name="name"/> names a defined member; otherwise <c>false</c>.</returns>
    public static bool TryParse(string name, out TEnum value)
    {
        ArgumentNullException.ThrowIfNull(name);
        return _valuesByName.TryGetValue(name, out value);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the cached result of <see cref="Enum.GetNames{TEnum}()"/>.
    ///</summary>
    public static IReadOnlyList<string> Names => _names;

    ///<summary>
    ///Gets the cached result of <see cref="Enum.GetValues{TEnum}()"/>.
    ///</summary>
    public static IReadOnlyList<TEnum> Values => _values;
    #endregion
}
