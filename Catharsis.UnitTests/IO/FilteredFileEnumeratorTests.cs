using Catharsis.IO;

namespace Catharsis.UnitTests.IO;

///<summary>
///Unit tests for the <see cref="FilteredFileEnumerator"/> class.
///</summary>
[TestClass]
public class FilteredFileEnumeratorTests
{
    #region Constructor validation

    [TestMethod]
    public void EnumerateFiles_NullRootDirectory_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => FilteredFileEnumerator.EnumerateFiles(null!).ToList()); }

    [TestMethod]
    public void EnumerateFiles_WhitespaceRootDirectory_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => FilteredFileEnumerator.EnumerateFiles("   ").ToList()); }

    #endregion

    #region Enumeration

    [TestMethod]
    public void EnumerateFiles_NoFilters_ReturnsAllFilesRecursively()
    {
        using TempDirectory root = new();
        root.CreateFile("a.txt");
        root.CreateFile("sub/b.txt");

        List<string> files = [.. FilteredFileEnumerator.EnumerateFiles(root.Path)];

        Assert.HasCount(2, files);
    }

    [TestMethod]
    public void EnumerateFiles_FileFilter_ExcludesNonMatchingFiles()
    {
        using TempDirectory root = new();
        root.CreateFile("a.txt");
        root.CreateFile("b.log");

        List<string> files = [.. FilteredFileEnumerator.EnumerateFiles(root.Path, fileFilter: path => path.EndsWith(".txt", StringComparison.Ordinal))];

        Assert.HasCount(1, files);
        Assert.IsTrue(files[0].EndsWith("a.txt", StringComparison.Ordinal));
    }

    [TestMethod]
    public void EnumerateFiles_ShouldDescendDirectoryFalse_SkipsWholeSubtree()
    {
        using TempDirectory root = new();
        root.CreateFile("a.txt");
        root.CreateFile("excluded/b.txt");
        root.CreateFile("excluded/nested/c.txt");

        List<string> files = [.. FilteredFileEnumerator.EnumerateFiles(root.Path, shouldDescendDirectory: path => !path.EndsWith("excluded", StringComparison.Ordinal))];

        Assert.HasCount(1, files);
        Assert.IsTrue(files[0].EndsWith("a.txt", StringComparison.Ordinal));
    }

    [TestMethod]
    public void EnumerateFiles_EmptyDirectory_ReturnsNoFiles()
    {
        using TempDirectory root = new();
        Assert.IsEmpty(FilteredFileEnumerator.EnumerateFiles(root.Path).ToList());
    }

    #endregion

    #region Test infrastructure
    sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void CreateFile(string relativePath)
        {
            string fullPath = System.IO.Path.Combine(Path, relativePath);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, "content");
        }

        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
    #endregion
}
