using System.Reflection;

namespace Catharsis.ComponentModel;

///<summary>
///Captures the value of every public, readable and writable, non-indexer property of a <typeparamref name="T"/>
///instance, and can later restore that state — a building block for undo/redo. Complements
///<see cref="ChangeTracker{T}"/>, which tracks a single value rather than an object's full property set.
///</summary>
///<typeparam name="T">The type of object to snapshot.</typeparam>
public sealed class ComponentSnapshot<T> where T : class
{
    #region Fields
    static readonly PropertyInfo[] _properties = [.. typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(static property => property.CanRead && property.CanWrite && (property.GetIndexParameters().Length == 0))];

    readonly Dictionary<PropertyInfo, object?> _values;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="ComponentSnapshot{T}"/> and immediately captures the current state of
    ///<paramref name="source"/>.
    ///</summary>
    ///<param name="source">The object to snapshot.</param>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public ComponentSnapshot(T source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Source = source;
        _values = _properties.ToDictionary(static property => property, property => property.GetValue(source));
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Re-captures the current value of every tracked property from <see cref="Source"/>, overwriting the previous
    ///snapshot.
    ///</summary>
    public void Capture()
    {
        foreach (PropertyInfo property in _properties)
        {
            _values[property] = property.GetValue(Source);
        }
    }

    ///<summary>
    ///Writes every captured property value back onto <see cref="Source"/>.
    ///</summary>
    public void Restore()
    {
        foreach (KeyValuePair<PropertyInfo, object?> entry in _values)
        {
            entry.Key.SetValue(Source, entry.Value);
        }
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the object this snapshot was captured from and restores to.
    ///</summary>
    public T Source { get; }
    #endregion
}
