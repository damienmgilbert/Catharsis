using Catharsis.IO;
using System.Text;

namespace Catharsis.UnitTests.IO;

///<summary>
///Unit tests for the <see cref="AtomicFileWriter"/> class.
///</summary>
[TestClass]
public class AtomicFileWriterTests
{
    #region Private methods
    private static string CreateTempFilePath() => Path.Combine(Path.GetTempPath(), $"catharsis-atomic-{Guid.NewGuid():N}.tmp");
    #endregion

    #region Write

    [TestMethod]
    public void Write_NullPath_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => AtomicFileWriter.Write(null!, [1, 2, 3])); }

    [TestMethod]
    public void Write_NewFile_CreatesFileWithContent()
    {
        string path = CreateTempFilePath();

        try
        {
            AtomicFileWriter.Write(path, Encoding.UTF8.GetBytes("hello"));

            Assert.AreEqual("hello", File.ReadAllText(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void Write_ExistingFile_OverwritesContent()
    {
        string path = CreateTempFilePath();

        try
        {
            File.WriteAllText(path, "old content");
            AtomicFileWriter.Write(path, Encoding.UTF8.GetBytes("new content"));

            Assert.AreEqual("new content", File.ReadAllText(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void Write_DoesNotLeaveTemporaryFileBehind()
    {
        string path = CreateTempFilePath();
        string directory = Path.GetDirectoryName(path)!;
        string prefix = Path.GetFileName(path);

        try
        {
            AtomicFileWriter.Write(path, Encoding.UTF8.GetBytes("hello"));

            string[] leftovers = Directory.GetFiles(directory, $"{prefix}.*.tmp");
            Assert.IsEmpty(leftovers);
        }
        finally
        {
            File.Delete(path);
        }
    }

    #endregion

    #region WriteAsync

    [TestMethod]
    public async Task WriteAsync_NullPath_Throws() { await Assert.ThrowsExactlyAsync<ArgumentNullException>(static () => AtomicFileWriter.WriteAsync(null!, new byte[] { 1 })); }

    [TestMethod]
    public async Task WriteAsync_NewFile_CreatesFileWithContent()
    {
        string path = CreateTempFilePath();

        try
        {
            await AtomicFileWriter.WriteAsync(path, Encoding.UTF8.GetBytes("hello async"));

            Assert.AreEqual("hello async", await File.ReadAllTextAsync(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    #endregion
}
