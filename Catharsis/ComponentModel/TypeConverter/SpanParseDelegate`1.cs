using System.Diagnostics.CodeAnalysis;

namespace Catharsis.ComponentModel.TypeConverter;

#region Delegates
///<summary>
///A delegate that attempts to parse a value of <typeparamref name="T"/> from a <see cref="ReadOnlySpan{T}"/> of <see
///cref="char"/>.
///</summary>
///<typeparam name="T">The target type.</typeparam>
///<param name="span">The character span to parse.</param>
///<param name="provider">The format provider for culture-aware parsing.</param>
///<param name="result">
///When this method returns, contains the parsed value if successful; otherwise, the default value of <typeparamref
///name="T"/>.
///</param>
///<returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
public delegate bool SpanParseDelegate<T>(ReadOnlySpan<char> span, IFormatProvider? provider, [MaybeNullWhen(false)] out T result);
#endregion
