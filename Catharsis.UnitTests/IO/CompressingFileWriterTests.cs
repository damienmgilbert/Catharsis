using Catharsis.IO;
using System.IO.Compression;
using System.Text;

namespace Catharsis.UnitTests.IO;

///<summary>
///Unit tests for the <see cref="CompressingFileWriter"/> class.
///</summary>
[TestClass]
public class CompressingFileWriterTests
{
    #region Write

    [TestMethod]
    public void Write_NullPath_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => CompressingFileWriter.Write(null!, "data"u8)); }

    [TestMethod]
    public void Write_WhitespacePath_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => CompressingFileWriter.Write("   ", "data"u8)); }

    [TestMethod]
    public void Write_GZip_ProducesGZipDecodableFile()
    {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gz");

        try
        {
            CompressingFileWriter.Write(path, "hello, world"u8, CompressionFormat.GZip);

            using FileStream fileStream = new(path, FileMode.Open, FileAccess.Read);
            using GZipStream gzipStream = new(fileStream, CompressionMode.Decompress);
            using StreamReader reader = new(gzipStream, Encoding.UTF8);

            Assert.AreEqual("hello, world", reader.ReadToEnd());
        } finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void Write_Brotli_ProducesBrotliDecodableFile()
    {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.br");

        try
        {
            CompressingFileWriter.Write(path, "hello, brotli"u8, CompressionFormat.Brotli);

            using FileStream fileStream = new(path, FileMode.Open, FileAccess.Read);
            using BrotliStream brotliStream = new(fileStream, CompressionMode.Decompress);
            using StreamReader reader = new(brotliStream, Encoding.UTF8);

            Assert.AreEqual("hello, brotli", reader.ReadToEnd());
        } finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void Write_ExistingFile_OverwritesAtomically()
    {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gz");

        try
        {
            CompressingFileWriter.Write(path, "first"u8);
            CompressingFileWriter.Write(path, "second"u8);

            using FileStream fileStream = new(path, FileMode.Open, FileAccess.Read);
            using GZipStream gzipStream = new(fileStream, CompressionMode.Decompress);
            using StreamReader reader = new(gzipStream, Encoding.UTF8);

            Assert.AreEqual("second", reader.ReadToEnd());
        } finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void Write_DoesNotLeaveTemporaryFileBehind()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "output.gz");

        try
        {
            CompressingFileWriter.Write(path, "data"u8);
            Assert.HasCount(1, Directory.GetFiles(directory));
        } finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    #endregion

    #region WriteAsync

    [TestMethod]
    public async Task WriteAsync_NullPath_Throws() { await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => CompressingFileWriter.WriteAsync(null!, new byte[] { 1 })); }

    [TestMethod]
    public async Task WriteAsync_GZip_ProducesGZipDecodableFile()
    {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gz");

        try
        {
            await CompressingFileWriter.WriteAsync(path, Encoding.UTF8.GetBytes("async hello"));

            await using FileStream fileStream = new(path, FileMode.Open, FileAccess.Read);
            await using GZipStream gzipStream = new(fileStream, CompressionMode.Decompress);
            using StreamReader reader = new(gzipStream, Encoding.UTF8);

            Assert.AreEqual("async hello", await reader.ReadToEndAsync());
        } finally
        {
            File.Delete(path);
        }
    }

    #endregion
}
