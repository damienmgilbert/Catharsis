using System.Linq.Expressions;

using Catharsis.Linq.Expressions;

namespace Catharsis.UnitTests.Linq.Expressions;

///<summary>
///Unit tests for the <see cref="ExpressionInvoker"/> class.
///</summary>
[TestClass]
public class ExpressionInvokerTests
{
    #region Compile

    [TestMethod]
    public void Compile_LambdaExpression_ReturnsDelegate()
    {
        ParameterExpression p = Expression.Parameter(typeof(int), "x");
        LambdaExpression lambda = Expression.Lambda(Expression.Add(p, Expression.Constant(1)), p);

        Delegate compiled = ExpressionInvoker.Compile(lambda);

        Assert.IsNotNull(compiled);
        Assert.AreEqual(6, compiled.DynamicInvoke(5));
    }

    [TestMethod]
    public void Compile_StronglyTyped_ReturnsTypedDelegate()
    {
        Expression<Func<int, int>> expr = static x => x * 2;

        Func<int, int> compiled = ExpressionInvoker.Compile(expr);

        Assert.AreEqual(10, compiled(5));
    }

    #endregion

    #region Invoke (parameterless)

    [TestMethod]
    public void Invoke_Parameterless_ReturnsResult()
    {
        Expression<Func<int>> expr = static () => 42;

        int result = ExpressionInvoker.Invoke(expr);

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void Invoke_Parameterless_Void_Executes()
    {
        int sideEffect = 0;
        Expression<Action> expr = () => Interlocked.Increment(ref sideEffect);

        ExpressionInvoker.Invoke(expr);

        Assert.AreEqual(1, sideEffect);
    }

    #endregion

    #region Invoke (1 arg)

    [TestMethod]
    public void Invoke_OneArg_ReturnsResult()
    {
        Expression<Func<int, string>> expr = static x => x.ToString();

        string result = ExpressionInvoker.Invoke(expr, 42);

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void Invoke_OneArg_Void_Executes()
    {
        int captured = 0;
        Expression<Action<int>> expr = x => Interlocked.Exchange(ref captured, x);

        ExpressionInvoker.Invoke(expr, 99);

        Assert.AreEqual(99, captured);
    }

    #endregion

    #region Invoke (2 args)

    [TestMethod]
    public void Invoke_TwoArgs_ReturnsResult()
    {
        Expression<Func<int, int, int>> expr = static (a, b) => a + b;

        int result = ExpressionInvoker.Invoke(expr, 3, 7);

        Assert.AreEqual(10, result);
    }

    #endregion

    #region Invoke (3 args)

    [TestMethod]
    public void Invoke_ThreeArgs_ReturnsResult()
    {
        Expression<Func<int, int, int, int>> expr = static (a, b, c) => a + b + c;

        int result = ExpressionInvoker.Invoke(expr, 1, 2, 3);

        Assert.AreEqual(6, result);
    }

    #endregion

    #region Invoke (4 args)

    [TestMethod]
    public void Invoke_FourArgs_ReturnsResult()
    {
        Expression<Func<int, int, int, int, int>> expr = static (a, b, c, d) => a * b + c * d;

        int result = ExpressionInvoker.Invoke(expr, 2, 3, 4, 5);

        Assert.AreEqual(26, result);
    }

    #endregion

    #region DynamicInvoke

    [TestMethod]
    public void DynamicInvoke_InvokesWithArguments()
    {
        ParameterExpression p = Expression.Parameter(typeof(int), "x");
        LambdaExpression lambda = Expression.Lambda(Expression.Multiply(p, Expression.Constant(3)), p);

        object? result = ExpressionInvoker.DynamicInvoke(lambda, 4);

        Assert.AreEqual(12, result);
    }

    [TestMethod]
    public void DynamicInvoke_NullLambda_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            static () => ExpressionInvoker.DynamicInvoke(null!));
    }

    #endregion

    #region InvokeAsync

    [TestMethod]
    public async Task InvokeAsync_Task_Parameterless_ReturnsResult()
    {
        Expression<Func<Task<int>>> expr = static () => Task.FromResult(42);

        int result = await ExpressionInvoker.InvokeAsync(expr);

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public async Task InvokeAsync_ValueTask_Parameterless_ReturnsResult()
    {
        Expression<Func<ValueTask<int>>> expr = static () => ValueTask.FromResult(42);

        int result = await ExpressionInvoker.InvokeAsync(expr);

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public async Task InvokeAsync_Task_OneArg_ReturnsResult()
    {
        Expression<Func<int, Task<int>>> expr = static x => Task.FromResult(x * 2);

        int result = await ExpressionInvoker.InvokeAsync(expr, 5);

        Assert.AreEqual(10, result);
    }

    [TestMethod]
    public async Task InvokeAsync_ValueTask_OneArg_ReturnsResult()
    {
        Expression<Func<int, ValueTask<int>>> expr = static x => ValueTask.FromResult(x * 2);

        int result = await ExpressionInvoker.InvokeAsync(expr, 5);

        Assert.AreEqual(10, result);
    }

    #endregion

    #region TryInvoke

    [TestMethod]
    public void TryInvoke_Success_ReturnsTrueWithResult()
    {
        Expression<Func<int>> expr = static () => 42;

        bool success = ExpressionInvoker.TryInvoke<int>(expr, out int result);

        Assert.IsTrue(success);
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void TryInvoke_Failure_ReturnsFalse()
    {
        Expression<Func<int>> expr = static () => ThrowHelper();

        bool success = ExpressionInvoker.TryInvoke<int>(expr, out int result);

        Assert.IsFalse(success);
        Assert.AreEqual(default, result);
    }

    [TestMethod]
    public void TryInvoke_OneArg_Success_ReturnsTrueWithResult()
    {
        Expression<Func<int, string>> expr = static x => x.ToString();

        bool success = ExpressionInvoker.TryInvoke<int, string>(expr, 42, out string? result);

        Assert.IsTrue(success);
        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void TryInvoke_OneArg_Failure_ReturnsFalse()
    {
        Expression<Func<string, int>> expr = static s => s.Length;

        bool success = ExpressionInvoker.TryInvoke<string, int>(expr, null!, out int result);

        Assert.IsFalse(success);
    }

    #endregion

    #region Null guards

    [TestMethod]
    public void Invoke_NullExpression_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            static () => ExpressionInvoker.Invoke<int>(null!));
    }

    [TestMethod]
    public void Compile_NullLambda_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            static () => ExpressionInvoker.Compile((LambdaExpression)null!));
    }

    #endregion

    private static int ThrowHelper() => throw new InvalidOperationException("test");
}
