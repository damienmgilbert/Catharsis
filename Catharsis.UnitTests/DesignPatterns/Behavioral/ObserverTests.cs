using Catharsis.DesignPatterns.Behavioral;

namespace Catharsis.UnitTests.DesignPatterns.Behavioral;

[TestClass]
public class ObserverTests
{
    #region Public methods

    ///<summary>
    ///Verifies that Notify with an empty observers array does not invoke any observers and returns the original object.
    ///</summary>
    [TestMethod]
    public void Notify_EmptyObservers_ReturnsOriginalObjectWithoutInvokingObservers()
    {
        // Arrange
        string obj = "test";
        // Act
        string result = new Observer().Notify(obj);
        // Assert
        Assert.AreSame(obj, result);
    }

    ///<summary>
    ///Verifies that Notify can be chained fluently by using the returned object.
    ///</summary>
    [TestMethod]
    public void Notify_FluentChaining_WorksCorrectly()
    {
        // Arrange
        int obj = 100;
        int invocationCount = 0;
        Action<int> observer = x => invocationCount++;
        // Act
        Observer n = new Observer();
        int result = n.Notify(n.Notify(n.Notify(obj, observer), observer), observer);
        // Assert
        Assert.AreEqual(3, invocationCount);
        Assert.AreEqual(obj, result);
    }

    ///<summary>
    ///Verifies that Notify with multiple observers invokes all observers in order with the correct object and returns
    ///the original object.
    ///</summary>
    [TestMethod]
    public void Notify_MultipleObservers_InvokesAllObserversInOrderAndReturnsOriginalObject()
    {
        // Arrange
        string obj = "notify";
        List<int> invocationOrder = new List<int>();
        Action<string> observer1 = x => invocationOrder.Add(1);
        Action<string> observer2 = x => invocationOrder.Add(2);
        Action<string> observer3 = x => invocationOrder.Add(3);
        // Act
        string result = new Observer().Notify(obj, observer1, observer2, observer3);
        // Assert
        Assert.HasCount(3, invocationOrder);
        Assert.AreEqual(1, invocationOrder[0]);
        Assert.AreEqual(2, invocationOrder[1]);
        Assert.AreEqual(3, invocationOrder[2]);
        Assert.AreSame(obj, result);
    }

    ///<summary>
    ///Verifies that Notify works correctly when observers modify shared state.
    ///</summary>
    [TestMethod]
    public void Notify_ObserversModifySharedState_AllModificationsApplied()
    {
        // Arrange
        int counter = 0;
        string obj = "state";
        Action<string> observer1 = x => counter += 10;
        Action<string> observer2 = x => counter += 20;
        Action<string> observer3 = x => counter += 30;
        // Act
        string result = new Observer().Notify(obj, observer1, observer2, observer3);
        // Assert
        Assert.AreEqual(60, counter);
        Assert.AreSame(obj, result);
    }

    ///<summary>
    ///Verifies that Notify with a single observer invokes that observer exactly once with the correct object and
    ///returns the original object.
    ///</summary>
    [TestMethod]
    public void Notify_SingleObserver_InvokesObserverOnceAndReturnsOriginalObject()
    {
        // Arrange
        int obj = 42;
        int invocationCount = 0;
        int receivedValue = 0;
        Action<int> observer = x =>
        {
            invocationCount++;
            receivedValue = x;
        };
        // Act
        int result = new Observer().Notify(obj, observer);
        // Assert
        Assert.AreEqual(1, invocationCount);
        Assert.AreEqual(obj, receivedValue);
        Assert.AreEqual(obj, result);
    }

    ///<summary>
    ///Verifies that Notify with a large number of observers invokes all of them.
    ///</summary>
    [TestMethod]
    public void Notify_WithManyObservers_InvokesAllObservers()
    {
        // Arrange
        string obj = "many";
        int invocationCount = 0;
        Action<string>[] observers = new Action<string>[100];
        for(int i = 0; i < observers.Length; i++)
        {
            observers[i] = x => invocationCount++;
        }

        // Act
        string result = new Observer().Notify(obj, observers);
        // Assert
        Assert.AreEqual(100, invocationCount);
        Assert.AreSame(obj, result);
    }

    ///<summary>
    ///Verifies that Notify works correctly with nullable reference types.
    ///</summary>
    [TestMethod]
    public void Notify_WithNullableReferenceType_WorksCorrectly()
    {
        // Arrange
        string? obj = null;
        string? receivedValue = "not null";
        Action<string?> observer = x => receivedValue = x;
        // Act
        string? result = new Observer().Notify(obj, observer);
        // Assert
        Assert.IsNull(receivedValue);
        Assert.IsNull(result);
    }

    ///<summary>
    ///Verifies that Notify works correctly with reference types and all observers receive the same object reference.
    ///</summary>
    [TestMethod]
    public void Notify_WithReferenceType_AllObserversReceiveSameReference()
    {
        // Arrange
        List<int> obj = new List<int> { 1, 2, 3 };
        List<int>? receivedByObserver1 = null;
        List<int>? receivedByObserver2 = null;
        Action<List<int>> observer1 = x => receivedByObserver1 = x;
        Action<List<int>> observer2 = x => receivedByObserver2 = x;
        // Act
        List<int> result = new Observer().Notify(obj, observer1, observer2);
        // Assert
        Assert.AreSame(obj, receivedByObserver1);
        Assert.AreSame(obj, receivedByObserver2);
        Assert.AreSame(obj, result);
    }

    ///<summary>
    ///Verifies that Notify works correctly with value types.
    ///</summary>
    [TestMethod]
    public void Notify_WithValueType_AllObserversReceiveCorrectValue()
    {
        // Arrange
        int obj = 99;
        int receivedByObserver1 = 0;
        int receivedByObserver2 = 0;
        Action<int> observer1 = x => receivedByObserver1 = x;
        Action<int> observer2 = x => receivedByObserver2 = x;
        // Act
        int result = new Observer().Notify(obj, observer1, observer2);
        // Assert
        Assert.AreEqual(obj, receivedByObserver1);
        Assert.AreEqual(obj, receivedByObserver2);
        Assert.AreEqual(obj, result);
    }
    #endregion
}
