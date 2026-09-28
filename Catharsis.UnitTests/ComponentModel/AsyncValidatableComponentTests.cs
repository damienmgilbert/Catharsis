using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="AsyncValidatableComponent"/> class.
///</summary>
[TestClass]
public class AsyncValidatableComponentTests
{
    #region SetPropertyAndValidateAsync

    [TestMethod]
    public async Task SetPropertyAndValidateAsync_NullSetter_Throws()
    {
        TestComponent component = new();
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => component.SetName(null!, "Alice"));
    }

    [TestMethod]
    public async Task SetPropertyAndValidateAsync_InvokesSetterAndRaisesPropertyChanged()
    {
        TestComponent component = new();
        List<string?> changedProperties = [];
        component.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

        await component.SetName(value => component.Name = value, "Alice");

        Assert.AreEqual("Alice", component.Name);
        CollectionAssert.Contains(changedProperties, nameof(TestComponent.Name));
    }

    [TestMethod]
    public async Task SetPropertyAndValidateAsync_RunsAsyncValidation()
    {
        TestComponent component = new();
        await component.SetName(value => component.Name = value, "");

        Assert.IsTrue(component.HasErrors);
        CollectionAssert.Contains(component.GetErrors(nameof(TestComponent.Name)).Cast<string>().ToList(), "Name must not be empty.");
    }

    #endregion

    #region DisposeAsync

    [TestMethod]
    public async Task DisposeAsync_InvokesDisposeAsyncCore()
    {
        TestComponent component = new();
        await component.DisposeAsync();

        Assert.IsTrue(component.DisposeAsyncCoreCalled);
    }

    #endregion

    private sealed class TestComponent : AsyncValidatableComponent
    {
        #region Public methods
        public Task SetName(Action<string> setter, string value) => SetPropertyAndValidateAsync(value, setter, propertyName: nameof(Name));

        protected override async Task ValidatePropertyAsync(string? propertyName, object? value, CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            if(propertyName == nameof(Name))
            {
                if(string.IsNullOrEmpty(value as string))
                {
                    SetErrors(["Name must not be empty."], propertyName);
                } else
                {
                    ClearErrors(propertyName);
                }
            }
        }

        protected override ValueTask DisposeAsyncCore()
        {
            DisposeAsyncCoreCalled = true;
            return base.DisposeAsyncCore();
        }
        #endregion

        #region Public properties
        public bool DisposeAsyncCoreCalled { get; private set; }
        public string? Name { get; set; }
        #endregion
    }
}
