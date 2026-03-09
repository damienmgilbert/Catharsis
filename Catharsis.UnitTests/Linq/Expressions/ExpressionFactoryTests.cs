using System.Linq.Expressions;

using Catharsis.Linq.Expressions;

namespace Catharsis.UnitTests.Linq.Expressions;

///<summary>
///Unit tests for the <see cref="ExpressionFactory"/> class.
///</summary>
[TestClass]
public class ExpressionFactoryTests
{
    private static readonly ConstantExpression Left = Expression.Constant(5);
    private static readonly ConstantExpression Right = Expression.Constant(3);

    #region Arithmetic

    [TestMethod]
    public void Add_CreatesAddExpression()
    {
        BinaryExpression result = ExpressionFactory.Add(Left, Right);

        Assert.AreEqual(ExpressionType.Add, result.NodeType);
        Assert.AreEqual(8, Expression.Lambda<Func<int>>(result).Compile()());
    }

    [TestMethod]
    public void Add_NullLeft_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(static () => ExpressionFactory.Add(null!, Right));
    }

    [TestMethod]
    public void AddChecked_CreatesCheckedAddExpression()
    {
        BinaryExpression result = ExpressionFactory.AddChecked(Left, Right);

        Assert.AreEqual(ExpressionType.AddChecked, result.NodeType);
    }

    #endregion

    #region Logic

    [TestMethod]
    public void And_CreatesBitwiseAndExpression()
    {
        ConstantExpression a = Expression.Constant(0b1100);
        ConstantExpression b = Expression.Constant(0b1010);

        BinaryExpression result = ExpressionFactory.And(a, b);

        Assert.AreEqual(ExpressionType.And, result.NodeType);
        Assert.AreEqual(0b1000, Expression.Lambda<Func<int>>(result).Compile()());
    }

    [TestMethod]
    public void AndAlso_CreatesShortCircuitAnd()
    {
        ConstantExpression t = Expression.Constant(true);
        ConstantExpression f = Expression.Constant(false);

        BinaryExpression result = ExpressionFactory.AndAlso(t, f);

        Assert.AreEqual(ExpressionType.AndAlso, result.NodeType);
        Assert.IsFalse(Expression.Lambda<Func<bool>>(result).Compile()());
    }

    #endregion

    #region Array

    [TestMethod]
    public void ArrayIndex_CreatesIndexExpression()
    {
        ConstantExpression arr = Expression.Constant(new[] { 10, 20, 30 });
        ConstantExpression idx = Expression.Constant(1);

        BinaryExpression result = ExpressionFactory.ArrayIndex(arr, idx);

        Assert.AreEqual(ExpressionType.ArrayIndex, result.NodeType);
        Assert.AreEqual(20, Expression.Lambda<Func<int>>(result).Compile()());
    }

    [TestMethod]
    public void ArrayLength_CreatesLengthExpression()
    {
        ConstantExpression arr = Expression.Constant(new[] { 10, 20, 30 });

        UnaryExpression result = ExpressionFactory.ArrayLength(arr);

        Assert.AreEqual(ExpressionType.ArrayLength, result.NodeType);
        Assert.AreEqual(3, Expression.Lambda<Func<int>>(result).Compile()());
    }

    #endregion

    #region Assignment

    [TestMethod]
    public void Assign_CreatesAssignExpression()
    {
        ParameterExpression variable = Expression.Variable(typeof(int), "x");
        ConstantExpression value = Expression.Constant(42);

        BinaryExpression result = ExpressionFactory.Assign(variable, value);

        Assert.AreEqual(ExpressionType.Assign, result.NodeType);
    }

    #endregion

    #region Block

    [TestMethod]
    public void Block_CreatesBlockExpression()
    {
        ConstantExpression expr1 = Expression.Constant(1);
        ConstantExpression expr2 = Expression.Constant(2);

        BlockExpression result = ExpressionFactory.Block(expr1, expr2);

        Assert.AreEqual(ExpressionType.Block, result.NodeType);
        Assert.AreEqual(2, result.Expressions.Count);
    }

    [TestMethod]
    public void Block_WithVariables_CreatesBlockWithLocals()
    {
        ParameterExpression variable = Expression.Variable(typeof(int), "x");
        ConstantExpression value = Expression.Constant(10);

        BlockExpression result = ExpressionFactory.Block([variable], Expression.Assign(variable, value), variable);

        Assert.AreEqual(1, result.Variables.Count);
    }

    [TestMethod]
    public void Block_WithType_CreatesTypedBlock()
    {
        BlockExpression result = ExpressionFactory.Block(typeof(int), Expression.Constant(42));

        Assert.AreEqual(typeof(int), result.Type);
    }

    #endregion

    #region Bind

    [TestMethod]
    public void Bind_CreatesMemberAssignment()
    {
        var member = typeof(SampleClass).GetProperty(nameof(SampleClass.Value))!;
        ConstantExpression value = Expression.Constant(42);

        MemberAssignment result = ExpressionFactory.Bind(member, value);

        Assert.AreEqual(member, result.Member);
    }

    #endregion

    #region Break

    [TestMethod]
    public void Break_CreatesGotoExpression()
    {
        LabelTarget target = Expression.Label();
        GotoExpression result = ExpressionFactory.Break(target);

        Assert.AreEqual(GotoExpressionKind.Break, result.Kind);
    }

    #endregion

    #region ArrayAccess

    [TestMethod]
    public void ArrayAccess_CreatesIndexExpression()
    {
        ConstantExpression arr = Expression.Constant(new[] { 10, 20, 30 });
        ConstantExpression idx = Expression.Constant(2);

        IndexExpression result = ExpressionFactory.ArrayAccess(arr, idx);

        Assert.AreEqual(30, Expression.Lambda<Func<int>>(result).Compile()());
    }

    #endregion

    private sealed class SampleClass
    {
        public int Value { get; set; }
    }
}
