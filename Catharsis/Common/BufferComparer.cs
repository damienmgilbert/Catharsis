using CommunityToolkit.Diagnostics;

namespace Catharsis.Common;

///<summary>
///Provides efficient comparison operations for buffers (spans and arrays) of elements, including equality,
///lexicographic ordering, and hash computation.
///</summary>
///<typeparam name="T">The element type. Must implement <see cref="IEquatable{T}"/>.</typeparam>
public sealed class BufferComparer<T> : IEqualityComparer<T[]>, IComparer<T[]> where T : IEquatable<T>, IComparable<T>
{
    #region Public methods

    ///<summary>
    ///Performs a lexicographic comparison of two spans.
    ///</summary>
    ///<param name="x">The first span.</param>
    ///<param name="y">The second span.</param>
    ///<returns>A negative value if x &lt; y, zero if equal, positive if x &gt; y.</returns>
    public static int Compare(ReadOnlySpan<T> x, ReadOnlySpan<T> y)
    {
        int minLength = Math.Min(x.Length, y.Length);
        for(int i = 0; i < minLength; i++)
        {
            int cmp = x[i].CompareTo(y[i]);
            if(cmp != 0)
            {
                return cmp;
            }
        }
        return x.Length.CompareTo(y.Length);
    }

    ///<summary>
    ///Performs a lexicographic comparison of two arrays.
    ///</summary>
    ///<param name="x">The first array.</param>
    ///<param name="y">The second array.</param>
    ///<returns>A negative value if x &lt; y, zero if equal, positive if x &gt; y.</returns>
    public int Compare(T[]? x, T[]? y)
    {
        if(ReferenceEquals(x, y))
        {
            return 0;
        }

        if(x is null)
        {
            return -1;
        }

        if(y is null)
        {
            return 1;
        }

        return Compare((ReadOnlySpan<T>)x, (ReadOnlySpan<T>)y);
    }

    ///<summary>
    ///Determines whether two spans are element-wise equal.
    ///</summary>
    ///<param name="x">The first span.</param>
    ///<param name="y">The second span.</param>
    ///<returns><c>true</c> if both spans have equal length and elements; otherwise <c>false</c>.</returns>
    public static bool Equals(ReadOnlySpan<T> x, ReadOnlySpan<T> y) => x.SequenceEqual(y);

    ///<summary>
    ///Determines whether two arrays are element-wise equal.
    ///</summary>
    ///<param name="x">The first array.</param>
    ///<param name="y">The second array.</param>
    ///<returns><c>true</c> if both arrays have equal length and elements; otherwise <c>false</c>.</returns>
    public bool Equals(T[]? x, T[]? y)
    {
        if(ReferenceEquals(x, y))
        {
            return true;
        }

        if((x is null) || (y is null))
        {
            return false;
        }

        return Equals((ReadOnlySpan<T>)x, (ReadOnlySpan<T>)y);
    }

    ///<summary>
    ///Computes a hash code over the elements of the specified span.
    ///</summary>
    ///<param name="span">The span to hash.</param>
    ///<returns>A hash code.</returns>
    public static int GetHashCode(ReadOnlySpan<T> span)
    {
        HashCode hash = new();
        foreach(T item in span)
        {
            hash.Add(item);
        }

        return hash.ToHashCode();
    }

    ///<summary>
    ///Computes a hash code for the specified array.
    ///</summary>
    ///<param name="obj">The array to hash.</param>
    ///<returns>A hash code.</returns>
    public int GetHashCode(T[] obj)
    {
        Guard.IsNotNull(obj);
        return GetHashCode((ReadOnlySpan<T>)obj);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets a default instance of <see cref="BufferComparer{T}"/>.
    ///</summary>
    public static BufferComparer<T> Default { get; } = new();
    #endregion
}
