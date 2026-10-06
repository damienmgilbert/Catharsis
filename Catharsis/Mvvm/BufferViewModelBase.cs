using CommunityToolkit.Mvvm.ComponentModel;

namespace Catharsis.Mvvm;

///<summary>
///A base view model for buffer-based data presentation, providing common MVVM properties and state management for
///buffer operations.
///</summary>
public abstract class BufferViewModelBase : ObservableObject, IDisposable
{
    #region Fields
    private long _dataSize;
    private bool _disposed;
    private string? _errorMessage;
    private bool _hasData;
    private bool _isLoading;
    #endregion

    #region Protected methods
    ///<summary>
    ///Disposes resources used by this view model.
    ///</summary>
    ///<param name="disposing">Whether managed resources should be disposed.</param>
    protected virtual void Dispose(bool disposing)
    {
        if(!_disposed)
        {
            if(disposing)
            {
                ClearData();
            }

            _disposed = true;
        }
    }

    ///<summary>
    ///When overridden, performs the actual asynchronous data loading.
    ///</summary>
    ///<param name="cancellationToken">A cancellation token.</param>
    ///<returns>A task representing the async operation.</returns>
    protected abstract Task LoadCoreAsync(CancellationToken cancellationToken);
    #endregion

    #region Public methods
    ///<summary>
    ///Clears the buffer data and resets the view model state.
    ///</summary>
    public virtual void ClearData()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        HasData = false;
        DataSize = 0;
        ErrorMessage = null;
    }

    ///<inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ///<summary>
    ///Loads data into the view model's buffer. Derived classes must implement the actual loading logic.
    ///</summary>
    ///<param name="cancellationToken">A cancellation token.</param>
    ///<returns>A task representing the async operation.</returns>
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            await LoadCoreAsync(cancellationToken);
            HasData = DataSize > 0;
        } catch(OperationCanceledException)
        {
            ErrorMessage = "Operation was cancelled.";
        } catch(Exception ex)
        {
            ErrorMessage = ex.Message;
            HasData = false;
        } finally
        {
            IsLoading = false;
        }
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets or sets the size of the loaded data.
    ///</summary>
    public long DataSize { get => _dataSize; protected set => SetProperty(ref _dataSize, value); }

    ///<summary>
    ///Gets or sets the error message, if any.
    ///</summary>
    public string? ErrorMessage { get => _errorMessage; protected set => SetProperty(ref _errorMessage, value); }

    ///<summary>
    ///Gets or sets whether the view model has data.
    ///</summary>
    public bool HasData { get => _hasData; protected set => SetProperty(ref _hasData, value); }

    ///<summary>
    ///Gets whether an error has occurred.
    ///</summary>
    public bool HasError => ErrorMessage is not null;

    ///<summary>
    ///Gets or sets whether data is currently loading.
    ///</summary>
    public bool IsLoading { get => _isLoading; protected set => SetProperty(ref _isLoading, value); }
    #endregion
}
