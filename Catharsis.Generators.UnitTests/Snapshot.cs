using System.Runtime.CompilerServices;

namespace Catharsis.Generators.UnitTests;

///<summary>
///Compares generated source with a checked-in snapshot so any change to generator output shows up as a reviewable diff.
///Line endings are normalized, so the snapshots are valid on every platform. To accept new output, run the tests with the
///environment variable <c>UPDATE_SNAPSHOTS=1</c>, which rewrites the snapshot files in the source tree.
///</summary>
internal static class Snapshot
{
    internal static void AssertMatches(string snapshotName, string actual, [CallerFilePath] string callerPath = "")
    {
        string path = Path.Combine(Path.GetDirectoryName(callerPath)!, "Snapshots", snapshotName);
        string normalized = Normalize(actual);

        if(Environment.GetEnvironmentVariable("UPDATE_SNAPSHOTS") == "1")
        {
            File.WriteAllText(path, normalized + "\n");
            return;
        }

        Assert.IsTrue(File.Exists(path), $"Snapshot '{snapshotName}' is missing. Run with UPDATE_SNAPSHOTS=1 to create it.");
        Assert.AreEqual(Normalize(File.ReadAllText(path)), normalized, $"Generated output no longer matches snapshot '{snapshotName}'. Run with UPDATE_SNAPSHOTS=1 to accept the change.");
    }

    static string Normalize(string text) => text.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n');
}
