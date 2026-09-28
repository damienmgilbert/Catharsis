using Catharsis.IO;

namespace Catharsis.UnitTests.IO;

///<summary>
///Unit tests for the <see cref="DirectoryWatcherDebounced"/> class.
///</summary>
[TestClass]
public class DirectoryWatcherDebouncedTests
{
    #region Private methods
    private static string CreateTempDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), $"catharsis-watcher-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static async Task<bool> WaitForAsync(Func<bool> condition, TimeSpan timeout)
    {
        DateTime deadline = DateTime.UtcNow + timeout;

        while(DateTime.UtcNow < deadline)
        {
            if(condition())
            {
                return true;
            }

            await Task.Delay(20);
        }

        return condition();
    }
    #endregion

    #region Constructor

    [TestMethod]
    public void Constructor_NullPath_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new DirectoryWatcherDebounced(null!, TimeSpan.FromMilliseconds(50))); }

    #endregion

    #region Start / Changed

    [TestMethod]
    public async Task Changed_FileModified_FiresAfterDebounceDelay()
    {
        string directory = CreateTempDirectory();
        string filePath = Path.Combine(directory, "watched.txt");
        File.WriteAllText(filePath, "initial");

        try
        {
            using DirectoryWatcherDebounced watcher = new(directory, TimeSpan.FromMilliseconds(100));
            int fireCount = 0;
            watcher.Changed += (_, _) => Interlocked.Increment(ref fireCount);
            watcher.Start();

            File.WriteAllText(filePath, "updated");

            bool fired = await WaitForAsync(() => fireCount > 0, TimeSpan.FromSeconds(5));
            Assert.IsTrue(fired);
        } finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public async Task Changed_RapidBurstOfWrites_CoalescesIntoFewerNotifications()
    {
        string directory = CreateTempDirectory();
        string filePath = Path.Combine(directory, "burst.txt");
        File.WriteAllText(filePath, "initial");

        try
        {
            using DirectoryWatcherDebounced watcher = new(directory, TimeSpan.FromMilliseconds(300));
            int fireCount = 0;
            watcher.Changed += (_, _) => Interlocked.Increment(ref fireCount);
            watcher.Start();

            for(int i = 0; i < 10; i++)
            {
                File.WriteAllText(filePath, $"update-{i}");
                await Task.Delay(10);
            }

            await WaitForAsync(() => fireCount > 0, TimeSpan.FromSeconds(5));
            await Task.Delay(200);

            Assert.IsTrue(fireCount < 10);
        } finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void Start_AfterDispose_Throws()
    {
        string directory = CreateTempDirectory();

        try
        {
            DirectoryWatcherDebounced watcher = new(directory, TimeSpan.FromMilliseconds(50));
            watcher.Dispose();

            Assert.ThrowsExactly<ObjectDisposedException>(watcher.Start);
        } finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    #endregion
}
