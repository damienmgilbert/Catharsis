using System.Collections.Concurrent;
using Catharsis.DesignPatterns.Creational;

namespace Catharsis.UnitTests.DesignPatterns.Creational;

[TestClass]
public class SingletonPatternTests
{
    #region Public methods
    ///<summary>
    ///Tests that Singleton maintains separate instances for different keys.
    ///</summary>
    [TestMethod]
    public void Singleton_DifferentKeys_MaintainsSeparateInstances()
    {
        // Arrange
        string obj1 = "value1";
        string obj2 = "value2";
        string key1 = "key1";
        string key2 = "key2";
        ConcurrentDictionary<string, string> cache = new();
        // Act
        string result1 = SingletonPattern.Singleton(obj1, key1, cache);
        string result2 = SingletonPattern.Singleton(obj2, key2, cache);
        // Assert
        Assert.AreEqual(obj1, result1);
        Assert.AreEqual(obj2, result2);
        Assert.HasCount(2, cache);
        Assert.AreEqual(obj1, cache[key1]);
        Assert.AreEqual(obj2, cache[key2]);
    }

    ///<summary>
    ///Tests that Singleton works with empty string key.
    ///</summary>
    [TestMethod]
    public void Singleton_EmptyStringKey_WorksCorrectly()
    {
        // Arrange
        string obj = "test-value";
        string key = string.Empty;
        ConcurrentDictionary<string, string> cache = new();
        // Act
        string result = SingletonPattern.Singleton(obj, key, cache);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.IsTrue(cache.ContainsKey(key));
    }

    ///<summary>
    ///Tests that Singleton adds and returns the object on first access.
    ///</summary>
    [TestMethod]
    public void Singleton_FirstAccess_AddsAndReturnsObject()
    {
        // Arrange
        string obj = "test-value";
        string key = "key1";
        ConcurrentDictionary<string, string> cache = new();
        // Act
        string result = SingletonPattern.Singleton(obj, key, cache);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.IsTrue(cache.ContainsKey(key));
        Assert.AreEqual(obj, cache[key]);
    }

    ///<summary>
    ///Tests that Singleton works with Guid keys.
    ///</summary>
    [TestMethod]
    public void Singleton_GuidKey_WorksCorrectly()
    {
        // Arrange
        string obj = "test-value";
        Guid key = Guid.NewGuid();
        ConcurrentDictionary<Guid, string> cache = new();
        // Act
        string result = SingletonPattern.Singleton(obj, key, cache);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.IsTrue(cache.ContainsKey(key));
    }

    ///<summary>
    ///Tests that Singleton works with different key types.
    ///</summary>
    [TestMethod]
    public void Singleton_IntKey_WorksCorrectly()
    {
        // Arrange
        string obj = "test-value";
        int key = 123;
        ConcurrentDictionary<int, string> cache = new();
        // Act
        string result = SingletonPattern.Singleton(obj, key, cache);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.IsTrue(cache.ContainsKey(key));
    }

    ///<summary>
    ///Tests that Singleton works correctly with null object for reference types.
    ///</summary>
    [TestMethod]
    public void Singleton_NullObjectForReferenceType_CachesNull()
    {
        // Arrange
        string? obj = null;
        string key = "key1";
        ConcurrentDictionary<string, string?> cache = new();
        // Act
        string? result = SingletonPattern.Singleton(obj, key, cache);
        // Assert
        Assert.IsNull(result);
        Assert.IsTrue(cache.ContainsKey(key));
        Assert.IsNull(cache[key]);
    }

    ///<summary>
    ///Tests that Singleton correctly handles pre-populated cache.
    ///</summary>
    [TestMethod]
    public void Singleton_PrePopulatedCache_ReturnsExistingValue()
    {
        // Arrange
        string existingValue = "existing-value";
        string newValue = "FileName-value";
        string key = "key1";
        ConcurrentDictionary<string, string> cache = new();
        cache.TryAdd(key, existingValue);
        // Act
        string result = SingletonPattern.Singleton(newValue, key, cache);
        // Assert
        Assert.AreEqual(existingValue, result);
        Assert.AreNotEqual(newValue, result);
        Assert.HasCount(1, cache);
    }

    ///<summary>
    ///Tests that Singleton works with special characters in string key.
    ///</summary>
    [TestMethod]
    public void Singleton_SpecialCharactersInKey_WorksCorrectly()
    {
        // Arrange
        string obj = "test-value";
        string key = "key!@#$%^&*()_+-=[]{}|;':\",./<>?\t\n\r";
        ConcurrentDictionary<string, string> cache = new();
        // Act
        string result = SingletonPattern.Singleton(obj, key, cache);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.IsTrue(cache.ContainsKey(key));
    }

    ///<summary>
    ///Tests that Singleton returns cached instance on subsequent access with same key, ignoring the FileName object
    ///parameter.
    ///</summary>
    [TestMethod]
    public void Singleton_SubsequentAccessSameKey_ReturnsCachedInstance()
    {
        // Arrange
        string firstObj = "first-value";
        string secondObj = "second-value";
        string key = "key1";
        ConcurrentDictionary<string, string> cache = new();
        // Act
        string firstResult = SingletonPattern.Singleton(firstObj, key, cache);
        string secondResult = SingletonPattern.Singleton(secondObj, key, cache);
        // Assert
        Assert.AreEqual(firstObj, firstResult);
        Assert.AreEqual(firstObj, secondResult);
        Assert.AreNotEqual(secondObj, secondResult);
        Assert.HasCount(1, cache);
    }

    ///<summary>
    ///Tests that Singleton works correctly with value types.
    ///</summary>
    [DataRow(0)]
    [DataRow(42)]
    [DataRow(-1)]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    [TestMethod]
    public void Singleton_ValueType_CachesAndReturnsValue(int value)
    {
        // Arrange
        string key = "key1";
        ConcurrentDictionary<string, int> cache = new();
        // Act
        int firstResult = SingletonPattern.Singleton(value, key, cache);
        int secondResult = SingletonPattern.Singleton(value + 1, key, cache);
        // Assert
        Assert.AreEqual(value, firstResult);
        Assert.AreEqual(value, secondResult);
        Assert.HasCount(1, cache);
    }

    ///<summary>
    ///Tests that Singleton works with very long string key.
    ///</summary>
    [TestMethod]
    public void Singleton_VeryLongStringKey_WorksCorrectly()
    {
        // Arrange
        string obj = "test-value";
        string key = new('a', 10000);
        ConcurrentDictionary<string, string> cache = new();
        // Act
        string result = SingletonPattern.Singleton(obj, key, cache);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.IsTrue(cache.ContainsKey(key));
    }
    #endregion
}
