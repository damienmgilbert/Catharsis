namespace Catharsis.Extensions;

///<summary>
///Provides extension methods for <see cref="ICollection{T}"/> and <see cref="IList{T}"/> to add, remove, and modify
///elements or ranges of elements.
///</summary>
public static class CollectionExtensions
{
    #region Public methods
    ///<summary>
    ///Adds all elements from <paramref name="items"/> to the <paramref name="source"/> collection.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target collection.</param>
    ///<param name="items">The items to add.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    public static ICollection<T> AddRange<T>(this ICollection<T> source, IEnumerable<T> items)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source collection must not be null.");
        }

        if(items is null)
        {
            throw new ArgumentNullException(nameof(items), "Items must not be null.");
        }

        foreach(T item in items)
        {
            source.Add(item);
        }

        return source;
    }

    ///<summary>
    ///Inserts all elements from <paramref name="items"/> into the list starting at <paramref name="index"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target list.</param>
    ///<param name="index">The zero-based index at which to begin inserting.</param>
    ///<param name="items">The items to insert.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the valid insert range.</exception>
    public static IList<T> InsertRange<T>(this IList<T> source, int index, IEnumerable<T> items)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source list must not be null.");
        }

        if(items is null)
        {
            throw new ArgumentNullException(nameof(items), "Items must not be null.");
        }

        if((index < 0) || (index > source.Count))
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is outside the valid insert range.");
        }

        int offset = 0;
        foreach(T item in items)
        {
            source.Insert(index + offset, item);
            offset++;
        }

        return source;
    }

    ///<summary>
    ///Modifies all elements in the list by applying <paramref name="modifier"/> to each.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target list.</param>
    ///<param name="modifier">A function that transforms each element.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="modifier"/> is <c>null</c>.</exception>
    public static IList<T> ModifyAll<T>(this IList<T> source, Func<T, T> modifier)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source list must not be null.");
        }

        if(modifier is null)
        {
            throw new ArgumentNullException(nameof(modifier), "Modifier function must not be null.");
        }

        for(int i = 0; i < source.Count; i++)
        {
            source[i] = modifier(source[i]);
        }

        return source;
    }

    ///<summary>
    ///Modifies elements matching <paramref name="predicate"/> by applying <paramref name="modifier"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target list.</param>
    ///<param name="predicate">A function that returns <c>true</c> for elements to modify.</param>
    ///<param name="modifier">A function that transforms matching elements.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="predicate"/>, or <paramref name="modifier"/> is <c>null</c>.</exception>
    public static IList<T> ModifyWhere<T>(this IList<T> source, Func<T, bool> predicate, Func<T, T> modifier)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source list must not be null.");
        }

        if(predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate), "Predicate must not be null.");
        }

        if(modifier is null)
        {
            throw new ArgumentNullException(nameof(modifier), "Modifier function must not be null.");
        }

        for(int i = 0; i < source.Count; i++)
        {
            if(predicate(source[i]))
            {
                source[i] = modifier(source[i]);
            }
        }

        return source;
    }

    ///<summary>
    ///Removes all elements from <paramref name="items"/> from the <paramref name="source"/> collection.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target collection.</param>
    ///<param name="items">The items to remove.</param>
    ///<returns>The number of items successfully removed.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    public static int RemoveRange<T>(this ICollection<T> source, IEnumerable<T> items)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source collection must not be null.");
        }

        if(items is null)
        {
            throw new ArgumentNullException(nameof(items), "Items must not be null.");
        }

        int removed = 0;
        foreach(T item in items)
        {
            if(source.Remove(item))
            {
                removed++;
            }
        }

        return removed;
    }

    ///<summary>
    ///Removes a range of elements from the list starting at <paramref name="index"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target list.</param>
    ///<param name="index">The zero-based starting index of the range to remove.</param>
    ///<param name="count">The number of elements to remove.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="index"/> or <paramref name="count"/> is invalid.</exception>
    public static IList<T> RemoveRange<T>(this IList<T> source, int index, int count)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source list must not be null.");
        }

        if(index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index must not be negative.");
        }

        if(count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must not be negative.");
        }

        if(index + count > source.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Range extends beyond the list bounds.");
        }

        for(int i = 0; i < count; i++)
        {
            source.RemoveAt(index);
        }

        return source;
    }

    ///<summary>
    ///Removes all elements matching <paramref name="predicate"/> from the <paramref name="source"/> collection.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target collection.</param>
    ///<param name="predicate">A function that returns <c>true</c> for elements to remove.</param>
    ///<returns>The number of items removed.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static int RemoveWhere<T>(this ICollection<T> source, Func<T, bool> predicate)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source collection must not be null.");
        }

        if(predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate), "Predicate must not be null.");
        }

        List<T> toRemove = source.Where(predicate).ToList();
        int removed = 0;
        foreach(T item in toRemove)
        {
            if(source.Remove(item))
            {
                removed++;
            }
        }

        return removed;
    }

    ///<summary>
    ///Replaces all occurrences of <paramref name="oldItem"/> with <paramref name="newItem"/> in the list.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target list.</param>
    ///<param name="oldItem">The item to find.</param>
    ///<param name="newItem">The replacement item.</param>
    ///<returns>The number of replacements made.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static int ReplaceAll<T>(this IList<T> source, T oldItem, T newItem)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source list must not be null.");
        }

        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        int count = 0;
        for(int i = 0; i < source.Count; i++)
        {
            if(comparer.Equals(source[i], oldItem))
            {
                source[i] = newItem;
                count++;
            }
        }

        return count;
    }

    ///<summary>
    ///Replaces the element at <paramref name="index"/> in the list with <paramref name="item"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target list.</param>
    ///<param name="index">The zero-based index of the element to replace.</param>
    ///<param name="item">The replacement item.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the bounds of the list.</exception>
    public static IList<T> ReplaceAt<T>(this IList<T> source, int index, T item)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source list must not be null.");
        }

        if((index < 0) || (index >= source.Count))
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is outside the bounds of the list.");
        }

        source[index] = item;
        return source;
    }

    ///<summary>
    ///Swaps the elements at positions <paramref name="indexA"/> and <paramref name="indexB"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The target list.</param>
    ///<param name="indexA">The zero-based index of the first element.</param>
    ///<param name="indexB">The zero-based index of the second element.</param>
    ///<returns>The original <paramref name="source"/> for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException">Either index is outside the bounds of the list.</exception>
    public static IList<T> Swap<T>(this IList<T> source, int indexA, int indexB)
    {
        if(source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source list must not be null.");
        }

        if((indexA < 0) || (indexA >= source.Count))
        {
            throw new ArgumentOutOfRangeException(nameof(indexA), "Index A is outside the bounds of the list.");
        }

        if((indexB < 0) || (indexB >= source.Count))
        {
            throw new ArgumentOutOfRangeException(nameof(indexB), "Index B is outside the bounds of the list.");
        }

        (source[indexA], source[indexB]) = (source[indexB], source[indexA]);
        return source;
    }
    #endregion
}
