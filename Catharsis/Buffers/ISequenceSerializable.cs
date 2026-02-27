using System.Buffers;

namespace Catharsis.Buffers;

///<summary>
///Defines an object that can serialize itself into an <see cref="IBufferWriter{T}"/>.
///</summary>
public interface ISequenceSerializable
{
    #region Public methods
    ///<summary>
    ///Gets the number of bytes required to serialize this instance.
    ///</summary>
    ///<returns>The size in bytes, or -1 if the size is not known in advance.</returns>
    int GetSerializedSize();

    ///<summary>
    ///Serializes this instance into the specified buffer writer.
    ///</summary>
    ///<param name="writer">The buffer writer to serialize into.</param>
    void Serialize(IBufferWriter<byte> writer);
    #endregion
}
