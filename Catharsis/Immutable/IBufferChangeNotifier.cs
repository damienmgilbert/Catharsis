using Microsoft.Extensions.Primitives;

namespace Catharsis.Immutable;

///<summary>
///Provides change notification for buffer contents using <see cref="IChangeToken"/>.
///</summary>
public interface IBufferChangeNotifier
{
    #region Public methods

    ///<summary>
    ///Gets a <see cref="IChangeToken"/> that is signaled when the buffer contents change.
    ///</summary>
    ///<returns>A change token that can be used to observe buffer modifications.</returns>
    IChangeToken GetChangeToken();
    #endregion

    #region Public properties
    ///<summary>
    ///Gets a value indicating whether the buffer has been modified since the last observation.
    ///</summary>
    bool HasChanged { get; }
    #endregion
}
