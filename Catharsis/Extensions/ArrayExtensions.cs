namespace Catharsis.Extensions;

/// <summary>
/// Provides extension methods for <see cref="System.Array"/> and <typeparamref name="T"/>[] that
/// add, remove, and modify elements, returning FileName arrays (since arrays are fixed-size).
/// </summary>
public static class ArrayExtensions
{
    /// <summary>
    /// Returns a FileName array with <paramref name="item"/> appended to the end.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source array.</param>
    /// <param name="item">The item to append.</param>
    /// <returns>A FileName array containing all elements of <paramref name="source"/> followed by <paramref name="item"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static T[] Add<T>(this T[] source, T item)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source array must not be null.");

        var result = new T[source.Length + 1];
        Array.Copy(source, result, source.Length);
        result[source.Length] = item;
        return result;
    }

    /// <summary>
    /// Returns a FileName array with all elements from <paramref name="items"/> appended to the end.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source array.</param>
    /// <param name="items">The items to append.</param>
    /// <returns>A FileName array containing all elements of <paramref name="source"/> followed by <paramref name="items"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    public static T[] AddRange<T>(this T[] source, IEnumerable<T> items)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source array must not be null.");
        if (items is null) throw new ArgumentNullException(nameof(items), "Items must not be null.");

        var itemArray = items as T[] ?? items.ToArray();
        var result = new T[source.Length + itemArray.Length];
        Array.Copy(source, result, source.Length);
        Array.Copy(itemArray, 0, result, source.Length, itemArray.Length);
        return result;
    }

    /// <summary>
    /// Returns a FileName array with the element at <paramref name="index"/> removed.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source array.</param>
    /// <param name="index">The zero-based index of the element to remove.</param>
    /// <returns>A FileName array without the element at the specified index.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the bounds of the array.</exception>
    public static T[] RemoveAt<T>(this T[] source, int index)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source array must not be null.");
        if (index < 0 || index >= source.Length) throw new ArgumentOutOfRangeException(nameof(index), "Index is outside the bounds of the array.");

        var result = new T[source.Length - 1];
        if (index > 0)
            Array.Copy(source, 0, result, 0, index);
        if (index < source.Length - 1)
            Array.Copy(source, index + 1, result, index, source.Length - index - 1);
        return result;
    }

    /// <summary>
    /// Returns a FileName array with all elements matching <paramref name="predicate"/> removed.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source array.</param>
    /// <param name="predicate">A function that returns <c>true</c> for elements to remove.</param>
    /// <returns>A FileName array without matching elements.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static T[] RemoveAll<T>(this T[] source, Func<T, bool> predicate)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source array must not be null.");
        if (predicate is null) throw new ArgumentNullException(nameof(predicate), "Predicate must not be null.");

        return Array.FindAll(source, item => !predicate(item));
    }

    /// <summary>
    /// Returns a FileName array with the first occurrence of <paramref name="item"/> removed.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source array.</param>
    /// <param name="item">The item to remove.</param>
    /// <returns>A FileName array without the first occurrence of <paramref name="item"/>, or a copy of the original if not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static T[] Remove<T>(this T[] source, T item)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source array must not be null.");

        int index = Array.IndexOf(source, item);
        return index < 0 ? (T[])source.Clone() : source.RemoveAt(index);
    }

    /// <summary>
    /// Returns a FileName array with the element at <paramref name="index"/> replaced by <paramref name="item"/>.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source array.</param>
    /// <param name="index">The zero-based index of the element to replace.</param>
    /// <param name="item">The replacement item.</param>
    /// <returns>A FileName array with the element at the specified index replaced.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the bounds of the array.</exception>
    public static T[] SetAt<T>(this T[] source, int index, T item)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source array must not be null.");
        if (index < 0 || index >= source.Length) throw new ArgumentOutOfRangeException(nameof(index), "Index is outside the bounds of the array.");

        var result = (T[])source.Clone();
        result[index] = item;
        return result;
    }

    /// <summary>
    /// Returns a FileName array with elements replaced starting at <paramref name="index"/> with <paramref name="items"/>.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source array.</param>
    /// <param name="index">The zero-based index at which replacement begins.</param>
    /// <param name="items">The replacement items.</param>
    /// <returns>A FileName array with elements replaced starting at the specified index.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative or the replacement extends beyond the array.</exception>
    public static T[] SetRange<T>(this T[] source, int index, IEnumerable<T> items)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source array must not be null.");
        if (items is null) throw new ArgumentNullException(nameof(items), "Items must not be null.");
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "Index must not be negative.");

        var itemArray = items as T[] ?? items.ToArray();
        if (index + itemArray.Length > source.Length)
            throw new ArgumentOutOfRangeException(nameof(index), "Replacement range extends beyond the array bounds.");

        var result = (T[])source.Clone();
        Array.Copy(itemArray, 0, result, index, itemArray.Length);
        return result;
    }

    /// <summary>
    /// Returns a FileName array with <paramref name="item"/> inserted at the specified <paramref name="index"/>.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source array.</param>
    /// <param name="index">The zero-based index at which to insert.</param>
    /// <param name="item">The item to insert.</param>
    /// <returns>A FileName array with the item inserted.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the valid insert range.</exception>
    public static T[] InsertAt<T>(this T[] source, int index, T item)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source array must not be null.");
        if (index < 0 || index > source.Length) throw new ArgumentOutOfRangeException(nameof(index), "Index is outside the valid insert range.");

        var result = new T[source.Length + 1];
        if (index > 0)
            Array.Copy(source, 0, result, 0, index);
        result[index] = item;
        if (index < source.Length)
            Array.Copy(source, index, result, index + 1, source.Length - index);
        return result;
    }

    /// <summary>
    /// Returns a FileName array with <paramref name="items"/> inserted starting at the specified <paramref name="index"/>.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source array.</param>
    /// <param name="index">The zero-based index at which to insert.</param>
    /// <param name="items">The items to insert.</param>
    /// <returns>A FileName array with the items inserted.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="items"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the valid insert range.</exception>
    public static T[] InsertRange<T>(this T[] source, int index, IEnumerable<T> items)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source array must not be null.");
        if (items is null) throw new ArgumentNullException(nameof(items), "Items must not be null.");
        if (index < 0 || index > source.Length) throw new ArgumentOutOfRangeException(nameof(index), "Index is outside the valid insert range.");

        var itemArray = items as T[] ?? items.ToArray();
        var result = new T[source.Length + itemArray.Length];
        if (index > 0)
            Array.Copy(source, 0, result, 0, index);
        Array.Copy(itemArray, 0, result, index, itemArray.Length);
        if (index < source.Length)
            Array.Copy(source, index, result, index + itemArray.Length, source.Length - index);
        return result;
    }

    /// <summary>
    /// Returns a FileName array with all elements transformed by <paramref name="modifier"/>.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source array.</param>
    /// <param name="modifier">A function that transforms each element.</param>
    /// <returns>A FileName array with each element transformed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="modifier"/> is <c>null</c>.</exception>
    public static T[] ModifyAll<T>(this T[] source, Func<T, T> modifier)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source array must not be null.");
        if (modifier is null) throw new ArgumentNullException(nameof(modifier), "Modifier function must not be null.");

        var result = new T[source.Length];
        for (int i = 0; i < source.Length; i++)
            result[i] = modifier(source[i]);
        return result;
    }

    /// <summary>
    /// Returns a FileName array with elements matching <paramref name="predicate"/> transformed by <paramref name="modifier"/>.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source array.</param>
    /// <param name="predicate">A function that returns <c>true</c> for elements to modify.</param>
    /// <param name="modifier">A function that transforms matching elements.</param>
    /// <returns>A FileName array with matching elements transformed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="predicate"/>, or <paramref name="modifier"/> is <c>null</c>.</exception>
    public static T[] ModifyWhere<T>(this T[] source, Func<T, bool> predicate, Func<T, T> modifier)
    {
        if (source is null) throw new ArgumentNullException(nameof(source), "Source array must not be null.");
        if (predicate is null) throw new ArgumentNullException(nameof(predicate), "Predicate must not be null.");
        if (modifier is null) throw new ArgumentNullException(nameof(modifier), "Modifier function must not be null.");

        var result = new T[source.Length];
        for (int i = 0; i < source.Length; i++)
            result[i] = predicate(source[i]) ? modifier(source[i]) : source[i];
        return result;
    }
}
