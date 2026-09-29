using Catharsis.Operators;

namespace Catharsis.UnitTests.Operators;

///<summary>
///Unit tests for the <see cref="Money"/> struct.
///</summary>
[TestClass]
public class MoneyTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullCurrency_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new Money(1m, null!)); }

    [TestMethod]
    [DataRow("")]
    [DataRow("US")]
    [DataRow("USDX")]
    [DataRow("U1D")]
    public void Constructor_InvalidCurrency_Throws(string currency) { Assert.ThrowsExactly<ArgumentException>(() => new Money(1m, currency)); }

    [TestMethod]
    public void Constructor_LowerCaseCurrency_IsUpperCased() { Assert.AreEqual("USD", new Money(1m, "usd").Currency); }

    #endregion

    #region Operators

    [TestMethod]
    public void Add_SameCurrency_Sums() { Assert.AreEqual(new Money(5m, "USD"), new Money(2m, "USD") + new Money(3m, "USD")); }

    [TestMethod]
    public void Add_DifferentCurrency_Throws() { Assert.ThrowsExactly<InvalidOperationException>(static () => new Money(2m, "USD") + new Money(3m, "EUR")); }

    [TestMethod]
    public void Subtract_SameCurrency_Differences() { Assert.AreEqual(new Money(-1m, "USD"), new Money(2m, "USD") - new Money(3m, "USD")); }

    [TestMethod]
    public void Negate_FlipsSign() { Assert.AreEqual(new Money(-2m, "USD"), -new Money(2m, "USD")); }

    [TestMethod]
    public void Multiply_ByFactorEitherOrder_Scales()
    {
        Money value = new(2m, "USD");
        Assert.AreEqual(new Money(6m, "USD"), value * 3m);
        Assert.AreEqual(new Money(6m, "USD"), 3m * value);
    }

    [TestMethod]
    public void Divide_ByZero_Throws() { Assert.ThrowsExactly<DivideByZeroException>(static () => new Money(2m, "USD") / 0m); }

    [TestMethod]
    public void Compare_DifferentCurrency_Throws() { Assert.ThrowsExactly<InvalidOperationException>(static () => new Money(2m, "USD") < new Money(3m, "EUR")); }

    [TestMethod]
    public void Compare_SameCurrency_Orders()
    {
        Assert.IsTrue(new Money(2m, "USD") < new Money(3m, "USD"));
        Assert.IsTrue(new Money(3m, "USD") >= new Money(3m, "USD"));
    }

    [TestMethod]
    public void Equality_DifferentCurrencySameAmount_NotEqual() { Assert.IsTrue(new Money(2m, "USD") != new Money(2m, "EUR")); }

    [TestMethod]
    public void ExplicitConversion_ToDecimal_ReturnsAmount() { Assert.AreEqual(2.5m, (decimal)new Money(2.5m, "USD")); }

    #endregion

    #region Allocate

    [TestMethod]
    public void Allocate_NonPositiveParts_Throws() { Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new Money(1m, "USD").Allocate(0)); }

    [TestMethod]
    public void Allocate_UnevenSplit_SumsToOriginalWithLeftoverToFirst()
    {
        Money[] parts = new Money(100m, "USD").Allocate(3);
        Assert.AreEqual(33.34m, parts[0].Amount);
        Assert.AreEqual(33.33m, parts[1].Amount);
        Assert.AreEqual(33.33m, parts[2].Amount);
        Assert.AreEqual(100m, parts.Sum(static p => p.Amount));
    }

    [TestMethod]
    public void Allocate_NegativeAmount_SumsToOriginal()
    {
        Money[] parts = new Money(-100m, "USD").Allocate(3);
        Assert.AreEqual(-100m, parts.Sum(static p => p.Amount));
    }

    #endregion

    #region Parse / conversion

    [TestMethod]
    public void Parse_RoundTripsToString() { Assert.AreEqual(new Money(12.5m, "USD"), Money.Parse(new Money(12.5m, "USD").ToString(), System.Globalization.CultureInfo.InvariantCulture)); }

    [TestMethod]
    [DataRow("")]
    [DataRow("12.50")]
    [DataRow("abc USD")]
    [DataRow("12.50 US")]
    [DataRow("12.50 USD extra")]
    public void TryParse_Invalid_ReturnsFalse(string text) { Assert.IsFalse(Money.TryParse(text, null, out _)); }

    [TestMethod]
    public void TryParse_Null_ReturnsFalse() { Assert.IsFalse(Money.TryParse(null, null, out _)); }

    [TestMethod]
    public void Parse_Invalid_Throws() { Assert.ThrowsExactly<FormatException>(static () => Money.Parse("nope", System.Globalization.CultureInfo.InvariantCulture)); }

    [TestMethod]
    public void ImplicitConversion_FromTuple_Works()
    {
        Money money = (5m, "eur");
        Assert.AreEqual(new Money(5m, "EUR"), money);
    }

    [TestMethod]
    public void GenericParse_ViaIParsable_Works() { Assert.AreEqual(new Money(1m, "USD"), ParseGeneric<Money>("1 USD")); }

    static T ParseGeneric<T>(string text)
        where T : IParsable<T> => T.Parse(text, null);

    #endregion

    #region Round / ToString

    [TestMethod]
    public void Round_UsesBankersRounding() { Assert.AreEqual(2m, new Money(2.5m, "USD").Round(0).Amount); }

    [TestMethod]
    public void ToString_FormatsAmountAndCurrency() { Assert.AreEqual("12.50 USD", new Money(12.5m, "usd").ToString()); }

    #endregion
}
