using System.IO.Enumeration;

namespace Catharsis.IO;

///<summary>
///A <see cref="FileSystemEnumerable{TResult}"/>-based recursive file enumerator that can skip entire subtrees via a
///directory predicate. Unlike <see cref="Directory.EnumerateFiles(string, string, SearchOption)"/> combined with a
///LINQ <c>Where</c>, an excluded directory is never descended into or enumerated in the first place.
///</summary>
public static class FilteredFileEnumerator
{
    #region Public methods
    ///<summary>
    ///Recursively enumerates files under <paramref name="rootDirectory"/>.
    ///</summary>
    ///<param name="rootDirectory">The directory to start enumerating from.</param>
    ///<param name="shouldDescendDirectory">
    ///A predicate receiving a subdirectory's full path, returning whether to descend into it. Directories for which
    ///this returns <c>false</c> are skipped entirely, along with everything under them. <c>null</c> descends into
    ///every subdirectory.
    ///</param>
    ///<param name="fileFilter">
    ///A predicate receiving a file's full path, returning whether to include it. <c>null</c> includes every file.
    ///</param>
    ///<returns>The full paths of every matching file.</returns>
    ///<exception cref="ArgumentException"><paramref name="rootDirectory"/> is <c>null</c>, empty, or whitespace.</exception>
    public static IEnumerable<string> EnumerateFiles(string rootDirectory, Func<string, bool>? shouldDescendDirectory = null, Func<string, bool>? fileFilter = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);

        return new FileSystemEnumerable<string>(rootDirectory, static (ref FileSystemEntry entry) => entry.ToFullPath(), new EnumerationOptions { RecurseSubdirectories = true })
        {
            ShouldIncludePredicate = (ref FileSystemEntry entry) => !entry.IsDirectory && ((fileFilter is null) || fileFilter(entry.ToFullPath())),
            ShouldRecursePredicate = (ref FileSystemEntry entry) => (shouldDescendDirectory is null) || shouldDescendDirectory(entry.ToFullPath())
        };
    }
    #endregion
}
