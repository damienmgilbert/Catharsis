using Microsoft.Extensions.Primitives;

namespace Catharsis.Immutable;

/// <summary>
/// Provides change notification for buffer contents using <see cref="IChangeToken"/>.
/// </summary>
public interface IBufferChangeNotifier
{
    /// <summary>
    /// Gets a <see cref="IChangeToken"/> that is signaled when the buffer contents change.
    /// </summary>
    /// <returns>A change token that can be used to observe buffer modifications.</returns>
    IChangeToken GetChangeToken();

    /// <summary>
    /// Gets a value indicating whether the buffer has been modified since the last observation.
    /// </summary>
    bool HasChanged { get; }
}
