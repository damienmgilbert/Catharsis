using System.Security.Cryptography;
using System.Text;

namespace Catharsis.Security;

///<summary>
///Compares byte sequences or strings in time that does not depend on where the first difference occurs, preventing
///a timing side-channel from leaking information such as a secret's length or content (e.g. when comparing a
///submitted token against a stored one).
///</summary>
public static class ConstantTimeComparer
{
    #region Public methods
    ///<summary>
    ///Determines whether two byte sequences are equal, in constant time relative to their length.
    ///</summary>
    ///<param name="left">The first sequence.</param>
    ///<param name="right">The second sequence.</param>
    ///<returns><c>true</c> if the sequences are equal; otherwise <c>false</c>.</returns>
    public static bool Equals(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right) => CryptographicOperations.FixedTimeEquals(left, right);

    ///<summary>
    ///Determines whether two strings are equal, in constant time relative to their UTF-8 byte length.
    ///</summary>
    ///<param name="left">The first string.</param>
    ///<param name="right">The second string.</param>
    ///<returns><c>true</c> if the strings are equal; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="left"/> or <paramref name="right"/> is <c>null</c>.</exception>
    public static bool Equals(string left, string right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(left), Encoding.UTF8.GetBytes(right));
    }
    #endregion
}
