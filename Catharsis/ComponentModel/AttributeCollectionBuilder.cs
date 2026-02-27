using System.ComponentModel;

namespace Catharsis.ComponentModel;

/// <summary>
/// A fluent builder for composing <see cref="AttributeCollection"/> instances
/// with support for adding, removing, replacing, and merging attributes.
/// </summary>
/// <remarks>
/// <para>
/// When merging, attributes of the same type are replaced by the incoming
/// attribute (last-in wins). Use <see cref="Merge(AttributeCollection)"/>
/// to combine two collections.
/// </para>
/// <para>
/// Call <see cref="Build"/> to produce an immutable <see cref="AttributeCollection"/>.
/// The builder can be reused after building.
/// </para>
/// </remarks>
public sealed class AttributeCollectionBuilder
{
    private readonly Dictionary<Type, Attribute> _attributes = [];

    /// <summary>
    /// Initializes a new, empty <see cref="AttributeCollectionBuilder"/>.
    /// </summary>
    public AttributeCollectionBuilder() { }

    /// <summary>
    /// Initializes a new <see cref="AttributeCollectionBuilder"/> seeded
    /// with the attributes from the specified collection.
    /// </summary>
    /// <param name="existing">The collection to seed from.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="existing"/> is <c>null</c>.
    /// </exception>
    public AttributeCollectionBuilder(AttributeCollection existing)
    {
        ArgumentNullException.ThrowIfNull(existing);

        foreach (Attribute attr in existing)
        {
            _attributes[attr.GetType()] = attr;
        }
    }

    /// <summary>
    /// Gets the number of attributes currently in the builder.
    /// </summary>
    public int Count => _attributes.Count;

    /// <summary>
    /// Adds or replaces an attribute of the specified type.
    /// </summary>
    /// <param name="attribute">The attribute to add.</param>
    /// <returns>This instance, for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="attribute"/> is <c>null</c>.
    /// </exception>
    public AttributeCollectionBuilder Add(Attribute attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);
        _attributes[attribute.GetType()] = attribute;
        return this;
    }

    /// <summary>
    /// Adds or replaces multiple attributes.
    /// </summary>
    /// <param name="attributes">The attributes to add.</param>
    /// <returns>This instance, for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="attributes"/> is <c>null</c>.
    /// </exception>
    public AttributeCollectionBuilder AddRange(IEnumerable<Attribute> attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes);

        foreach (var attr in attributes)
        {
            _attributes[attr.GetType()] = attr;
        }

        return this;
    }

    /// <summary>
    /// Removes an attribute of the specified type.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to remove.</typeparam>
    /// <returns>This instance, for fluent chaining.</returns>
    public AttributeCollectionBuilder Remove<TAttribute>() where TAttribute : Attribute
    {
        _attributes.Remove(typeof(TAttribute));
        return this;
    }

    /// <summary>
    /// Removes an attribute of the specified type.
    /// </summary>
    /// <param name="attributeType">The attribute type to remove.</param>
    /// <returns>This instance, for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="attributeType"/> is <c>null</c>.
    /// </exception>
    public AttributeCollectionBuilder Remove(Type attributeType)
    {
        ArgumentNullException.ThrowIfNull(attributeType);
        _attributes.Remove(attributeType);
        return this;
    }

    /// <summary>
    /// Merges attributes from the specified collection into this builder.
    /// Attributes of the same type are replaced by the incoming attribute.
    /// </summary>
    /// <param name="other">The collection to merge from.</param>
    /// <returns>This instance, for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="other"/> is <c>null</c>.
    /// </exception>
    public AttributeCollectionBuilder Merge(AttributeCollection other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (Attribute attr in other)
        {
            _attributes[attr.GetType()] = attr;
        }

        return this;
    }

    /// <summary>
    /// Merges attributes from the specified array into this builder.
    /// Attributes of the same type are replaced by the incoming attribute.
    /// </summary>
    /// <param name="attributes">The attributes to merge.</param>
    /// <returns>This instance, for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="attributes"/> is <c>null</c>.
    /// </exception>
    public AttributeCollectionBuilder Merge(params Attribute[] attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes);

        foreach (var attr in attributes)
        {
            _attributes[attr.GetType()] = attr;
        }

        return this;
    }

    /// <summary>
    /// Gets a value indicating whether the builder contains an attribute
    /// of the specified type.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to check for.</typeparam>
    /// <returns><c>true</c> if the attribute is present; otherwise, <c>false</c>.</returns>
    public bool Contains<TAttribute>() where TAttribute : Attribute =>
        _attributes.ContainsKey(typeof(TAttribute));

    /// <summary>
    /// Removes all attributes from the builder.
    /// </summary>
    /// <returns>This instance, for fluent chaining.</returns>
    public AttributeCollectionBuilder Clear()
    {
        _attributes.Clear();
        return this;
    }

    /// <summary>
    /// Builds an immutable <see cref="AttributeCollection"/> from the
    /// current set of attributes.
    /// </summary>
    /// <returns>A new <see cref="AttributeCollection"/>.</returns>
    public AttributeCollection Build()
    {
        if (_attributes.Count == 0)
            return AttributeCollection.Empty;

        return new AttributeCollection([.. _attributes.Values]);
    }
}
