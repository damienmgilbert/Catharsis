namespace Catharsis.Generics;

///<summary>
///The outcome of an operation that either succeeded with a <typeparamref name="T"/> or failed with a
///<typeparamref name="TError"/>, making the failure path part of the method signature instead of an exception.
///Note that <c>default(Result&lt;T, TError&gt;)</c> is a failure carrying <c>default(TError)</c>.
///</summary>
///<typeparam name="T">The type of the success value.</typeparam>
///<typeparam name="TError">The type of the failure value.</typeparam>
public readonly struct Result<T, TError> : IEquatable<Result<T, TError>>
{
    #region Fields
    readonly T? _value;
    readonly TError? _error;
    #endregion

    #region Constructors
    Result(bool isSuccess, T? value, TError? error)
    {
        IsSuccess = isSuccess;
        _value = value;
        _error = error;
    }
    #endregion

    #region Operators
    ///<summary>Determines whether two results are equal.</summary>
    public static bool operator ==(Result<T, TError> left, Result<T, TError> right) => left.Equals(right);

    ///<summary>Determines whether two results differ.</summary>
    public static bool operator !=(Result<T, TError> left, Result<T, TError> right) => !left.Equals(right);

    ///<summary>Wraps a value as a successful result.</summary>
    public static implicit operator Result<T, TError>(T value) => Ok(value);
    #endregion

    #region Public methods
    ///<summary>Creates a successful result.</summary>
    ///<param name="value">The success value.</param>
    public static Result<T, TError> Ok(T value) => new(true, value, default);

    ///<summary>Creates a failed result.</summary>
    ///<param name="error">The failure value.</param>
    public static Result<T, TError> Fail(TError error) => new(false, default, error);

    ///<summary>Transforms the success value, leaving a failure untouched.</summary>
    ///<typeparam name="TNext">The new success type.</typeparam>
    ///<param name="mapper">Converts the success value.</param>
    ///<exception cref="ArgumentNullException"><paramref name="mapper"/> is <c>null</c>.</exception>
    public Result<TNext, TError> Map<TNext>(Func<T, TNext> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return IsSuccess ? Result<TNext, TError>.Ok(mapper(_value!)) : Result<TNext, TError>.Fail(_error!);
    }

    ///<summary>Chains an operation that can itself fail, short-circuiting on the first failure.</summary>
    ///<typeparam name="TNext">The next operation's success type.</typeparam>
    ///<param name="binder">Produces the next result from the success value.</param>
    ///<exception cref="ArgumentNullException"><paramref name="binder"/> is <c>null</c>.</exception>
    public Result<TNext, TError> Bind<TNext>(Func<T, Result<TNext, TError>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        return IsSuccess ? binder(_value!) : Result<TNext, TError>.Fail(_error!);
    }

    ///<summary>Transforms the failure value, leaving a success untouched.</summary>
    ///<typeparam name="TNextError">The new failure type.</typeparam>
    ///<param name="mapper">Converts the failure value.</param>
    ///<exception cref="ArgumentNullException"><paramref name="mapper"/> is <c>null</c>.</exception>
    public Result<T, TNextError> MapError<TNextError>(Func<TError, TNextError> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return IsSuccess ? Result<T, TNextError>.Ok(_value!) : Result<T, TNextError>.Fail(mapper(_error!));
    }

    ///<summary>Collapses the result into a single value by handling both cases.</summary>
    ///<typeparam name="TOut">The type produced.</typeparam>
    ///<param name="onSuccess">Handles a success.</param>
    ///<param name="onFailure">Handles a failure.</param>
    ///<exception cref="ArgumentNullException">A handler is <c>null</c>.</exception>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<TError, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess ? onSuccess(_value!) : onFailure(_error!);
    }

    ///<summary>Gets the success value, or <paramref name="fallback"/> for a failure.</summary>
    public T GetValueOrDefault(T fallback) => IsSuccess ? _value! : fallback;

    ///<summary>Attempts to read the success value.</summary>
    ///<returns><c>true</c> for a success.</returns>
    public bool TryGetValue(out T value)
    {
        value = _value!;
        return IsSuccess;
    }

    ///<summary>Attempts to read the failure value.</summary>
    ///<returns><c>true</c> for a failure.</returns>
    public bool TryGetError(out TError error)
    {
        error = _error!;
        return !IsSuccess;
    }

    ///<inheritdoc/>
    public bool Equals(Result<T, TError> other) => IsSuccess == other.IsSuccess && EqualityComparer<T?>.Default.Equals(_value, other._value) && EqualityComparer<TError?>.Default.Equals(_error, other._error);

    ///<inheritdoc/>
    public override bool Equals(object? obj) => (obj is Result<T, TError> other) && Equals(other);

    ///<inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(IsSuccess, _value, _error);

    ///<inheritdoc/>
    public override string ToString() => IsSuccess ? $"Ok({_value})" : $"Fail({_error})";
    #endregion

    #region Public properties
    ///<summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    ///<summary>Gets a value indicating whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    ///<summary>Gets the success value.</summary>
    ///<exception cref="InvalidOperationException">The result is a failure.</exception>
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("A failed result has no value.");

    ///<summary>Gets the failure value.</summary>
    ///<exception cref="InvalidOperationException">The result is a success.</exception>
    public TError Error => !IsSuccess ? _error! : throw new InvalidOperationException("A successful result has no error.");
    #endregion
}
