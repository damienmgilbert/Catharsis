namespace Catharsis.DesignPatterns.Enterprise;

///<summary>
///Implements the Specification design pattern.
///</summary>
///<remarks>
///This composes plain <see cref="Func{T, TResult}"/> predicates for in-memory evaluation. For composing
///<see cref="System.Linq.Expressions.Expression{TDelegate}"/> predicates against a query provider (e.g. Entity
///Framework), use <see cref="Catharsis.Linq.Expressions.PredicateCombinator"/> instead.
///</remarks>
public class SpecificationPattern
{
    #region Public methods
    ///<summary>
    ///Specification — combines two specifications so the result is satisfied only when both are.
    ///</summary>
    ///<typeparam name="T">The type being evaluated against the specification.</typeparam>
    ///<param name="left">The first specification.</param>
    ///<param name="right">The second specification.</param>
    ///<returns>A specification satisfied when both <paramref name="left"/> and <paramref name="right"/> are.</returns>
    public static Func<T, bool> And<T>(Func<T, bool> left, Func<T, bool> right)
    {
        if(left is null)
        {
            throw new ArgumentNullException(nameof(left), "Left specification must not be null.");
        }

        if(right is null)
        {
            throw new ArgumentNullException(nameof(right), "Right specification must not be null.");
        }

        return candidate => left(candidate) && right(candidate);
    }

    ///<summary>
    ///Specification — negates a specification.
    ///</summary>
    ///<typeparam name="T">The type being evaluated against the specification.</typeparam>
    ///<param name="specification">The specification to negate.</param>
    ///<returns>A specification satisfied when <paramref name="specification"/> is not.</returns>
    public static Func<T, bool> Not<T>(Func<T, bool> specification)
    {
        if(specification is null)
        {
            throw new ArgumentNullException(nameof(specification), "Specification must not be null.");
        }

        return candidate => !specification(candidate);
    }

    ///<summary>
    ///Specification — combines two specifications so the result is satisfied when either is.
    ///</summary>
    ///<typeparam name="T">The type being evaluated against the specification.</typeparam>
    ///<param name="left">The first specification.</param>
    ///<param name="right">The second specification.</param>
    ///<returns>A specification satisfied when either <paramref name="left"/> or <paramref name="right"/> is.</returns>
    public static Func<T, bool> Or<T>(Func<T, bool> left, Func<T, bool> right)
    {
        if(left is null)
        {
            throw new ArgumentNullException(nameof(left), "Left specification must not be null.");
        }

        if(right is null)
        {
            throw new ArgumentNullException(nameof(right), "Right specification must not be null.");
        }

        return candidate => left(candidate) || right(candidate);
    }
    #endregion
}
