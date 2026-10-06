namespace Catharsis.IO;

///<summary>
///Writes a file "atomically" by first writing to a temporary file in the same directory as the target, then
///renaming it into place. A reader can never observe a partially written file: it either sees the previous
///contents or the complete new contents, never something in between. If the write fails, the target file is left
///untouched and the temporary file is cleaned up.
///</summary>
public static class AtomicFileWriter
{
    #region Private methods
    static string CreateTempPath(string path) => $"{path}.{Guid.NewGuid():N}.tmp";
    #endregion

    #region Public methods
    ///<summary>
    ///Atomically writes <paramref name="data"/> to <paramref name="path"/>.
    ///</summary>
    ///<param name="path">The destination file path.</param>
    ///<param name="data">The bytes to write.</param>
    ///<exception cref="ArgumentException"><paramref name="path"/> is <c>null</c>, empty, or whitespace.</exception>
    public static void Write(string path, ReadOnlySpan<byte> data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string tempPath = CreateTempPath(path);

        try
        {
            using (FileStream stream = new(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(data);
            }

            File.Move(tempPath, path, overwrite: true);
        }
        catch
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            throw;
        }
    }

    ///<summary>
    ///Atomically writes <paramref name="data"/> to <paramref name="path"/>.
    ///</summary>
    ///<param name="path">The destination file path.</param>
    ///<param name="data">The bytes to write.</param>
    ///<param name="cancellationToken">A token that can cancel the write before the file is moved into place.</param>
    ///<returns>A task representing the asynchronous write operation.</returns>
    ///<exception cref="ArgumentException"><paramref name="path"/> is <c>null</c>, empty, or whitespace.</exception>
    public static async Task WriteAsync(string path, ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string tempPath = CreateTempPath(path);

        try
        {
            await using (FileStream stream = new(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
            }

            File.Move(tempPath, path, overwrite: true);
        }
        catch
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            throw;
        }
    }
    #endregion
}
