using System.Text;

namespace Catharsis.Security;

///<summary>
///Encodes and decodes RFC 4648 base32 (the variant commonly used for TOTP secrets and other tokens meant to be
///typed or read aloud, since it avoids visually ambiguous characters like <c>0</c>/<c>O</c> and <c>1</c>/<c>I</c>).
///</summary>
public static class Base32Codec
{
    #region Fields
    const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
    #endregion

    #region Public methods
    ///<summary>
    ///Decodes a base32 string back into its original bytes.
    ///</summary>
    ///<param name="encoded">The base32 string to decode. Padding (<c>=</c>) is optional and ignored if present.</param>
    ///<returns>The decoded bytes.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="encoded"/> is <c>null</c>.</exception>
    ///<exception cref="FormatException"><paramref name="encoded"/> contains a character outside the base32 alphabet.</exception>
    public static byte[] Decode(string encoded)
    {
        ArgumentNullException.ThrowIfNull(encoded);

        string trimmed = encoded.TrimEnd('=');
        List<byte> output = new((trimmed.Length * 5) / 8);
        int bitBuffer = 0;
        int bitCount = 0;

        foreach(char c in trimmed)
        {
            int index = Alphabet.IndexOf(char.ToUpperInvariant(c));

            if(index < 0)
            {
                throw new FormatException($"'{c}' is not a valid base32 character.");
            }

            bitBuffer = ((bitBuffer << 5) | index) & 0xFFF;
            bitCount += 5;

            if(bitCount >= 8)
            {
                bitCount -= 8;
                output.Add((byte)((bitBuffer >> bitCount) & 0xFF));
            }
        }

        return [.. output];
    }

    ///<summary>
    ///Encodes a byte sequence as a padded base32 string.
    ///</summary>
    ///<param name="data">The bytes to encode.</param>
    ///<returns>The base32-encoded, <c>=</c>-padded representation of <paramref name="data"/>.</returns>
    public static string Encode(ReadOnlySpan<byte> data)
    {
        if(data.IsEmpty)
        {
            return string.Empty;
        }

        StringBuilder result = new(((data.Length * 8) + 4) / 5);
        int bitBuffer = 0;
        int bitCount = 0;

        foreach(byte b in data)
        {
            bitBuffer = ((bitBuffer << 8) | b) & 0xFFF;
            bitCount += 8;

            while(bitCount >= 5)
            {
                bitCount -= 5;
                result.Append(Alphabet[(bitBuffer >> bitCount) & 0x1F]);
            }
        }

        if(bitCount > 0)
        {
            result.Append(Alphabet[(bitBuffer << (5 - bitCount)) & 0x1F]);
        }

        while((result.Length % 8) != 0)
        {
            result.Append('=');
        }

        return result.ToString();
    }
    #endregion
}
