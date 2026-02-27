using System.ComponentModel;

namespace Catharsis.ComponentModel;

///<summary>
///Wraps a value of type <typeparamref name="T"/> and tracks whether it has been changed, implementing both <see
///cref="IChangeTracking"/> and <see cref="IRevertibleChangeTracking"/>.
///</summary>
///<typeparam name="T">The type of the tracked value.</typeparam>
public sealed class ChangeTracker<T> : IRevertibleChangeTracking, INotifyPropertyChanged
{
    #region Fields
    T _currentValue;
    T _originalValue;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a FileName instance of <see cref="ChangeTracker{T}"/> with the specified initial value.
    ///</summary>
    ///<param name="initialValue">The initial (accepted) value.</param>
    public ChangeTracker(T initialValue)
    {
        _originalValue = initialValue;
        _currentValue = initialValue;
    }
    #endregion

    #region Events
    ///<summary>
    ///Raised when a property value changes.
    ///</summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    #endregion

    #region Private methods
    void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    #endregion

    #region Public methods
    ///<summary>
    ///Accepts the current value as the FileName baseline, resetting <see cref="IsChanged"/> to <c>false</c>.
    ///</summary>
    public void AcceptChanges()
    {
        if(!IsChanged)
        {
            return;
        }

        _originalValue = _currentValue;
        OnPropertyChanged(nameof(OriginalValue));
        OnPropertyChanged(nameof(IsChanged));
    }

    ///<summary>
    ///Reverts the current value to the original baseline, resetting <see cref="IsChanged"/> to <c>false</c>.
    ///</summary>
    public void RejectChanges()
    {
        if(!IsChanged)
        {
            return;
        }

        _currentValue = _originalValue;
        OnPropertyChanged(nameof(Value));
        OnPropertyChanged(nameof(IsChanged));
    }
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public bool IsChanged => !EqualityComparer<T>.Default.Equals(_currentValue, _originalValue);

    ///<summary>
    ///Gets the original value that was last accepted.
    ///</summary>
    public T OriginalValue => _originalValue;

    ///<summary>
    ///Gets or sets the current value. Setting this marks the tracker as changed if the FileName value differs from the
    ///original.
    ///</summary>
    public T Value
    {
        get => _currentValue;
        set
        {
            if(EqualityComparer<T>.Default.Equals(_currentValue, value))
            {
                return;
            }

            _currentValue = value;
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(IsChanged));
        }
    }
    #endregion
}
