using System.Globalization;
using System.Reflection;
using System.Xml.Linq;

namespace Catharsis.Xml;

///<summary>
///Reads instances of <typeparamref name="T"/> from XML, mapping each matching element's child elements or
///attributes to public, writable properties by name (case-insensitive). Mirrors what
///<see cref="Catharsis.Serialization.DelimitedRecordReader{T}"/> does for CSV, but for <see cref="XElement"/> trees.
///</summary>
///<typeparam name="T">The record type to populate. Must have a public parameterless constructor.</typeparam>
///<param name="recordElementName">The element name identifying a single record. Defaults to <c>"Record"</c>.</param>
public sealed class XmlRecordReader<T>(string recordElementName = "Record") where T : new()
{
    #region Fields
    static readonly PropertyInfo[] _properties = [.. typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(static property => property.CanWrite)];

    readonly string _recordElementName = string.IsNullOrWhiteSpace(recordElementName) ? throw new ArgumentException("Record element name must not be null, empty, or whitespace.", nameof(recordElementName)) : recordElementName;
    #endregion

    #region Private methods
    static object? ConvertField(string text, Type targetType) => (targetType == typeof(string)) ? text : Convert.ChangeType(text, targetType, CultureInfo.InvariantCulture);
    #endregion

    #region Public methods
    ///<summary>
    ///Reads every record found anywhere under <paramref name="container"/>, matching by
    ///<see cref="XmlRecordReader{T}"/>'s configured record element name at any depth.
    ///</summary>
    ///<param name="container">The document or element to search.</param>
    ///<returns>The records parsed from every matching element.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="container"/> is <c>null</c>.</exception>
    public IEnumerable<T> ReadRecords(XContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);

        foreach(XElement recordElement in container.Descendants(_recordElementName))
        {
            T record = new();

            foreach(PropertyInfo property in _properties)
            {
                string? text = recordElement.Element(property.Name)?.Value ?? recordElement.Attribute(property.Name)?.Value;

                if(text is not null)
                {
                    property.SetValue(record, ConvertField(text, property.PropertyType));
                }
            }

            yield return record;
        }
    }
    #endregion
}
