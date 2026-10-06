using System.Collections;
using System.ComponentModel;

namespace Catharsis.Mvvm;

///<summary>
///An <see cref="ObservablePooledBuffer{T}"/> that automatically revalidates its written contents against a
///configured validator after every write or clear, exposing the result through <see cref="INotifyDataErrorInfo"/>
///for binding to validation UI.
///</summary>
///<typeparam name="T">The type of elements in the buffer.</typeparam>
public sealed class ObservableValidatedBuffer<T> : ObservablePooledBuffer<T>, INotifyDataErrorInfo
{
    #region Fields
    readonly Func<ReadOnlyMemory<T>, IEnumerable<string>> _validator;
    List<string> _errors = [];
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="ObservableValidatedBuffer{T}"/> with the specified validator.
    ///</summary>
    ///<param name="validator">A delegate that inspects the buffer's written contents and returns any validation errors.</param>
    ///<param name="initialCapacity">The initial buffer capacity.</param>
    ///<exception cref="ArgumentNullException"><paramref name="validator"/> is <c>null</c>.</exception>
    public ObservableValidatedBuffer(Func<ReadOnlyMemory<T>, IEnumerable<string>> validator, int initialCapacity = 256) : base(initialCapacity)
    {
        ArgumentNullException.ThrowIfNull(validator);

        _validator = validator;
        PropertyChanged += OnBufferPropertyChanged;
    }
    #endregion

    #region Events
    ///<inheritdoc/>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    #endregion

    #region Private methods
    void OnBufferPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if(args.PropertyName == nameof(Count))
        {
            Revalidate();
        }
    }

    void Revalidate()
    {
        _errors = [.. _validator(WrittenMemory)];
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(WrittenMemory)));
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public IEnumerable GetErrors(string? propertyName) => _errors;
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public bool HasErrors => _errors.Count > 0;
    #endregion
}
