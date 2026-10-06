using System.IO.Compression;

namespace Catharsis.IO;

///<summary>
///Pipes <see cref="AtomicFileWriter"/>'s safe-write step through a compression stream: data is compressed while being
///written to a temporary file in the target's directory, then the temporary file is renamed into place, so a reader
///never observes a partially written or partially compressed file.
///</summary>
public static class CompressingFileWriter
{
    #region Private methods
    private static Stream CreateCompressionStream(Stream destination, CompressionFormat format) => format switch
    {
        CompressionFormat.GZip => new GZipStream(destination, CompressionLevel.Optimal),
        CompressionFormat.Brotli => new BrotliStream(destination, CompressionLevel.Optimal),
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unrecognized compression format.")
    };

    private static string CreateTempPath(string path) => $"{path}.{Guid.NewGuid():N}.tmp";
    #endregion

    #region Public methods
    ///<summary>
    ///Atomically writes <paramref name="data"/> to <paramref name="path"/>, compressed with <paramref name="format"/>.
    ///</summary>
    ///<param name="path">The destination file path.</param>
    ///<param name="data">The uncompressed bytes to write.</param>
    ///<param name="format">The compression format to use.</param>
    ///<exception cref="ArgumentException"><paramref name="path"/> is <c>null</c>, empty, or whitespace.</exception>
    public static void Write(string path, ReadOnlySpan<byte> data, CompressionFormat format = CompressionFormat.GZip)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string tempPath = CreateTempPath(path);

        try
        {
            using(FileStream fileStream = new(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using(Stream compressionStream = CreateCompressionStream(fileStream, format))
                {
                    compressionStream.Write(data);
                }
            }

            File.Move(tempPath, path, overwrite: true);
        } catch
        {
            if(File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            throw;
        }
    }

    ///<summary>
    ///Atomically writes <paramref name="data"/> to <paramref name="path"/>, compressed with <paramref name="format"/>.
    ///</summary>
    ///<param name="path">The destination file path.</param>
    ///<param name="data">The uncompressed bytes to write.</param>
    ///<param name="format">The compression format to use.</param>
    ///<param name="cancellationToken">A token that can cancel the write before the file is moved into place.</param>
    ///<returns>A task representing the asynchronous write operation.</returns>
    ///<exception cref="ArgumentException"><paramref name="path"/> is <c>null</c>, empty, or whitespace.</exception>
    public static async Task WriteAsync(string path, ReadOnlyMemory<byte> data, CompressionFormat format = CompressionFormat.GZip, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string tempPath = CreateTempPath(path);

        try
        {
            await using(FileStream fileStream = new(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await using(Stream compressionStream = CreateCompressionStream(fileStream, format))
                {
                    await compressionStream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
                }
            }

            File.Move(tempPath, path, overwrite: true);
        } catch
        {
            if(File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            throw;
        }
    }
    #endregion
}
