using Catharsis.Generics;

namespace Catharsis.UnitTests.Generics;

///<summary>
///Unit tests for the <see cref="Result{T, TError}"/> struct.
///</summary>
[TestClass]
public class ResultTests
{
    static Result<int, string> Parse(string text) => int.TryParse(text, out int value) ? Result<int, string>.Ok(value) : Result<int, string>.Fail($"bad:{text}");

    [TestMethod]
    public void Ok_ExposesValue()
    {
        Result<int, string> result = Result<int, string>.Ok(5);
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.IsFailure);
        Assert.AreEqual(5, result.Value);
        Assert.ThrowsExactly<InvalidOperationException>(() => result.Error);
    }

    [TestMethod]
    public void Fail_ExposesError()
    {
        Result<int, string> result = Result<int, string>.Fail("nope");
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual("nope", result.Error);
        Assert.ThrowsExactly<InvalidOperationException>(() => result.Value);
    }

    [TestMethod]
    public void Default_IsFailure() { Assert.IsTrue(default(Result<int, string>).IsFailure); }

    [TestMethod]
    public void ImplicitConversion_FromValue_IsSuccess()
    {
        Result<int, string> result = 7;
        Assert.AreEqual(7, result.Value);
    }

    [TestMethod]
    public void Map_Success_TransformsValue_FailureUntouched()
    {
        Assert.AreEqual(10, Parse("5").Map(static x => x * 2).Value);
        Assert.AreEqual("bad:x", Parse("x").Map(static x => x * 2).Error);
    }

    [TestMethod]
    public void Bind_ShortCircuitsOnFirstFailure()
    {
        bool secondRan = false;
        Result<int, string> result = Parse("x").Bind(v => { secondRan = true; return Parse(v.ToString(System.Globalization.CultureInfo.InvariantCulture)); });

        Assert.IsTrue(result.IsFailure);
        Assert.IsFalse(secondRan);
        Assert.AreEqual(9, Parse("4").Bind(static v => Result<int, string>.Ok(v + 5)).Value);
    }

    [TestMethod]
    public void MapError_TransformsFailureOnly()
    {
        Assert.AreEqual(5, Parse("x").MapError(static e => e.Length).Error);
        Assert.AreEqual(3, Parse("3").MapError(static e => e.Length).Value);
    }

    [TestMethod]
    public void Match_PicksBranch()
    {
        Assert.AreEqual("ok:2", Parse("2").Match(static v => $"ok:{v}", static e => e));
        Assert.AreEqual("bad:q", Parse("q").Match(static v => $"ok:{v}", static e => e));
    }

    [TestMethod]
    public void TryGet_ReportsCase()
    {
        Assert.IsTrue(Parse("2").TryGetValue(out int value));
        Assert.AreEqual(2, value);
        Assert.IsTrue(Parse("q").TryGetError(out string error));
        Assert.AreEqual("bad:q", error);
        Assert.IsFalse(Parse("q").TryGetValue(out _));
    }

    [TestMethod]
    public void GetValueOrDefault_FallsBackOnFailure() { Assert.AreEqual(-1, Parse("q").GetValueOrDefault(-1)); }

    [TestMethod]
    public void Equality_ComparesCaseAndPayload()
    {
        Assert.IsTrue(Parse("1") == Parse("1"));
        Assert.IsTrue(Parse("1") != Parse("2"));
        Assert.IsTrue(Parse("x") != Parse("1"));
    }

    [TestMethod]
    public void Map_NullMapper_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => Parse("1").Map<int>(null!)); }
}
