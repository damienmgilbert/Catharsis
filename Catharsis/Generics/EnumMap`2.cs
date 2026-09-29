using Catharsis.Common;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Catharsis.Generics;

///<summary>
///A dictionary keyed by every member of an enum, stored as a dense array so lookups are an index calculation rather
///than a hash. When the enum's values are contiguous (the common case) a lookup is a subtraction and a bounds check;
///otherwise it is a binary search over the underlying integer values, which needs no boxing or enum comparer. The
///members come from <see cref="EnumCache{TEnum}"/>, so they are enumerated by reflection only once
///per enum type. Every member always has a slot (initially <c>default</c>), so a missing key is impossible.
///</summary>
///<typeparam name="TEnum">The enum type.</typeparam>
///<typeparam name="TValue">The value stored for each member.</typeparam>
public sealed class EnumMap<TEnum, TValue> : IEnumerable<KeyValuePair<TEnum, TValue>>
    where TEnum : struct, Enum
{
    #region Fields
    static readonly bool IsSigned = Enum.GetUnderlyingType(typeof(TEnum)) == typeof(sbyte)
        || Enum.GetUnderlyingType(typeof(TEnum)) == typeof(short)
        || Enum.GetUnderlyingType(typeof(TEnum)) == typeof(int)
        || Enum.GetUnderlyingType(typeof(TEnum)) == typeof(long);

    static readonly TEnum[] Members = [.. EnumCache<TEnum>.Values.Distinct().OrderBy(ToLong)];
    static readonly long[] Keys = [.. Members.Select(ToLong)];
    static readonly long MinKey = Keys.Length == 0 ? 0 : Keys[0];
    static readonly bool Dense = Keys.Length > 0 && Keys[^1] - Keys[0] == Keys.Length - 1;
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
        long value = ToLong(key);

        if(Dense)
        {
            long offset = value - MinKey;

            if((ulong)offset < (ulong)Keys.Length)
            {
                return (int)offset;
            }
        }
        else
        {
            int index = Array.BinarySearch(Keys, value);

            if(index >= 0)
            {
                return index;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(key), key, "Not a defined member of the enum.");
    }

    // Reads the enum's underlying integer without boxing; the JIT folds the size and signedness checks per enum type.
    static long ToLong(TEnum value)
    {
        return Unsafe.SizeOf<TEnum>() switch
        {
            1 => IsSigned ? Unsafe.As<TEnum, sbyte>(ref value) : Unsafe.As<TEnum, byte>(ref value),
            2 => IsSigned ? Unsafe.As<TEnum, short>(ref value) : Unsafe.As<TEnum, ushort>(ref value),
            4 => IsSigned ? Unsafe.As<TEnum, int>(ref value) : Unsafe.As<TEnum, uint>(ref value),
            _ => Unsafe.As<TEnum, long>(ref value)
        };
    }
    #endregion
}
