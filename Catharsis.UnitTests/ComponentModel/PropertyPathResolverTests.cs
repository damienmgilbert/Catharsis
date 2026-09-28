using Catharsis.ComponentModel;

namespace Catharsis.UnitTests.ComponentModel;

///<summary>
///Unit tests for the <see cref="PropertyPathResolver"/> class.
///</summary>
[TestClass]
public class PropertyPathResolverTests
{
    #region GetValue

    [TestMethod]
    public void GetValue_NullSource_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => PropertyPathResolver.GetValue(null!, "Name")); }

    [TestMethod]
    public void GetValue_NullPath_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => PropertyPathResolver.GetValue(new Person(), null!)); }

    [TestMethod]
    public void GetValue_EmptyPath_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => PropertyPathResolver.GetValue(new Person(), "")); }

    [TestMethod]
    public void GetValue_SingleSegment_ReturnsPropertyValue()
    {
        Person person = new() { Name = "Alice" };
        Assert.AreEqual("Alice", PropertyPathResolver.GetValue(person, "Name"));
    }

    [TestMethod]
    public void GetValue_NestedSegments_ReturnsNestedValue()
    {
        Person person = new() { Address = new Address { City = "Metropolis" } };
        Assert.AreEqual("Metropolis", PropertyPathResolver.GetValue(person, "Address.City"));
    }

    [TestMethod]
    public void GetValue_NullIntermediateValue_ReturnsNull()
    {
        Person person = new() { Address = null };
        Assert.IsNull(PropertyPathResolver.GetValue(person, "Address.City"));
    }

    [TestMethod]
    public void GetValue_UnknownProperty_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => PropertyPathResolver.GetValue(new Person(), "DoesNotExist")); }

    #endregion

    #region SetValue

    [TestMethod]
    public void SetValue_NullSource_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => PropertyPathResolver.SetValue(null!, "Name", "x")); }

    [TestMethod]
    public void SetValue_SingleSegment_SetsPropertyValue()
    {
        Person person = new();
        PropertyPathResolver.SetValue(person, "Name", "Alice");
        Assert.AreEqual("Alice", person.Name);
    }

    [TestMethod]
    public void SetValue_NestedSegments_SetsNestedValue()
    {
        Person person = new() { Address = new Address() };
        PropertyPathResolver.SetValue(person, "Address.City", "Metropolis");
        Assert.AreEqual("Metropolis", person.Address!.City);
    }

    [TestMethod]
    public void SetValue_NullIntermediateValue_Throws()
    {
        Person person = new() { Address = null };
        Assert.ThrowsExactly<InvalidOperationException>(() => PropertyPathResolver.SetValue(person, "Address.City", "Metropolis"));
    }

    [TestMethod]
    public void SetValue_UnknownFinalProperty_Throws() { Assert.ThrowsExactly<ArgumentException>(() => PropertyPathResolver.SetValue(new Person(), "DoesNotExist", "x")); }

    #endregion

    private sealed class Person
    {
        #region Public properties
        public Address? Address { get; set; }
        public string? Name { get; set; }
        #endregion
    }

    private sealed class Address
    {
        #region Public properties
        public string? City { get; set; }
        #endregion
    }
}
