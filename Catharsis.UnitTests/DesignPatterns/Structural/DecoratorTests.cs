using Catharsis.DesignPatterns.Structural;

namespace Catharsis.UnitTests.DesignPatterns.Structural;

[TestClass]
public class DecoratorTests
{
    #region Public methods

    ///<summary>
    ///Tests that Decorate allows a decorator to return null for reference types. Input: non-null object and decorator
    ///that returns null. Expected: null returned and subsequent decorators receive null.
    ///</summary>
    [TestMethod]
    public void Decorate_DecoratorReturnsNull_SubsequentDecoratorsReceiveNull()
    {
        // Arrange
        string obj = "test";
        Func<string?, string?> returnsNull = s => null;
        Func<string?, string?> checksNull = s => (s == null) ? "received null" : s;
        // Act
        string? result = new Decorator().Decorate(obj, returnsNull, checksNull);
        // Assert
        Assert.AreEqual("received null", result);
    }

    ///<summary>
    ///Tests that Decorate verifies order matters by applying decorators in different sequence. Input: same decorators
    ///but different order. Expected: different results demonstrating order-dependent application.
    ///</summary>
    [TestMethod]
    public void Decorate_DifferentOrder_ProducesDifferentResult()
    {
        // Arrange
        int obj = 5;
        Func<int, int> addTen = x => x + 10;
        Func<int, int> multiplyByTwo = x => x * 2;
        // Act
        int result1 = new Decorator().Decorate(obj, addTen, multiplyByTwo); // (5 + 10) * 2 = 30
        int result2 = new Decorator().Decorate(obj, multiplyByTwo, addTen); // (5 * 2) + 10 = 20
        // Assert
        Assert.AreEqual(30, result1);
        Assert.AreEqual(20, result2);
        Assert.AreNotEqual(result1, result2);
    }

    ///<summary>
    ///Tests that Decorate handles extreme values correctly with value types. Input: int.MaxValue and decorator
    ///operations. Expected: decorators applied respecting overflow behavior.
    ///</summary>
    [TestMethod]
    public void Decorate_ExtremeValueTypes_AppliesDecorators()
    {
        // Arrange
        int obj = int.MaxValue;
        Func<int, int> identity = x => x;
        // Act
        int result = new Decorator().Decorate(obj, identity);
        // Assert
        Assert.AreEqual(int.MaxValue, result);
    }

    ///<summary>
    ///Tests that Decorate works with identity decorator that returns input unchanged. Input: object and identity
    ///decorator. Expected: original object returned unchanged.
    ///</summary>
    [TestMethod]
    public void Decorate_IdentityDecorator_ReturnsOriginalObject()
    {
        // Arrange
        int obj = 100;
        Func<int, int> identity = x => x;
        // Act
        int result = new Decorator().Decorate(obj, identity);
        // Assert
        Assert.AreEqual(100, result);
    }

    ///<summary>
    ///Tests that Decorate handles many decorators correctly. Input: large number of decorators. Expected: all
    ///decorators applied in sequence.
    ///</summary>
    [TestMethod]
    public void Decorate_ManyDecorators_AppliesAll()
    {
        // Arrange
        int obj = 0;
        Func<int, int> incrementor = x => x + 1;
        Func<int, int>[] decorators = new Func<int, int>[100];
        for(int i = 0; i < 100; i++)
        {
            decorators[i] = incrementor;
        }

        // Act
        int result = new Decorator().Decorate(obj, decorators);
        // Assert
        Assert.AreEqual(100, result);
    }

    ///<summary>
    ///Tests that Decorate correctly chains multiple identity and transformation decorators. Input: mix of identity and
    ///transformation decorators. Expected: only transformations affect the result, identities pass through.
    ///</summary>
    [TestMethod]
    public void Decorate_MixedIdentityAndTransformations_AppliesCorrectly()
    {
        // Arrange
        int obj = 10;
        Func<int, int> identity = x => x;
        Func<int, int> addFive = x => x + 5;
        // Act
        int result = new Decorator().Decorate(obj, identity, addFive, identity, addFive, identity);
        // Assert
        Assert.AreEqual(20, result);
    }

    ///<summary>
    ///Tests that Decorate applies multiple decorators in sequence, left-to-right order. Input: object and multiple
    ///decorators. Expected: decorators applied in order, each receiving the result of the previous.
    ///</summary>
    [TestMethod]
    public void Decorate_MultipleDecorators_AppliesInOrder()
    {
        // Arrange
        int obj = 5;
        Func<int, int> addTen = x => x + 10; // 5 + 10 = 15
        Func<int, int> multiplyByTwo = x => x * 2; // 15 * 2 = 30
        Func<int, int> subtractThree = x => x - 3; // 30 - 3 = 27
        // Act
        int result = new Decorator().Decorate(obj, addTen, multiplyByTwo, subtractThree);
        // Assert
        Assert.AreEqual(27, result);
    }

    ///<summary>
    ///Tests that Decorate with no explicit decorators (params empty) returns original object. Input: object with no
    ///decorators passed. Expected: original object returned.
    ///</summary>
    [TestMethod]
    public void Decorate_NoDecoratorsProvided_ReturnsOriginalObject()
    {
        // Arrange
        int obj = 99;
        // Act
        int result = new Decorator().Decorate(obj);
        // Assert
        Assert.AreEqual(99, result);
    }

    ///<summary>
    ///Tests that Decorate handles null object with reference types when decorators allow it. Input: null string and
    ///decorator that handles null. Expected: decorator processes null input correctly.
    ///</summary>
    [TestMethod]
    public void Decorate_NullObjectReferenceType_AppliesDecorators()
    {
        // Arrange
        string? obj = null;
        Func<string?, string?> decorator = s => (s == null) ? "was null" : s.ToUpper();
        // Act
        string? result = new Decorator().Decorate(obj, decorator);
        // Assert
        Assert.AreEqual("was null", result);
    }

    ///<summary>
    ///Tests that Decorate works correctly with reference types. Input: string object and string transformation
    ///decorators. Expected: decorators applied in sequence to string.
    ///</summary>
    [TestMethod]
    public void Decorate_ReferenceType_AppliesDecorators()
    {
        // Arrange
        string obj = "hello";
        Func<string, string> toUpper = s => s.ToUpper();
        Func<string, string> addExclamation = s => $"{s}!";
        // Act
        string result = new Decorator().Decorate(obj, toUpper, addExclamation);
        // Assert
        Assert.AreEqual("HELLO!", result);
    }
    #endregion
}
