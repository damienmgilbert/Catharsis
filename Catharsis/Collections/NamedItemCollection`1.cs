using System.Collections.ObjectModel;

namespace Catharsis.Collections;

///<summary>
///A <see cref="KeyedCollection{TKey,TItem}"/> that derives each item's key from its own <see cref="INamedItem.Name"/>.
///</summary>
///<typeparam name="T">The type of items stored in the collection.</typeparam>
///<param name="comparer">The comparer used to compare item names. Defaults to <see cref="StringComparer.Ordinal"/>.</param>
public sealed class NamedItemCollection<T>(IEqualityComparer<string>? comparer = null) : KeyedCollection<string, T>(comparer ?? StringComparer.Ordinal) where T : INamedItem
{
    #region Protected methods

    ///<inheritdoc/>
    protected override string GetKeyForItem(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return item.Name;
    }
    #endregion
}
