using Catharsis.DesignPatterns.Creational;
using System.Collections.Concurrent;

namespace Catharsis.Extensions.UnitTests;

[TestClass]
public class SingletonPatternTests
{
    /// <summary>
    /// Tests that Singleton adds and returns the object on first access.
    /// </summary>
    [TestMethod]
    public void Singleton_FirstAccess_AddsAndReturnsObject()
    {
        // Arrange
        var obj = "test-value";
        var key = "key1";
        var cache = new ConcurrentDictionary<string, string>();
        // Act
        var result = new SingletonPattern().Singleton(obj, key, cache);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.IsTrue(cache.ContainsKey(key));
        Assert.AreEqual(obj, cache[key]);
    }

    /// <summary>
    /// Tests that Singleton returns cached instance on subsequent access with same key,
    /// ignoring the new object parameter.
    /// </summary>
    [TestMethod]
    public void Singleton_SubsequentAccessSameKey_ReturnsCachedInstance()
    {
        // Arrange
        var firstObj = "first-value";
        var secondObj = "second-value";
        var key = "key1";
        var cache = new ConcurrentDictionary<string, string>();
        // Act
        var firstResult = new SingletonPattern().Singleton(firstObj, key, cache);
        var secondResult = new SingletonPattern().Singleton(secondObj, key, cache);
        // Assert
        Assert.AreEqual(firstObj, firstResult);
        Assert.AreEqual(firstObj, secondResult);
        Assert.AreNotEqual(secondObj, secondResult);
        Assert.AreEqual(1, cache.Count);
    }

    /// <summary>
    /// Tests that Singleton maintains separate instances for different keys.
    /// </summary>
    [TestMethod]
    public void Singleton_DifferentKeys_MaintainsSeparateInstances()
    {
        // Arrange
        var obj1 = "value1";
        var obj2 = "value2";
        var key1 = "key1";
        var key2 = "key2";
        var cache = new ConcurrentDictionary<string, string>();
        // Act
        var result1 = new SingletonPattern().Singleton(obj1, key1, cache);
        var result2 = new SingletonPattern().Singleton(obj2, key2, cache);
        // Assert
        Assert.AreEqual(obj1, result1);
        Assert.AreEqual(obj2, result2);
        Assert.AreEqual(2, cache.Count);
        Assert.AreEqual(obj1, cache[key1]);
        Assert.AreEqual(obj2, cache[key2]);
    }

    /// <summary>
    /// Tests that Singleton works correctly with null object for reference types.
    /// </summary>
    [TestMethod]
    public void Singleton_NullObjectForReferenceType_CachesNull()
    {
        // Arrange
        string? obj = null;
        var key = "key1";
        var cache = new ConcurrentDictionary<string, string?>();
        // Act
        var result = new SingletonPattern().Singleton(obj, key, cache);
        // Assert
        Assert.IsNull(result);
        Assert.IsTrue(cache.ContainsKey(key));
        Assert.IsNull(cache[key]);
    }

    /// <summary>
    /// Tests that Singleton works correctly with value types.
    /// </summary>
    [DataRow(0)]
    [DataRow(42)]
    [DataRow(-1)]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    [TestMethod]
    public void Singleton_ValueType_CachesAndReturnsValue(int value)
    {
        // Arrange
        var key = "key1";
        var cache = new ConcurrentDictionary<string, int>();
        // Act
        var firstResult = new SingletonPattern().Singleton(value, key, cache);
        var secondResult = new SingletonPattern().Singleton(value + 1, key, cache);
        // Assert
        Assert.AreEqual(value, firstResult);
        Assert.AreEqual(value, secondResult);
        Assert.AreEqual(1, cache.Count);
    }

    /// <summary>
    /// Tests that Singleton works with different key types.
    /// </summary>
    [TestMethod]
    public void Singleton_IntKey_WorksCorrectly()
    {
        // Arrange
        var obj = "test-value";
        var key = 123;
        var cache = new ConcurrentDictionary<int, string>();
        // Act
        var result = new SingletonPattern().Singleton(obj, key, cache);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.IsTrue(cache.ContainsKey(key));
    }

    /// <summary>
    /// Tests that Singleton works with Guid keys.
    /// </summary>
    [TestMethod]
    public void Singleton_GuidKey_WorksCorrectly()
    {
        // Arrange
        var obj = "test-value";
        var key = Guid.NewGuid();
        var cache = new ConcurrentDictionary<Guid, string>();
        // Act
        var result = new SingletonPattern().Singleton(obj, key, cache);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.IsTrue(cache.ContainsKey(key));
    }

    /// <summary>
    /// Tests that Singleton works with empty string key.
    /// </summary>
    [TestMethod]
    public void Singleton_EmptyStringKey_WorksCorrectly()
    {
        // Arrange
        var obj = "test-value";
        var key = string.Empty;
        var cache = new ConcurrentDictionary<string, string>();
        // Act
        var result = new SingletonPattern().Singleton(obj, key, cache);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.IsTrue(cache.ContainsKey(key));
    }

    /// <summary>
    /// Tests that Singleton works with very long string key.
    /// </summary>
    [TestMethod]
    public void Singleton_VeryLongStringKey_WorksCorrectly()
    {
        // Arrange
        var obj = "test-value";
        var key = new string('a', 10000);
        var cache = new ConcurrentDictionary<string, string>();
        // Act
        var result = new SingletonPattern().Singleton(obj, key, cache);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.IsTrue(cache.ContainsKey(key));
    }

    /// <summary>
    /// Tests that Singleton works with special characters in string key.
    /// </summary>
    [TestMethod]
    public void Singleton_SpecialCharactersInKey_WorksCorrectly()
    {
        // Arrange
        var obj = "test-value";
        var key = "key!@#$%^&*()_+-=[]{}|;':\",./<>?\t\n\r";
        var cache = new ConcurrentDictionary<string, string>();
        // Act
        var result = new SingletonPattern().Singleton(obj, key, cache);
        // Assert
        Assert.AreEqual(obj, result);
        Assert.IsTrue(cache.ContainsKey(key));
    }

    /// <summary>
    /// Tests that Singleton correctly handles pre-populated cache.
    /// </summary>
    [TestMethod]
    public void Singleton_PrePopulatedCache_ReturnsExistingValue()
    {
        // Arrange
        var existingValue = "existing-value";
        var newValue = "new-value";
        var key = "key1";
        var cache = new ConcurrentDictionary<string, string>();
        cache.TryAdd(key, existingValue);
        // Act
        var result = new SingletonPattern().Singleton(newValue, key, cache);
        // Assert
        Assert.AreEqual(existingValue, result);
        Assert.AreNotEqual(newValue, result);
        Assert.AreEqual(1, cache.Count);
    }
}
