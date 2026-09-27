using Catharsis.Mvvm;

namespace Catharsis.UnitTests.Mvvm;

///<summary>
///Unit tests for the <see cref="DebouncedObservableProperty{T}"/> class.
///</summary>
[TestClass]
public class DebouncedObservablePropertyTests
{
    #region Value

    [TestMethod]
    public void Value_Set_UpdatesImmediately()
    {
        using DebouncedObservableProperty<string> property = new("initial", TimeSpan.FromMilliseconds(50));
        property.Value = "updated";
        Assert.AreEqual("updated", property.Value);
    }

    [TestMethod]
    public void Value_Set_RaisesPropertyChangedImmediately()
    {
        using DebouncedObservableProperty<string> property = new("initial", TimeSpan.FromMilliseconds(200));
        bool raised = false;
        property.PropertyChanged += (_, args) => raised |= args.PropertyName == nameof(DebouncedObservableProperty<string>.Value);

        property.Value = "updated";

        Assert.IsTrue(raised);
    }

    [TestMethod]
    public void Value_SetAfterDispose_Throws()
    {
        DebouncedObservableProperty<string> property = new("initial", TimeSpan.FromMilliseconds(50));
        property.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => property.Value = "updated");
    }

    #endregion

    #region DebouncedValueChanged

    [TestMethod]
    public async Task DebouncedValueChanged_AfterDelayWithNoFurtherChanges_Fires()
    {
        using DebouncedObservableProperty<string> property = new("initial", TimeSpan.FromMilliseconds(30));
        string? debounced = null;
        property.DebouncedValueChanged += (_, value) => debounced = value;

        property.Value = "final";
        await Task.Delay(TimeSpan.FromMilliseconds(200));

        Assert.AreEqual("final", debounced);
    }

    [TestMethod]
    public async Task DebouncedValueChanged_RapidChanges_OnlyFinalValueFires()
    {
        using DebouncedObservableProperty<string> property = new("initial", TimeSpan.FromMilliseconds(50));
        List<string> debouncedValues = [];
        property.DebouncedValueChanged += (_, value) => debouncedValues.Add(value);

        property.Value = "a";
        property.Value = "b";
        property.Value = "c";
        await Task.Delay(TimeSpan.FromMilliseconds(300));

        CollectionAssert.AreEqual(new[] { "c" }, debouncedValues);
    }

    [TestMethod]
    public async Task Dispose_PendingDebounce_NeverFires()
    {
        DebouncedObservableProperty<string> property = new("initial", TimeSpan.FromMilliseconds(50));
        bool fired = false;
        property.DebouncedValueChanged += (_, _) => fired = true;

        property.Value = "updated";
        property.Dispose();
        await Task.Delay(TimeSpan.FromMilliseconds(200));

        Assert.IsFalse(fired);
    }

    #endregion
}
