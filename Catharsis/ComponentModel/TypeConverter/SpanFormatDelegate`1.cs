namespace Catharsis.ComponentModel.TypeConverter;

#region Delegates
///<summary>
///A delegate that attempts to format a value of <typeparamref name="T"/> into a <see cref="Span{T}"/> of <see
///cref="char"/>.
///</summary>
///<typeparam name="T">The source type.</typeparam>
///<param name="value">The value to format.</param>
///<param name="destination">The destination span.</param>
///<param name="provider">The format provider for culture-aware formatting.</param>
///<param name="charsWritten">The number of characters written.</param>
///<returns><c>true</c> if formatting succeeded; otherwise, <c>false</c>.</returns>
public delegate bool SpanFormatDelegate<T>(T value, Span<char> destination, IFormatProvider? provider, out int charsWritten);
#endregion
