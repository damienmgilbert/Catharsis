using Catharsis.ComponentModel;
using System.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

[TestClass]
public class PropertyObserverTests
{
    private sealed class NotifySource : INotifyPropertyChanged
    {
        private string _name = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public void RaiseAllPropertiesChanged() =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    [TestMethod]
    public void OnChanged_InvokesHandlerWhenPropertyChanges()
    {
        var source = new NotifySource();
        var callCount = 0;

        using var observer = new PropertyObserver(source)
            .OnChanged(nameof(NotifySource.Name), () => callCount++);

        source.Name = "Alice";

        Assert.AreEqual(1, callCount);
    }

    [TestMethod]
    public void OnChanged_DoesNotInvokeForOtherProperties()
    {
        var source = new NotifySource();
        var callCount = 0;

        using var observer = new PropertyObserver(source)
            .OnChanged("OtherProperty", () => callCount++);

        source.Name = "Alice";

        Assert.AreEqual(0, callCount);
    }

    [TestMethod]
    public void NullPropertyName_InvokesAllHandlers()
    {
        var source = new NotifySource();
        var callCount = 0;

        using var observer = new PropertyObserver(source)
            .OnChanged(nameof(NotifySource.Name), () => callCount++);

        source.RaiseAllPropertiesChanged();

        Assert.AreEqual(1, callCount);
    }

    [TestMethod]
    public void StopObserving_RemovesHandlersForProperty()
    {
        var source = new NotifySource();
        var callCount = 0;

        using var observer = new PropertyObserver(source)
            .OnChanged(nameof(NotifySource.Name), () => callCount++)
            .StopObserving(nameof(NotifySource.Name));

        source.Name = "Alice";

        Assert.AreEqual(0, callCount);
    }

    [TestMethod]
    public void StopAll_RemovesAllHandlers()
    {
        var source = new NotifySource();
        var callCount = 0;

        using var observer = new PropertyObserver(source)
            .OnChanged(nameof(NotifySource.Name), () => callCount++)
            .StopAll();

        source.Name = "Alice";

        Assert.AreEqual(0, callCount);
    }

    [TestMethod]
    public void Dispose_UnsubscribesFromSource()
    {
        var source = new NotifySource();
        var callCount = 0;

        var observer = new PropertyObserver(source)
            .OnChanged(nameof(NotifySource.Name), () => callCount++);

        observer.Dispose();
        source.Name = "Alice";

        Assert.AreEqual(0, callCount);
    }

    [TestMethod]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var source = new NotifySource();
        var observer = new PropertyObserver(source);

        observer.Dispose();
        observer.Dispose(); // should not throw
    }

    [TestMethod]
    public void Constructor_NullSource_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new PropertyObserver(null!));
    }

    [TestMethod]
    public void OnChanged_AfterDispose_Throws()
    {
        var source = new NotifySource();
        var observer = new PropertyObserver(source);
        observer.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => observer.OnChanged("Name", () => { }));
    }

    [TestMethod]
    public void MultipleHandlers_SameProperty_AllInvoked()
    {
        var source = new NotifySource();
        var count1 = 0;
        var count2 = 0;

        using var observer = new PropertyObserver(source)
            .OnChanged(nameof(NotifySource.Name), () => count1++)
            .OnChanged(nameof(NotifySource.Name), () => count2++);

        source.Name = "Test";

        Assert.AreEqual(1, count1);
        Assert.AreEqual(1, count2);
    }
}
