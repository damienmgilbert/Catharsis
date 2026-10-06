using System.ComponentModel;

namespace Catharsis.ComponentModel.Observability;

///<summary>
///Defines a contract for objects that support undo/redo-friendly change tracking with named property changes and a
///queryable change history.
///</summary>
///<remarks>
///Extends <see cref="IRevertibleChangeTracking"/> with the ability to retrieve the full <see cref="ChangeSet"/> and
///undo/redo individual changes.
///</remarks>
public interface IChangeTrackable : IRevertibleChangeTracking
{
    #region Public methods
    ///<summary>
    ///Redoes the most recently undone change.
    ///</summary>
    ///<returns>
    ///The <see cref="ChangeEntry"/> that was redone, or <c>null</c> if there was nothing to redo.
    ///</returns>
    ChangeEntry? Redo();

    ///<summary>
    ///Undoes the most recent change, restoring the previous property value.
    ///</summary>
    ///<returns>
    ///The <see cref="ChangeEntry"/> that was undone, or <c>null</c> if there was nothing to undo.
    ///</returns>
    ChangeEntry? Undo();
    #endregion

    #region Public properties
    ///<summary>
    ///Gets a value indicating whether there are changes that can be redone.
    ///</summary>
    bool CanRedo { get; }

    ///<summary>
    ///Gets a value indicating whether there are changes that can be undone.
    ///</summary>
    bool CanUndo { get; }

    ///<summary>
    ///Gets the current set of uncommitted changes.
    ///</summary>
    ChangeSet Changes { get; }
    #endregion
}
