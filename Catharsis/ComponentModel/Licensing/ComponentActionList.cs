using System.Collections;

namespace Catharsis.ComponentModel.Licensing;

///<summary>
///A collection of <see cref="ComponentVerb"/> instances that a component designer exposes as design-time actions,
///analogous to <c>DesignerActionList</c> in the Windows Forms designer infrastructure.
///</summary>
public class ComponentActionList : IReadOnlyList<ComponentVerb>
{
    #region Fields
    private readonly List<ComponentVerb> _verbs = [];
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="ComponentActionList"/> with the specified design context.
    ///</summary>
    ///<param name="context">The design context for the component.</param>
    ///<exception cref="ArgumentNullException">
    public ComponentActionList(ComponentDesignContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        Context = context;
    }
    #endregion

    #region Indexers
    ///<summary>
    ///Gets the verb at the specified index.
    ///</summary>
    ///<param name="index">The zero-based index.</param>
    ///<returns>The verb at the specified index.</returns>
    public ComponentVerb this[int index] => _verbs[index];
    #endregion

    #region Explicit interface implementations
    ///<inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    #region Protected methods
    ///<summary>
    ///Adds a verb to the action list.
    ///</summary>
    ///<param name="verb">The verb to add.</param>
    ///<exception cref="ArgumentNullException">
    protected void AddVerb(ComponentVerb verb)
    {
        ArgumentNullException.ThrowIfNull(verb);
        _verbs.Add(verb);
    }

    ///<summary>
    ///Adds a verb with the specified text and action.
    ///</summary>
    ///<param name="text">The display text.</param>
    ///<param name="action">The action to execute.</param>
    ///<param name="description">An optional description.</param>
    protected void AddVerb(string text, Action action, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(action);
        _verbs.Add(new ComponentVerb(text, action, description));
    }

    ///<summary>
    ///Removes all verbs from the action list.
    ///</summary>
    protected void ClearVerbs() => _verbs.Clear();
    #endregion

    #region Protected properties
    ///<summary>
    ///Gets the design context for the component.
    ///</summary>
    protected ComponentDesignContext Context { get; }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public IEnumerator<ComponentVerb> GetEnumerator() => _verbs.GetEnumerator();
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of verbs in this list.
    ///</summary>
    public int Count => _verbs.Count;
    #endregion
}
