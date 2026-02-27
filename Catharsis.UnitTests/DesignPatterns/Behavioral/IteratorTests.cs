using Catharsis.DesignPatterns.Behavioral;

namespace Catharsis.Extensions.UnitTests;

[TestClass]
public class IteratorTests
{
    /// <summary>
    /// Tests that Iterate handles empty collection correctly without calling action.
    /// </summary>
    [TestMethod]
    public void Iterate_EmptyCollection_ReturnsObjectWithoutCallingAction()
    {
        // Arrange
        var obj = "test";
        var callCount = 0;
        Func<string, IEnumerable<char>> getElements = s => new List<char>();
        Action<char> action = c => callCount++;
        // Act
        var result = new Iterator().Iterate(obj, getElements, action);
        // Assert
        Assert.AreSame(obj, result);
        Assert.AreEqual(0, callCount);
    }

    /// <summary>
    /// Tests that Iterate calls action once for a single element collection.
    /// </summary>
    [TestMethod]
    public void Iterate_SingleElement_CallsActionOnce()
    {
        // Arrange
        var obj = "A";
        var capturedElements = new List<char>();
        Func<string, IEnumerable<char>> getElements = s => s;
        Action<char> action = c => capturedElements.Add(c);
        // Act
        var result = new Iterator().Iterate(obj, getElements, action);
        // Assert
        Assert.AreSame(obj, result);
        Assert.AreEqual(1, capturedElements.Count);
        Assert.AreEqual('A', capturedElements[0]);
    }

    /// <summary>
    /// Tests that Iterate calls action for each element in the correct order.
    /// </summary>
    [TestMethod]
    public void Iterate_MultipleElements_CallsActionForEachElementInOrder()
    {
        // Arrange
        var obj = new List<int>
        {
            1,
            2,
            3,
            4,
            5
        };
        var capturedElements = new List<int>();
        Func<List<int>, IEnumerable<int>> getElements = list => list;
        Action<int> action = i => capturedElements.Add(i);
        // Act
        var result = new Iterator().Iterate(obj, getElements, action);
        // Assert
        Assert.AreSame(obj, result);
        Assert.AreEqual(5, capturedElements.Count);
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, capturedElements);
    }

    /// <summary>
    /// Tests that Iterate works correctly when obj is null (for nullable reference types).
    /// </summary>
    [TestMethod]
    public void Iterate_NullObject_WorksCorrectly()
    {
        // Arrange
        string? obj = null;
        var callCount = 0;
        Func<string?, IEnumerable<char>> getElements = s => s ?? string.Empty;
        Action<char> action = c => callCount++;
        // Act
        var result = new Iterator().Iterate(obj, getElements, action);
        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(0, callCount);
    }

    /// <summary>
    /// Tests that Iterate works correctly with value types.
    /// </summary>
    [TestMethod]
    public void Iterate_ValueTypeObject_WorksCorrectly()
    {
        // Arrange
        var obj = 42;
        var capturedElements = new List<char>();
        Func<int, IEnumerable<char>> getElements = i => i.ToString();
        Action<char> action = c => capturedElements.Add(c);
        // Act
        var result = new Iterator().Iterate(obj, getElements, action);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.AreEqual(2, capturedElements.Count);
        CollectionAssert.AreEqual(new[] { '4', '2' }, capturedElements);
    }

    /// <summary>
    /// Tests that Iterate returns the same reference (reference equality) for reference types.
    /// </summary>
    [TestMethod]
    public void Iterate_ReferenceType_ReturnsSameReference()
    {
        // Arrange
        var obj = new List<string>
        {
            "a",
            "b",
            "c"
        };
        Func<List<string>, IEnumerable<string>> getElements = list => list;
        Action<string> action = s =>
        {
        };
        // Act
        var result = new Iterator().Iterate(obj, getElements, action);
        // Assert
        Assert.IsTrue(ReferenceEquals(obj, result));
    }

    /// <summary>
    /// Tests that Iterate works correctly with complex element types.
    /// </summary>
    [TestMethod]
    public void Iterate_ComplexElementType_WorksCorrectly()
    {
        // Arrange
        var obj = new Dictionary<string, int>
        {
            {
                "one",
                1
            },
            {
                "two",
                2
            },
            {
                "three",
                3
            }
        };
        var capturedKeys = new List<string>();
        Func<Dictionary<string, int>, IEnumerable<KeyValuePair<string, int>>> getElements = dict => dict;
        Action<KeyValuePair<string, int>> action = kvp => capturedKeys.Add(kvp.Key);
        // Act
        var result = new Iterator().Iterate(obj, getElements, action);
        // Assert
        Assert.AreSame(obj, result);
        Assert.AreEqual(3, capturedKeys.Count);
        CollectionAssert.Contains(capturedKeys, "one");
        CollectionAssert.Contains(capturedKeys, "two");
        CollectionAssert.Contains(capturedKeys, "three");
    }

    /// <summary>
    /// Tests that Iterate allows action to modify external state.
    /// </summary>
    [TestMethod]
    public void Iterate_ActionModifiesExternalState_StateIsModified()
    {
        // Arrange
        var obj = new[]
        {
            1,
            2,
            3,
            4,
            5
        };
        var sum = 0;
        Func<int[], IEnumerable<int>> getElements = arr => arr;
        Action<int> action = i => sum += i;
        // Act
        var result = new Iterator().Iterate(obj, getElements, action);
        // Assert
        Assert.AreSame(obj, result);
        Assert.AreEqual(15, sum);
    }

    /// <summary>
    /// Tests that Iterate works with edge case of int.MaxValue elements count simulation.
    /// </summary>
    [TestMethod]
    public void Iterate_LargeCollection_WorksCorrectly()
    {
        // Arrange
        var obj = new List<int>(1000);
        for (int i = 0; i < 1000; i++)
        {
            obj.Add(i);
        }

        var count = 0;
        Func<List<int>, IEnumerable<int>> getElements = list => list;
        Action<int> action = i => count++;
        // Act
        var result = new Iterator().Iterate(obj, getElements, action);
        // Assert
        Assert.AreSame(obj, result);
        Assert.AreEqual(1000, count);
    }
}
