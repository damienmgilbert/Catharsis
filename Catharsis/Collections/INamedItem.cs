namespace Catharsis.Collections;

///<summary>
///An item that can supply its own unique name, used as the key for a <see cref="NamedItemCollection{T}"/>.
///</summary>
public interface INamedItem
{
    #region Public properties

    ///<summary>
    ///The item's unique name.
    ///</summary>
    string Name { get; }
    #endregion
}
