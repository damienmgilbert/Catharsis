using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Catharsis.Generics;

///<summary>
///Caches compiled expression trees. Compiling a <see cref="LambdaExpression"/> is expensive (typically tens to hundreds
///of microseconds) while calling the result is cheap, so anything invoked repeatedly should be compiled once and
///reused. Entries are keyed by a caller-supplied key because two lambdas with the same shape are distinct objects.
///</summary>
public sealed class ExpressionCache
{
    #region Fields
    private readonly ConcurrentDictionary<object, Delegate> _compiled = new();
    #endregion

    #region Public methods
    ///<summary>
    ///Discards every compiled delegate.
    ///</summary>
    public void Clear() => _compiled.Clear();

    ///<summary>
    ///Gets a compiled getter for a public instance property, avoiding reflection on each read.
    ///</summary>
    ///<typeparam name="TSource">The declaring type.</typeparam>
    ///<typeparam name="TValue">The property type.</typeparam>
    ///<param name="propertyName">The property name.</param>
    ///<returns>A delegate reading the property.</returns>
    ///<exception cref="ArgumentException">No public readable property has that name and type.</exception>
    public Func<TSource, TValue> GetGetter<TSource, TValue>(string propertyName)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);

        return (Func<TSource, TValue>)_compiled.GetOrAdd(
                                      (typeof(TSource), propertyName, typeof(TValue)),
                                      static key =>
                                      {
                                          (Type owner, string name, Type valueType) = ((Type, string, Type))key;
                                          PropertyInfo? property = owner.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);

                                          if(property?.GetMethod?.IsPublic != true || !valueType.IsAssignableFrom(property.PropertyType))
                                          {
                                              throw new ArgumentException($"{owner.Name} has no public readable property '{name}' assignable to {valueType.Name}.", nameof(propertyName));
                                          }

                                          ParameterExpression instance = Expression.Parameter(owner, "instance");
                                          return Expression.Lambda<Func<TSource, TValue>>(Expression.Convert(Expression.Property(instance, property), valueType), instance).Compile();
                                      });
    }

        ///<summary>
///Gets the compiled form of <paramref name="expression"/>, compiling it on the first request for ///<paramref
///name="key"/>.
///</summary>
    ///<typeparam name="TDelegate">The delegate type of the lambda.</typeparam>
    ///<param name="key">Identifies the expression. Later calls with an equal key return the first compiled delegate.</param>
    ///<param name="expression">The expression to compile if <paramref name="key"/> is new.</param>
    ///<returns>The compiled delegate.</returns>
    ///<exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    ///<exception cref="InvalidCastException"><paramref name="key"/> was earlier used for a different delegate type.</exception>
    public TDelegate GetOrCompile<TDelegate>(object key, Expression<TDelegate> expression) where TDelegate : Delegate
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(expression);

        return (TDelegate)_compiled.GetOrAdd(key, static(_, expr) => expr.Compile(), expression);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of compiled delegates held.
    ///</summary>
    public int Count => _compiled.Count;
    #endregion
}
