using System.Linq.Expressions;

namespace Catharsis.Linq.Expressions;

///<summary>
///Provides methods for compiling and invoking <see cref="LambdaExpression"/> trees. Supports strongly-typed invocation
///with various arities, asynchronous invocation, and <c>try</c>-style safe invocation.
///</summary>
public static class ExpressionInvoker
{
    #region Public methods

    ///<summary>
    ///Compiles a <see cref="LambdaExpression"/> into a <see cref="Delegate"/>.
    ///</summary>
    ///<param name="lambda">The lambda expression to compile.</param>
    ///<returns>A <see cref="Delegate"/> that can be invoked at runtime.</returns>
    public static Delegate Compile(LambdaExpression lambda)
    {
        ArgumentNullException.ThrowIfNull(lambda, nameof(lambda));
        return lambda.Compile();
    }

    ///<summary>
    ///Compiles a strongly-typed <see cref="Expression{TDelegate}"/> into a <typeparamref name="TDelegate"/>.
    ///</summary>
    ///<typeparam name="TDelegate">The delegate type.</typeparam>
    ///<param name="expression">The expression to compile.</param>
    ///<returns>A compiled <typeparamref name="TDelegate"/>.</returns>
    public static TDelegate Compile<TDelegate>(Expression<TDelegate> expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        return expression.Compile();
    }

    ///<summary>
    ///Compiles a <see cref="LambdaExpression"/> and invokes it dynamically with the specified arguments.
    ///</summary>
    ///<param name="lambda">The lambda expression to compile and invoke.</param>
    ///<param name="arguments">The arguments to pass to the compiled delegate.</param>
    ///<returns>The result of the invocation, or <c>null</c> if the delegate returns void.</returns>
    public static object? DynamicInvoke(LambdaExpression lambda, params object?[] arguments)
    {
        ArgumentNullException.ThrowIfNull(lambda, nameof(lambda));
        return lambda.Compile().DynamicInvoke(arguments);
    }

    ///<summary>
    ///Compiles and invokes a parameterless <see cref="Expression{TDelegate}"/>.
    ///</summary>
    ///<typeparam name="TResult">The return type.</typeparam>
    ///<param name="expression">The expression to invoke.</param>
    ///<returns>The result of the invocation.</returns>
    public static TResult Invoke<TResult>(Expression<Func<TResult>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        return expression.Compile().Invoke();
    }

    ///<summary>
    ///Compiles and invokes a parameterless <see cref="Expression{TDelegate}"/> returning void.
    ///</summary>
    ///<param name="expression">The expression to invoke.</param>
    public static void Invoke(Expression<Action> expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        expression.Compile().Invoke();
    }

    ///<summary>
    ///Compiles and invokes a single-argument <see cref="Expression{TDelegate}"/>.
    ///</summary>
    ///<typeparam name="T">The argument type.</typeparam>
    ///<typeparam name="TResult">The return type.</typeparam>
    ///<param name="expression">The expression to invoke.</param>
    ///<param name="arg">The argument value.</param>
    ///<returns>The result of the invocation.</returns>
    public static TResult Invoke<T, TResult>(Expression<Func<T, TResult>> expression, T arg)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        return expression.Compile().Invoke(arg);
    }

    ///<summary>
    ///Compiles and invokes a single-argument <see cref="Expression{TDelegate}"/> returning void.
    ///</summary>
    ///<typeparam name="T">The argument type.</typeparam>
    ///<param name="expression">The expression to invoke.</param>
    ///<param name="arg">The argument value.</param>
    public static void Invoke<T>(Expression<Action<T>> expression, T arg)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        expression.Compile().Invoke(arg);
    }

    ///<summary>
    ///Compiles and invokes a two-argument <see cref="Expression{TDelegate}"/>.
    ///</summary>
    ///<typeparam name="T1">The first argument type.</typeparam>
    ///<typeparam name="T2">The second argument type.</typeparam>
    ///<typeparam name="TResult">The return type.</typeparam>
    ///<param name="expression">The expression to invoke.</param>
    ///<param name="arg1">The first argument.</param>
    ///<param name="arg2">The second argument.</param>
    ///<returns>The result of the invocation.</returns>
    public static TResult Invoke<T1, T2, TResult>(Expression<Func<T1, T2, TResult>> expression, T1 arg1, T2 arg2)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        return expression.Compile().Invoke(arg1, arg2);
    }

    ///<summary>
    ///Compiles and invokes a two-argument <see cref="Expression{TDelegate}"/> returning void.
    ///</summary>
    ///<typeparam name="T1">The first argument type.</typeparam>
    ///<typeparam name="T2">The second argument type.</typeparam>
    ///<param name="expression">The expression to invoke.</param>
    ///<param name="arg1">The first argument.</param>
    ///<param name="arg2">The second argument.</param>
    public static void Invoke<T1, T2>(Expression<Action<T1, T2>> expression, T1 arg1, T2 arg2)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        expression.Compile().Invoke(arg1, arg2);
    }

    ///<summary>
    ///Compiles and invokes a three-argument <see cref="Expression{TDelegate}"/>.
    ///</summary>
    ///<typeparam name="T1">The first argument type.</typeparam>
    ///<typeparam name="T2">The second argument type.</typeparam>
    ///<typeparam name="T3">The third argument type.</typeparam>
    ///<typeparam name="TResult">The return type.</typeparam>
    ///<param name="expression">The expression to invoke.</param>
    ///<param name="arg1">The first argument.</param>
    ///<param name="arg2">The second argument.</param>
    ///<param name="arg3">The third argument.</param>
    ///<returns>The result of the invocation.</returns>
    public static TResult Invoke<T1, T2, T3, TResult>(Expression<Func<T1, T2, T3, TResult>> expression, T1 arg1, T2 arg2, T3 arg3)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        return expression.Compile().Invoke(arg1, arg2, arg3);
    }

    ///<summary>
    ///Compiles and invokes a three-argument <see cref="Expression{TDelegate}"/> returning void.
    ///</summary>
    ///<typeparam name="T1">The first argument type.</typeparam>
    ///<typeparam name="T2">The second argument type.</typeparam>
    ///<typeparam name="T3">The third argument type.</typeparam>
    ///<param name="expression">The expression to invoke.</param>
    ///<param name="arg1">The first argument.</param>
    ///<param name="arg2">The second argument.</param>
    ///<param name="arg3">The third argument.</param>
    public static void Invoke<T1, T2, T3>(Expression<Action<T1, T2, T3>> expression, T1 arg1, T2 arg2, T3 arg3)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        expression.Compile().Invoke(arg1, arg2, arg3);
    }

    ///<summary>
    ///Compiles and invokes a four-argument <see cref="Expression{TDelegate}"/>.
    ///</summary>
    ///<typeparam name="T1">The first argument type.</typeparam>
    ///<typeparam name="T2">The second argument type.</typeparam>
    ///<typeparam name="T3">The third argument type.</typeparam>
    ///<typeparam name="T4">The fourth argument type.</typeparam>
    ///<typeparam name="TResult">The return type.</typeparam>
    ///<param name="expression">The expression to invoke.</param>
    ///<param name="arg1">The first argument.</param>
    ///<param name="arg2">The second argument.</param>
    ///<param name="arg3">The third argument.</param>
    ///<param name="arg4">The fourth argument.</param>
    ///<returns>The result of the invocation.</returns>
    public static TResult Invoke<T1, T2, T3, T4, TResult>(Expression<Func<T1, T2, T3, T4, TResult>> expression, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        return expression.Compile().Invoke(arg1, arg2, arg3, arg4);
    }

    ///<summary>
    ///Compiles and invokes a four-argument <see cref="Expression{TDelegate}"/> returning void.
    ///</summary>
    ///<typeparam name="T1">The first argument type.</typeparam>
    ///<typeparam name="T2">The second argument type.</typeparam>
    ///<typeparam name="T3">The third argument type.</typeparam>
    ///<typeparam name="T4">The fourth argument type.</typeparam>
    ///<param name="expression">The expression to invoke.</param>
    ///<param name="arg1">The first argument.</param>
    ///<param name="arg2">The second argument.</param>
    ///<param name="arg3">The third argument.</param>
    ///<param name="arg4">The fourth argument.</param>
    public static void Invoke<T1, T2, T3, T4>(Expression<Action<T1, T2, T3, T4>> expression, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        expression.Compile().Invoke(arg1, arg2, arg3, arg4);
    }

    ///<summary>
    ///Compiles and invokes a parameterless expression that returns a <see cref="Task{TResult}"/>.
    ///</summary>
    ///<typeparam name="TResult">The result type of the task.</typeparam>
    ///<param name="expression">The expression returning a <see cref="Task{TResult}"/>.</param>
    ///<param name="cancellationToken">An optional cancellation token (the caller is responsible for honoring it within the expression).</param>
    ///<returns>The awaited result.</returns>
    public static async Task<TResult> InvokeAsync<TResult>(Expression<Func<Task<TResult>>> expression, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        cancellationToken.ThrowIfCancellationRequested();
        return await expression.Compile().Invoke().ConfigureAwait(false);
    }

    ///<summary>
    ///Compiles and invokes a parameterless expression that returns a <see cref="ValueTask{TResult}"/>.
    ///</summary>
    ///<typeparam name="TResult">The result type of the task.</typeparam>
    ///<param name="expression">The expression returning a <see cref="ValueTask{TResult}"/>.</param>
    ///<param name="cancellationToken">An optional cancellation token (the caller is responsible for honoring it within the expression).</param>
    ///<returns>The awaited result.</returns>
    public static async ValueTask<TResult> InvokeAsync<TResult>(Expression<Func<ValueTask<TResult>>> expression, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        cancellationToken.ThrowIfCancellationRequested();
        return await expression.Compile().Invoke().ConfigureAwait(false);
    }

    ///<summary>
    ///Compiles and invokes a single-argument expression that returns a <see cref="Task{TResult}"/>.
    ///</summary>
    ///<typeparam name="T">The argument type.</typeparam>
    ///<typeparam name="TResult">The result type of the task.</typeparam>
    ///<param name="expression">The expression returning a <see cref="Task{TResult}"/>.</param>
    ///<param name="arg">The argument value.</param>
    ///<param name="cancellationToken">An optional cancellation token.</param>
    ///<returns>The awaited result.</returns>
    public static async Task<TResult> InvokeAsync<T, TResult>(Expression<Func<T, Task<TResult>>> expression, T arg, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        cancellationToken.ThrowIfCancellationRequested();
        return await expression.Compile().Invoke(arg).ConfigureAwait(false);
    }

    ///<summary>
    ///Compiles and invokes a single-argument expression that returns a <see cref="ValueTask{TResult}"/>.
    ///</summary>
    ///<typeparam name="T">The argument type.</typeparam>
    ///<typeparam name="TResult">The result type of the task.</typeparam>
    ///<param name="expression">The expression returning a <see cref="ValueTask{TResult}"/>.</param>
    ///<param name="arg">The argument value.</param>
    ///<param name="cancellationToken">An optional cancellation token.</param>
    ///<returns>The awaited result.</returns>
    public static async ValueTask<TResult> InvokeAsync<T, TResult>(Expression<Func<T, ValueTask<TResult>>> expression, T arg, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        cancellationToken.ThrowIfCancellationRequested();
        return await expression.Compile().Invoke(arg).ConfigureAwait(false);
    }

    ///<summary>
    ///Attempts to compile and invoke a parameterless expression. Returns a value indicating whether invocation
    ///succeeded.
    ///</summary>
    ///<typeparam name="TResult">The return type.</typeparam>
    ///<param name="expression">The expression to invoke.</param>
    ///<param name="result">When this method returns, contains the result if successful; otherwise <c>default</c>.</param>
    ///<returns><c>true</c> if invocation succeeded; <c>false</c> if an exception was thrown.</returns>
    public static bool TryInvoke<TResult>(Expression<Func<TResult>> expression, out TResult? result)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));

        try
        {
            result = expression.Compile().Invoke();
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    ///<summary>
    ///Attempts to compile and invoke a single-argument expression. Returns a value indicating whether invocation
    ///succeeded.
    ///</summary>
    ///<typeparam name="T">The argument type.</typeparam>
    ///<typeparam name="TResult">The return type.</typeparam>
    ///<param name="expression">The expression to invoke.</param>
    ///<param name="arg">The argument value.</param>
    ///<param name="result">When this method returns, contains the result if successful; otherwise <c>default</c>.</param>
    ///<returns><c>true</c> if invocation succeeded; <c>false</c> if an exception was thrown.</returns>
    public static bool TryInvoke<T, TResult>(Expression<Func<T, TResult>> expression, T arg, out TResult? result)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));

        try
        {
            result = expression.Compile().Invoke(arg);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }
    #endregion
}
