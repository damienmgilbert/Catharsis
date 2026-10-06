namespace Catharsis.Extensions;

///<summary>
///Provides fluent flow-control extension methods for any type. These helpers allow conditional execution and loop-style
///operations in a fluent style.
///</summary>
public static class ControlFlow
{
    #region Public methods

    ///<summary>
    ///Repeatedly executes <paramref name="action"/> until <paramref name="condition"/> evaluates to <c>true</c>, then
    ///returns the original object.
    ///</summary>
    ///<typeparam name="T">The type of the object the extension is called on.</typeparam>
    ///<param name="obj">The object supplied to the condition and action on each iteration.</param>
    ///<param name="condition">A function that receives the object and returns <c>true</c> to stop looping.</param>
    ///<param name="action">The action to execute on each iteration until the condition becomes <c>true</c>.</param>
    ///<returns>The original <paramref name="obj"/> after the loop completes.</returns>
    ///<remarks>
    ///If the <paramref name="action"/> does not change the state used by <paramref name="condition"/>, this method may
    ///produce an infinite loop. Use with care.
    ///</remarks>
    public static T DoUntil<T>(this T obj, Func<T, bool> condition, Action<T> action)
    {
        while(!condition(obj))
        {
            action(obj);
        }
        return obj;
    }

    ///<summary>
    ///Repeatedly executes <paramref name="action"/> until <paramref name="condition"/> evaluates to <c>true</c>, then
    ///returns the original object.
    ///</summary>
    public static async Task<T> DoUntilAsync<T>(this T obj, Func<T, CancellationToken, Task<bool>> condition, Func<T, CancellationToken, Task> action, CancellationToken cancellation = default)
    {
        if(condition is null)
        {
            throw new ArgumentNullException(nameof(condition), "Condition function must not be null.");
        }

        if(action is null)
        {
            throw new ArgumentNullException(nameof(action), "Action must not be null.");
        }

        cancellation.ThrowIfCancellationRequested();
        while(!(await condition(obj, cancellation).ConfigureAwait(false)))
        {
            cancellation.ThrowIfCancellationRequested();
            await action(obj, cancellation).ConfigureAwait(false);
        }
        return obj;
    }

    ///<summary>
    ///Repeatedly executes <paramref name="action"/> until <paramref name="condition"/> evaluates to <c>true</c>, then
    ///returns the original object (ValueTask overload).
    ///</summary>
    public static ValueTask<T> DoUntilAsync<T>(this T obj, Func<T, CancellationToken, ValueTask<bool>> condition, Func<T, CancellationToken, ValueTask> action, CancellationToken cancellation = default)
    {
        if(condition is null)
        {
            throw new ArgumentNullException(nameof(condition), "Condition function must not be null.");
        }

        if(action is null)
        {
            throw new ArgumentNullException(nameof(action), "Action must not be null.");
        }

        cancellation.ThrowIfCancellationRequested();
        ValueTask<bool> conditionTask = condition(obj, cancellation);
        if(conditionTask.IsCompletedSuccessfully)
        {
            // Consume the completed ValueTask exactly once; hand the slow path a fresh
            // instance rather than re-awaiting the one already read here.
            if(conditionTask.Result)
            {
                return ValueTask.FromResult(obj);
            }

            return SlowPath(obj, new ValueTask<bool>(false), condition, action, cancellation);
        }

        return SlowPath(obj, conditionTask, condition, action, cancellation);

        static async ValueTask<T> SlowPath(T obj, ValueTask<bool> initial, Func<T, CancellationToken, ValueTask<bool>> condition, Func<T, CancellationToken, ValueTask> action, CancellationToken cancellation)
        {
            if(!(await initial.ConfigureAwait(false)))
            {
                await action(obj, cancellation).ConfigureAwait(false);

                while(!(await condition(obj, cancellation).ConfigureAwait(false)))
                {
                    cancellation.ThrowIfCancellationRequested();
                    await action(obj, cancellation).ConfigureAwait(false);
                }
            }
            return obj;
        }
    }

    ///<summary>
    ///Repeatedly executes <paramref name="action"/> while <paramref name="condition"/> evaluates to <c>true</c>, then
    ///returns the original object.
    ///</summary>
    ///<typeparam name="T">The type of the object the extension is called on.</typeparam>
    ///<param name="obj">The object supplied to the condition and action on each iteration.</param>
    ///<param name="condition">A function that receives the object and returns <c>true</c> to continue looping.</param>
    ///<param name="action">The action to execute on each iteration while the condition is <c>true</c>.</param>
    ///<returns>The original <paramref name="obj"/> after the loop completes.</returns>
    ///<remarks>
    ///If the <paramref name="action"/> does not change the state used by <paramref name="condition"/>, this method may
    ///produce an infinite loop. Use with care.
    ///</remarks>
    public static T DoWhile<T>(this T obj, Func<T, bool> condition, Action<T> action)
    {
        while(condition(obj))
        {
            action(obj);
        }
        return obj;
    }

    ///<summary>
    ///Repeatedly executes <paramref name="action"/> while <paramref name="condition"/> evaluates to <c>true</c>, then
    ///returns the original object.
    ///</summary>
    public static async Task<T> DoWhileAsync<T>(this T obj, Func<T, CancellationToken, Task<bool>> condition, Func<T, CancellationToken, Task> action, CancellationToken cancellation = default)
    {
        if(condition is null)
        {
            throw new ArgumentNullException(nameof(condition), "Condition function must not be null.");
        }

        if(action is null)
        {
            throw new ArgumentNullException(nameof(action), "Action must not be null.");
        }

        cancellation.ThrowIfCancellationRequested();
        while(await condition(obj, cancellation).ConfigureAwait(false))
        {
            cancellation.ThrowIfCancellationRequested();
            await action(obj, cancellation).ConfigureAwait(false);
        }
        return obj;
    }

    ///<summary>
    ///Repeatedly executes <paramref name="action"/> while <paramref name="condition"/> evaluates to <c>true</c>, then
    ///returns the original object (ValueTask overload).
    ///</summary>
    public static ValueTask<T> DoWhileAsync<T>(this T obj, Func<T, CancellationToken, ValueTask<bool>> condition, Func<T, CancellationToken, ValueTask> action, CancellationToken cancellation = default)
    {
        if(condition is null)
        {
            throw new ArgumentNullException(nameof(condition), "Condition function must not be null.");
        }

        if(action is null)
        {
            throw new ArgumentNullException(nameof(action), "Action must not be null.");
        }

        cancellation.ThrowIfCancellationRequested();
        ValueTask<bool> conditionTask = condition(obj, cancellation);
        if(conditionTask.IsCompletedSuccessfully)
        {
            // Consume the completed ValueTask exactly once; hand the slow path a fresh
            // instance rather than re-awaiting the one already read here.
            if(!conditionTask.Result)
            {
                return ValueTask.FromResult(obj);
            }

            return SlowPath(obj, new ValueTask<bool>(true), condition, action, cancellation);
        }

        return SlowPath(obj, conditionTask, condition, action, cancellation);

        static async ValueTask<T> SlowPath(T obj, ValueTask<bool> initial, Func<T, CancellationToken, ValueTask<bool>> condition, Func<T, CancellationToken, ValueTask> action, CancellationToken cancellation)
        {
            if(await initial.ConfigureAwait(false))
            {
                await action(obj, cancellation).ConfigureAwait(false);

                while(await condition(obj, cancellation).ConfigureAwait(false))
                {
                    cancellation.ThrowIfCancellationRequested();
                    await action(obj, cancellation).ConfigureAwait(false);
                }
            }
            return obj;
        }
    }

    ///<summary>
    ///Executes <paramref name="action"/> when <paramref name="condition"/> evaluates to <c>true</c> for the current
    ///object.
    ///</summary>
    ///<typeparam name="T">The type of the object the extension is called on.</typeparam>
    ///<param name="obj">The object to test and pass to the action.</param>
    ///<param name="condition">A function that receives the object and returns <c>true</c> to run the action.</param>
    ///<param name="action">The action to execute when the condition is satisfied.</param>
    public static void If<T>(this T obj, Func<T, bool> condition, Action<T> action)
    {
        if(condition(obj))
        {
            action(obj);
        }
    }

    ///<summary>
    ///Asynchronously executes <paramref name="action"/> when <paramref name="condition"/> evaluates to <c>true</c>.
    ///</summary>
    public static async Task IfAsync<T>(this T obj, Func<T, CancellationToken, Task<bool>> condition, Func<T, CancellationToken, Task> action, CancellationToken cancellation = default)
    {
        if(condition is null)
        {
            throw new ArgumentNullException(nameof(condition), "Condition function must not be null.");
        }

        if(action is null)
        {
            throw new ArgumentNullException(nameof(action), "Action must not be null.");
        }

        cancellation.ThrowIfCancellationRequested();
        if(await condition(obj, cancellation).ConfigureAwait(false))
        {
            await action(obj, cancellation).ConfigureAwait(false);
        }
    }

    ///<summary>
    ///Asynchronously executes <paramref name="action"/> when <paramref name="condition"/> evaluates to <c>true</c>
    ///(ValueTask overload).
    ///</summary>
    public static ValueTask IfAsync<T>(this T obj, Func<T, CancellationToken, ValueTask<bool>> condition, Func<T, CancellationToken, ValueTask> action, CancellationToken cancellation = default)
    {
        if(condition is null)
        {
            throw new ArgumentNullException(nameof(condition), "Condition function must not be null.");
        }

        if(action is null)
        {
            throw new ArgumentNullException(nameof(action), "Action must not be null.");
        }

        cancellation.ThrowIfCancellationRequested();
        ValueTask<bool> conditionTask = condition(obj, cancellation);
        if(conditionTask.IsCompletedSuccessfully)
        {
            return conditionTask.Result ? action(obj, cancellation) : ValueTask.CompletedTask;
        }

        return SlowPath(obj, conditionTask, action, cancellation);

        static async ValueTask SlowPath(T obj, ValueTask<bool> conditionTask, Func<T, CancellationToken, ValueTask> action, CancellationToken cancellation)
        {
            if(await conditionTask.ConfigureAwait(false))
            {
                await action(obj, cancellation).ConfigureAwait(false);
            }
        }
    }

    ///<summary>
    ///Executes either <paramref name="ifAction"/> or <paramref name="elseAction"/> depending on the result of <paramref
    ///name="condition"/>.
    ///</summary>
    ///<typeparam name="T">The type of the object the extension is called on.</typeparam>
    ///<param name="obj">The object to test and pass to the selected action.</param>
    ///<param name="condition">A function that receives the object and returns <c>true</c> to run <paramref name="ifAction"/>.</param>
    ///<param name="ifAction">The action to execute when the condition is <c>true</c>.</param>
    ///<param name="elseAction">The action to execute when the condition is <c>false</c>.</param>
    public static void IfElse<T>(this T obj, Func<T, bool> condition, Action<T> ifAction, Action<T> elseAction)
    {
        if(condition(obj))
        {
            ifAction(obj);
        } else
        {
            elseAction(obj);
        }
    }

    ///<summary>
    ///Asynchronously executes either <paramref name="ifAction"/> or <paramref name="elseAction"/> depending on the
    ///result of <paramref name="condition"/>.
    ///</summary>
    public static async Task IfElseAsync<T>(this T obj, Func<T, CancellationToken, Task<bool>> condition, Func<T, CancellationToken, Task> ifAction, Func<T, CancellationToken, Task> elseAction, CancellationToken cancellation = default)
    {
        if(condition is null)
        {
            throw new ArgumentNullException(nameof(condition), "Condition function must not be null.");
        }

        if(ifAction is null)
        {
            throw new ArgumentNullException(nameof(ifAction), "If action must not be null.");
        }

        if(elseAction is null)
        {
            throw new ArgumentNullException(nameof(elseAction), "Else action must not be null.");
        }

        cancellation.ThrowIfCancellationRequested();
        if(await condition(obj, cancellation).ConfigureAwait(false))
        {
            await ifAction(obj, cancellation).ConfigureAwait(false);
        } else
        {
            await elseAction(obj, cancellation).ConfigureAwait(false);
        }
    }

    ///<summary>
    ///Asynchronously executes either <paramref name="ifAction"/> or <paramref name="elseAction"/> depending on the
    ///result of <paramref name="condition"/> (ValueTask overload).
    ///</summary>
    public static ValueTask IfElseAsync<T>(this T obj, Func<T, CancellationToken, ValueTask<bool>> condition, Func<T, CancellationToken, ValueTask> ifAction, Func<T, CancellationToken, ValueTask> elseAction, CancellationToken cancellation = default)
    {
        if(condition is null)
        {
            throw new ArgumentNullException(nameof(condition), "Condition function must not be null.");
        }

        if(ifAction is null)
        {
            throw new ArgumentNullException(nameof(ifAction), "If action must not be null.");
        }

        if(elseAction is null)
        {
            throw new ArgumentNullException(nameof(elseAction), "Else action must not be null.");
        }

        cancellation.ThrowIfCancellationRequested();
        ValueTask<bool> conditionTask = condition(obj, cancellation);
        if(conditionTask.IsCompletedSuccessfully)
        {
            return conditionTask.Result ? ifAction(obj, cancellation) : elseAction(obj, cancellation);
        }

        return SlowPath(obj, conditionTask, ifAction, elseAction, cancellation);

        static async ValueTask SlowPath(T obj, ValueTask<bool> conditionTask, Func<T, CancellationToken, ValueTask> ifAction, Func<T, CancellationToken, ValueTask> elseAction, CancellationToken cancellation)
        {
            if(await conditionTask.ConfigureAwait(false))
            {
                await ifAction(obj, cancellation).ConfigureAwait(false);
            } else
            {
                await elseAction(obj, cancellation).ConfigureAwait(false);
            }
        }
    }

    ///<summary>
    ///Executes <paramref name="action"/> when <paramref name="condition"/> evaluates to <c>false</c> for the current
    ///object.
    ///</summary>
    ///<typeparam name="T">The type of the object the extension is called on.</typeparam>
    ///<param name="obj">The object to test and pass to the action.</param>
    ///<param name="condition">A function that receives the object and returns <c>true</c> to skip the action.</param>
    ///<param name="action">The action to execute when the condition is not satisfied.</param>
    public static void IfNot<T>(this T obj, Func<T, bool> condition, Action<T> action)
    {
        if(!condition(obj))
        {
            action(obj);
        }
    }

    ///<summary>
    ///Asynchronously executes <paramref name="action"/> when <paramref name="condition"/> evaluates to <c>false</c>.
    ///</summary>
    public static async Task IfNotAsync<T>(this T obj, Func<T, CancellationToken, Task<bool>> condition, Func<T, CancellationToken, Task> action, CancellationToken cancellation = default)
    {
        if(condition is null)
        {
            throw new ArgumentNullException(nameof(condition), "Condition function must not be null.");
        }

        if(action is null)
        {
            throw new ArgumentNullException(nameof(action), "Action must not be null.");
        }

        cancellation.ThrowIfCancellationRequested();
        if(!(await condition(obj, cancellation).ConfigureAwait(false)))
        {
            await action(obj, cancellation).ConfigureAwait(false);
        }
    }

    ///<summary>
    ///Asynchronously executes <paramref name="action"/> when <paramref name="condition"/> evaluates to <c>false</c>
    ///(ValueTask overload).
    ///</summary>
    public static ValueTask IfNotAsync<T>(this T obj, Func<T, CancellationToken, ValueTask<bool>> condition, Func<T, CancellationToken, ValueTask> action, CancellationToken cancellation = default)
    {
        if(condition is null)
        {
            throw new ArgumentNullException(nameof(condition), "Condition function must not be null.");
        }

        if(action is null)
        {
            throw new ArgumentNullException(nameof(action), "Action must not be null.");
        }

        cancellation.ThrowIfCancellationRequested();
        ValueTask<bool> conditionTask = condition(obj, cancellation);
        if(conditionTask.IsCompletedSuccessfully)
        {
            return (!conditionTask.Result) ? action(obj, cancellation) : ValueTask.CompletedTask;
        }

        return SlowPath(obj, conditionTask, action, cancellation);

        static async ValueTask SlowPath(T obj, ValueTask<bool> conditionTask, Func<T, CancellationToken, ValueTask> action, CancellationToken cancellation)
        {
            if(!(await conditionTask.ConfigureAwait(false)))
            {
                await action(obj, cancellation).ConfigureAwait(false);
            }
        }
    }

    ///<summary>
    ///Returns the result of <paramref name="action"/> when <paramref name="condition"/> evaluates to <c>true</c>;
    ///otherwise returns the original object.
    ///</summary>
    ///<typeparam name="T">The type of the object the extension is called on and returned from the action.</typeparam>
    ///<param name="obj">The object to test and pass to the action.</param>
    ///<param name="condition">A function that receives the object and returns <c>true</c> to apply the action.</param>
    ///<param name="action">A function that receives the object and returns a replacement object when the condition is <c>true</c>.</param>
    ///<returns>
    ///The result of <paramref name="action"/> when the condition is <c>true</c>; otherwise the original <paramref
    ///name="obj"/>.
    ///</returns>
    public static T ReturnIf<T>(this T obj, Func<T, bool> condition, Func<T, T> action) { return condition(obj) ? action(obj) : obj; }

    ///<summary>
    ///Asynchronously returns the result of <paramref name="action"/> when <paramref name="condition"/> evaluates to
    public static async Task<T> ReturnIfAsync<T>(this T obj, Func<T, CancellationToken, Task<bool>> condition, Func<T, CancellationToken, Task<T>> action, CancellationToken cancellation = default)
    {
        if(condition is null)
        {
            throw new ArgumentNullException(nameof(condition), "Condition function must not be null.");
        }

        if(action is null)
        {
            throw new ArgumentNullException(nameof(action), "Action must not be null.");
        }

        cancellation.ThrowIfCancellationRequested();
        return (await condition(obj, cancellation).ConfigureAwait(false)) ? (await action(obj, cancellation).ConfigureAwait(false)) : obj;
    }

    ///<summary>
    ///Asynchronously returns the result of <paramref name="action"/> when <paramref name="condition"/> evaluates to
    public static ValueTask<T> ReturnIfAsync<T>(this T obj, Func<T, CancellationToken, ValueTask<bool>> condition, Func<T, CancellationToken, ValueTask<T>> action, CancellationToken cancellation = default)
    {
        if(condition is null)
        {
            throw new ArgumentNullException(nameof(condition), "Condition function must not be null.");
        }

        if(action is null)
        {
            throw new ArgumentNullException(nameof(action), "Action must not be null.");
        }

        cancellation.ThrowIfCancellationRequested();
        ValueTask<bool> conditionTask = condition(obj, cancellation);
        if(conditionTask.IsCompletedSuccessfully)
        {
            return conditionTask.Result ? action(obj, cancellation) : ValueTask.FromResult(obj);
        }

        return SlowPath(obj, conditionTask, action, cancellation);

        static async ValueTask<T> SlowPath(T obj, ValueTask<bool> conditionTask, Func<T, CancellationToken, ValueTask<T>> action, CancellationToken cancellation) { return (await conditionTask.ConfigureAwait(false)) ? (await action(obj, cancellation).ConfigureAwait(false)) : obj; }
    }

    ///<summary>
    ///Returns the result of either <paramref name="ifAction"/> or <paramref name="elseAction"/> depending on the result
    ///of <paramref name="condition"/>.
    ///</summary>
    ///<typeparam name="T">The type of the object the extension is called on and returned from the actions.</typeparam>
    ///<param name="obj">The object to test and pass to the selected function.</param>
    ///<param name="condition">A function that receives the object and returns <c>true</c> to run <paramref name="ifAction"/>.</param>
    ///<param name="ifAction">The function to execute when the condition is <c>true</c>.</param>
    ///<param name="elseAction">The function to execute when the condition is <c>false</c>.</param>
    ///<returns>The result of the selected function.</returns>
    public static T ReturnIfElse<T>(this T obj, Func<T, bool> condition, Func<T, T> ifAction, Func<T, T> elseAction) { return condition(obj) ? ifAction(obj) : elseAction(obj); }

    ///<summary>
    ///Asynchronously returns the result of either <paramref name="ifAction"/> or <paramref name="elseAction"/>
    ///depending on the result of <paramref name="condition"/>.
    ///</summary>
    public static async Task<T> ReturnIfElseAsync<T>(this T obj, Func<T, CancellationToken, Task<bool>> condition, Func<T, CancellationToken, Task<T>> ifAction, Func<T, CancellationToken, Task<T>> elseAction, CancellationToken cancellation = default)
    {
        if(condition is null)
        {
            throw new ArgumentNullException(nameof(condition), "Condition function must not be null.");
        }

        if(ifAction is null)
        {
            throw new ArgumentNullException(nameof(ifAction), "If action must not be null.");
        }

        if(elseAction is null)
        {
            throw new ArgumentNullException(nameof(elseAction), "Else action must not be null.");
        }

        cancellation.ThrowIfCancellationRequested();
        return (await condition(obj, cancellation).ConfigureAwait(false)) ? (await ifAction(obj, cancellation).ConfigureAwait(false)) : (await elseAction(obj, cancellation).ConfigureAwait(false));
    }

    ///<summary>
    ///Asynchronously returns the result of either <paramref name="ifAction"/> or <paramref name="elseAction"/>
    ///depending on the result of <paramref name="condition"/> (ValueTask overload).
    ///</summary>
    public static ValueTask<T> ReturnIfElseAsync<T>(this T obj, Func<T, CancellationToken, ValueTask<bool>> condition, Func<T, CancellationToken, ValueTask<T>> ifAction, Func<T, CancellationToken, ValueTask<T>> elseAction, CancellationToken cancellation = default)
    {
        if(condition is null)
        {
            throw new ArgumentNullException(nameof(condition), "Condition function must not be null.");
        }

        if(ifAction is null)
        {
            throw new ArgumentNullException(nameof(ifAction), "If action must not be null.");
        }

        if(elseAction is null)
        {
            throw new ArgumentNullException(nameof(elseAction), "Else action must not be null.");
        }

        cancellation.ThrowIfCancellationRequested();
        ValueTask<bool> conditionTask = condition(obj, cancellation);
        if(conditionTask.IsCompletedSuccessfully)
        {
            return conditionTask.Result ? ifAction(obj, cancellation) : elseAction(obj, cancellation);
        }

        return SlowPath(obj, conditionTask, ifAction, elseAction, cancellation);

        static async ValueTask<T> SlowPath(T obj, ValueTask<bool> conditionTask, Func<T, CancellationToken, ValueTask<T>> ifAction, Func<T, CancellationToken, ValueTask<T>> elseAction, CancellationToken cancellation)
        { return (await conditionTask.ConfigureAwait(false)) ? (await ifAction(obj, cancellation).ConfigureAwait(false)) : (await elseAction(obj, cancellation).ConfigureAwait(false)); }
    }

    ///<summary>
    ///Returns the result of <paramref name="action"/> when <paramref name="condition"/> evaluates to <c>false</c>;
    ///otherwise returns the original object.
    ///</summary>
    ///<typeparam name="T">The type of the object the extension is called on and returned from the action.</typeparam>
    ///<param name="obj">The object to test and pass to the action.</param>
    ///<param name="condition">A function that receives the object and returns <c>true</c> to skip the action.</param>
    ///<param name="action">A function that receives the object and returns a replacement object when the condition is <c>false</c>.</param>
    ///<returns>
    ///The result of <paramref name="action"/> when the condition is <c>false</c>; otherwise the original <paramref
    ///name="obj"/>.
    ///</returns>
    public static T ReturnIfNot<T>(this T obj, Func<T, bool> condition, Func<T, T> action) { return (!condition(obj)) ? action(obj) : obj; }

    ///<summary>
    ///Asynchronously returns the result of <paramref name="action"/> when <paramref name="condition"/> evaluates to
    public static async Task<T> ReturnIfNotAsync<T>(this T obj, Func<T, CancellationToken, Task<bool>> condition, Func<T, CancellationToken, Task<T>> action, CancellationToken cancellation = default)
    {
        if(condition is null)
        {
            throw new ArgumentNullException(nameof(condition), "Condition function must not be null.");
        }

        if(action is null)
        {
            throw new ArgumentNullException(nameof(action), "Action must not be null.");
        }

        cancellation.ThrowIfCancellationRequested();
        return (!(await condition(obj, cancellation).ConfigureAwait(false))) ? (await action(obj, cancellation).ConfigureAwait(false)) : obj;
    }

    ///<summary>
    ///Asynchronously returns the result of <paramref name="action"/> when <paramref name="condition"/> evaluates to
    public static ValueTask<T> ReturnIfNotAsync<T>(this T obj, Func<T, CancellationToken, ValueTask<bool>> condition, Func<T, CancellationToken, ValueTask<T>> action, CancellationToken cancellation = default)
    {
        if(condition is null)
        {
            throw new ArgumentNullException(nameof(condition), "Condition function must not be null.");
        }

        if(action is null)
        {
            throw new ArgumentNullException(nameof(action), "Action must not be null.");
        }

        cancellation.ThrowIfCancellationRequested();
        ValueTask<bool> conditionTask = condition(obj, cancellation);
        if(conditionTask.IsCompletedSuccessfully)
        {
            return (!conditionTask.Result) ? action(obj, cancellation) : ValueTask.FromResult(obj);
        }

        return SlowPath(obj, conditionTask, action, cancellation);

        static async ValueTask<T> SlowPath(T obj, ValueTask<bool> conditionTask, Func<T, CancellationToken, ValueTask<T>> action, CancellationToken cancellation) { return (!(await conditionTask.ConfigureAwait(false))) ? (await action(obj, cancellation).ConfigureAwait(false)) : obj; }
    }

    ///<summary>
    ///Returns the original object if it is not <c>null</c>; otherwise returns the result of <paramref name="action"/>.
    ///</summary>
    ///<typeparam name="T">A reference type.</typeparam>
    ///<param name="obj">The object to check for <c>null</c>.</param>
    ///<param name="action">A function that produces a replacement object when <paramref name="obj"/> is <c>null</c>.</param>
    ///<returns>
    ///The original <paramref name="obj"/> when not <c>null</c>; otherwise the object returned by <paramref
    ///name="action"/>.
    ///</returns>
    ///<remarks>
    ///This method is constrained to reference types by <c>where T : class</c>.
    ///</remarks>
    public static T ReturnIfNull<T>(this T obj, Func<T> action) where T : class? { return obj ?? action(); }

    ///<summary>
    ///Asynchronously returns the original object if it is not <c>null</c>; otherwise returns the result of <paramref
    ///name="action"/>.
    ///</summary>
    public static Task<T> ReturnIfNullAsync<T>(this T obj, Func<CancellationToken, Task<T>> action, CancellationToken cancellation = default) where T : class?
    {
        if(action is null)
        {
            throw new ArgumentNullException(nameof(action), "Action must not be null.");
        }

        cancellation.ThrowIfCancellationRequested();
        if(obj is not null)
        {
            return Task.FromResult(obj);
        }

        return action(cancellation);
    }

    ///<summary>
    ///Asynchronously returns the original object if it is not <c>null</c>; otherwise returns the result of <paramref
    ///name="action"/> (ValueTask overload).
    ///</summary>
    public static ValueTask<T> ReturnIfNullAsync<T>(this T obj, Func<CancellationToken, ValueTask<T>> action, CancellationToken cancellation = default) where T : class?
    {
        if(action is null)
        {
            throw new ArgumentNullException(nameof(action), "Action must not be null.");
        }

        cancellation.ThrowIfCancellationRequested();
        return (obj is not null) ? ValueTask.FromResult(obj) : action(cancellation);
    }
    #endregion
}