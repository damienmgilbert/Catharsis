namespace Catharsis.Randomization;

///<summary>
///Generates random strings from a configurable character set, useful for tokens, short codes, and test data. For a
///cryptographically secure token, supply a <see cref="Random"/> backed by a cryptographic source, or generate the raw
///bytes separately.
///</summary>
///<example>
public sealed class RandomStringGenerator
{
    #region Constants
    ///<summary>
    ///Lowercase letters only.
    ///</summary>
    public const string AlphaLower = "abcdefghijklmnopqrstuvwxyz";
        ///<summary>
///Uppercase and lowercase letters plus digits.
///</summary>
    public const string Alphanumeric = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    ///<summary>
    ///Uppercase letters only.
    ///</summary>
    public const string AlphaUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    ///<summary>
    ///Digits 0-9.
    ///</summary>
    public const string Digits = "0123456789";
    ///<summary>
    ///Lowercase hexadecimal digits.
    ///</summary>
    public const string Hexadecimal = "0123456789abcdef";
    #endregion

    #region Fields
    private readonly string _charset;
    private readonly Random _random;
    #endregion

    #region Constructors
    ///<summary>
    ///Creates a generator using the specified character set.
    ///</summary>
    ///<param name="charset">The characters to draw from. Defaults to <see cref="Alphanumeric"/>.</param>
    ///<param name="random">The random source to use, or <c>null</c> to use <see cref="Random.Shared"/>.</param>
    ///<exception cref="ArgumentException"><paramref name="charset"/> is <c>null</c> or empty.</exception>
    public RandomStringGenerator(string charset = Alphanumeric, Random? random = null)
    {
        if(string.IsNullOrEmpty(charset))
        {
            throw new ArgumentException("Character set must not be null or empty.", nameof(charset));
        }

        _charset = charset;
        _random = random ?? Random.Shared;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Generates a random string of the specified length, drawing from the configured character set.
    ///</summary>
    ///<param name="length">The length of the string to generate.</param>
    ///<returns>The generated string.</returns>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    public string Generate(int length)
    {
        if(length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length must not be negative.");
        }

        return string.Create(
               length,
               (_charset, _random),
               static(span, state) =>
               {
                   (string charset, Random random) = state;

                   for(int i = 0; i < span.Length; i++)
                   {
                       span[i] = charset[random.Next(charset.Length)];
                   }
               });
    }
    #endregion
}
