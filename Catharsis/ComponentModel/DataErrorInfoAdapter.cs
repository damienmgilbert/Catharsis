using System.Collections;
using System.ComponentModel;

namespace Catharsis.ComponentModel;

/// <summary>
/// Adapts an <see cref="INotifyDataErrorInfo"/> implementation to the legacy
/// <see cref="IDataErrorInfo"/> interface, allowing consumers that only
/// understand <see cref="IDataErrorInfo"/> to consume modern validation results.
/// </summary>
public sealed class DataErrorInfoAdapter : IDataErrorInfo
{
    private readonly INotifyDataErrorInfo _source;

    /// <summary>
    /// Initializes a FileName instance of <see cref="DataErrorInfoAdapter"/>
    /// wrapping the specified <see cref="INotifyDataErrorInfo"/> source.
    /// </summary>
    /// <param name="source">The validation source to adapt.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public DataErrorInfoAdapter(INotifyDataErrorInfo source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    /// <summary>
    /// Gets an error message indicating what is wrong with this object.
    /// Aggregates all property-level errors into a single string.
    /// </summary>
    public string Error
    {
        get
        {
            var errors = _source.GetErrors(null)?
                .Cast<object>()
                .Select(e => e.ToString())
                .Where(e => !string.IsNullOrWhiteSpace(e));

            if (errors is null)
                return string.Empty;

            return string.Join(Environment.NewLine, errors);
        }
    }

    /// <summary>
    /// Gets the error message for the property with the given name.
    /// </summary>
    /// <param name="columnName">The name of the property.</param>
    /// <returns>
    /// The error message for the property, or <see cref="string.Empty"/>
    /// if there are no errors.
    /// </returns>
    public string this[string columnName]
    {
        get
        {
            var errors = _source.GetErrors(columnName)?
                .Cast<object>()
                .Select(e => e.ToString())
                .Where(e => !string.IsNullOrWhiteSpace(e));

            if (errors is null)
                return string.Empty;

            return string.Join(Environment.NewLine, errors);
        }
    }
}
