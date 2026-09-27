using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

namespace Catharsis.Extensions;

///<summary>
///Provides extension methods for <see cref="Enum"/> values: decomposing flags, reading <see
///cref="DescriptionAttribute"/> text, and checking membership without the allocation overhead of <see
///cref="Enum.IsDefined(Type, object)"/>.
///</summary>
public static class EnumExtensions
{
    #region Fields
    static readonly ConcurrentDictionary<(Type, string), string> DescriptionCache = new();
    #endregion

    #region Public methods

    ///<summary>
    ///Decomposes a flags enum value into its individual set flags.
    ///</summary>
    ///<typeparam name="TEnum">The enum type.</typeparam>
    ///<param name="value">The flags value to decompose.</param>
    ///<returns>Each individual flag that is set in <paramref name="value"/>.</returns>
    public static IEnumerable<TEnum> GetFlags<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        foreach(TEnum candidate in Enum.GetValues<TEnum>())
        {
            if(Convert.ToUInt64(candidate, System.Globalization.CultureInfo.InvariantCulture) != 0 && value.HasFlag(candidate))
            {
                yield return candidate;
            }
        }
    }

    ///<summary>
    ///Gets the text from the value's <see cref="DescriptionAttribute"/>, or its name if no attribute is present.
    ///Results are cached per enum value after the first lookup.
    ///</summary>
    ///<typeparam name="TEnum">The enum type.</typeparam>
    ///<param name="value">The enum value.</param>
    ///<returns>The description text, or the value's name if undecorated.</returns>
    public static string GetDescription<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        string name = value.ToString();

        return DescriptionCache.GetOrAdd((typeof(TEnum), name), static key =>
        {
            (Type enumType, string valueName) = key;
            MemberInfo? member = enumType.GetField(valueName);
            DescriptionAttribute? attribute = member?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? valueName;
        });
    }

    ///<summary>
    ///Determines whether the specified value is a named member of <typeparamref name="TEnum"/>, without the boxing
    ///that <see cref="Enum.IsDefined(Type, object)"/> incurs.
    ///</summary>
    ///<typeparam name="TEnum">The enum type.</typeparam>
    ///<param name="value">The value to test.</param>
    ///<returns><c>true</c> if <paramref name="value"/> is a named member; otherwise <c>false</c>.</returns>
    public static bool IsDefined<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        foreach(TEnum candidate in Enum.GetValues<TEnum>())
        {
            if(candidate.Equals(value))
            {
                return true;
            }
        }

        return false;
    }
    #endregion
}
