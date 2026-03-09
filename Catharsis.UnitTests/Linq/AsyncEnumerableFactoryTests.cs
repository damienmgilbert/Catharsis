using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

[TestClass]
public class AsyncEnumerableFactoryTests
{
    private static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> source)
    {
        List<T> result = [];
        await foreach (T item in source)
        {
            result.Add(item);
        }
        return result;
    }

    [TestMethod]
    public async Task Create_GeneratesElementsFromFactory()
    {
        List<int> result = await ToListAsync(AsyncEnumerableFactory.Create(3, i => i * 10));

        CollectionAssert.AreEqual(new[] { 0, 10, 20 }, result);
    }

    [TestMethod]
    public void Create_NegativeCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => AsyncEnumerableFactory.Create(-1, i => i));
    }

    [TestMethod]
    public void Create_NullFactory_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => AsyncEnumerableFactory.Create<int>(3, null!));
    }

    [TestMethod]
    public async Task CreateAsync_GeneratesElementsFromAsyncFactory()
    {
        List<int> result = await ToListAsync(
            AsyncEnumerableFactory.CreateAsync(3, (i, ct) => ValueTask.FromResult(i + 1)));

        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
    }

    [TestMethod]
    public async Task Empty_ReturnsEmptySequence()
    {
        List<int> result = await ToListAsync(AsyncEnumerableFactory.Empty<int>());

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task FromEnumerable_WrapsEnumerable()
    {
        List<int> result = await ToListAsync(AsyncEnumerableFactory.FromEnumerable([1, 2, 3]));

        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
    }

    [TestMethod]
    public void FromEnumerable_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => AsyncEnumerableFactory.FromEnumerable<int>(null!));
    }

    [TestMethod]
    public async Task Generate_ProducesSequence()
    {
        List<int> result = await ToListAsync(
            AsyncEnumerableFactory.Generate(1, x => x <= 8, x => x * 2));

        CollectionAssert.AreEqual(new[] { 1, 2, 4, 8 }, result);
    }

    [TestMethod]
    public async Task GenerateAsync_ProducesSequence()
    {
        List<int> result = await ToListAsync(
            AsyncEnumerableFactory.GenerateAsync(1, x => x <= 8, (x, ct) => ValueTask.FromResult(x * 2)));

        CollectionAssert.AreEqual(new[] { 1, 2, 4, 8 }, result);
    }

    [TestMethod]
    public async Task Range_ProducesConsecutiveIntegers()
    {
        List<int> result = await ToListAsync(AsyncEnumerableFactory.Range(5, 3));

        CollectionAssert.AreEqual(new[] { 5, 6, 7 }, result);
    }

    [TestMethod]
    public void Range_NegativeCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => AsyncEnumerableFactory.Range(0, -1));
    }

    [TestMethod]
    public async Task Repeat_RepeatsValue()
    {
        List<string> result = await ToListAsync(AsyncEnumerableFactory.Repeat("x", 4));

        CollectionAssert.AreEqual(new[] { "x", "x", "x", "x" }, result);
    }

    [TestMethod]
    public async Task Return_SingleElement()
    {
        List<int> result = await ToListAsync(AsyncEnumerableFactory.Return(42));

        CollectionAssert.AreEqual(new[] { 42 }, result);
    }

    [TestMethod]
    public async Task Defer_DefersCreation()
    {
        int callCount = 0;
        IAsyncEnumerable<int> deferred = AsyncEnumerableFactory.Defer<int>(ct =>
        {
            callCount++;
            return AsyncEnumerableFactory.FromEnumerable(new[] { 1, 2, 3 });
        });

        Assert.AreEqual(0, callCount);

        List<int> result = await ToListAsync(deferred);

        Assert.AreEqual(1, callCount);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
    }
}
