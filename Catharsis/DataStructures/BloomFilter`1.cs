using System.Collections;

namespace Catharsis.DataStructures;

///<summary>
///A probabilistic set-membership structure: <see cref="MightContain"/> never returns a false negative, but may
///return a false positive at a rate close to the configured target. Uses a fixed-size bit array sized from the
///expected item count and target false-positive rate, with the required number of hash functions derived from two
///independent hashes (the Kirsch-Mitzenmacher technique).
///</summary>
///<typeparam name="T">The type of item to test for membership.</typeparam>
///<example>
///<code>
///BloomFilter&lt;string&gt; seen = new(expectedItemCount: 10_000, falsePositiveRate: 0.01);
///seen.Add(url);
///
///if(!seen.MightContain(url))
///{
///    // Definitely not seen before.
///}
///</code>
///</example>
public sealed class BloomFilter<T>
{
    #region Fields
    readonly BitArray _bits;
    readonly int _hashFunctionCount;
    #endregion

    #region Public methods
    ///<summary>
    ///Creates a filter sized for the expected number of items and target false-positive rate.
    ///</summary>
    ///<param name="expectedItemCount">The number of items the filter is expected to hold.</param>
    ///<param name="falsePositiveRate">The target false-positive rate, strictly between 0 and 1. Defaults to 0.01 (1%).</param>
    ///<exception cref="ArgumentOutOfRangeException">
    ///<paramref name="expectedItemCount"/> is less than 1, or <paramref name="falsePositiveRate"/> is not strictly between 0 and 1.
    ///</exception>
    public BloomFilter(int expectedItemCount, double falsePositiveRate = 0.01)
    {
        if(expectedItemCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(expectedItemCount), "Expected item count must be at least 1.");
        }

        if(falsePositiveRate <= 0 || falsePositiveRate >= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(falsePositiveRate), "False-positive rate must be strictly between 0 and 1.");
        }

        double rawBitCount = -(expectedItemCount * Math.Log(falsePositiveRate)) / Math.Pow(Math.Log(2), 2);
        int bitCount = Math.Max(1, (int)Math.Ceiling(rawBitCount));

        _hashFunctionCount = Math.Max(1, (int)Math.Round((bitCount / (double)expectedItemCount) * Math.Log(2)));
        _bits = new BitArray(bitCount);
    }

    ///<summary>
    ///Adds an item to the filter.
    ///</summary>
    ///<param name="item">The item to add.</param>
    public void Add(T item)
    {
        foreach(int index in GetIndices(item))
        {
            _bits[index] = true;
        }
    }

    ///<summary>
    ///Removes every item from the filter.
    ///</summary>
    public void Clear() => _bits.SetAll(false);

    ///<summary>
    ///Tests whether the item might have been added.
    ///</summary>
    ///<param name="item">The item to test.</param>
    ///<returns><c>false</c> if the item was definitely never added; <c>true</c> if it probably was.</returns>
    public bool MightContain(T item)
    {
        foreach(int index in GetIndices(item))
        {
            if(!_bits[index])
            {
                return false;
            }
        }

        return true;
    }

    IEnumerable<int> GetIndices(T item)
    {
        int h1 = item?.GetHashCode() ?? 0;
        int h2 = unchecked((int)(h1 * 0x9E3779B9));

        if(h2 == 0)
        {
            h2 = 1;
        }

        for(int i = 0; i < _hashFunctionCount; i++)
        {
            int combined = unchecked(h1 + (i * h2));
            int index = combined % _bits.Count;

            if(index < 0)
            {
                index += _bits.Count;
            }

            yield return index;
        }
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the size of the underlying bit array.
    ///</summary>
    public int BitCount => _bits.Count;

    ///<summary>
    ///Gets the number of hash functions applied per item.
    ///</summary>
    public int HashFunctionCount => _hashFunctionCount;
    #endregion
}
