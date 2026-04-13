using Catharsis.ComponentModel.Observability;

namespace Catharsis.UnitTests.ComponentModel.Observability;

///<summary>
///Unit tests for the <see cref="ChangeEntry"/> class.
///</summary>
[TestClass]
public sealed class ChangeEntryTests
{
    [TestMethod]
    public void Constructor_ExplicitTimestamp_UsesProvided()
    {
        DateTime ts = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        ChangeEntry entry = new("X", null, null, ts);

        Assert.AreEqual(ts, entry.Timestamp);
    }

    [TestMethod]
    public void Constructor_NullTimestamp_DefaultsToUtcNow()
    {
        DateTime before = DateTime.UtcNow;
        ChangeEntry entry = new("Prop", "a", "b");
        DateTime after = DateTime.UtcNow;

        Assert.IsTrue((entry.Timestamp >= before) && (entry.Timestamp <= after));
    }

    [TestMethod]
    public void Constructor_SetsProperties()
    {
        ChangeEntry entry = new("Name", "old", "new");

        Assert.AreEqual("Name", entry.PropertyName);
        Assert.AreEqual("old", entry.OldValue);
        Assert.AreEqual("new", entry.NewValue);
        Assert.IsNotNull(entry.Timestamp);
    }

    [TestMethod]
    public void Equality_SamePropertyNameAndValues_DifferentTimestamp_AreNotEqual()
    {
        DateTime ts1 = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime ts2 = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        ChangeEntry a = new("P", "old", "new", ts1);
        ChangeEntry b = new("P", "old", "new", ts2);

        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void ToString_ContainsPropertyNameAndValues()
    {
        ChangeEntry entry = new("Name", "old", "new");
        string result = entry.ToString();

        Assert.Contains("Name", result);
        Assert.Contains("old", result);
        Assert.Contains("new", result);
    }
}
