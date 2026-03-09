using System.ComponentModel;
using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="PropertyObserver"/> class.
///</summary>
[TestClass]
public class PropertyObserverTests
{
    #region Public methods
    [TestMethod]
    public void Constructor_NullSource_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new PropertyObserver(null!)); }
    [TestMethod]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        NotifySource source = new NotifySource();
        PropertyObserver observer = new PropertyObserver(source);

        observer.Dispose();
        observer.Dispose(); // should not throw
    }

    [TestMethod]
    public void Dispose_UnsubscribesFromSource()
    {
        NotifySource source = new NotifySource();
        int callCount = 0;

        PropertyObserver observer = new PropertyObserver(source)
            .OnChanged(nameof(NotifySource.Name), () => callCount++);

        observer.Dispose();
        source.Name = "Alice";

        Assert.AreEqual(0, callCount);
    }

    [TestMethod]
    public void MultipleHandlers_SameProperty_AllInvoked()
    {
        NotifySource source = new NotifySource();
        int count1 = 0;
        int count2 = 0;

        using PropertyObserver observer = new PropertyObserver(source)
            .OnChanged(nameof(NotifySource.Name), () => count1++)
            .OnChanged(nameof(NotifySource.Name), () => count2++);

        source.Name = "Test";

        Assert.AreEqual(1, count1);
        Assert.AreEqual(1, count2);
    }

    [TestMethod]
    public void NullPropertyName_InvokesAllHandlers()
    {
        NotifySource source = new NotifySource();
        int callCount = 0;

        using PropertyObserver observer = new PropertyObserver(source)
            .OnChanged(nameof(NotifySource.Name), () => callCount++);

        source.RaiseAllPropertiesChanged();

        Assert.AreEqual(1, callCount);
    }

    [TestMethod]
    public void OnChanged_AfterDispose_Throws()
    {
        NotifySource source = new NotifySource();
        PropertyObserver observer = new PropertyObserver(source);
        observer.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(
        () => observer.OnChanged(
              "Name",
              () =>
        {
        }));
    }

    [TestMethod]
    public void OnChanged_DoesNotInvokeForOtherProperties()
    {
        NotifySource source = new NotifySource();
        int callCount = 0;

        using PropertyObserver observer = new PropertyObserver(source)
            .OnChanged("OtherProperty", () => callCount++);

        source.Name = "Alice";

        Assert.AreEqual(0, callCount);
    }

    [TestMethod]
    public void OnChanged_InvokesHandlerWhenPropertyChanges()
    {
        NotifySource source = new NotifySource();
        int callCount = 0;

        using PropertyObserver observer = new PropertyObserver(source)
            .OnChanged(nameof(NotifySource.Name), () => callCount++);

        source.Name = "Alice";

        Assert.AreEqual(1, callCount);
    }

    [TestMethod]
    public void StopAll_RemovesAllHandlers()
    {
        NotifySource source = new NotifySource();
        int callCount = 0;

        using PropertyObserver observer = new PropertyObserver(source)
            .OnChanged(nameof(NotifySource.Name), () => callCount++)
            .StopAll();

        source.Name = "Alice";

        Assert.AreEqual(0, callCount);
    }

    [TestMethod]
    public void StopObserving_RemovesHandlersForProperty()
    {
        NotifySource source = new NotifySource();
        int callCount = 0;

        using PropertyObserver observer = new PropertyObserver(source)
            .OnChanged(nameof(NotifySource.Name), () => callCount++)
            .StopObserving(nameof(NotifySource.Name));

        source.Name = "Alice";

        Assert.AreEqual(0, callCount);
    }
    #endregion

    sealed class NotifySource : INotifyPropertyChanged
    {
        #region Fields
        string _name = string.Empty;
        #endregion

        #region Events
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion

        #region Public methods
        public void RaiseAllPropertiesChanged() { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null)); }
        #endregion

        #region Public properties
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
        #endregion
    }
}
