using Catharsis;
using Catharsis.Collections;

namespace Catharsis.UnitTests.Collections;

[TestClass]
public class EventCollectionTests
{
    [TestMethod]
    public void Add_RaisesItemAddedEvent()
    {
        EventCollection<int> c = new EventCollection<int>();
        int raised = -1;
        c.ItemAdded += (_, item) => raised = item;

        c.Add(42);

        Assert.AreEqual(42, raised);
        Assert.HasCount(1, c);
    }

    [TestMethod]
    public void Remove_ExistingItem_RaisesItemRemovedEvent()
    {
        EventCollection<int> c = new EventCollection<int>();
        c.Add(1);
        int raised = -1;
        c.ItemRemoved += (_, item) => raised = item;

        Assert.IsTrue(c.Remove(1));
        Assert.AreEqual(1, raised);
    }

    [TestMethod]
    public void Remove_NonexistentItem_ReturnsFalse()
    {
        EventCollection<int> c = new EventCollection<int>();
        bool eventFired = false;
        c.ItemRemoved += (_, _) => eventFired = true;

        Assert.IsFalse(c.Remove(99));
        Assert.IsFalse(eventFired);
    }

    [TestMethod]
    public void Clear_RaisesClearedEvent()
    {
        EventCollection<int> c = new EventCollection<int>();
        c.Add(1);
        bool cleared = false;
        c.Cleared += (_, _) => cleared = true;

        c.Clear();

        Assert.IsTrue(cleared);
        Assert.IsEmpty(c);
    }

    [TestMethod]
    public void Contains_ReturnsCorrectResult()
    {
        EventCollection<string> c = new EventCollection<string>();
        c.Add("test");
        Assert.IsTrue(c.Contains("test"));
        Assert.IsFalse(c.Contains("other"));
    }
}
