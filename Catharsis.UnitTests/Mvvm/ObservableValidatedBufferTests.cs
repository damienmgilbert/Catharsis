using Catharsis.Mvvm;
using System.ComponentModel;

namespace Catharsis.UnitTests.Mvvm;

///<summary>
///Unit tests for the <see cref="ObservableValidatedBuffer{T}"/> class.
///</summary>
[TestClass]
public class ObservableValidatedBufferTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullValidator_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new ObservableValidatedBuffer<byte>(null!)); }

    #endregion

    #region Validation

    [TestMethod]
    public void Write_ValidatorPasses_NoErrors()
    {
        using ObservableValidatedBuffer<byte> buffer = new(static _ => []);
        buffer.Write([1, 2, 3]);

        Assert.IsFalse(buffer.HasErrors);
    }

    [TestMethod]
    public void Write_ValidatorFails_HasErrorsAndExposesMessages()
    {
        using ObservableValidatedBuffer<byte> buffer = new(static memory => memory.Length > 2 ? ["Too many bytes."] : []);
        buffer.Write([1, 2, 3]);

        Assert.IsTrue(buffer.HasErrors);
        CollectionAssert.Contains(buffer.GetErrors(null).Cast<string>().ToList(), "Too many bytes.");
    }

    [TestMethod]
    public void Write_ValidatorFailsThenPasses_ClearsErrors()
    {
        using ObservableValidatedBuffer<byte> buffer = new(static memory => memory.Length > 2 ? ["Too many bytes."] : []);
        buffer.Write([1, 2, 3]);
        buffer.Clear();

        Assert.IsFalse(buffer.HasErrors);
    }

    [TestMethod]
    public void Write_ValidatorFails_RaisesErrorsChanged()
    {
        using ObservableValidatedBuffer<byte> buffer = new(static memory => memory.Length > 2 ? ["Too many bytes."] : []);
        bool raised = false;
        buffer.ErrorsChanged += (_, _) => raised = true;

        buffer.Write([1, 2, 3]);

        Assert.IsTrue(raised);
    }

    [TestMethod]
    public void ValidatorReceivesWrittenContents()
    {
        byte[]? received = null;
        using ObservableValidatedBuffer<byte> buffer = new(memory => { received = memory.ToArray(); return []; });

        buffer.Write([1, 2, 3]);

        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, received);
    }

    #endregion

    #region Inherited buffer behavior

    [TestMethod]
    public void Write_AppendsToUnderlyingBuffer()
    {
        using ObservableValidatedBuffer<byte> buffer = new(static _ => []);
        buffer.Write([1, 2]);
        buffer.Write([3]);

        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, buffer.ToArray());
    }

    #endregion
}
