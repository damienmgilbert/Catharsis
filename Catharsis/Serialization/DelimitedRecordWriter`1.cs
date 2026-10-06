using System.Globalization;
using System.Reflection;

namespace Catharsis.Serialization;

///<summary>
///Writes instances of <typeparamref name="T"/> as delimited (CSV/TSV-style) text, deriving column names from public,
///readable properties and quoting fields that contain the delimiter, a quote character, or a line break.
///</summary>
///<typeparam name="T">The record type to write.</typeparam>
///<param name="delimiter">The field delimiter. Defaults to a comma.</param>
public sealed class DelimitedRecordWriter<T>(char delimiter = ',')
{
    #region Fields
    static readonly PropertyInfo[] _properties = [.. typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(static property => property.CanRead)];
    #endregion

    #region Private methods
    string FormatField(object? value)
    {
        string text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        bool needsQuoting = text.Contains(delimiter) || text.Contains('"') || text.Contains('\n') || text.Contains('\r');

        return needsQuoting ? $"\"{text.Replace("\"", "\"\"")}\"" : text;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Writes the header row, one column per public, readable property, in declaration order.
    ///</summary>
    ///<param name="writer">The text destination.</param>
    ///<exception cref="ArgumentNullException"><paramref name="writer"/> is <c>null</c>.</exception>
    public void WriteHeader(TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteLine(string.Join(delimiter, _properties.Select(static property => property.Name)));
    }

    ///<summary>
    ///Writes a single record as one delimited line.
    ///</summary>
    ///<param name="writer">The text destination.</param>
    ///<param name="record">The record to write.</param>
    ///<exception cref="ArgumentNullException"><paramref name="writer"/> or <paramref name="record"/> is <c>null</c>.</exception>
    public void WriteRecord(TextWriter writer, T record)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(record);

        writer.WriteLine(string.Join(delimiter, _properties.Select(property => FormatField(property.GetValue(record)))));
    }
    #endregion
}
