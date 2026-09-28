using Catharsis.Diagnostics;
using System.Diagnostics;

namespace Catharsis.UnitTests.Diagnostics;

///<summary>
///Unit tests for the <see cref="ActivityScope"/> class.
///</summary>
[TestClass]
public class ActivityScopeTests
{
    #region Private methods
    ///<remarks>
    ///<see cref="ActivityListener.ShouldListenTo"/> is scoped to this test's own uniquely named source (not a
    ///blanket "listen to everything" predicate), since the test suite runs in parallel and a global listener would
    ///affect other tests' sources too.
    ///</remarks>
    private static ActivitySource CreateListenedSource(out ActivityListener listener)
    {
        string sourceName = $"test-source-{Guid.NewGuid():N}";
        ActivitySource source = new(sourceName);

        listener = new ActivityListener
        {
            ShouldListenTo = candidate => candidate.Name == sourceName,
            Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };

        ActivitySource.AddActivityListener(listener);
        return source;
    }
    #endregion

    #region Start

    [TestMethod]
    public void Start_NullSource_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => ActivityScope.Start(null!, "operation")); }

    [TestMethod]
    public void Start_NullName_Throws()
    {
        using ActivitySource source = new($"test-source-{Guid.NewGuid():N}");
        Assert.ThrowsExactly<ArgumentNullException>(() => ActivityScope.Start(source, null!));
    }

    [TestMethod]
    public void Start_WhitespaceName_Throws()
    {
        using ActivitySource source = new($"test-source-{Guid.NewGuid():N}");
        Assert.ThrowsExactly<ArgumentException>(() => ActivityScope.Start(source, "   "));
    }

    [TestMethod]
    public void Start_NoListener_ActivityIsNull()
    {
        using ActivitySource source = new($"test-source-{Guid.NewGuid():N}");
        using ActivityScope scope = ActivityScope.Start(source, "operation");

        Assert.IsNull(scope.Activity);
    }

    [TestMethod]
    public void Start_WithListener_ReturnsActivityWithOperationName()
    {
        using ActivitySource source = CreateListenedSource(out ActivityListener listener);

        try
        {
            using ActivityScope scope = ActivityScope.Start(source, "operation");

            Assert.IsNotNull(scope.Activity);
            Assert.AreEqual("operation", scope.Activity!.OperationName);
        } finally
        {
            listener.Dispose();
        }
    }

    #endregion

    #region RecordException

    [TestMethod]
    public void RecordException_NullException_Throws()
    {
        using ActivitySource source = new($"test-source-{Guid.NewGuid():N}");
        using ActivityScope scope = ActivityScope.Start(source, "operation");

        Assert.ThrowsExactly<ArgumentNullException>(() => scope.RecordException(null!));
    }

    [TestMethod]
    public void RecordException_NoActivity_DoesNotThrow()
    {
        using ActivitySource source = new($"test-source-{Guid.NewGuid():N}");
        using ActivityScope scope = ActivityScope.Start(source, "operation");

        scope.RecordException(new InvalidOperationException("boom"));
    }

    [TestMethod]
    public void RecordException_WithActivity_AddsExceptionEventAndErrorStatus()
    {
        using ActivitySource source = CreateListenedSource(out ActivityListener listener);

        try
        {
            using ActivityScope scope = ActivityScope.Start(source, "operation");
            scope.RecordException(new InvalidOperationException("boom"));

            Assert.AreEqual(ActivityStatusCode.Error, scope.Activity!.Status);
            Assert.IsTrue(scope.Activity.Events.Any(static e => e.Name == "exception"));
        } finally
        {
            listener.Dispose();
        }
    }

    #endregion

    #region Dispose

    [TestMethod]
    public void Dispose_NoActivity_DoesNotThrow()
    {
        using ActivitySource source = new($"test-source-{Guid.NewGuid():N}");
        ActivityScope scope = ActivityScope.Start(source, "operation");

        scope.Dispose();
    }

    [TestMethod]
    public void Dispose_WithActivity_SetsDuration()
    {
        using ActivitySource source = CreateListenedSource(out ActivityListener listener);

        try
        {
            ActivityScope scope = ActivityScope.Start(source, "operation");
            Activity activity = scope.Activity!;

            scope.Dispose();

            Assert.AreNotEqual(TimeSpan.Zero, activity.Duration);
        } finally
        {
            listener.Dispose();
        }
    }

    #endregion
}
