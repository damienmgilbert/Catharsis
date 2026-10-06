namespace Catharsis.IO;

///<summary>
///The compression format used by <see cref="CompressingFileWriter"/>.
///</summary>
public enum CompressionFormat
{
    ///<summary>
    ///The GZip format.
    ///</summary>
    GZip,

    ///<summary>
    ///The Brotli format.
    ///</summary>
    Brotli
}
