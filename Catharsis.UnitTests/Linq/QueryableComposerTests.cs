using System.Linq.Expressions;

using Catharsis.Linq;

namespace Catharsis.UnitTests.Linq;

[TestClass]
public class QueryableComposerTests
{
    private static IQueryable<int> Source => new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.AsQueryable();

    [TestMethod]
    public void AndWhere_FiltersWithAdditionalPredicate()
    {
        List<int> result = Source.AndWhere(x => x > 5).ToList();
        CollectionAssert.AreEqual(new[] { 6, 7, 8, 9, 10 }, result);
    }

    [TestMethod]
    public void AndWhere_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IQueryable<int>)null!).AndWhere(x => true));
    }

    [TestMethod]
    public void GetExpression_ReturnsExpressionTree()
    {
        Expression expr = Source.GetExpression();
        Assert.IsNotNull(expr);
    }

    [TestMethod]
    public void GetExpression_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IQueryable<int>)null!).GetExpression());
    }

    [TestMethod]
    public void GetProvider_ReturnsQueryProvider()
    {
        IQueryProvider provider = Source.GetProvider();
        Assert.IsNotNull(provider);
    }

    [TestMethod]
    public void Page_ReturnsPaginatedResults()
    {
        List<int> result = Source.Page(1, 3).ToList(); // Skip 3, take 3
        CollectionAssert.AreEqual(new[] { 4, 5, 6 }, result);
    }

    [TestMethod]
    public void Page_FirstPage_ReturnsFirstElements()
    {
        List<int> result = Source.Page(0, 4).ToList();
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, result);
    }

    [TestMethod]
    public void Page_NegativePageIndex_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Source.Page(-1, 10));
    }

    [TestMethod]
    public void Page_PageSizeLessThan1_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Source.Page(0, 0));
    }

    [TestMethod]
    public void Pipe_TransformsQuery()
    {
        IQueryable<int> result = QueryableComposer.Pipe<int>(Source, q => q.Where(x => x > 8));
        CollectionAssert.AreEqual(new[] { 9, 10 }, result.ToList());
    }

    [TestMethod]
    public void Pipe_TypeChanging_TransformsQuery()
    {
        List<string> result = Source.Pipe<int, string>(q => q.Select(x => x.ToString())).Take(3).ToList();
        CollectionAssert.AreEqual(new[] { "1", "2", "3" }, result);
    }

    [TestMethod]
    public void PipeResult_TerminatesPipeline()
    {
        int result = Source.PipeResult(q => q.Count());
        Assert.AreEqual(10, result);
    }

    [TestMethod]
    public void WhereAll_CombinesWithAnd()
    {
        Expression<Func<int, bool>>[] predicates = [x => x > 3, x => x < 8];
        List<int> result = Source.WhereAll(predicates).ToList();
        CollectionAssert.AreEqual(new[] { 4, 5, 6, 7 }, result);
    }

    [TestMethod]
    public void WhereAny_CombinesWithOr()
    {
        Expression<Func<int, bool>>[] predicates = [x => x == 1, x => x == 10];
        List<int> result = Source.WhereAny(predicates).ToList();
        CollectionAssert.AreEqual(new[] { 1, 10 }, result);
    }

    [TestMethod]
    public void WhereIf_ConditionTrue_AppliesFilter()
    {
        List<int> result = Source.WhereIf(true, x => x > 8).ToList();
        CollectionAssert.AreEqual(new[] { 9, 10 }, result);
    }

    [TestMethod]
    public void WhereIf_ConditionFalse_ReturnsUnchanged()
    {
        List<int> result = Source.WhereIf(false, x => x > 8).ToList();
        Assert.AreEqual(10, result.Count);
    }

    [TestMethod]
    public void WhereIfNotNull_ClassValue_NotNull_AppliesFilter()
    {
        string? threshold = "5";
        List<int> result = Source.WhereIfNotNull(threshold, v => x => x > int.Parse(v)).ToList();
        CollectionAssert.AreEqual(new[] { 6, 7, 8, 9, 10 }, result);
    }

    [TestMethod]
    public void WhereIfNotNull_ClassValue_Null_ReturnsAll()
    {
        string? threshold = null;
        List<int> result = Source.WhereIfNotNull(threshold, v => x => x > int.Parse(v)).ToList();
        Assert.AreEqual(10, result.Count);
    }

    [TestMethod]
    public void WhereIfNotNull_StructValue_HasValue_AppliesFilter()
    {
        int? min = 7;
        List<int> result = Source.WhereIfNotNull(min, v => x => x >= v).ToList();
        CollectionAssert.AreEqual(new[] { 7, 8, 9, 10 }, result);
    }

    [TestMethod]
    public void WhereIfNotNull_StructValue_Null_ReturnsAll()
    {
        int? min = null;
        List<int> result = Source.WhereIfNotNull(min, v => x => x >= v).ToList();
        Assert.AreEqual(10, result.Count);
    }

    [TestMethod]
    public void WhereOr_CombinesTwoPredicatesWithOr()
    {
        List<int> result = Source.WhereOr(x => x == 1, x => x == 10).ToList();
        CollectionAssert.AreEqual(new[] { 1, 10 }, result);
    }

    [TestMethod]
    public void WhereOr_NullSource_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((IQueryable<int>)null!).WhereOr(x => true, x => true));
    }
}
