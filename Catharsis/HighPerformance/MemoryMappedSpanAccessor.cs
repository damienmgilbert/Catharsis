using System.IO.MemoryMappedFiles;
using CommunityToolkit.Diagnostics;

namespace Catharsis.HighPerformance;

///<summary>
///Provides safe span-based access to memory-mapped file regions, enabling zero-copy reads of large files.
///</summary>
public sealed class MemoryMappedSpanAccessor : IDisposable
{
    #region Fields
    readonly MemoryMappedViewAccessor _accessor;
    bool _disposed;
    readonly MemoryMappedFile _file;
    readonly int _length;
    readonly long _offset;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a FileName <see cref="MemoryMappedSpanAccessor"/> from the specified file.
    ///</summary>
    ///<param name="filePath">The path to the file to map.</param>
    ///<param name="offset">The byte offset at which to start the mapping.</param>
    ///<param name="length">The number of bytes to map.</param>
    public MemoryMappedSpanAccessor(string filePath, long offset = 0, int length = 0)
    {
        Guard.IsNotNullOrWhiteSpace(filePath);
        Guard.IsGreaterThanOrEqualTo(offset, 0);

        FileInfo fileInfo = new(filePath);
        Guard.IsTrue(fileInfo.Exists);

        int mapLength = (length > 0) ? length : ((int)Math.Min(fileInfo.Length - offset, int.MaxValue));
        Guard.IsGreaterThan(mapLength, 0);

        _offset = offset;
        _length = mapLength;
        _file = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
        _accessor = _file.CreateViewAccessor(offset, mapLength, MemoryMappedFileAccess.Read);
    }
    #endregion

    #region Public methods
    ///<inheritdoc/>
    public void Dispose()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;
        _accessor.Dispose();
        _file.Dispose();
    }

    ///<summary>
    ///Reads the specified number of bytes from the mapped region starting at the given offset.
    ///</summary>
    ///<param name="buffer">The destination buffer.</param>
    ///<param name="offset">The offset within the mapped region.</param>
    ///<returns>The number of bytes actually read.</returns>
    public int Read(Span<byte> buffer, int offset = 0)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.IsGreaterThanOrEqualTo(offset, 0);

        int available = Math.Min(buffer.Length, _length - offset);
        Guard.IsGreaterThanOrEqualTo(available, 0);

        for(int i = 0; i < available; i++)
        {
            buffer[i] = _accessor.ReadByte(offset + i);
        }

        return available;
    }

    ///<summary>
    ///Reads an unmanaged value from the mapped region at the specified offset.
    ///</summary>
    ///<typeparam name="T">The unmanaged type to read.</typeparam>
    ///<param name="offset">The offset within the mapped region.</param>
    ///<returns>The value read.</returns>
    public T ReadValue<T>(int offset = 0) where T : unmanaged
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.IsGreaterThanOrEqualTo(offset, 0);

        _accessor.Read(offset, out T value);
        return value;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the length of the mapped region in bytes.
    ///</summary>
    public int Length => _length;

    ///<summary>
    ///Gets the offset of the mapped region from the start of the file.
    ///</summary>
    public long Offset => _offset;
    #endregion
}
