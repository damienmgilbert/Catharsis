namespace Catharsis.Randomization;

///<summary>
///A pseudo-random sequence that can be restarted from its original seed, reproducing the exact same sequence of
///values again. Useful for reproducible simulations, tests, and procedural generation.
///</summary>
///<example>
///<code>
///SeededRandomSequence sequence = new(seed: 12345);
///int first = sequence.Next();
///
///sequence.Reset();
///int again = sequence.Next(); // Equal to 'first'.
///</code>
///</example>
///<param name="seed">The seed that determines the sequence of produced values.</param>
public sealed class SeededRandomSequence(int seed)
{
    #region Fields
    readonly int _seed = seed;
    Random _random = new(seed);
    #endregion

    #region Public methods

    ///<summary>
    ///Returns a non-negative random integer.
    ///</summary>
    public int Next() => _random.Next();

    ///<summary>
    ///Returns a non-negative random integer less than <paramref name="maxValue"/>.
    ///</summary>
    ///<param name="maxValue">The exclusive upper bound.</param>
    public int Next(int maxValue) => _random.Next(maxValue);

    ///<summary>
    ///Returns a random integer within the specified range.
    ///</summary>
    ///<param name="minValue">The inclusive lower bound.</param>
    ///<param name="maxValue">The exclusive upper bound.</param>
    public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);

    ///<summary>
    ///Returns a random double in the range [0.0, 1.0).
    ///</summary>
    public double NextDouble() => _random.NextDouble();

    ///<summary>
    ///Fills the specified buffer with random bytes.
    ///</summary>
    ///<param name="buffer">The buffer to fill.</param>
    public void NextBytes(Span<byte> buffer) => _random.NextBytes(buffer);

    ///<summary>
    ///Restarts the sequence from the original seed, so subsequent calls reproduce the same values as after
    ///construction.
    ///</summary>
    public void Reset() => _random = new Random(_seed);
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the seed this sequence was created with.
    ///</summary>
    public int Seed => _seed;
    #endregion
}
