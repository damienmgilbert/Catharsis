using System.ComponentModel;
using System.Runtime.CompilerServices;

using Catharsis.ComponentModel.Binding;

namespace Catharsis.UnitTests.ComponentModel.Binding;

[TestClass]
public sealed class ComponentModelBinderTests
{
    private sealed class NotifyModel : INotifyPropertyChanged
    {
        private string? _name;
        private int _age;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public int Age
        {
            get => _age;
            set { _age = value; OnPropertyChanged(); }
        }

        private void OnPropertyChanged([CallerMemberName] string? prop = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    [TestMethod]
    public void Bind_NullSource_ThrowsArgumentNullException()
    {
        using var binder = new ComponentModelBinder();
        var target = new NotifyModel();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => binder.Bind(null!, "Name", target, "Name"));
    }

    [TestMethod]
    public void Bind_NullTarget_ThrowsArgumentNullException()
    {
        using var binder = new ComponentModelBinder();
        var source = new NotifyModel();

        Assert.ThrowsExactly<ArgumentNullException>(
            () => binder.Bind(source, "Name", null!, "Name"));
    }

    [TestMethod]
    public void Bind_InvalidProperty_ThrowsArgumentException()
    {
        using var binder = new ComponentModelBinder();
        var source = new NotifyModel();
        var target = new NotifyModel();

        Assert.ThrowsExactly<ArgumentException>(
            () => binder.Bind(source, "NonExistent", target, "Name"));
    }

    [TestMethod]
    public void Bind_TwoWay_InitialSyncsSourceToTarget()
    {
        using var binder = new ComponentModelBinder();
        var source = new NotifyModel { Name = "Alice" };
        var target = new NotifyModel();

        binder.Bind(source, "Name", target, "Name");

        Assert.AreEqual("Alice", target.Name);
    }

    [TestMethod]
    public void Bind_TwoWay_SourceChangePropagatesToTarget()
    {
        using var binder = new ComponentModelBinder();
        var source = new NotifyModel { Name = "Alice" };
        var target = new NotifyModel();
        binder.Bind(source, "Name", target, "Name");

        source.Name = "Bob";

        Assert.AreEqual("Bob", target.Name);
    }

    [TestMethod]
    public void Bind_TwoWay_TargetChangePropagatesToSource()
    {
        using var binder = new ComponentModelBinder();
        var source = new NotifyModel { Name = "Alice" };
        var target = new NotifyModel();
        binder.Bind(source, "Name", target, "Name");

        target.Name = "Charlie";

        Assert.AreEqual("Charlie", source.Name);
    }

    [TestMethod]
    public void BindOneWay_SourceChangePropagatesToTarget()
    {
        using var binder = new ComponentModelBinder();
        var source = new NotifyModel { Name = "Alice" };
        var target = new NotifyModel();
        binder.BindOneWay(source, "Name", target, "Name");

        source.Name = "Bob";

        Assert.AreEqual("Bob", target.Name);
    }

    [TestMethod]
    public void BindOneWay_TargetChangeDoesNotPropagateToSource()
    {
        using var binder = new ComponentModelBinder();
        var source = new NotifyModel { Name = "Alice" };
        var target = new NotifyModel();
        binder.BindOneWay(source, "Name", target, "Name");

        target.Name = "Charlie";

        Assert.AreEqual("Alice", source.Name);
    }

    [TestMethod]
    public void Count_ReflectsActiveBindings()
    {
        using var binder = new ComponentModelBinder();
        var source = new NotifyModel();
        var target = new NotifyModel();

        Assert.AreEqual(0, binder.Count);

        binder.Bind(source, "Name", target, "Name");
        Assert.AreEqual(1, binder.Count);
    }

    [TestMethod]
    public void Unbind_RemovesSpecificBindings()
    {
        using var binder = new ComponentModelBinder();
        var source = new NotifyModel { Name = "Alice" };
        var target = new NotifyModel();
        binder.Bind(source, "Name", target, "Name");

        binder.Unbind(source, target);

        Assert.AreEqual(0, binder.Count);

        source.Name = "Bob";
        Assert.AreEqual("Alice", target.Name);
    }

    [TestMethod]
    public void UnbindAll_RemovesAllBindings()
    {
        using var binder = new ComponentModelBinder();
        var s1 = new NotifyModel();
        var t1 = new NotifyModel();
        var s2 = new NotifyModel();
        var t2 = new NotifyModel();
        binder.Bind(s1, "Name", t1, "Name");
        binder.Bind(s2, "Age", t2, "Age");

        binder.UnbindAll();

        Assert.AreEqual(0, binder.Count);
    }

    [TestMethod]
    public void Dispose_CleansUpBindings()
    {
        var source = new NotifyModel { Name = "Alice" };
        var target = new NotifyModel();
        var binder = new ComponentModelBinder();
        binder.Bind(source, "Name", target, "Name");

        binder.Dispose();

        source.Name = "Bob";
        Assert.AreEqual("Alice", target.Name);
    }

    [TestMethod]
    public void Bind_AfterDispose_ThrowsObjectDisposedException()
    {
        var binder = new ComponentModelBinder();
        binder.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => binder.Bind(new NotifyModel(), "Name", new NotifyModel(), "Name"));
    }
}
