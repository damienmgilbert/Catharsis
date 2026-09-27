using System.Diagnostics;

namespace Catharsis.Diagnostics;

///<summary>
///Validates a value against a predicate, but only in builds compiled with the <c>DEBUG</c> symbol defined. In a
///Release build, calls to <see cref="Validate"/> are removed entirely by the compiler at the call site, so the
///predicate is never invoked and has zero runtime cost.
///</summary>
///<typeparam name="T">The type of the value to validate.</typeparam>
///<remarks>
///This mirrors how <see cref="Debug.Assert(bool)"/> is compiled out of Release builds via
///<see cref="ConditionalAttribute"/>. Because the conditional compilation is resolved in the caller's assembly, a
///caller compiled without <c>DEBUG</c> will skip the call even if this library itself was built with it.
///</remarks>
public static class DebugOnlyValidator<T>
{
    #region Public methods
    ///<summary>
    ///Throws <see cref="InvalidOperationException"/> if <paramref name="predicate"/> returns <c>false</c> for
    ///<paramref name="value"/>. Compiled out entirely when the calling assembly does not define <c>DEBUG</c>.
    ///</summary>
    ///<param name="value">The value to validate.</param>
    ///<param name="predicate">The predicate that <paramref name="value"/> must satisfy.</param>
    ///<param name="message">The message used when validation fails, or <c>null</c> for a default message.</param>
    ///<exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException"><paramref name="predicate"/> returned <c>false</c>.</exception>
    [Conditional("DEBUG")]
    public static void Validate(T value, Func<T, bool> predicate, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        if(!predicate(value))
        {
            throw new InvalidOperationException(message ?? "Debug-only validation failed.");
        }
    }
    #endregion
}
