using System.Buffers;
using Catharsis.Buffers;

namespace Catharsis.Services;

///<summary>
///Defines a parser that reads structured data from a <see cref="ReadOnlySequence{T}"/>.
///</summary>
public interface ISequenceParser
{
    #region Public methods

    ///<summary>
    ///Resets the parser to its initial state for reuse.
    ///</summary>
    void Reset();

    ///<summary>
    ///Attempts to parse data from the specified sequence.
    ///</summary>
    ///<param name="sequence">The input sequence to parse.</param>
    ///<param name="consumed">The position up to which data was consumed.</param>
    ///<param name="examined">The position up to which data was examined.</param>
    ///<returns>The parse status indicating the result of the operation.</returns>
    SequenceParseStatus TryParse(in ReadOnlySequence<byte> sequence, out SequencePosition consumed, out SequencePosition examined);
    #endregion
}
