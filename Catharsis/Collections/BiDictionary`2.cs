using System.Diagnostics.CodeAnalysis;

namespace Catharsis.Collections;

///<summary>
///A two-way dictionary that maps left values to right values and right values back to left values, with both
///directions kept in sync. Each value may appear at most once on its side.
///</summary>
///<typeparam name="TLeft">The type of the left-side values.</typeparam>
///<typeparam name="TRight">The type of the right-side values.</typeparam>
///<param name="leftComparer">The equality comparer used to match left values, or <c>null</c> to use the default comparer.</param>
///<param name="rightComparer">The equality comparer used to match right values, or <c>null</c> to use the default comparer.</param>
public sealed class BiDictionary<TLeft, TRight>(IEqualityComparer<TLeft>? leftComparer = null, IEqualityComparer<TRight>? rightComparer = null)
    where TLeft : notnull where TRight : notnull
{
    #region Fields
    readonly Dictionary<TLeft, TRight> _forward = new(leftComparer);
    readonly Dictionary<TRight, TLeft> _reverse = new(rightComparer);
    #endregion

    #region Public methods
    ///<summary>
    ///Adds a left/right pair. Neither value may already be present on its side.
    ///</summary>
    ///<param name="left">The left value.</param>
    ///<param name="right">The right value.</param>
    ///<exception cref="ArgumentNullException"><paramref name="left"/> or <paramref name="right"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException">Either value is already present on its side.</exception>
    public void Add(TLeft left, TRight right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        if (_forward.ContainsKey(left))
        {
            throw new ArgumentException("The left value is already present.", nameof(left));
        }

        if (_reverse.ContainsKey(right))
        {
            throw new ArgumentException("The right value is already present.", nameof(right));
        }

        _forward.Add(left, right);
        _reverse.Add(right, left);
    }

    ///<summary>
    ///Removes all entries from the dictionary.
    ///</summary>
    public void Clear()
    {
        _forward.Clear();
        _reverse.Clear();
    }

    ///<summary>
    ///Determines whether the specified left value is present.
    ///</summary>
    ///<param name="left">The left value to look for.</param>
    ///<returns><c>true</c> if the value exists; otherwise <c>false</c>.</returns>
    public bool ContainsLeft(TLeft left) => _forward.ContainsKey(left);

    ///<summary>
    ///Determines whether the specified right value is present.
    ///</summary>
    ///<param name="right">The right value to look for.</param>
    ///<returns><c>true</c> if the value exists; otherwise <c>false</c>.</returns>
    public bool ContainsRight(TRight right) => _reverse.ContainsKey(right);

    ///<summary>
    ///Removes the entry with the specified left value.
    ///</summary>
    ///<param name="left">The left value to remove.</param>
    ///<returns><c>true</c> if the entry was found and removed; otherwise <c>false</c>.</returns>
    public bool RemoveByLeft(TLeft left)
    {
        if (!_forward.TryGetValue(left, out TRight? right))
        {
            return false;
        }

        _forward.Remove(left);
        _reverse.Remove(right);
        return true;
    }

    ///<summary>
    ///Removes the entry with the specified right value.
    ///</summary>
    ///<param name="right">The right value to remove.</param>
    ///<returns><c>true</c> if the entry was found and removed; otherwise <c>false</c>.</returns>
    public bool RemoveByRight(TRight right)
    {
        if (!_reverse.TryGetValue(right, out TLeft? left))
        {
            return false;
        }

        _reverse.Remove(right);
        _forward.Remove(left);
        return true;
    }

    ///<summary>
    ///Attempts to retrieve the right value associated with the specified left value.
    ///</summary>
    ///<param name="left">The left value to look up.</param>
    ///<param name="right">The associated right value, if found.</param>
    ///<returns><c>true</c> if the left value was found; otherwise <c>false</c>.</returns>
    public bool TryGetByLeft(TLeft left, [MaybeNullWhen(false)] out TRight right) => _forward.TryGetValue(left, out right);

    ///<summary>
    ///Attempts to retrieve the left value associated with the specified right value.
    ///</summary>
    ///<param name="right">The right value to look up.</param>
    ///<param name="left">The associated left value, if found.</param>
    ///<returns><c>true</c> if the right value was found; otherwise <c>false</c>.</returns>
    public bool TryGetByRight(TRight right, [MaybeNullWhen(false)] out TLeft left) => _reverse.TryGetValue(right, out left);
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of pairs in the dictionary.
    ///</summary>
    public int Count => _forward.Count;
    #endregion
}
