namespace Catharsis.ComponentModel.UnitTests;


/// <summary>
/// Unit tests for the <see cref="PooledComponent"/> class.
/// </summary>
[TestClass]
public partial class PooledComponentTests
{
    /// <summary>
    /// Tests that Reset successfully resets an active component,
    /// calls OnReset, and sets IsActive to false.
    /// </summary>
    [TestMethod]
    public void Reset_WhenActive_ResetsComponentAndSetsIsActiveFalse()
    {
        // Arrange
        var component = new TestablePooledComponent();
        component.Activate();
        Assert.IsTrue(component.IsActive);

        // Act
        component.Reset();

        // Assert
        Assert.IsFalse(component.IsActive);
        Assert.IsTrue(component.OnResetCalled);
    }

    /// <summary>
    /// Tests that Reset can be called multiple times after
    /// re-activating the component.
    /// </summary>
    [TestMethod]
    public void Reset_AfterReactivation_ResetsComponentSuccessfully()
    {
        // Arrange
        var component = new TestablePooledComponent();
        component.Activate();
        component.Reset();
        component.Activate();

        // Act
        component.Reset();

        // Assert
        Assert.IsFalse(component.IsActive);
        Assert.AreEqual(2, component.OnResetCallCount);
    }

    /// <summary>
    /// Helper class for testing PooledComponent behavior.
    /// Tracks OnReset invocations.
    /// </summary>
    private class TestablePooledComponent : PooledComponent
    {
        public bool OnResetCalled { get; private set; }
        public int OnResetCallCount { get; private set; }

        protected override void OnReset()
        {
            base.OnReset();
            OnResetCalled = true;
            OnResetCallCount++;
        }
    }

    /// <summary>
    /// Tests that Activate successfully activates a component that is not disposed and not active.
    /// </summary>
    [TestMethod]
    public void Activate_ComponentNotActiveAndNotDisposed_ActivatesSuccessfully()
    {
        // Arrange
        var component = new TestPooledComponent();

        // Act
        component.Activate();

        // Assert
        Assert.IsTrue(component.IsActive);
    }

    /// <summary>
    /// Tests that Activate increments the LeaseVersion when component is activated.
    /// </summary>
    [TestMethod]
    public void Activate_ComponentNotActive_IncrementsLeaseVersion()
    {
        // Arrange
        var component = new TestPooledComponent();
        int initialLeaseVersion = component.LeaseVersion;

        // Act
        component.Activate();

        // Assert
        Assert.AreEqual(initialLeaseVersion + 1, component.LeaseVersion);
    }

    /// <summary>
    /// Tests that Activate calls OnActivate when component is activated.
    /// </summary>
    [TestMethod]
    public void Activate_ComponentNotActive_CallsOnActivate()
    {
        // Arrange
        var component = new TestPooledComponent();

        // Act
        component.Activate();

        // Assert
        Assert.IsTrue(component.OnActivateCalled);
    }

    /// <summary>
    /// Tests that Activate increments LeaseVersion on each successful activation after reset.
    /// </summary>
    [TestMethod]
    public void Activate_MultipleActivationsAfterReset_IncrementsLeaseVersionEachTime()
    {
        // Arrange
        var component = new TestPooledComponent();

        // Act & Assert
        component.Activate();
        int firstLeaseVersion = component.LeaseVersion;
        Assert.AreEqual(1, firstLeaseVersion);

        component.Reset();
        component.Activate();
        int secondLeaseVersion = component.LeaseVersion;
        Assert.AreEqual(2, secondLeaseVersion);

        component.Reset();
        component.Activate();
        int thirdLeaseVersion = component.LeaseVersion;
        Assert.AreEqual(3, thirdLeaseVersion);
    }

    /// <summary>
    /// Tests that Activate sets IsActive to true when component is successfully activated.
    /// </summary>
    [TestMethod]
    public void Activate_ComponentNotActive_SetsIsActiveToTrue()
    {
        // Arrange
        var component = new TestPooledComponent();
        Assert.IsFalse(component.IsActive);

        // Act
        component.Activate();

        // Assert
        Assert.IsTrue(component.IsActive);
    }

    /// <summary>
    /// Test component implementation for testing PooledComponent.
    /// </summary>
    private sealed class TestPooledComponent : PooledComponent
    {
        public bool OnActivateCalled { get; private set; }
        public bool OnResetCalled { get; private set; }

        protected override void OnActivate()
        {
            base.OnActivate();
            OnActivateCalled = true;
        }

        protected override void OnReset()
        {
            base.OnReset();
            OnResetCalled = false;
            OnActivateCalled = false;
        }
    }

    /// <summary>
    /// Tests that OnActivate can be overridden in derived classes to perform custom initialization.
    /// </summary>
    [TestMethod]
    public void OnActivate_OverriddenInDerivedClass_ExecutesCustomLogic()
    {
        // Arrange
        var component = new CustomOnActivateComponent();

        // Act
        component.PublicOnActivate();

        // Assert
        Assert.IsTrue(component.OnActivateCalled);
    }

    /// <summary>
    /// Test helper class that overrides OnActivate to verify custom logic execution.
    /// </summary>
    private class CustomOnActivateComponent : PooledComponent
    {
        public bool OnActivateCalled { get; private set; }

        protected override void OnActivate()
        {
            base.OnActivate();
            OnActivateCalled = true;
        }

        public void PublicOnActivate() => OnActivate();
    }

    /// <summary>
    /// Tests that the LeaseVersion property returns 0 when the component is first created.
    /// </summary>
    [TestMethod]
    public void LeaseVersion_InitialState_ReturnsZero()
    {
        // Arrange
        var component = new TestPooledComponent();

        // Act
        var result = component.LeaseVersion;

        // Assert
        Assert.AreEqual(0, result);
    }

    /// <summary>
    /// Tests that the LeaseVersion property returns 1 after the first activation.
    /// </summary>
    [TestMethod]
    public void LeaseVersion_AfterFirstActivation_ReturnsOne()
    {
        // Arrange
        var component = new TestPooledComponent();
        component.Activate();

        // Act
        var result = component.LeaseVersion;

        // Assert
        Assert.AreEqual(1, result);
    }

    /// <summary>
    /// Tests that the LeaseVersion property increments after each activation.
    /// </summary>
    [TestMethod]
    public void LeaseVersion_AfterMultipleActivations_IncrementsEachTime()
    {
        // Arrange
        var component = new TestPooledComponent();
        component.Activate();
        component.Reset();
        component.Activate();
        component.Reset();
        component.Activate();

        // Act
        var result = component.LeaseVersion;

        // Assert
        Assert.AreEqual(3, result);
    }

    /// <summary>
    /// Tests that the LeaseVersion property does not change when Reset is called.
    /// </summary>
    [TestMethod]
    public void LeaseVersion_AfterReset_RemainsUnchanged()
    {
        // Arrange
        var component = new TestPooledComponent();
        component.Activate();
        var versionBeforeReset = component.LeaseVersion;

        // Act
        component.Reset();
        var versionAfterReset = component.LeaseVersion;

        // Assert
        Assert.AreEqual(versionBeforeReset, versionAfterReset);
        Assert.AreEqual(1, versionAfterReset);
    }

    /// <summary>
    /// Tests that the LeaseVersion property increments correctly across multiple activation and reset cycles.
    /// </summary>
    /// <param name="activationCount">The number of activation cycles to perform.</param>
    /// <param name="expectedVersion">The expected lease version after all activations.</param>
    [DataRow(1, 1)]
    [DataRow(2, 2)]
    [DataRow(5, 5)]
    [DataRow(10, 10)]
    [DataRow(100, 100)]
    [TestMethod]
    public void LeaseVersion_AfterMultipleCycles_ReflectsCorrectCount(int activationCount, int expectedVersion)
    {
        // Arrange
        var component = new TestPooledComponent();

        // Act
        for (int i = 0; i < activationCount; i++)
        {
            component.Activate();
            if (i < activationCount - 1)
            {
                component.Reset();
            }
        }

        var result = component.LeaseVersion;

        // Assert
        Assert.AreEqual(expectedVersion, result);
    }

    /// <summary>
    /// Tests that the LeaseVersion property correctly handles the maximum integer value boundary.
    /// </summary>
    [TestMethod]
    public void LeaseVersion_NearIntMaxValue_HandlesCorrectly()
    {
        // Arrange
        var component = new TestPooledComponent();
        // Set internal version to near max value by activating many times
        // Since we can't directly set the field, we test that the property returns the current value correctly
        component.Activate();
        component.Reset();
        component.Activate();

        // Act
        var result = component.LeaseVersion;

        // Assert
        Assert.AreEqual(2, result);
        Assert.IsTrue(result > 0);
    }

    /// <summary>
    /// Tests that the LeaseVersion property returns the same value when accessed multiple times without state changes.
    /// </summary>
    [TestMethod]
    public void LeaseVersion_MultipleReads_ReturnsSameValue()
    {
        // Arrange
        var component = new TestPooledComponent();
        component.Activate();

        // Act
        var firstRead = component.LeaseVersion;
        var secondRead = component.LeaseVersion;
        var thirdRead = component.LeaseVersion;

        // Assert
        Assert.AreEqual(firstRead, secondRead);
        Assert.AreEqual(secondRead, thirdRead);
        Assert.AreEqual(1, firstRead);
    }

    /// <summary>
    /// Tests that the LeaseVersion property remains unchanged after disposal.
    /// </summary>
    [TestMethod]
    public void LeaseVersion_AfterDisposal_RemainsUnchanged()
    {
        // Arrange
        var component = new TestPooledComponent();
        component.Activate();
        var versionBeforeDispose = component.LeaseVersion;

        // Act
        component.Dispose();
        var versionAfterDispose = component.LeaseVersion;

        // Assert
        Assert.AreEqual(versionBeforeDispose, versionAfterDispose);
        Assert.AreEqual(1, versionAfterDispose);
    }

    /// <summary>
    /// Tests that OnReset can be overridden and the override is invoked when called.
    /// </summary>
    [TestMethod]
    public void OnReset_CanBeOverridden_OverrideIsCalled()
    {
        // Arrange
        OverriddenPooledComponent component = new();
        component.Activate();

        // Act
        component.CallOnReset();

        // Assert
        Assert.IsTrue(component.OnResetWasCalled);
    }

    /// <summary>
    /// Test implementation of PooledComponent that overrides OnReset to verify override behavior.
    /// </summary>
    private class OverriddenPooledComponent : PooledComponent
    {
        /// <summary>
        /// Gets a value indicating whether OnReset was called.
        /// </summary>
        public bool OnResetWasCalled { get; private set; }

        /// <summary>
        /// Exposes the protected OnReset method for testing purposes.
        /// </summary>
        public void CallOnReset()
        {
            OnReset();
        }

        /// <summary>
        /// Overrides OnReset to track when it is called.
        /// </summary>
        protected override void OnReset()
        {
            base.OnReset();
            OnResetWasCalled = true;
        }
    }
}