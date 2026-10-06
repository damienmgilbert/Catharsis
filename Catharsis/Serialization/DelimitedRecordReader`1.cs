using Catharsis.Text.RegularExpressions;
using System.Globalization;
using System.Reflection;

namespace Catharsis.Serialization;

///<summary>
///Reads delimited (CSV/TSV-style) text into instances of <typeparamref name="T"/>, mapping columns to public,
///writable properties by header name. Built on <see cref="CsvLineTokenizer"/> for quote-aware field splitting.
///</summary>
///<typeparam name="T">The record type to populate. Must have a public parameterless constructor.</typeparam>
///<param name="delimiter">The field delimiter. Defaults to a comma.</param>
public sealed class DelimitedRecordReader<T>(char delimiter = ',') where T : new()
{
    #region Fields
    static readonly PropertyInfo[] _properties = [.. typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(static property => property.CanWrite)];

    readonly CsvLineTokenizer _tokenizer = new(delimiter);
    #endregion

    #region Private methods
    static object? ConvertField(string field, Type targetType) => (targetType == typeof(string)) ? field : Convert.ChangeType(field, targetType, CultureInfo.InvariantCulture);
    #endregion

    #region Public methods
    ///<summary>
    ///Reads every record from <paramref name="reader"/>, treating the first line as a header row that maps columns
    ///to properties by name (case-insensitive). Columns with no matching property are ignored.
    ///</summary>
    ///<param name="reader">The text source, with a header line followed by zero or more record lines.</param>
    ///<returns>The records parsed from every non-empty line after the header.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="reader"/> is <c>null</c>.</exception>
    public IEnumerable<T> ReadRecords(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        string? headerLine = reader.ReadLine();

        if (headerLine is null)
        {
            yield break;
        }

        IReadOnlyList<string> headers = _tokenizer.Tokenize(headerLine);
        PropertyInfo?[] mapping = [.. headers.Select(static header => Array.Find(_properties, property => string.Equals(property.Name, header, StringComparison.OrdinalIgnoreCase)))];

        string? line;

        while ((line = reader.ReadLine()) is not null)
        {
            if (line.Length == 0)
            {
                continue;
            }

            IReadOnlyList<string> fields = _tokenizer.Tokenize(line);
            T record = new();

            for (int index = 0; (index < fields.Count) && (index < mapping.Length); index++)
            {
                PropertyInfo? property = mapping[index];

                if (property is not null)
                {
                    property.SetValue(record, ConvertField(fields[index], property.PropertyType));
                }
            }

            yield return record;
        }
    }
    #endregion
}
