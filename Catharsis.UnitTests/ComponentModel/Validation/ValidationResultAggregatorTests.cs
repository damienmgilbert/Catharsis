using System.ComponentModel.DataAnnotations;
using Catharsis.ComponentModel.Validation;

namespace Catharsis.UnitTests.ComponentModel.Validation;

[TestClass]
public sealed class ValidationResultAggregatorTests
{
    #region Public methods
    [TestMethod]
    public void Add_NullResult_IsIgnored()
    {
        ValidationResultAggregator agg = new ValidationResultAggregator();

        agg.Add(null);

        Assert.AreEqual(0, agg.Count);
    }

    [TestMethod]
    public void Add_SuccessResult_IsIgnored()
    {
        ValidationResultAggregator agg = new ValidationResultAggregator();

        agg.Add(ValidationResult.Success);

        Assert.AreEqual(0, agg.Count);
    }

    [TestMethod]
    public void Add_ValidResult_IncreasesCount()
    {
        ValidationResultAggregator agg = new ValidationResultAggregator();

        agg.Add(new ValidationResult("Bad value.", [ "Name" ]));

        Assert.AreEqual(1, agg.Count);
        Assert.IsTrue(agg.HasResults);
    }

    [TestMethod]
    public void AddRange_AddsMultipleResults()
    {
        ValidationResultAggregator agg = new ValidationResultAggregator();
        ValidationResult[] results = new[] { new ValidationResult("err1", [ "A" ]), new ValidationResult("err2", [ "B" ]) };

        agg.AddRange(results, ValidationSeverity.Warning);

        Assert.AreEqual(2, agg.Count);
    }

    [TestMethod]
    public void AddRange_NullResults_ThrowsArgumentNullException()
    {
        ValidationResultAggregator agg = new ValidationResultAggregator();

        Assert.ThrowsExactly<ArgumentNullException>(() => agg.AddRange(null!));
    }

    [TestMethod]
    public void Clear_RemovesAllResults()
    {
        ValidationResultAggregator agg = new ValidationResultAggregator();
        agg.Add(new ValidationResult("err"));
        agg.Add(new ValidationResult("warn"), ValidationSeverity.Warning);

        agg.Clear();

        Assert.AreEqual(0, agg.Count);
        Assert.IsFalse(agg.HasResults);
    }

    [TestMethod]
    public void GetAffectedMembers_ReturnsDistinctNames()
    {
        ValidationResultAggregator agg = new ValidationResultAggregator();
        agg.Add(new ValidationResult("e1", [ "Name" ]));
        agg.Add(new ValidationResult("e2", [ "Age" ]));
        agg.Add(new ValidationResult("e3", [ "Name" ]));

        IReadOnlyList<string> members = agg.GetAffectedMembers();

        Assert.AreEqual(2, members.Count);
        CollectionAssert.Contains(members.ToList(), "Name");
        CollectionAssert.Contains(members.ToList(), "Age");
    }

    [TestMethod]
    public void GetAll_ReturnsAllEntriesWithSeverity()
    {
        ValidationResultAggregator agg = new ValidationResultAggregator();
        agg.Add(new ValidationResult("e1"), ValidationSeverity.Error);
        agg.Add(new ValidationResult("w1"), ValidationSeverity.Warning);

        IReadOnlyList<(ValidationResult Result, ValidationSeverity Severity)> all = agg.GetAll();

        Assert.AreEqual(2, all.Count);
    }

    [TestMethod]
    public void GetByMember_FiltersByMemberName()
    {
        ValidationResultAggregator agg = new ValidationResultAggregator();
        agg.Add(new ValidationResult("e1", [ "Name" ]));
        agg.Add(new ValidationResult("e2", [ "Age" ]));
        agg.Add(new ValidationResult("e3", [ "Name" ]));

        IReadOnlyList<ValidationResult> nameErrors = agg.GetByMember("Name");

        Assert.AreEqual(2, nameErrors.Count);
    }

    [TestMethod]
    public void GetByMember_NullMemberName_ThrowsArgumentNullException()
    {
        ValidationResultAggregator agg = new ValidationResultAggregator();

        Assert.ThrowsExactly<ArgumentNullException>(() => agg.GetByMember(null!));
    }

    [TestMethod]
    public void GetBySeverity_FiltersCorrectly()
    {
        ValidationResultAggregator agg = new ValidationResultAggregator();
        agg.Add(new ValidationResult("e1"), ValidationSeverity.Error);
        agg.Add(new ValidationResult("w1"), ValidationSeverity.Warning);
        agg.Add(new ValidationResult("e2"), ValidationSeverity.Error);

        IReadOnlyList<ValidationResult> errors = agg.GetBySeverity(ValidationSeverity.Error);

        Assert.AreEqual(2, errors.Count);
    }

    [TestMethod]
    public void HasErrors_OnlyWarnings_ReturnsFalse()
    {
        ValidationResultAggregator agg = new ValidationResultAggregator();
        agg.Add(new ValidationResult("warn"), ValidationSeverity.Warning);

        Assert.IsFalse(agg.HasErrors);
    }

    [TestMethod]
    public void HasErrors_WithError_ReturnsTrue()
    {
        ValidationResultAggregator agg = new ValidationResultAggregator();
        agg.Add(new ValidationResult("error"), ValidationSeverity.Error);

        Assert.IsTrue(agg.HasErrors);
    }

    [TestMethod]
    public void Initial_State_IsEmpty()
    {
        ValidationResultAggregator agg = new ValidationResultAggregator();

        Assert.AreEqual(0, agg.Count);
        Assert.IsFalse(agg.HasResults);
        Assert.IsFalse(agg.HasErrors);
    }

    [TestMethod]
    public void ToErrorInfos_ConvertsCorrectly()
    {
        ValidationResultAggregator agg = new ValidationResultAggregator();
        agg.Add(new ValidationResult("err1", [ "Name" ]), ValidationSeverity.Error);
        agg.Add(new ValidationResult("warn1"), ValidationSeverity.Warning);

        IReadOnlyList<ErrorInfo> infos = agg.ToErrorInfos();

        Assert.AreEqual(2, infos.Count);
        Assert.AreEqual("err1", infos[0].Message);
        Assert.AreEqual(ValidationSeverity.Error, infos[0].Severity);
        Assert.AreEqual("Name", infos[0].PropertyName);
        Assert.AreEqual(ValidationSeverity.Warning, infos[1].Severity);
        Assert.IsNull(infos[1].PropertyName);
    }

    [TestMethod]
    public void ToErrorInfos_MultipleMembers_CreatesOnePerMember()
    {
        ValidationResultAggregator agg = new ValidationResultAggregator();
        agg.Add(new ValidationResult("cross", [ "A", "B" ]));

        IReadOnlyList<ErrorInfo> infos = agg.ToErrorInfos();

        Assert.AreEqual(2, infos.Count);
        Assert.AreEqual("A", infos[0].PropertyName);
        Assert.AreEqual("B", infos[1].PropertyName);
    }
    #endregion
}
