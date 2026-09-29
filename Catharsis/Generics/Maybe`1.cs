namespace Catharsis.Generics;

///<summary>
///A value that may or may not be present, as an explicit alternative to <c>null</c>. Unlike
///<see cref="Nullable{T}"/> it works for reference types and value types alike, and it distinguishes "absent" from
///"present but <c>null</c>-like" by refusing to wrap <c>null</c> as a value.
///</summary>
///<typeparam name="T">The type of the wrapped value.</typeparam>
public readonly struct Maybe<T> : IEquatable<Maybe<T>>
{
    #region Fields
    readonly T? _value;
    #endregion

    #region Constructors
    Maybe(T value)
    {
        _value = value;
        HasValue = true;
    }
    #endregion

    #region Operators
    ///<summary>Determines whether two maybes are equal.</summary>
    public static bool operator ==(Maybe<T> left, Maybe<T> right) => left.Equals(right);

    ///<summary>Determines whether two maybes differ.</summary>
    public static bool operator !=(Maybe<T> left, Maybe<T> right) => !left.Equals(right);

    ///<summary>Wraps a value, treating <c>null</c> as <see cref="None"/>.</summary>
    public static implicit operator Maybe<T>(T? value) => From(value);
    #endregion

    #region Public methods
    ///<summary>Gets the empty option.</summary>
    public static Maybe<T> None => default;

    ///<summary>Wraps a value that must not be <c>null</c>.</summary>
    ///<exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    public static Maybe<T> Some(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new Maybe<T>(value);
    }

    ///<summary>Wraps a value, returning <see cref="None"/> if it is <c>null</c>.</summary>
    public static Maybe<T> From(T? value) => value is null ? default : new Maybe<T>(value);

    ///<summary>Transforms the value if present.</summary>
    ///<typeparam name="TResult">The new value type.</typeparam>
    ///<param name="mapper">Converts the value. A <c>null</c> result becomes <see cref="Maybe{T}.None"/>.</param>
    ///<exception cref="ArgumentNullException"><paramref name="mapper"/> is <c>null</c>.</exception>
    public Maybe<TResult> Map<TResult>(Func<T, TResult?> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return HasValue ? Maybe<TResult>.From(mapper(_value!)) : default;
    }

    ///<summary>Chains an operation that itself returns a maybe.</summary>
    ///<typeparam name="TResult">The next value type.</typeparam>
    ///<param name="binder">Produces the next option from the value.</param>
    ///<exception cref="ArgumentNullException"><paramref name="binder"/> is <c>null</c>.</exception>
    public Maybe<TResult> Bind<TResult>(Func<T, Maybe<TResult>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        return HasValue ? binder(_value!) : default;
    }

    ///<summary>Keeps the value only if it satisfies <paramref name="predicate"/>.</summary>
    ///<exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
    public Maybe<T> Where(Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return HasValue && predicate(_value!) ? this : default;
    }

    ///<summary>Collapses the maybe into one value by handling both cases.</summary>
    ///<typeparam name="TOut">The type produced.</typeparam>
    ///<param name="onSome">Handles a present value.</param>
    ///<param name="onNone">Handles absence.</param>
    ///<exception cref="ArgumentNullException">A handler is <c>null</c>.</exception>
    public TOut Match<TOut>(Func<T, TOut> onSome, Func<TOut> onNone)
    {
        ArgumentNullException.ThrowIfNull(onSome);
        ArgumentNullException.ThrowIfNull(onNone);

        return HasValue ? onSome(_value!) : onNone();
    }

    ///<summary>Gets the value, or <paramref name="fallback"/> when absent.</summary>
    public T GetValueOrDefault(T fallback) => HasValue ? _value! : fallback;

    ///<summary>Attempts to read the value.</summary>
    ///<returns><c>true</c> if a value is present.</returns>
    public bool TryGetValue(out T value)
    {
        value = _value!;
        return HasValue;
    }

    ///<inheritdoc/>
    public bool Equals(Maybe<T> other) => HasValue == other.HasValue && EqualityComparer<T?>.Default.Equals(_value, other._value);

    ///<inheritdoc/>
    public override bool Equals(object? obj) => (obj is Maybe<T> other) && Equals(other);

    ///<inheritdoc/>
    public override int GetHashCode() => HasValue ? HashCode.Combine(true, _value) : 0;

    ///<inheritdoc/>
    public override string ToString() => HasValue ? $"Some({_value})" : "None";
    #endregion

    #region Public properties
    ///<summary>Gets a value indicating whether a value is present.</summary>
    public bool HasValue { get; }

    ///<summary>Gets the value.</summary>
    ///<exception cref="InvalidOperationException">The maybe is empty.</exception>
    public T Value => HasValue ? _value! : throw new InvalidOperationException("The maybe has no value.");
    #endregion
}
