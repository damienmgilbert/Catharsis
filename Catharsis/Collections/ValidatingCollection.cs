using System.Collections.ObjectModel;

namespace Catharsis.Collections;

///<summary>
///A collection that validates each element against a predicate before insertion or replacement. Inherits from
///<see cref="Collection{T}"/> and overrides the insert and set operations to enforce the validation rule.
///</summary>
///<typeparam name="T">The type of elements stored in the collection.</typeparam>
public class ValidatingCollection<T> : Collection<T>
{
    #region Fields
    readonly Predicate<T> _validator;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="ValidatingCollection{T}"/> with the specified validation predicate.
    ///</summary>
    ///<param name="validator">
    ///A predicate that returns <c>true</c> if the element is valid and <c>false</c> otherwise.
    ///</param>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="validator"/> is <c>null</c>.</exception>
    public ValidatingCollection(Predicate<T> validator)
    {
        if(validator is null)
        {
            throw new ArgumentNullException(nameof(validator), "Validator predicate must not be null.");
        }

        _validator = validator;
    }
    #endregion

    #region Protected methods
    ///<inheritdoc/>
    protected override void InsertItem(int index, T item)
    {
        ThrowIfInvalid(item);
        base.InsertItem(index, item);
    }

    ///<inheritdoc/>
    protected override void SetItem(int index, T item)
    {
        ThrowIfInvalid(item);
        base.SetItem(index, item);
    }
    #endregion

    #region Private methods
    void ThrowIfInvalid(T item)
    {
        if(!_validator(item))
        {
            throw new ArgumentException("The item does not satisfy the validation rule.", nameof(item));
        }
    }
    #endregion
}
