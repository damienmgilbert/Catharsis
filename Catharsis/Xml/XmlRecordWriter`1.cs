using System.Globalization;
using System.Reflection;
using System.Xml.Linq;

namespace Catharsis.Xml;

///<summary>
///Writes instances of <typeparamref name="T"/> as XML, one child element per public, readable property. Mirrors
///what <see cref="Catharsis.Serialization.DelimitedRecordWriter{T}"/> does for CSV, but produces an
///<see cref="XElement"/> tree instead of delimited text.
///</summary>
///<typeparam name="T">The record type to write.</typeparam>
///<param name="recordElementName">The element name to use for a single record. Defaults to <c>"Record"</c>.</param>
public sealed class XmlRecordWriter<T>(string recordElementName = "Record")
{
    #region Fields
    static readonly PropertyInfo[] _properties = [.. typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(static property => property.CanRead)];

    readonly string _recordElementName = string.IsNullOrWhiteSpace(recordElementName) ? throw new ArgumentException("Record element name must not be null, empty, or whitespace.", nameof(recordElementName)) : recordElementName;
    #endregion

    #region Public methods
    ///<summary>
    ///Writes a single record as an <see cref="XElement"/>, one child element per public, readable property.
    ///</summary>
    ///<param name="record">The record to write.</param>
    ///<returns>An <see cref="XElement"/> representing <paramref name="record"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="record"/> is <c>null</c>.</exception>
    public XElement WriteRecord(T record)
    {
        ArgumentNullException.ThrowIfNull(record);

        XElement element = new(_recordElementName);

        foreach (PropertyInfo property in _properties)
        {
            object? value = property.GetValue(record);
            element.Add(new XElement(property.Name, Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty));
        }

        return element;
    }

    ///<summary>
    ///Writes every record as a single container element wrapping one child element per record (via <see cref="WriteRecord"/>).
    ///</summary>
    ///<param name="records">The records to write.</param>
    ///<param name="containerElementName">The element name for the wrapping container. Defaults to <c>"Records"</c>.</param>
    ///<returns>An <see cref="XElement"/> containing one child per record.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="records"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="containerElementName"/> is <c>null</c>, empty, or whitespace.</exception>
    public XElement WriteRecords(IEnumerable<T> records, string containerElementName = "Records")
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentException.ThrowIfNullOrWhiteSpace(containerElementName);

        XElement container = new(containerElementName);

        foreach (T record in records)
        {
            container.Add(WriteRecord(record));
        }

        return container;
    }
    #endregion
}
