using Catharsis.ComponentModel;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

namespace Catharsis.Contracts;

///<summary>
///Reads custom attributes through reflection and caches the result per member, so hot paths that repeatedly ask the
///same question ("which properties carry this attribute?") pay for reflection once. Raw attribute reads are cached
///here; <c>TypeDescriptor</c> property data (which includes attributes added through type descriptors) is delegated to
///a shared <see cref="ComponentReflectionCache"/>.
///</summary>
///<param name="componentCache">
///The cache used for <c>TypeDescriptor</c>-based lookups. Pass a shared instance so the same descriptors are not
///reflected twice; a private one is created when <c>null</c>.
///</param>
public sealed class TypeMetadataReader(ComponentReflectionCache? componentCache = null)
{
    #region Fields
    readonly ConcurrentDictionary<(MemberInfo Member, Type Attribute, bool Inherit), Attribute[]> _cache = new();
    readonly ComponentReflectionCache _componentCache = componentCache ?? new ComponentReflectionCache();
    #endregion

    #region Public methods
    ///<summary>
    ///Gets the single attribute of type <typeparamref name="TAttribute"/> on <paramref name="member"/>.
    ///</summary>
    ///<typeparam name="TAttribute">The attribute type.</typeparam>
    ///<param name="member">The member to inspect.</param>
    ///<param name="inherit">Whether to include attributes inherited from base members.</param>
    ///<returns>The attribute, or <c>null</c> if the member has none.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="member"/> is <c>null</c>.</exception>
    ///<exception cref="AmbiguousMatchException">The member has more than one such attribute.</exception>
    public TAttribute? GetAttribute<TAttribute>(MemberInfo member, bool inherit = true)
        where TAttribute : Attribute
    {
        IReadOnlyList<TAttribute> all = GetAttributes<TAttribute>(member, inherit);

        return all.Count switch
        {
            0 => null,
            1 => all[0],
            _ => throw new AmbiguousMatchException($"{member.Name} has {all.Count} {typeof(TAttribute).Name} attributes.")
        };
    }

    ///<summary>
    ///Gets every attribute of type <typeparamref name="TAttribute"/> on <paramref name="member"/>.
    ///</summary>
    ///<typeparam name="TAttribute">The attribute type.</typeparam>
    ///<param name="member">The member to inspect.</param>
    ///<param name="inherit">Whether to include attributes inherited from base members.</param>
    ///<returns>The attributes, possibly empty.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="member"/> is <c>null</c>.</exception>
    public IReadOnlyList<TAttribute> GetAttributes<TAttribute>(MemberInfo member, bool inherit = true)
        where TAttribute : Attribute
    {
        ArgumentNullException.ThrowIfNull(member);

        Attribute[] cached = _cache.GetOrAdd((member, typeof(TAttribute), inherit), static key => [.. key.Member.GetCustomAttributes(key.Attribute, key.Inherit).Cast<Attribute>()]);

        return [.. cached.Cast<TAttribute>()];
    }

    ///<summary>
    ///Gets every public instance property of <paramref name="type"/> that carries <typeparamref name="TAttribute"/>,
    ///paired with that attribute.
    ///</summary>
    ///<typeparam name="TAttribute">The attribute type.</typeparam>
    ///<param name="type">The type whose properties are inspected.</param>
    ///<returns>The annotated properties in declaration order.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
    public IReadOnlyList<(PropertyInfo Property, TAttribute Attribute)> GetAnnotatedProperties<TAttribute>(Type type)
        where TAttribute : Attribute
    {
        ArgumentNullException.ThrowIfNull(type);

        List<(PropertyInfo, TAttribute)> result = [];

        foreach(PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            foreach(TAttribute attribute in GetAttributes<TAttribute>(property))
            {
                result.Add((property, attribute));
            }
        }

        return result;
    }

    ///<summary>
    ///Gets every property of <paramref name="type"/> whose <see cref="TypeDescriptor"/> metadata carries
    ///<typeparamref name="TAttribute"/>, paired with that attribute. Unlike
    ///<see cref="GetAnnotatedProperties{TAttribute}(Type)"/> this sees attributes added through type descriptors, and
    ///the descriptors come from the shared <see cref="ComponentReflectionCache"/>.
    ///</summary>
    ///<remarks>
    ///<see cref="TypeDescriptor"/> keeps only one attribute per <see cref="Attribute.TypeId"/>, so a repeated
    ///<c>AllowMultiple</c> attribute collapses to a single instance here unless it overrides <c>TypeId</c>. Use
    ///<see cref="GetAnnotatedProperties{TAttribute}(Type)"/> when every repetition matters.
    ///</remarks>
    ///<typeparam name="TAttribute">The attribute type.</typeparam>
    ///<param name="type">The type whose properties are inspected.</param>
    ///<returns>The annotated property descriptors.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
    public IReadOnlyList<(PropertyDescriptor Property, TAttribute Attribute)> GetAnnotatedDescriptors<TAttribute>(Type type)
        where TAttribute : Attribute
    {
        ArgumentNullException.ThrowIfNull(type);

        List<(PropertyDescriptor, TAttribute)> result = [];

        foreach(PropertyDescriptor descriptor in _componentCache.GetProperties(type))
        {
            foreach(TAttribute attribute in descriptor.Attributes.OfType<TAttribute>())
            {
                result.Add((descriptor, attribute));
            }
        }

        return result;
    }

    ///<summary>
    ///Discards every cached result, including the shared component cache's.
    ///</summary>
    public void Clear()
    {
        _cache.Clear();
        _componentCache.Clear();
    }
    #endregion

    #region Public properties
    ///<summary>Gets the number of cached lookups.</summary>
    public int CachedCount => _cache.Count;
    #endregion
}
