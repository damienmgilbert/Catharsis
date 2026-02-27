using System.ComponentModel;
using System.Runtime.CompilerServices;
using Catharsis.ComponentModel.Binding;

namespace Catharsis.UnitTests.ComponentModel.Binding;

[TestClass]
public sealed class ComponentModelBinderTests
{
    #region Public methods
    [TestMethod]
    public void Bind_AfterDispose_ThrowsObjectDisposedException()
    {
        ComponentModelBinder binder = new ComponentModelBinder();
        binder.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => binder.Bind(new NotifyModel(), "Name", new NotifyModel(), "Name"));
    }

    [TestMethod]
    public void Bind_InvalidProperty_ThrowsArgumentException()
    {
        using ComponentModelBinder binder = new ComponentModelBinder();
        NotifyModel source = new NotifyModel();
        NotifyModel target = new NotifyModel();

        Assert.ThrowsExactly<ArgumentException>(() => binder.Bind(source, "NonExistent", target, "Name"));
    }

    [TestMethod]
    public void Bind_NullSource_ThrowsArgumentNullException()
    {
        using ComponentModelBinder binder = new ComponentModelBinder();
        NotifyModel target = new NotifyModel();

        Assert.ThrowsExactly<ArgumentNullException>(() => binder.Bind(null!, "Name", target, "Name"));
    }

    [TestMethod]
    public void Bind_NullTarget_ThrowsArgumentNullException()
    {
        using ComponentModelBinder binder = new ComponentModelBinder();
        NotifyModel source = new NotifyModel();

        Assert.ThrowsExactly<ArgumentNullException>(() => binder.Bind(source, "Name", null!, "Name"));
    }

    [TestMethod]
    public void Bind_TwoWay_InitialSyncsSourceToTarget()
    {
        using ComponentModelBinder binder = new ComponentModelBinder();
        NotifyModel source = new NotifyModel { Name = "Alice" };
        NotifyModel target = new NotifyModel();

        binder.Bind(source, "Name", target, "Name");

        Assert.AreEqual("Alice", target.Name);
    }

    [TestMethod]
    public void Bind_TwoWay_SourceChangePropagatesToTarget()
    {
        using ComponentModelBinder binder = new ComponentModelBinder();
        NotifyModel source = new NotifyModel { Name = "Alice" };
        NotifyModel target = new NotifyModel();
        binder.Bind(source, "Name", target, "Name");

        source.Name = "Bob";

        Assert.AreEqual("Bob", target.Name);
    }

    [TestMethod]
    public void Bind_TwoWay_TargetChangePropagatesToSource()
    {
        using ComponentModelBinder binder = new ComponentModelBinder();
        NotifyModel source = new NotifyModel { Name = "Alice" };
        NotifyModel target = new NotifyModel();
        binder.Bind(source, "Name", target, "Name");

        target.Name = "Charlie";

        Assert.AreEqual("Charlie", source.Name);
    }

    [TestMethod]
    public void BindOneWay_SourceChangePropagatesToTarget()
    {
        using ComponentModelBinder binder = new ComponentModelBinder();
        NotifyModel source = new NotifyModel { Name = "Alice" };
        NotifyModel target = new NotifyModel();
        binder.BindOneWay(source, "Name", target, "Name");

        source.Name = "Bob";

        Assert.AreEqual("Bob", target.Name);
    }

    [TestMethod]
    public void BindOneWay_TargetChangeDoesNotPropagateToSource()
    {
        using ComponentModelBinder binder = new ComponentModelBinder();
        NotifyModel source = new NotifyModel { Name = "Alice" };
        NotifyModel target = new NotifyModel();
        binder.BindOneWay(source, "Name", target, "Name");

        target.Name = "Charlie";

        Assert.AreEqual("Alice", source.Name);
    }

    [TestMethod]
    public void Count_ReflectsActiveBindings()
    {
        using ComponentModelBinder binder = new ComponentModelBinder();
        NotifyModel source = new NotifyModel();
        NotifyModel target = new NotifyModel();

        Assert.AreEqual(0, binder.Count);

        binder.Bind(source, "Name", target, "Name");
        Assert.AreEqual(1, binder.Count);
    }

    [TestMethod]
    public void Dispose_CleansUpBindings()
    {
        NotifyModel source = new NotifyModel { Name = "Alice" };
        NotifyModel target = new NotifyModel();
        ComponentModelBinder binder = new ComponentModelBinder();
        binder.Bind(source, "Name", target, "Name");

        binder.Dispose();

        source.Name = "Bob";
        Assert.AreEqual("Alice", target.Name);
    }

    [TestMethod]
    public void Unbind_RemovesSpecificBindings()
    {
        using ComponentModelBinder binder = new ComponentModelBinder();
        NotifyModel source = new NotifyModel { Name = "Alice" };
        NotifyModel target = new NotifyModel();
        binder.Bind(source, "Name", target, "Name");

        binder.Unbind(source, target);

        Assert.AreEqual(0, binder.Count);

        source.Name = "Bob";
        Assert.AreEqual("Alice", target.Name);
    }

    [TestMethod]
    public void UnbindAll_RemovesAllBindings()
    {
        using ComponentModelBinder binder = new ComponentModelBinder();
        NotifyModel s1 = new NotifyModel();
        NotifyModel t1 = new NotifyModel();
        NotifyModel s2 = new NotifyModel();
        NotifyModel t2 = new NotifyModel();
        binder.Bind(s1, "Name", t1, "Name");
        binder.Bind(s2, "Age", t2, "Age");

        binder.UnbindAll();

        Assert.AreEqual(0, binder.Count);
    }
    #endregion

    sealed class NotifyModel : INotifyPropertyChanged
    {
        #region Fields
        int _age;
        string? _name;
        #endregion

        #region Events
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion

        #region Private methods
        void OnPropertyChanged([CallerMemberName] string? prop = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop)); }
        #endregion

        #region Public properties
        public int Age
        {
            get => _age;
            set
            {
                _age = value;
                OnPropertyChanged();
            }
        }

        public string? Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        #endregion
    }
}
