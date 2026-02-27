using System.Buffers;
using Catharsis.Buffers;

namespace Catharsis.Buffers.UnitTests;

/// <summary>
/// Unit tests for the <see cref="MemoryPoolManager"/> class.
/// </summary>
[TestClass]
public class MemoryPoolManagerTests
{
    /// <summary>
    /// Tests that the default constructor creates a valid instance.
    /// </summary>
    [TestMethod]
    public void Constructor_Default_CreatesInstance()
    {
        using var manager = new MemoryPoolManager();

        Assert.AreEqual(0, manager.ActiveRentals);
    }

    /// <summary>
    /// Tests that the constructor throws when arrayPool is null.
    /// </summary>
    [TestMethod]
    public void Constructor_NullArrayPool_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new MemoryPoolManager(null!, MemoryPool<byte>.Shared));
    }

    /// <summary>
    /// Tests that the constructor throws when memoryPool is null.
    /// </summary>
    [TestMethod]
    public void Constructor_NullMemoryPool_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new MemoryPoolManager(ArrayPool<byte>.Shared, null!));
    }

    /// <summary>
    /// Tests that RentArray returns an array of at least the requested length.
    /// </summary>
    [TestMethod]
    public void RentArray_ReturnsArrayOfAtLeastRequestedLength()
    {
        using var manager = new MemoryPoolManager();

        byte[] array = manager.RentArray(100);

        Assert.IsTrue(array.Length >= 100);
        manager.ReturnArray(array);
    }

    /// <summary>
    /// Tests that RentArray increments ActiveRentals.
    /// </summary>
    [TestMethod]
    public void RentArray_IncrementsActiveRentals()
    {
        using var manager = new MemoryPoolManager();

        byte[] array = manager.RentArray(10);

        Assert.AreEqual(1, manager.ActiveRentals);
        manager.ReturnArray(array);
    }

    /// <summary>
    /// Tests that RentArray throws for negative length.
    /// </summary>
    [TestMethod]
    public void RentArray_NegativeLength_ThrowsArgumentOutOfRangeException()
    {
        using var manager = new MemoryPoolManager();

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => manager.RentArray(-1));
    }

    /// <summary>
    /// Tests that RentArray throws after disposal.
    /// </summary>
    [TestMethod]
    public void RentArray_AfterDispose_ThrowsObjectDisposedException()
    {
        var manager = new MemoryPoolManager();
        manager.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => manager.RentArray(10));
    }

    /// <summary>
    /// Tests that ReturnArray decrements ActiveRentals.
    /// </summary>
    [TestMethod]
    public void ReturnArray_DecrementsActiveRentals()
    {
        using var manager = new MemoryPoolManager();
        byte[] array = manager.RentArray(10);

        manager.ReturnArray(array);

        Assert.AreEqual(0, manager.ActiveRentals);
    }

    /// <summary>
    /// Tests that ReturnArray throws when array is null.
    /// </summary>
    [TestMethod]
    public void ReturnArray_NullArray_ThrowsArgumentNullException()
    {
        using var manager = new MemoryPoolManager();

        Assert.ThrowsExactly<ArgumentNullException>(() => manager.ReturnArray(null!));
    }

    /// <summary>
    /// Tests that ReturnArray throws after disposal.
    /// </summary>
    [TestMethod]
    public void ReturnArray_AfterDispose_ThrowsObjectDisposedException()
    {
        var manager = new MemoryPoolManager();
        byte[] array = manager.RentArray(10);
        manager.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => manager.ReturnArray(array));
    }

    /// <summary>
    /// Tests that RentMemory returns a valid memory owner.
    /// </summary>
    [TestMethod]
    public void RentMemory_ReturnsValidMemoryOwner()
    {
        using var manager = new MemoryPoolManager();

        using IMemoryOwner<byte> owner = manager.RentMemory(64);

        Assert.IsTrue(owner.Memory.Length >= 64);
    }

    /// <summary>
    /// Tests that RentMemory increments ActiveRentals.
    /// </summary>
    [TestMethod]
    public void RentMemory_IncrementsActiveRentals()
    {
        using var manager = new MemoryPoolManager();

        using IMemoryOwner<byte> owner = manager.RentMemory(32);

        Assert.AreEqual(1, manager.ActiveRentals);
    }

    /// <summary>
    /// Tests that disposing the rented memory owner decrements ActiveRentals.
    /// </summary>
    [TestMethod]
    public void RentMemory_DisposeOwner_DecrementsActiveRentals()
    {
        using var manager = new MemoryPoolManager();
        IMemoryOwner<byte> owner = manager.RentMemory(32);

        owner.Dispose();

        Assert.AreEqual(0, manager.ActiveRentals);
    }

    /// <summary>
    /// Tests that disposing the rented memory owner twice does not decrement below zero.
    /// </summary>
    [TestMethod]
    public void RentMemory_DisposeOwnerTwice_DoesNotDecrementBelowZero()
    {
        using var manager = new MemoryPoolManager();
        IMemoryOwner<byte> owner = manager.RentMemory(32);

        owner.Dispose();
        owner.Dispose();

        Assert.AreEqual(0, manager.ActiveRentals);
    }

    /// <summary>
    /// Tests that accessing Memory on disposed owner throws ObjectDisposedException.
    /// </summary>
    [TestMethod]
    public void RentMemory_AccessMemoryAfterOwnerDispose_ThrowsObjectDisposedException()
    {
        using var manager = new MemoryPoolManager();
        IMemoryOwner<byte> owner = manager.RentMemory(32);
        owner.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = owner.Memory);
    }

    /// <summary>
    /// Tests that RentMemory throws after manager disposal.
    /// </summary>
    [TestMethod]
    public void RentMemory_AfterDispose_ThrowsObjectDisposedException()
    {
        var manager = new MemoryPoolManager();
        manager.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => manager.RentMemory(10));
    }

    /// <summary>
    /// Tests that multiple rent/return cycles track correctly.
    /// </summary>
    [TestMethod]
    public void ActiveRentals_MultipleRentReturn_TracksCorrectly()
    {
        using var manager = new MemoryPoolManager();

        byte[] a1 = manager.RentArray(10);
        byte[] a2 = manager.RentArray(20);
        using IMemoryOwner<byte> m1 = manager.RentMemory(16);

        Assert.AreEqual(3, manager.ActiveRentals);

        manager.ReturnArray(a1);
        Assert.AreEqual(2, manager.ActiveRentals);

        manager.ReturnArray(a2);
        Assert.AreEqual(1, manager.ActiveRentals);
    }

    /// <summary>
    /// Tests that Dispose is idempotent.
    /// </summary>
    [TestMethod]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var manager = new MemoryPoolManager();

        manager.Dispose();
        manager.Dispose();
    }
}
