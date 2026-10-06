using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Catharsis.ComponentModel;
using Catharsis.ComponentModel.Observability;
using Catharsis.ComponentModel.Validation;

namespace Catharsis.Dynamic;

///<summary>
///A schema-less, self-validating, undoable dynamic object that ties together eight independent ///<see
///cref="Catharsis.ComponentModel"/> primitives that nobody currently combines: properties can be added at runtime
///(backed by an <see cref="ExpandoBackedBag{T}"/>), every mutation is change-tracked (<see cref="ChangeSet"/>) and
///undoable (<see cref="IChangeTrackable"/>), validation rules can be attached per property name and run through a <see
///cref="ValidationPipeline"/> before a value is committed, a point-in-time snapshot can be captured and restored,
///nested dotted paths (<c>"Address.City"</c>) can be read and written, and the whole thing can be exposed through <see
///cref="ICustomTypeDescriptor"/> for WinForms/WPF-style binding consumers via <see cref="DynamicTypeDescriptor"/>/<see
///cref="DynamicPropertyDescriptor"/>.
///</summary>
///<remarks>
public sealed class FlexibleEntity : IChangeTrackable
{
    #region Fields
    private readonly ChangeSet _changeSetStorage = new();
    private readonly string _pathPrefix;
    private readonly FlexibleEntity _root;
    private readonly ExpandoBackedBag<object?> _storage = new();
    private readonly ValidationContextFactory _validationContextFactory = new();
    private readonly Dictionary<string, ValidationPipeline> _validationPipelines = new(StringComparer.Ordinal);
    #endregion

    #region Constructors
    ///<summary>
    ///Creates an entity that shares <paramref name="root"/>'s change history, recording its own mutations under
    private FlexibleEntity(FlexibleEntity root, string pathPrefix)
    {
        _root = root;
        _pathPrefix = pathPrefix;
    }

        ///<summary>
///Creates an empty, root-level entity.
///</summary>
    public FlexibleEntity()
    {
        _root = this;
        _pathPrefix = string.Empty;
    }
    #endregion

    #region Indexers
    ///<summary>
    ///Gets or sets the value at the specified dotted path. The setter throws if a validation rule registered for the
    ///target property rejects the value; use <see cref="Set"/> for a non-throwing alternative.
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
    private void ApplyRaw(string key, object? value) => _storage[key] = value;

    ///<summary>
    ///Applies a previously recorded change back onto whichever entity in the tree owns <paramref name="path"/>, used by
    ///<see cref="Undo"/>/<see cref="Redo"/>/<see cref="RejectChanges"/> to replay a root-relative, dotted-path change
    ///entry. Called on the root entity. If the tree no longer matches the recorded path (e.g. an intermediate segment
    ///was since replaced with a non-entity value), the entry is silently skipped rather than throwing, since undo/redo
    ///is a best-effort replay of history, not a fresh mutation.
    ///</summary>
    private void ApplyRawAtPath(string path, object? value)
    {
        string[] segments = path.Split('.');
        FlexibleEntity target = this;

        for(int index = 0; index < (segments.Length - 1); index++)
        {
            if(target.GetRaw(segments[index]) is not FlexibleEntity nested)
            {
                return;
            }

            target = nested;
        }

        target.ApplyRaw(segments[^1], value);
    }

    private string FullPath(string key) => (_pathPrefix.Length == 0) ? key : $"{_pathPrefix}.{key}";

    private object? GetRaw(string key) => _storage.TryGetValue(key, out object? value) ? value : null;

    ///<summary>
    ///Resolves every segment but the last of a dotted path, auto-creating a nested <see cref="FlexibleEntity"/> for any
    ///missing intermediate segment. Used by <see cref="Set"/>, which is allowed to create structure; contrast with <see
    ///cref="TryResolveParentForRead"/>, used by <see cref="Get{T}"/>, which is not.
    ///</summary>
    private FlexibleEntity ResolveParent(string[] segments)
    {
        FlexibleEntity target = this;

        for(int index = 0; index < (segments.Length - 1); index++)
        {
            object? next = target.GetRaw(segments[index]);

            if(next is null)
            {
                FlexibleEntity created = new(target._root, target.FullPath(segments[index]));
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

    private bool SetLeaf(string propertyName, object? value)
    {
        if(_validationPipelines.TryGetValue(propertyName, out ValidationPipeline? pipeline))
        {
            ValidationContext context = _validationContextFactory.CreatePropertyContext(this, propertyName);
            ValidationResultAggregator aggregator = pipeline.Execute(value, context);

            if(aggregator.HasErrors)
            {
                Errors.SetErrors(propertyName, [ .. aggregator.GetAll().Select(entry => new ErrorInfo(entry.Result.ErrorMessage ?? "Validation failed.", entry.Severity, propertyName)) ]);
                return false;
            }

            Errors.ClearErrors(propertyName);
        }

        SetRawAndRecord(propertyName, value);
        return true;
    }

    private void SetRawAndRecord(string key, object? value)
    {
        object? previous = GetRaw(key);
        _storage[key] = value;
        _root._changeSetStorage.Record(FullPath(key), previous, value);
    }

    ///<summary>
    ///Resolves every segment but the last of a dotted path without creating anything: a missing or non-entity
    ///intermediate segment means the path does not resolve.
    ///</summary>
    private FlexibleEntity? TryResolveParentForRead(string[] segments)
    {
        FlexibleEntity target = this;

        for(int index = 0; index < (segments.Length - 1); index++)
        {
            if(target.GetRaw(segments[index]) is not FlexibleEntity nested)
            {
                return null;
            }

            target = nested;
        }

        return target;
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public void AcceptChanges() => _root._changeSetStorage.AcceptAll();

    ///<summary>
    ///Registers a validation rule for the specified top-level property name. The rule runs on every subsequent ///<see
    ///cref="Set"/> call for that property before the value is committed.
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
    ///Exposes this entity's current top-level properties through <see cref="ICustomTypeDescriptor"/>, for WinForms/WPF-
    ///style binding consumers that discover properties via <see cref="TypeDescriptor"/>. The returned descriptor is a
    ///snapshot of the properties present at the time of the call.
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
    ///Captures a shallow copy of every current top-level property value.
    ///</summary>
    ///<returns>A snapshot that can later be passed to <see cref="RestoreSnapshot"/>.</returns>
    public IReadOnlyDictionary<string, object?> CreateSnapshot() => _storage.ToDictionary(static entry => entry.Key, static entry => entry.Value, StringComparer.Ordinal);

    ///<summary>
    ///Resolves the value at the specified dotted path. This is a pure read: unlike <see cref="Set"/>, it never creates
    ///a missing intermediate segment, so it has no effect on <see cref="Changes"/>.
    ///</summary>
    ///<typeparam name="T">The expected value type.</typeparam>
    ///<param name="path">A dotted property path, e.g. <c>"Address.City"</c>, or a single property name.</param>
    ///<returns>
    ///The resolved value, or <c>default</c> if the path does not resolve to an existing value of type ///<typeparamref
    ///name="T"/> — including when a non-final segment is missing or is not a <see cref="FlexibleEntity"/>.
    ///</returns>
    ///<exception cref="ArgumentException"><paramref name="path"/> is <c>null</c>, empty, or whitespace.</exception>
    public T? Get<T>(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string[] segments = path.Split('.');
        FlexibleEntity? parent = TryResolveParentForRead(segments);

        if(parent is null)
        {
            return default;
        }

        object? raw = parent.GetRaw(segments[^1]);
        return raw is T typed ? typed : default;
    }

    ///<inheritdoc/>
    public ChangeEntry? Redo()
    {
        ChangeEntry? entry = _root._changeSetStorage.Redo();

        if(entry is not null)
        {
            _root.ApplyRawAtPath(entry.PropertyName, entry.NewValue);
        }

        return entry;
    }

    ///<inheritdoc/>
    public void RejectChanges()
    {
        IReadOnlyList<ChangeEntry> entries = _root._changeSetStorage.GetAll();

        for(int index = entries.Count - 1; index >= 0; index--)
        {
            _root.ApplyRawAtPath(entries[index].PropertyName, entries[index].OldValue);
        }

        _root._changeSetStorage.Clear();
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
    ///Replaces every top-level property with the values from a previously captured snapshot, clearing this entity's
    ///whole tree's change history in the process (the restored state becomes the new undo/redo baseline).
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

        _root._changeSetStorage.Clear();
    }

    ///<summary>
    ///Sets the value at the specified dotted path, running any validation rules registered for the target property
    ///first. Missing intermediate segments are auto-created as nested <see cref="FlexibleEntity"/> instances that share
    ///this entity's root change history.
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

    ///<inheritdoc/>
    public ChangeEntry? Undo()
    {
        ChangeEntry? entry = _root._changeSetStorage.Undo();

        if(entry is not null)
        {
            _root.ApplyRawAtPath(entry.PropertyName, entry.OldValue);
        }

        return entry;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Exposes this entity's storage as a <c>dynamic</c> value, so its top-level entries can also be accessed as
    public dynamic AsDynamic => _storage.AsDynamic;

        ///<inheritdoc/>
    public bool CanRedo => _root._changeSetStorage.CanRedo;

    ///<inheritdoc/>
    public bool CanUndo => _root._changeSetStorage.CanUndo;

    ///<inheritdoc/>
    public ChangeSet Changes => _root._changeSetStorage;

    ///<summary>
    ///The validation errors currently recorded against this entity's properties.
    ///</summary>
    public ErrorDictionary Errors { get; } = new();

    ///<inheritdoc/>
    public bool IsChanged => _root._changeSetStorage.HasChanges;

    ///<summary>
    ///The names of every current top-level property.
    ///</summary>
    public IReadOnlyList<string> PropertyNames => [ .. _storage.Select(static entry => entry.Key) ];
    #endregion
}
