using System.Collections;

namespace Catharsis.Generics;

///<summary>
///A dictionary keyed by every member of an enum, stored as a dense array so lookups are an index calculation rather
///than a hash. Every member always has a slot (initially <c>default</c>), so a missing key is impossible.
///</summary>
///<typeparam name="TEnum">The enum type.</typeparam>
///<typeparam name="TValue">The value stored for each member.</typeparam>
public sealed class EnumMap<TEnum, TValue> : IEnumerable<KeyValuePair<TEnum, TValue>>
    where TEnum : struct, Enum
{
    #region Fields
    static readonly TEnum[] Members = [.. Enum.GetValues<TEnum>().Distinct().Order()];
    readonly TValue[] _values = new TValue[Members.Length];
    #endregion

    #region Constructors
    ///<summary>Creates a map where every member holds <c>default</c>.</summary>
    public EnumMap() { }

    ///<summary>Creates a map where every member is initialized by <paramref name="initializer"/>.</summary>
    ///<param name="initializer">Produces the initial value for each member.</param>
    ///<exception cref="ArgumentNullException"><paramref name="initializer"/> is <c>null</c>.</exception>
    public EnumMap(Func<TEnum, TValue> initializer)
    {
        ArgumentNullException.ThrowIfNull(initializer);

        for(int i = 0; i < Members.Length; i++)
        {
            _values[i] = initializer(Members[i]);
        }
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public IEnumerator<KeyValuePair<TEnum, TValue>> GetEnumerator()
    {
        for(int i = 0; i < Members.Length; i++)
        {
            yield return new KeyValuePair<TEnum, TValue>(Members[i], _values[i]);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    #region Public properties
    ///<summary>Gets the number of distinct members, which is the number of slots.</summary>
    public int Count => Members.Length;

    ///<summary>Gets or sets the value for <paramref name="key"/>.</summary>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="key"/> is not a defined member.</exception>
    public TValue this[TEnum key]
    {
        get => _values[IndexOf(key)];
        set => _values[IndexOf(key)] = value;
    }
    #endregion

    #region Private methods
    static int IndexOf(TEnum key)
    {
        int index = Array.BinarySearch(Members, key);

        return index >= 0 ? index : throw new ArgumentOutOfRangeException(nameof(key), key, "Not a defined member of the enum.");
    }
    #endregion
}
