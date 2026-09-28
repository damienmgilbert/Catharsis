using Catharsis.ComponentModel;
using Catharsis.ComponentModel.Observability;
using Catharsis.ComponentModel.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Catharsis.Dynamic;

///<summary>
///A schema-less, self-validating, undoable dynamic object that ties together eight independent
///<see cref="Catharsis.ComponentModel"/> primitives that nobody currently combines: properties can be added at
///runtime (backed by an <see cref="ExpandoBackedBag{T}"/>), every mutation is change-tracked
///(<see cref="ChangeSet"/>) and undoable (<see cref="IChangeTrackable"/>), validation rules can be attached per
///property name and run through a <see cref="ValidationPipeline"/> before a value is committed, a point-in-time
///snapshot can be captured and restored, nested dotted paths (<c>"Address.City"</c>) can be read and written, and
///the whole thing can be exposed through <see cref="ICustomTypeDescriptor"/> for WinForms/WPF-style binding
///consumers via <see cref="DynamicTypeDescriptor"/>/<see cref="DynamicPropertyDescriptor"/>.
///</summary>
///<remarks>
///Nested paths are resolved by traversing into nested <see cref="FlexibleEntity"/> values directly, rather than via
///<see cref="PropertyPathResolver"/>: that type reflects over real CLR <see cref="System.Reflection.PropertyInfo"/>
///declarations, which do not exist for members added to an <see cref="ExpandoBackedBag{T}"/> at runtime. For the
///same reason, snapshotting uses a plain dictionary copy rather than <see cref="ComponentSnapshot{T}"/>, which is
///likewise reflection-based.
///</remarks>
public sealed class FlexibleEntity : IChangeTrackable
{
    #region Fields
    readonly ExpandoBackedBag<object?> _storage = new();
    readonly ChangeSet _changes = new();
    readonly Dictionary<string, ValidationPipeline> _validationPipelines = new(StringComparer.Ordinal);
    readonly ValidationContextFactory _validationContextFactory = new();
    #endregion

    #region Indexers
    ///<summary>
    ///Gets or sets the value at the specified dotted path. The setter throws if a validation rule registered for
    ///the target property rejects the value; use <see cref="Set"/> for a non-throwing alternative.
    ///</summary>
    ///<param name="path">A dotted property path, e.g. <c>"Address.City"</c>, or a single property name.</param>
    ///<exception cref="ArgumentException"><paramref name="path"/> is <c>null</c>, empty, or whitespace.</exception>
    ///<exception cref="ValidationException">On set, a registered validation rule rejected the value.</exception>
    public object? this[string path]
    {
        get => Get<object?>(path);
        set
        {
            if(!Set(path, value))
            {
                throw new ValidationException($"One or more validation rules rejected the value for '{path}'.");
            }
        }
    }
    #endregion

    #region Private methods
    object? GetRaw(string key) { return _storage.TryGetValue(key, out object? value) ? value : null; }

    void SetRawAndRecord(string key, object? value)
    {
        object? previous = GetRaw(key);
        _storage[key] = value;
        _changes.Record(key, previous, value);
    }

    void ApplyRaw(string key, object? value) { _storage[key] = value; }

    ///<summary>
    ///Resolves every segment but the last of a dotted path, auto-creating a nested <see cref="FlexibleEntity"/> for
    ///any missing intermediate segment.
    ///</summary>
    FlexibleEntity ResolveParent(string[] segments)
    {
        FlexibleEntity target = this;

        for(int index = 0; index < (segments.Length - 1); index++)
        {
            object? next = target.GetRaw(segments[index]);

            if(next is null)
            {
                FlexibleEntity created = new();
                target.SetRawAndRecord(segments[index], created);
                next = created;
            }

            if(next is not FlexibleEntity nested)
            {
                throw new InvalidOperationException($"Cannot traverse into '{segments[index]}': the value at that point is not a {nameof(FlexibleEntity)}.");
            }

            target = nested;
        }

        return target;
    }

    bool SetLeaf(string propertyName, object? value)
    {
        if(_validationPipelines.TryGetValue(propertyName, out ValidationPipeline? pipeline))
        {
            ValidationContext context = _validationContextFactory.CreatePropertyContext(this, propertyName);
            ValidationResultAggregator aggregator = pipeline.Execute(value, context);

            if(aggregator.HasErrors)
            {
                Errors.SetErrors(propertyName, [.. aggregator.GetAll().Select(entry => new ErrorInfo(entry.Result.ErrorMessage ?? "Validation failed.", entry.Severity, propertyName))]);
                return false;
            }

            Errors.ClearErrors(propertyName);
        }

        SetRawAndRecord(propertyName, value);
        return true;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Registers a validation rule for the specified top-level property name. The rule runs on every subsequent
    ///<see cref="Set"/> call for that property before the value is committed.
    ///</summary>
    ///<param name="propertyName">The property name to validate.</param>
    ///<param name="rule">The validation rule to register.</param>
    ///<returns>This entity, for fluent chaining.</returns>
    ///<exception cref="ArgumentException"><paramref name="propertyName"/> is <c>null</c>, empty, or whitespace.</exception>
    ///<exception cref="ArgumentNullException"><paramref name="rule"/> is <c>null</c>.</exception>
    public FlexibleEntity AddValidationRule(string propertyName, IValidationRule rule)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        ArgumentNullException.ThrowIfNull(rule);

        if(!_validationPipelines.TryGetValue(propertyName, out ValidationPipeline? pipeline))
        {
            pipeline = new ValidationPipeline();
            _validationPipelines[propertyName] = pipeline;
        }

        pipeline.AddRule(rule);
        return this;
    }

    ///<summary>
    ///Captures a shallow copy of every current top-level property value.
    ///</summary>
    ///<returns>A snapshot that can later be passed to <see cref="RestoreSnapshot"/>.</returns>
    public IReadOnlyDictionary<string, object?> CreateSnapshot() { return _storage.ToDictionary(static entry => entry.Key, static entry => entry.Value, StringComparer.Ordinal); }

    ///<summary>
    ///Exposes this entity's current top-level properties through <see cref="ICustomTypeDescriptor"/>, for
    ///WinForms/WPF-style binding consumers that discover properties via <see cref="TypeDescriptor"/>. The returned
    ///descriptor is a snapshot of the properties present at the time of the call.
    ///</summary>
    ///<returns>A type descriptor exposing one property per current top-level key.</returns>
    public ICustomTypeDescriptor AsTypeDescriptor()
    {
        DynamicTypeDescriptor descriptor = new();

        foreach(string propertyName in PropertyNames)
        {
            string capturedName = propertyName;
            descriptor.AddProperty(new DynamicPropertyDescriptor(capturedName, typeof(object), typeof(FlexibleEntity), getter: component => ((FlexibleEntity)component).Get<object?>(capturedName), setter: (component, value) => ((FlexibleEntity)component).Set(capturedName, value)));
        }

        return descriptor;
    }

    ///<summary>
    ///Resolves the value at the specified dotted path.
    ///</summary>
    ///<typeparam name="T">The expected value type.</typeparam>
    ///<param name="path">A dotted property path, e.g. <c>"Address.City"</c>, or a single property name.</param>
    ///<returns>The resolved value, or <c>default</c> if the path does not resolve to an existing value.</returns>
    ///<exception cref="ArgumentException"><paramref name="path"/> is <c>null</c>, empty, or whitespace.</exception>
    ///<exception cref="InvalidOperationException">A non-final segment resolves to a value that is not a <see cref="FlexibleEntity"/>.</exception>
    public T? Get<T>(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string[] segments = path.Split('.');
        FlexibleEntity parent = ResolveParent(segments);
        object? raw = parent.GetRaw(segments[^1]);

        return raw is T typed ? typed : default;
    }

    ///<summary>
    ///Removes every validation rule registered for the specified property name.
    ///</summary>
    ///<param name="propertyName">The property name to clear rules for.</param>
    ///<returns><c>true</c> if any rules were registered for that property; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentException"><paramref name="propertyName"/> is <c>null</c>, empty, or whitespace.</exception>
    public bool RemoveValidationRules(string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        return _validationPipelines.Remove(propertyName);
    }

    ///<summary>
    ///Replaces every top-level property with the values from a previously captured snapshot, clearing change
    ///history in the process (the restored state becomes the new undo/redo baseline).
    ///</summary>
    ///<param name="snapshot">A snapshot previously returned by <see cref="CreateSnapshot"/>.</param>
    ///<exception cref="ArgumentNullException"><paramref name="snapshot"/> is <c>null</c>.</exception>
    public void RestoreSnapshot(IReadOnlyDictionary<string, object?> snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        foreach(string propertyName in PropertyNames)
        {
            _storage.Remove(propertyName);
        }

        foreach((string propertyName, object? value) in snapshot)
        {
            ApplyRaw(propertyName, value);
        }

        _changes.Clear();
    }

    ///<summary>
    ///Sets the value at the specified dotted path, running any validation rules registered for the target property
    ///first. Missing intermediate segments are auto-created as nested <see cref="FlexibleEntity"/> instances.
    ///</summary>
    ///<param name="path">A dotted property path, e.g. <c>"Address.City"</c>, or a single property name.</param>
    ///<param name="value">The value to assign.</param>
    ///<returns><c>true</c> if the value was committed; <c>false</c> if a validation rule rejected it.</returns>
    ///<exception cref="ArgumentException"><paramref name="path"/> is <c>null</c>, empty, or whitespace.</exception>
    ///<exception cref="InvalidOperationException">A non-final segment resolves to a value that is not a <see cref="FlexibleEntity"/>.</exception>
    public bool Set(string path, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string[] segments = path.Split('.');
        FlexibleEntity parent = ResolveParent(segments);
        return parent.SetLeaf(segments[^1], value);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Exposes this entity's storage as a <c>dynamic</c> value, so its top-level entries can also be accessed as
    ///<c>((dynamic)entity.AsDynamic).PropertyName</c>.
    ///</summary>
    public dynamic AsDynamic => _storage.AsDynamic;

    ///<summary>
    ///The validation errors currently recorded against this entity's properties.
    ///</summary>
    public ErrorDictionary Errors { get; } = new();

    ///<summary>
    ///The names of every current top-level property.
    ///</summary>
    public IReadOnlyList<string> PropertyNames => [.. _storage.Select(static entry => entry.Key)];
    #endregion

    #region IChangeTrackable
    ///<inheritdoc/>
    public bool CanRedo => _changes.CanRedo;

    ///<inheritdoc/>
    public bool CanUndo => _changes.CanUndo;

    ///<inheritdoc/>
    public ChangeSet Changes => _changes;

    ///<inheritdoc/>
    public bool IsChanged => _changes.HasChanges;

    ///<inheritdoc/>
    public void AcceptChanges() { _changes.AcceptAll(); }

    ///<inheritdoc/>
    public ChangeEntry? Redo()
    {
        ChangeEntry? entry = _changes.Redo();

        if(entry is not null)
        {
            ApplyRaw(entry.PropertyName, entry.NewValue);
        }

        return entry;
    }

    ///<inheritdoc/>
    public void RejectChanges()
    {
        IReadOnlyList<ChangeEntry> entries = _changes.GetAll();

        for(int index = entries.Count - 1; index >= 0; index--)
        {
            ApplyRaw(entries[index].PropertyName, entries[index].OldValue);
        }

        _changes.Clear();
    }

    ///<inheritdoc/>
    public ChangeEntry? Undo()
    {
        ChangeEntry? entry = _changes.Undo();

        if(entry is not null)
        {
            ApplyRaw(entry.PropertyName, entry.OldValue);
        }

        return entry;
    }
    #endregion
}
