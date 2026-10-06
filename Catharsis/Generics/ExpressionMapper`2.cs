using Catharsis.Linq.Expressions;
using System.Linq.Expressions;
using System.Reflection;

namespace Catharsis.Generics;

///<summary>
///Builds a fast <c>TSource -&gt; TDest</c> mapping function by assembling an expression tree and compiling it once, so
///every later call runs as ordinary compiled code with no reflection. The tree is assembled with
///<see cref="ExpressionFactory"/>, and each source expression is inlined into it with
///<see cref="ExpressionComposer.RebindParameters(Expression, IReadOnlyList{ParameterExpression}, IReadOnlyList{Expression})"/>
///rather than called through an <c>Invoke</c> node. Properties with the same name and an assignable
///type are mapped automatically; <see cref="Map{TProperty}"/> overrides or adds a mapping and
///<see cref="Ignore{TProperty}"/> removes one.
///</summary>
///<typeparam name="TSource">The type mapped from.</typeparam>
///<typeparam name="TDest">The type mapped to. Must have a public parameterless constructor.</typeparam>
public sealed class ExpressionMapper<TSource, TDest>
    where TDest : new()
{
    #region Fields
    readonly Dictionary<PropertyInfo, LambdaExpression?> _overrides = [];
    #endregion

    #region Public methods
    ///<summary>
    ///Maps a destination property from a source expression.
    ///</summary>
    ///<typeparam name="TProperty">The destination property type.</typeparam>
    ///<param name="destination">Selects the destination property, e.g. <c>d =&gt; d.Name</c>.</param>
    ///<param name="source">Computes its value from the source, e.g. <c>s =&gt; s.First + " " + s.Last</c>.</param>
    ///<returns>This mapper for chaining.</returns>
    ///<exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="destination"/> is not a settable property.</exception>
    public ExpressionMapper<TSource, TDest> Map<TProperty>(Expression<Func<TDest, TProperty>> destination, Expression<Func<TSource, TProperty>> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        _overrides[GetSettableProperty(destination)] = source;
        return this;
    }

    ///<summary>
    ///Excludes a destination property from mapping, leaving it at its constructor default.
    ///</summary>
    ///<typeparam name="TProperty">The destination property type.</typeparam>
    ///<param name="destination">Selects the destination property.</param>
    ///<returns>This mapper for chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="destination"/> is not a settable property.</exception>
    public ExpressionMapper<TSource, TDest> Ignore<TProperty>(Expression<Func<TDest, TProperty>> destination)
    {
        _overrides[GetSettableProperty(destination)] = null;
        return this;
    }

    ///<summary>
    ///Compiles the mapping.
    ///</summary>
    ///<returns>A delegate that creates a new <typeparamref name="TDest"/> from a <typeparamref name="TSource"/>.</returns>
    public Func<TSource, TDest> Build()
    {
        ParameterExpression sourceParameter = ExpressionFactory.Parameter(typeof(TSource), "source");
        List<MemberBinding> bindings = [];

        foreach (PropertyInfo destination in typeof(TDest).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(static p => p.SetMethod?.IsPublic == true))
        {
            Expression? value = null;

            if (_overrides.TryGetValue(destination, out LambdaExpression? mapping))
            {
                if (mapping is not null)
                {
                    value = ExpressionComposer.RebindParameters(mapping.Body, mapping.Parameters, [sourceParameter]);
                }
            }
            else
            {
                PropertyInfo? match = typeof(TSource).GetProperty(destination.Name, BindingFlags.Public | BindingFlags.Instance);

                if (match?.GetMethod?.IsPublic == true && destination.PropertyType.IsAssignableFrom(match.PropertyType))
                {
                    value = ExpressionFactory.Convert(ExpressionFactory.Property(sourceParameter, match), destination.PropertyType);
                }
            }

            if (value is not null)
            {
                bindings.Add(ExpressionFactory.Bind(destination, value));
            }
        }

        return ExpressionFactory.Lambda<Func<TSource, TDest>>(ExpressionFactory.MemberInit(ExpressionFactory.New(typeof(TDest)), [.. bindings]), sourceParameter).Compile();
    }
    #endregion

    #region Private methods
    static PropertyInfo GetSettableProperty<TProperty>(Expression<Func<TDest, TProperty>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        Expression body = selector.Body is UnaryExpression { NodeType: ExpressionType.Convert } convert ? convert.Operand : selector.Body;

        if (body is MemberExpression { Member: PropertyInfo property, Expression: ParameterExpression } && property.SetMethod?.IsPublic == true)
        {
            return property;
        }

        throw new ArgumentException("The selector must pick a public settable property of the destination directly, such as d => d.Name.", nameof(selector));
    }
    #endregion
}
