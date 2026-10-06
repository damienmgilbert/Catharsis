using System.Linq.Expressions;
using System.Reflection;

namespace Catharsis.Linq.Expressions;

///<summary>
///Provides factory methods for creating <see cref="Expression"/> nodes covering all <see cref="ExpressionType"/>
///values.
///</summary>
public static class ExpressionFactory
{
    #region Public methods

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents an addition operation.
    ///</summary>
    ///<param name="left">The left operand.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left + right</c>.</returns>
    public static BinaryExpression Add(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.Add(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents an addition assignment.
    ///</summary>
    ///<param name="left">The target expression.</param>
    ///<param name="right">The value to add.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left += right</c>.</returns>
    public static BinaryExpression AddAssign(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.AddAssign(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a checked addition assignment.
    ///</summary>
    ///<param name="left">The target expression.</param>
    ///<param name="right">The value to add.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing checked <c>left += right</c>.</returns>
    public static BinaryExpression AddAssignChecked(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.AddAssignChecked(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a checked addition operation.
    ///</summary>
    ///<param name="left">The left operand.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing checked <c>left + right</c>.</returns>
    public static BinaryExpression AddChecked(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.AddChecked(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a bitwise AND.
    ///</summary>
    ///<param name="left">The left operand.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left &amp; right</c>.</returns>
    public static BinaryExpression And(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.And(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a short-circuit logical AND.
    ///</summary>
    ///<param name="left">The left operand.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left &amp;&amp; right</c>.</returns>
    public static BinaryExpression AndAlso(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.AndAlso(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a bitwise AND assignment.
    ///</summary>
    ///<param name="left">The target expression.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left &amp;= right</c>.</returns>
    public static BinaryExpression AndAssign(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.AndAssign(left, right);
    }

    ///<summary>
    ///Creates an <see cref="IndexExpression"/> representing array access with multiple indices.
    ///</summary>
    ///<param name="array">The array expression.</param>
    ///<param name="indexes">The index expressions.</param>
    ///<returns>An <see cref="IndexExpression"/> for multi-dimensional array access.</returns>
    public static IndexExpression ArrayAccess(Expression array, params Expression[] indexes)
    {
        ArgumentNullException.ThrowIfNull(array, nameof(array));
        return Expression.ArrayAccess(array, indexes);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents an array-index operation.
    ///</summary>
    ///<param name="array">The array expression.</param>
    ///<param name="index">The index expression.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>array[index]</c>.</returns>
    public static BinaryExpression ArrayIndex(Expression array, Expression index)
    {
        ArgumentNullException.ThrowIfNull(array, nameof(array));
        ArgumentNullException.ThrowIfNull(index, nameof(index));
        return Expression.ArrayIndex(array, index);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> that represents getting the length of a one-dimensional array.
    ///</summary>
    ///<param name="array">The array expression.</param>
    ///<returns>A <see cref="UnaryExpression"/> representing <c>array.Length</c>.</returns>
    public static UnaryExpression ArrayLength(Expression array)
    {
        ArgumentNullException.ThrowIfNull(array, nameof(array));
        return Expression.ArrayLength(array);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents an assignment.
    ///</summary>
    ///<param name="left">The target of the assignment.</param>
    ///<param name="right">The value to assign.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left = right</c>.</returns>
    public static BinaryExpression Assign(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.Assign(left, right);
    }

    ///<summary>
    ///Creates a <see cref="MemberAssignment"/> that represents initializing a member.
    ///</summary>
    ///<param name="member">The member to bind.</param>
    ///<param name="expression">The value expression.</param>
    ///<returns>A <see cref="MemberAssignment"/>.</returns>
    public static MemberAssignment Bind(MemberInfo member, Expression expression)
    {
        ArgumentNullException.ThrowIfNull(member, nameof(member));
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        return Expression.Bind(member, expression);
    }

    ///<summary>
    ///Creates a <see cref="BlockExpression"/> from the specified expressions.
    ///</summary>
    ///<param name="expressions">The expressions in the block.</param>
    ///<returns>A <see cref="BlockExpression"/> containing the expressions.</returns>
    public static BlockExpression Block(params Expression[] expressions) => Expression.Block(expressions);

    ///<summary>
    ///Creates a <see cref="BlockExpression"/> with local variables.
    ///</summary>
    ///<param name="variables">The local variables for the block.</param>
    ///<param name="expressions">The expressions in the block.</param>
    ///<returns>A <see cref="BlockExpression"/> with declared variables.</returns>
    public static BlockExpression Block(IEnumerable<ParameterExpression> variables, params Expression[] expressions) => Expression.Block(variables, expressions);

    ///<summary>
    ///Creates a <see cref="BlockExpression"/> with an explicit result type.
    ///</summary>
    ///<param name="type">The result type of the block.</param>
    ///<param name="expressions">The expressions in the block.</param>
    ///<returns>A <see cref="BlockExpression"/> with the specified type.</returns>
    public static BlockExpression Block(Type type, params Expression[] expressions)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        return Expression.Block(type, expressions);
    }

    ///<summary>
    ///Creates a <see cref="GotoExpression"/> that represents a <c>break</c>.
    ///</summary>
    ///<param name="target">The label to break to.</param>
    ///<param name="value">An optional value to pass to the label.</param>
    ///<returns>A <see cref="GotoExpression"/> of kind <see cref="GotoExpressionKind.Break"/>.</returns>
    public static GotoExpression Break(LabelTarget target, Expression? value = null)
    {
        ArgumentNullException.ThrowIfNull(target, nameof(target));
        return value is null ? Expression.Break(target) : Expression.Break(target, value);
    }

    ///<summary>
    ///Creates a <see cref="MethodCallExpression"/> representing a static method call.
    ///</summary>
    ///<param name="method">The <see cref="MethodInfo"/> of the static method.</param>
    ///<param name="arguments">The arguments to pass to the method.</param>
    ///<returns>A <see cref="MethodCallExpression"/> representing the static method call.</returns>
    public static MethodCallExpression Call(MethodInfo method, params Expression[] arguments)
    {
        ArgumentNullException.ThrowIfNull(method, nameof(method));
        return Expression.Call(method, arguments);
    }

    ///<summary>
    ///Creates a <see cref="MethodCallExpression"/> representing a method call on an instance.
    ///</summary>
    ///<param name="instance">The object on which the method is called.</param>
    ///<param name="method">The <see cref="MethodInfo"/> of the method to call.</param>
    ///<param name="arguments">The arguments to pass to the method.</param>
    ///<returns>A <see cref="MethodCallExpression"/> representing the method call.</returns>
    public static MethodCallExpression Call(Expression? instance, MethodInfo method, params Expression[] arguments)
    {
        ArgumentNullException.ThrowIfNull(method, nameof(method));
        return Expression.Call(instance, method, arguments);
    }

    ///<summary>
    ///Creates a <see cref="MethodCallExpression"/> representing a method call by name.
    ///</summary>
    ///<param name="instance">The object on which the method is called.</param>
    ///<param name="methodName">The name of the method.</param>
    ///<param name="typeArguments">The generic type arguments, or <c>null</c> for non-generic methods.</param>
    ///<param name="arguments">The arguments to pass to the method.</param>
    ///<returns>A <see cref="MethodCallExpression"/> representing the method call.</returns>
    public static MethodCallExpression Call(Expression instance, string methodName, Type[]? typeArguments, params Expression[] arguments)
    {
        ArgumentNullException.ThrowIfNull(instance, nameof(instance));
        ArgumentNullException.ThrowIfNull(methodName, nameof(methodName));
        return Expression.Call(instance, methodName, typeArguments, arguments);
    }

    ///<summary>
    ///Creates a <see cref="CatchBlock"/> that catches the specified exception type.
    ///</summary>
    ///<param name="type">The exception type to catch.</param>
    ///<param name="body">The body to execute when caught.</param>
    ///<param name="variable">An optional variable bound to the caught exception.</param>
    ///<param name="filter">An optional filter expression.</param>
    ///<returns>A <see cref="CatchBlock"/>.</returns>
    public static CatchBlock Catch(Type type, Expression body, ParameterExpression? variable = null, Expression? filter = null)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        ArgumentNullException.ThrowIfNull(body, nameof(body));
        return Expression.MakeCatchBlock(type, variable, body, filter);
    }

    ///<summary>
    ///Creates a <see cref="DebugInfoExpression"/> that represents a sequence-point clear.
    ///</summary>
    ///<param name="document">The source document.</param>
    ///<returns>A <see cref="DebugInfoExpression"/> representing a clear sequence point.</returns>
    public static DebugInfoExpression ClearDebugInfo(SymbolDocumentInfo document)
    {
        ArgumentNullException.ThrowIfNull(document, nameof(document));
        return Expression.ClearDebugInfo(document);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a null-coalescing operation.
    ///</summary>
    ///<param name="left">The expression to test for null.</param>
    ///<param name="right">The fallback expression if <paramref name="left"/> is null.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left ?? right</c>.</returns>
    public static BinaryExpression Coalesce(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.Coalesce(left, right);
    }

    ///<summary>
    ///Creates a <see cref="ConditionalExpression"/> representing a ternary conditional.
    ///</summary>
    ///<param name="test">The test condition.</param>
    ///<param name="ifTrue">The expression evaluated when <paramref name="test"/> is true.</param>
    ///<param name="ifFalse">The expression evaluated when <paramref name="test"/> is false.</param>
    ///<returns>A <see cref="ConditionalExpression"/> representing <c>test ? ifTrue : ifFalse</c>.</returns>
    public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse)
    {
        ArgumentNullException.ThrowIfNull(test, nameof(test));
        ArgumentNullException.ThrowIfNull(ifTrue, nameof(ifTrue));
        ArgumentNullException.ThrowIfNull(ifFalse, nameof(ifFalse));
        return Expression.Condition(test, ifTrue, ifFalse);
    }

    ///<summary>
    ///Creates a <see cref="ConditionalExpression"/> representing a ternary conditional with an explicit type.
    ///</summary>
    ///<param name="test">The test condition.</param>
    ///<param name="ifTrue">The expression evaluated when <paramref name="test"/> is true.</param>
    ///<param name="ifFalse">The expression evaluated when <paramref name="test"/> is false.</param>
    ///<param name="type">The result type of the expression.</param>
    ///<returns>A <see cref="ConditionalExpression"/> with the specified result type.</returns>
    public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse, Type type)
    {
        ArgumentNullException.ThrowIfNull(test, nameof(test));
        ArgumentNullException.ThrowIfNull(ifTrue, nameof(ifTrue));
        ArgumentNullException.ThrowIfNull(ifFalse, nameof(ifFalse));
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        return Expression.Condition(test, ifTrue, ifFalse, type);
    }

    ///<summary>
    ///Creates a <see cref="ConstantExpression"/> with the specified value.
    ///</summary>
    ///<param name="value">The value of the constant.</param>
    ///<returns>A <see cref="ConstantExpression"/> representing the constant value.</returns>
    public static ConstantExpression Constant(object? value) => Expression.Constant(value);

    ///<summary>
    ///Creates a <see cref="ConstantExpression"/> with the specified value and explicit type.
    ///</summary>
    ///<param name="value">The value of the constant.</param>
    ///<param name="type">The explicit type of the constant.</param>
    ///<returns>A <see cref="ConstantExpression"/> representing the typed constant value.</returns>
    public static ConstantExpression Constant(object? value, Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        return Expression.Constant(value, type);
    }

    ///<summary>
    ///Creates a <see cref="GotoExpression"/> that represents a <c>continue</c>.
    ///</summary>
    ///<param name="target">The label to continue to.</param>
    ///<returns>A <see cref="GotoExpression"/> of kind <see cref="GotoExpressionKind.Continue"/>.</returns>
    public static GotoExpression Continue(LabelTarget target)
    {
        ArgumentNullException.ThrowIfNull(target, nameof(target));
        return Expression.Continue(target);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> that represents a type conversion.
    ///</summary>
    ///<param name="operand">The expression to convert.</param>
    ///<param name="type">The target type.</param>
    ///<returns>A <see cref="UnaryExpression"/> representing <c>(type)operand</c>.</returns>
    public static UnaryExpression Convert(Expression operand, Type type)
    {
        ArgumentNullException.ThrowIfNull(operand, nameof(operand));
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        return Expression.Convert(operand, type);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> that represents a checked type conversion.
    ///</summary>
    ///<param name="operand">The expression to convert.</param>
    ///<param name="type">The target type.</param>
    ///<returns>A <see cref="UnaryExpression"/> representing a checked <c>(type)operand</c>.</returns>
    public static UnaryExpression ConvertChecked(Expression operand, Type type)
    {
        ArgumentNullException.ThrowIfNull(operand, nameof(operand));
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        return Expression.ConvertChecked(operand, type);
    }

    ///<summary>
    ///Creates a <see cref="DebugInfoExpression"/> for associating debug information with an expression.
    ///</summary>
    ///<param name="document">The source document.</param>
    ///<param name="startLine">The start line number.</param>
    ///<param name="startColumn">The start column number.</param>
    ///<param name="endLine">The end line number.</param>
    ///<param name="endColumn">The end column number.</param>
    ///<returns>A <see cref="DebugInfoExpression"/>.</returns>
    public static DebugInfoExpression DebugInfo(SymbolDocumentInfo document, int startLine, int startColumn, int endLine, int endColumn)
    {
        ArgumentNullException.ThrowIfNull(document, nameof(document));
        return Expression.DebugInfo(document, startLine, startColumn, endLine, endColumn);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> that decrements the expression by 1.
    ///</summary>
    ///<param name="operand">The operand to decrement.</param>
    ///<returns>A <see cref="UnaryExpression"/> representing <c>operand - 1</c>.</returns>
    public static UnaryExpression Decrement(Expression operand)
    {
        ArgumentNullException.ThrowIfNull(operand, nameof(operand));
        return Expression.Decrement(operand);
    }

    ///<summary>
    ///Creates a <see cref="DefaultExpression"/> for the specified type.
    ///</summary>
    ///<param name="type">The type whose default value is represented.</param>
    ///<returns>A <see cref="DefaultExpression"/> representing <c>default(T)</c>.</returns>
    public static DefaultExpression Default(Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        return Expression.Default(type);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a division operation.
    ///</summary>
    ///<param name="left">The left operand.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left / right</c>.</returns>
    public static BinaryExpression Divide(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.Divide(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a division assignment.
    ///</summary>
    ///<param name="left">The target expression.</param>
    ///<param name="right">The divisor.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left /= right</c>.</returns>
    public static BinaryExpression DivideAssign(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.DivideAssign(left, right);
    }

    ///<summary>
    ///Creates a <see cref="DynamicExpression"/> using the specified binder.
    ///</summary>
    ///<param name="binder">The <see cref="System.Runtime.CompilerServices.CallSiteBinder"/> to use.</param>
    ///<param name="returnType">The return type of the dynamic operation.</param>
    ///<param name="arguments">The arguments to the dynamic operation.</param>
    ///<returns>A <see cref="DynamicExpression"/>.</returns>
    public static DynamicExpression Dynamic(System.Runtime.CompilerServices.CallSiteBinder binder, Type returnType, params Expression[] arguments)
    {
        ArgumentNullException.ThrowIfNull(binder, nameof(binder));
        ArgumentNullException.ThrowIfNull(returnType, nameof(returnType));
        return Expression.Dynamic(binder, returnType, arguments);
    }

    ///<summary>
    ///Creates an <see cref="ElementInit"/> for use in collection initializers.
    ///</summary>
    ///<param name="addMethod">The <c>Add</c> method used for initialization.</param>
    ///<param name="arguments">The arguments to the add method.</param>
    ///<returns>An <see cref="ElementInit"/>.</returns>
    public static ElementInit ElementInit(MethodInfo addMethod, params Expression[] arguments)
    {
        ArgumentNullException.ThrowIfNull(addMethod, nameof(addMethod));
        return Expression.ElementInit(addMethod, arguments);
    }

    ///<summary>
    ///Creates an empty expression that returns <see cref="void"/>.
    ///</summary>
    ///<returns>A <see cref="DefaultExpression"/> of type <see cref="void"/>.</returns>
    public static DefaultExpression Empty() => Expression.Empty();

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents an equality comparison.
    ///</summary>
    ///<param name="left">The left operand.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left == right</c>.</returns>
    public static BinaryExpression Equal(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.Equal(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a bitwise XOR.
    ///</summary>
    ///<param name="left">The left operand.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left ^ right</c>.</returns>
    public static BinaryExpression ExclusiveOr(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.ExclusiveOr(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a bitwise XOR assignment.
    ///</summary>
    ///<param name="left">The target expression.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left ^= right</c>.</returns>
    public static BinaryExpression ExclusiveOrAssign(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.ExclusiveOrAssign(left, right);
    }

    ///<summary>
    ///Creates a <see cref="MemberExpression"/> representing a field access by <see cref="FieldInfo"/>.
    ///</summary>
    ///<param name="expression">The object expression, or <c>null</c> for static fields.</param>
    ///<param name="field">The <see cref="FieldInfo"/> of the field.</param>
    ///<returns>A <see cref="MemberExpression"/> accessing the field.</returns>
    public static MemberExpression Field(Expression? expression, FieldInfo field)
    {
        ArgumentNullException.ThrowIfNull(field, nameof(field));
        return Expression.Field(expression, field);
    }

    ///<summary>
    ///Creates a <see cref="MemberExpression"/> representing a field access by name.
    ///</summary>
    ///<param name="expression">The object expression.</param>
    ///<param name="fieldName">The name of the field.</param>
    ///<returns>A <see cref="MemberExpression"/> accessing the named field.</returns>
    public static MemberExpression Field(Expression expression, string fieldName)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        ArgumentNullException.ThrowIfNull(fieldName, nameof(fieldName));
        return Expression.Field(expression, fieldName);
    }

    ///<summary>
    ///Creates a <see cref="GotoExpression"/> that represents a <c>goto</c>.
    ///</summary>
    ///<param name="target">The label to jump to.</param>
    ///<param name="value">An optional value to pass to the label.</param>
    ///<returns>A <see cref="GotoExpression"/>.</returns>
    public static GotoExpression Goto(LabelTarget target, Expression? value = null)
    {
        ArgumentNullException.ThrowIfNull(target, nameof(target));
        return value is null ? Expression.Goto(target) : Expression.Goto(target, value);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a greater-than comparison.
    ///</summary>
    ///<param name="left">The left operand.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left &gt; right</c>.</returns>
    public static BinaryExpression GreaterThan(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.GreaterThan(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a greater-than-or-equal comparison.
    ///</summary>
    ///<param name="left">The left operand.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left &gt;= right</c>.</returns>
    public static BinaryExpression GreaterThanOrEqual(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.GreaterThanOrEqual(left, right);
    }

    ///<summary>
    ///Creates a <see cref="ConditionalExpression"/> representing an <c>if-then</c> statement (no else).
    ///</summary>
    ///<param name="test">The test condition.</param>
    ///<param name="ifTrue">The expression executed when <paramref name="test"/> is true.</param>
    ///<returns>A <see cref="ConditionalExpression"/> representing <c>if (test) { ifTrue }</c>.</returns>
    public static ConditionalExpression IfThen(Expression test, Expression ifTrue)
    {
        ArgumentNullException.ThrowIfNull(test, nameof(test));
        ArgumentNullException.ThrowIfNull(ifTrue, nameof(ifTrue));
        return Expression.IfThen(test, ifTrue);
    }

    ///<summary>
    ///Creates a <see cref="ConditionalExpression"/> representing an <c>if-then-else</c> statement.
    ///</summary>
    ///<param name="test">The test condition.</param>
    ///<param name="ifTrue">The expression executed when <paramref name="test"/> is true.</param>
    ///<param name="ifFalse">The expression executed when <paramref name="test"/> is false.</param>
    ///<returns>A <see cref="ConditionalExpression"/> representing <c>if (test) { ifTrue } else { ifFalse }</c>.</returns>
    public static ConditionalExpression IfThenElse(Expression test, Expression ifTrue, Expression ifFalse)
    {
        ArgumentNullException.ThrowIfNull(test, nameof(test));
        ArgumentNullException.ThrowIfNull(ifTrue, nameof(ifTrue));
        ArgumentNullException.ThrowIfNull(ifFalse, nameof(ifFalse));
        return Expression.IfThenElse(test, ifTrue, ifFalse);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> that increments the expression by 1.
    ///</summary>
    ///<param name="operand">The operand to increment.</param>
    ///<returns>A <see cref="UnaryExpression"/> representing <c>operand + 1</c>.</returns>
    public static UnaryExpression Increment(Expression operand)
    {
        ArgumentNullException.ThrowIfNull(operand, nameof(operand));
        return Expression.Increment(operand);
    }

    ///<summary>
    ///Creates an <see cref="InvocationExpression"/> that applies a delegate or lambda to arguments.
    ///</summary>
    ///<param name="expression">The delegate or lambda expression to invoke.</param>
    ///<param name="arguments">The arguments to pass.</param>
    ///<returns>An <see cref="InvocationExpression"/> representing the invocation.</returns>
    public static InvocationExpression Invoke(Expression expression, params Expression[] arguments)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        return Expression.Invoke(expression, arguments);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> that tests whether the runtime value is false.
    ///</summary>
    ///<param name="operand">The operand to test.</param>
    ///<returns>A <see cref="UnaryExpression"/> representing <c>IsFalse(operand)</c>.</returns>
    public static UnaryExpression IsFalse(Expression operand)
    {
        ArgumentNullException.ThrowIfNull(operand, nameof(operand));
        return Expression.IsFalse(operand);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> that tests whether the runtime value is true.
    ///</summary>
    ///<param name="operand">The operand to test.</param>
    ///<returns>A <see cref="UnaryExpression"/> representing <c>IsTrue(operand)</c>.</returns>
    public static UnaryExpression IsTrue(Expression operand)
    {
        ArgumentNullException.ThrowIfNull(operand, nameof(operand));
        return Expression.IsTrue(operand);
    }

    ///<summary>
    ///Creates a <see cref="LabelTarget"/> with an optional name.
    ///</summary>
    ///<param name="name">An optional name for the label.</param>
    ///<returns>A <see cref="LabelTarget"/>.</returns>
    public static LabelTarget Label(string? name = null) => Expression.Label(name);

    ///<summary>
    ///Creates a <see cref="LabelTarget"/> with a result type and optional name.
    ///</summary>
    ///<param name="type">The result type of the label.</param>
    ///<param name="name">An optional name for the label.</param>
    ///<returns>A <see cref="LabelTarget"/> of the specified type.</returns>
    public static LabelTarget Label(Type type, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        return Expression.Label(type, name);
    }

    ///<summary>
    ///Creates a <see cref="LabelExpression"/> that marks a label in the expression tree.
    ///</summary>
    ///<param name="target">The label target.</param>
    ///<param name="defaultValue">An optional default value expression.</param>
    ///<returns>A <see cref="LabelExpression"/>.</returns>
    public static LabelExpression Label(LabelTarget target, Expression? defaultValue = null)
    {
        ArgumentNullException.ThrowIfNull(target, nameof(target));
        return defaultValue is null ? Expression.Label(target) : Expression.Label(target, defaultValue);
    }

    ///<summary>
    ///Creates a <see cref="LambdaExpression"/> from the specified body and parameters.
    ///</summary>
    ///<param name="body">The body of the lambda.</param>
    ///<param name="parameters">The parameters of the lambda.</param>
    ///<returns>A <see cref="LambdaExpression"/> with the given body and parameters.</returns>
    public static LambdaExpression Lambda(Expression body, params ParameterExpression[] parameters)
    {
        ArgumentNullException.ThrowIfNull(body, nameof(body));
        return Expression.Lambda(body, parameters);
    }

    ///<summary>
    ///Creates a strongly-typed <see cref="Expression{TDelegate}"/> from the specified body and parameters.
    ///</summary>
    ///<typeparam name="TDelegate">The delegate type.</typeparam>
    ///<param name="body">The body of the lambda.</param>
    ///<param name="parameters">The parameters of the lambda.</param>
    ///<returns>An <see cref="Expression{TDelegate}"/> with the given body and parameters.</returns>
    public static Expression<TDelegate> Lambda<TDelegate>(Expression body, params ParameterExpression[] parameters)
    {
        ArgumentNullException.ThrowIfNull(body, nameof(body));
        return Expression.Lambda<TDelegate>(body, parameters);
    }

    ///<summary>
    ///Creates a <see cref="LambdaExpression"/> with an explicit delegate type.
    ///</summary>
    ///<param name="delegateType">The delegate type for the lambda.</param>
    ///<param name="body">The body of the lambda.</param>
    ///<param name="parameters">The parameters of the lambda.</param>
    ///<returns>A <see cref="LambdaExpression"/> of the specified delegate type.</returns>
    public static LambdaExpression Lambda(Type delegateType, Expression body, params ParameterExpression[] parameters)
    {
        ArgumentNullException.ThrowIfNull(delegateType, nameof(delegateType));
        ArgumentNullException.ThrowIfNull(body, nameof(body));
        return Expression.Lambda(delegateType, body, parameters);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a left-shift operation.
    ///</summary>
    ///<param name="left">The value to shift.</param>
    ///<param name="right">The shift count.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left &lt;&lt; right</c>.</returns>
    public static BinaryExpression LeftShift(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.LeftShift(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a left-shift assignment.
    ///</summary>
    ///<param name="left">The target expression.</param>
    ///<param name="right">The shift count.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left &lt;&lt;= right</c>.</returns>
    public static BinaryExpression LeftShiftAssign(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.LeftShiftAssign(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a less-than comparison.
    ///</summary>
    ///<param name="left">The left operand.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left &lt; right</c>.</returns>
    public static BinaryExpression LessThan(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.LessThan(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a less-than-or-equal comparison.
    ///</summary>
    ///<param name="left">The left operand.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left &lt;= right</c>.</returns>
    public static BinaryExpression LessThanOrEqual(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.LessThanOrEqual(left, right);
    }

    ///<summary>
    ///Creates a <see cref="MemberListBinding"/> that represents initializing a member of a collection type.
    ///</summary>
    ///<param name="member">The member to bind.</param>
    ///<param name="initializers">The element initializers.</param>
    ///<returns>A <see cref="MemberListBinding"/>.</returns>
    public static MemberListBinding ListBind(MemberInfo member, params ElementInit[] initializers)
    {
        ArgumentNullException.ThrowIfNull(member, nameof(member));
        return Expression.ListBind(member, initializers);
    }

    ///<summary>
    ///Creates a <see cref="ListInitExpression"/> with element initializers.
    ///</summary>
    ///<param name="newExpression">The <see cref="NewExpression"/> that creates the collection.</param>
    ///<param name="initializers">The element initializers.</param>
    ///<returns>A <see cref="ListInitExpression"/> representing collection initialization.</returns>
    public static ListInitExpression ListInit(NewExpression newExpression, params ElementInit[] initializers)
    {
        ArgumentNullException.ThrowIfNull(newExpression, nameof(newExpression));
        return Expression.ListInit(newExpression, initializers);
    }

    ///<summary>
    ///Creates a <see cref="LoopExpression"/> with an optional break label.
    ///</summary>
    ///<param name="body">The body of the loop.</param>
    ///<param name="breakLabel">An optional label that a <c>break</c> targets.</param>
    ///<param name="continueLabel">An optional label that a <c>continue</c> targets.</param>
    ///<returns>A <see cref="LoopExpression"/>.</returns>
    public static LoopExpression Loop(Expression body, LabelTarget? breakLabel = null, LabelTarget? continueLabel = null)
    {
        ArgumentNullException.ThrowIfNull(body, nameof(body));
        return Expression.Loop(body, breakLabel, continueLabel);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> for the specified <see cref="ExpressionType"/>. Supports all binary
    public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.MakeBinary(binaryType, left, right);
    }

    ///<summary>
    ///Creates an <see cref="IndexExpression"/> representing indexed property access.
    ///</summary>
    ///<param name="instance">The object to index.</param>
    ///<param name="indexer">The <see cref="PropertyInfo"/> of the indexer.</param>
    ///<param name="arguments">The index arguments.</param>
    ///<returns>An <see cref="IndexExpression"/>.</returns>
    public static IndexExpression MakeIndex(Expression instance, PropertyInfo? indexer, IEnumerable<Expression>? arguments)
    {
        ArgumentNullException.ThrowIfNull(instance, nameof(instance));
        return Expression.MakeIndex(instance, indexer, arguments);
    }

    ///<summary>
    ///Creates a <see cref="MemberExpression"/> representing access to a field or property.
    ///</summary>
    ///<param name="expression">The object instance, or <c>null</c> for static members.</param>
    ///<param name="member">The <see cref="MemberInfo"/> describing the field or property.</param>
    ///<returns>A <see cref="MemberExpression"/> accessing the member.</returns>
    public static MemberExpression MakeMemberAccess(Expression? expression, MemberInfo member)
    {
        ArgumentNullException.ThrowIfNull(member, nameof(member));
        return Expression.MakeMemberAccess(expression, member);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> for the specified <see cref="ExpressionType"/>. Supports all unary
    public static UnaryExpression MakeUnary(ExpressionType unaryType, Expression operand, Type type)
    {
        ArgumentNullException.ThrowIfNull(operand, nameof(operand));
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        return Expression.MakeUnary(unaryType, operand, type);
    }

    ///<summary>
    ///Creates a <see cref="MemberMemberBinding"/> that represents recursive member initialization.
    ///</summary>
    ///<param name="member">The member to bind.</param>
    ///<param name="bindings">The nested bindings.</param>
    ///<returns>A <see cref="MemberMemberBinding"/>.</returns>
    public static MemberMemberBinding MemberBind(MemberInfo member, params MemberBinding[] bindings)
    {
        ArgumentNullException.ThrowIfNull(member, nameof(member));
        return Expression.MemberBind(member, bindings);
    }

    ///<summary>
    ///Creates a <see cref="MemberInitExpression"/> with member bindings.
    ///</summary>
    ///<param name="newExpression">The <see cref="NewExpression"/> to initialize from.</param>
    ///<param name="bindings">The member bindings to apply.</param>
    ///<returns>A <see cref="MemberInitExpression"/> representing object initialization.</returns>
    public static MemberInitExpression MemberInit(NewExpression newExpression, params MemberBinding[] bindings)
    {
        ArgumentNullException.ThrowIfNull(newExpression, nameof(newExpression));
        return Expression.MemberInit(newExpression, bindings);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a modulo operation.
    ///</summary>
    ///<param name="left">The left operand.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left % right</c>.</returns>
    public static BinaryExpression Modulo(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.Modulo(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a modulo assignment.
    ///</summary>
    ///<param name="left">The target expression.</param>
    ///<param name="right">The divisor.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left %= right</c>.</returns>
    public static BinaryExpression ModuloAssign(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.ModuloAssign(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a multiplication operation.
    ///</summary>
    ///<param name="left">The left operand.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left * right</c>.</returns>
    public static BinaryExpression Multiply(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.Multiply(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a multiplication assignment.
    ///</summary>
    ///<param name="left">The target expression.</param>
    ///<param name="right">The value to multiply by.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left *= right</c>.</returns>
    public static BinaryExpression MultiplyAssign(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.MultiplyAssign(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a checked multiplication assignment.
    ///</summary>
    ///<param name="left">The target expression.</param>
    ///<param name="right">The value to multiply by.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing checked <c>left *= right</c>.</returns>
    public static BinaryExpression MultiplyAssignChecked(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.MultiplyAssignChecked(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a checked multiplication operation.
    ///</summary>
    ///<param name="left">The left operand.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing checked <c>left * right</c>.</returns>
    public static BinaryExpression MultiplyChecked(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.MultiplyChecked(left, right);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> that represents arithmetic negation.
    ///</summary>
    ///<param name="operand">The operand to negate.</param>
    ///<returns>A <see cref="UnaryExpression"/> representing <c>-operand</c>.</returns>
    public static UnaryExpression Negate(Expression operand)
    {
        ArgumentNullException.ThrowIfNull(operand, nameof(operand));
        return Expression.Negate(operand);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> that represents checked arithmetic negation.
    ///</summary>
    ///<param name="operand">The operand to negate.</param>
    ///<returns>A <see cref="UnaryExpression"/> representing checked <c>-operand</c>.</returns>
    public static UnaryExpression NegateChecked(Expression operand)
    {
        ArgumentNullException.ThrowIfNull(operand, nameof(operand));
        return Expression.NegateChecked(operand);
    }

    ///<summary>
    ///Creates a <see cref="NewExpression"/> that represents calling the default constructor.
    ///</summary>
    ///<param name="type">The type to instantiate.</param>
    ///<returns>A <see cref="NewExpression"/> representing <c>new T()</c>.</returns>
    public static NewExpression New(Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        return Expression.New(type);
    }

    ///<summary>
    ///Creates a <see cref="NewExpression"/> that represents calling a constructor.
    ///</summary>
    ///<param name="constructor">The <see cref="ConstructorInfo"/> to call.</param>
    ///<param name="arguments">The arguments to pass to the constructor.</param>
    ///<returns>A <see cref="NewExpression"/> representing <c>new T(args)</c>.</returns>
    public static NewExpression New(ConstructorInfo constructor, params Expression[] arguments)
    {
        ArgumentNullException.ThrowIfNull(constructor, nameof(constructor));
        return Expression.New(constructor, arguments);
    }

    ///<summary>
    ///Creates a <see cref="NewArrayExpression"/> that represents creating a new array with bounds.
    ///</summary>
    ///<param name="type">The element type of the array.</param>
    ///<param name="bounds">Expressions specifying the dimensions.</param>
    ///<returns>A <see cref="NewArrayExpression"/> representing <c>new type[bounds]</c>.</returns>
    public static NewArrayExpression NewArrayBounds(Type type, params Expression[] bounds)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        return Expression.NewArrayBounds(type, bounds);
    }

    ///<summary>
    ///Creates a <see cref="NewArrayExpression"/> that represents creating a one-dimensional array with initializers.
    ///</summary>
    ///<param name="type">The element type of the array.</param>
    ///<param name="initializers">The initializer expressions.</param>
    ///<returns>A <see cref="NewArrayExpression"/> representing <c>new type[] { initializers }</c>.</returns>
    public static NewArrayExpression NewArrayInit(Type type, params Expression[] initializers)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        return Expression.NewArrayInit(type, initializers);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> that represents a logical NOT.
    ///</summary>
    ///<param name="operand">The operand.</param>
    ///<returns>A <see cref="UnaryExpression"/> representing <c>!operand</c>.</returns>
    public static UnaryExpression Not(Expression operand)
    {
        ArgumentNullException.ThrowIfNull(operand, nameof(operand));
        return Expression.Not(operand);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents an inequality comparison.
    ///</summary>
    ///<param name="left">The left operand.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left != right</c>.</returns>
    public static BinaryExpression NotEqual(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.NotEqual(left, right);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> that represents a ones complement (~).
    ///</summary>
    ///<param name="operand">The operand.</param>
    ///<returns>A <see cref="UnaryExpression"/> representing <c>~operand</c>.</returns>
    public static UnaryExpression OnesComplement(Expression operand)
    {
        ArgumentNullException.ThrowIfNull(operand, nameof(operand));
        return Expression.OnesComplement(operand);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a bitwise OR.
    ///</summary>
    ///<param name="left">The left operand.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left | right</c>.</returns>
    public static BinaryExpression Or(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.Or(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a bitwise OR assignment.
    ///</summary>
    ///<param name="left">The target expression.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left |= right</c>.</returns>
    public static BinaryExpression OrAssign(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.OrAssign(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a short-circuit logical OR.
    ///</summary>
    ///<param name="left">The left operand.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left || right</c>.</returns>
    public static BinaryExpression OrElse(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.OrElse(left, right);
    }

    ///<summary>
    ///Creates a <see cref="ParameterExpression"/> with the specified type and optional name.
    ///</summary>
    ///<param name="type">The type of the parameter.</param>
    ///<param name="name">An optional name for the parameter.</param>
    ///<returns>A <see cref="ParameterExpression"/> of the given type.</returns>
    public static ParameterExpression Parameter(Type type, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        return Expression.Parameter(type, name);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> that represents a post-decrement.
    ///</summary>
    ///<param name="operand">The operand to decrement.</param>
    ///<returns>A <see cref="UnaryExpression"/> representing <c>operand--</c>.</returns>
    public static UnaryExpression PostDecrementAssign(Expression operand)
    {
        ArgumentNullException.ThrowIfNull(operand, nameof(operand));
        return Expression.PostDecrementAssign(operand);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> that represents a post-increment.
    ///</summary>
    ///<param name="operand">The operand to increment.</param>
    ///<returns>A <see cref="UnaryExpression"/> representing <c>operand++</c>.</returns>
    public static UnaryExpression PostIncrementAssign(Expression operand)
    {
        ArgumentNullException.ThrowIfNull(operand, nameof(operand));
        return Expression.PostIncrementAssign(operand);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents raising a number to a power.
    ///</summary>
    ///<param name="left">The base expression.</param>
    ///<param name="right">The exponent expression.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left ** right</c>.</returns>
    public static BinaryExpression Power(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.Power(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a power assignment.
    ///</summary>
    ///<param name="left">The target expression.</param>
    ///<param name="right">The exponent.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left **= right</c>.</returns>
    public static BinaryExpression PowerAssign(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.PowerAssign(left, right);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> that represents a pre-decrement.
    ///</summary>
    ///<param name="operand">The operand to decrement.</param>
    ///<returns>A <see cref="UnaryExpression"/> representing <c>--operand</c>.</returns>
    public static UnaryExpression PreDecrementAssign(Expression operand)
    {
        ArgumentNullException.ThrowIfNull(operand, nameof(operand));
        return Expression.PreDecrementAssign(operand);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> that represents a pre-increment.
    ///</summary>
    ///<param name="operand">The operand to increment.</param>
    ///<returns>A <see cref="UnaryExpression"/> representing <c>++operand</c>.</returns>
    public static UnaryExpression PreIncrementAssign(Expression operand)
    {
        ArgumentNullException.ThrowIfNull(operand, nameof(operand));
        return Expression.PreIncrementAssign(operand);
    }

    ///<summary>
    ///Creates a <see cref="MemberExpression"/> representing a property access by name.
    ///</summary>
    ///<param name="expression">The object expression.</param>
    ///<param name="propertyName">The name of the property.</param>
    ///<returns>A <see cref="MemberExpression"/> accessing the named property.</returns>
    public static MemberExpression Property(Expression expression, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        ArgumentNullException.ThrowIfNull(propertyName, nameof(propertyName));
        return Expression.Property(expression, propertyName);
    }

    ///<summary>
    ///Creates a <see cref="MemberExpression"/> representing a property access by <see cref="PropertyInfo"/>.
    ///</summary>
    ///<param name="expression">The object expression, or <c>null</c> for static properties.</param>
    ///<param name="property">The <see cref="PropertyInfo"/> of the property.</param>
    ///<returns>A <see cref="MemberExpression"/> accessing the property.</returns>
    public static MemberExpression Property(Expression? expression, PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property, nameof(property));
        return Expression.Property(expression, property);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> that represents a quoting operation.
    ///</summary>
    ///<param name="expression">The <see cref="LambdaExpression"/> to quote.</param>
    ///<returns>A <see cref="UnaryExpression"/> of type <see cref="ExpressionType.Quote"/>.</returns>
    public static UnaryExpression Quote(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        return Expression.Quote(expression);
    }

    ///<summary>
    ///Creates a <see cref="GotoExpression"/> that represents a <c>return</c>.
    ///</summary>
    ///<param name="target">The label to return to.</param>
    ///<param name="value">An optional return value.</param>
    ///<returns>A <see cref="GotoExpression"/> of kind <see cref="GotoExpressionKind.Return"/>.</returns>
    public static GotoExpression Return(LabelTarget target, Expression? value = null)
    {
        ArgumentNullException.ThrowIfNull(target, nameof(target));
        return value is null ? Expression.Return(target) : Expression.Return(target, value);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a right-shift operation.
    ///</summary>
    ///<param name="left">The value to shift.</param>
    ///<param name="right">The shift count.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left &gt;&gt; right</c>.</returns>
    public static BinaryExpression RightShift(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.RightShift(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a right-shift assignment.
    ///</summary>
    ///<param name="left">The target expression.</param>
    ///<param name="right">The shift count.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left &gt;&gt;= right</c>.</returns>
    public static BinaryExpression RightShiftAssign(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.RightShiftAssign(left, right);
    }

    ///<summary>
    ///Creates a <see cref="RuntimeVariablesExpression"/> that provides runtime access to variables.
    ///</summary>
    ///<param name="variables">The variables to expose at runtime.</param>
    ///<returns>A <see cref="RuntimeVariablesExpression"/>.</returns>
    public static RuntimeVariablesExpression RuntimeVariables(params ParameterExpression[] variables) => Expression.RuntimeVariables(variables);

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a subtraction operation.
    ///</summary>
    ///<param name="left">The left operand.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left - right</c>.</returns>
    public static BinaryExpression Subtract(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.Subtract(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a subtraction assignment.
    ///</summary>
    ///<param name="left">The target expression.</param>
    ///<param name="right">The value to subtract.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing <c>left -= right</c>.</returns>
    public static BinaryExpression SubtractAssign(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.SubtractAssign(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a checked subtraction assignment.
    ///</summary>
    ///<param name="left">The target expression.</param>
    ///<param name="right">The value to subtract.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing checked <c>left -= right</c>.</returns>
    public static BinaryExpression SubtractAssignChecked(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.SubtractAssignChecked(left, right);
    }

    ///<summary>
    ///Creates a <see cref="BinaryExpression"/> that represents a checked subtraction operation.
    ///</summary>
    ///<param name="left">The left operand.</param>
    ///<param name="right">The right operand.</param>
    ///<returns>A <see cref="BinaryExpression"/> representing checked <c>left - right</c>.</returns>
    public static BinaryExpression SubtractChecked(Expression left, Expression right)
    {
        ArgumentNullException.ThrowIfNull(left, nameof(left));
        ArgumentNullException.ThrowIfNull(right, nameof(right));
        return Expression.SubtractChecked(left, right);
    }

    ///<summary>
    ///Creates a <see cref="SwitchExpression"/> with the specified cases.
    ///</summary>
    ///<param name="switchValue">The value to switch on.</param>
    ///<param name="defaultBody">The default case body, or <c>null</c> for no default.</param>
    ///<param name="cases">The switch cases.</param>
    ///<returns>A <see cref="SwitchExpression"/>.</returns>
    public static SwitchExpression Switch(Expression switchValue, Expression? defaultBody, params SwitchCase[] cases)
    {
        ArgumentNullException.ThrowIfNull(switchValue, nameof(switchValue));
        return Expression.Switch(switchValue, defaultBody, cases);
    }

    ///<summary>
    ///Creates a <see cref="SwitchCase"/> for use in a <see cref="SwitchExpression"/>.
    ///</summary>
    ///<param name="body">The body to execute when matched.</param>
    ///<param name="testValues">The test values for this case.</param>
    ///<returns>A <see cref="SwitchCase"/>.</returns>
    public static SwitchCase SwitchCase(Expression body, params Expression[] testValues)
    {
        ArgumentNullException.ThrowIfNull(body, nameof(body));
        return Expression.SwitchCase(body, testValues);
    }

    ///<summary>
    ///Creates a <see cref="SymbolDocumentInfo"/> representing a source file.
    ///</summary>
    ///<param name="fileName">The file name of the source document.</param>
    ///<returns>A <see cref="SymbolDocumentInfo"/>.</returns>
    public static SymbolDocumentInfo SymbolDocument(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName, nameof(fileName));
        return Expression.SymbolDocument(fileName);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> that represents a <c>throw</c> statement.
    ///</summary>
    ///<param name="value">The exception expression to throw.</param>
    ///<param name="type">The type of the expression; defaults to <see cref="void"/>.</param>
    ///<returns>A <see cref="UnaryExpression"/> representing the throw.</returns>
    public static UnaryExpression Throw(Expression? value, Type? type = null) => type is null ? Expression.Throw(value) : Expression.Throw(value, type);

    ///<summary>
    ///Creates a <see cref="TryExpression"/> with catch handlers.
    ///</summary>
    ///<param name="body">The try body.</param>
    ///<param name="handlers">The catch handlers.</param>
    ///<returns>A <see cref="TryExpression"/> with catch blocks.</returns>
    public static TryExpression TryCatch(Expression body, params CatchBlock[] handlers)
    {
        ArgumentNullException.ThrowIfNull(body, nameof(body));
        return Expression.TryCatch(body, handlers);
    }

    ///<summary>
    ///Creates a <see cref="TryExpression"/> with optional catch, finally, and fault handlers.
    ///</summary>
    ///<param name="body">The try body.</param>
    ///<param name="finally">The finally block, or <c>null</c>.</param>
    ///<param name="fault">The fault block, or <c>null</c>.</param>
    ///<param name="handlers">The catch handlers.</param>
    ///<returns>A <see cref="TryExpression"/>.</returns>
    public static TryExpression TryCatchFinally(Expression body, Expression? @finally, Expression? fault, params CatchBlock[] handlers)
    {
        ArgumentNullException.ThrowIfNull(body, nameof(body));
        return Expression.MakeTry(body.Type, body, @finally, fault, handlers);
    }

    ///<summary>
    ///Creates a <see cref="TryExpression"/> with a fault block.
    ///</summary>
    ///<param name="body">The try body.</param>
    ///<param name="fault">The fault block.</param>
    ///<returns>A <see cref="TryExpression"/> with a fault block.</returns>
    public static TryExpression TryFault(Expression body, Expression fault)
    {
        ArgumentNullException.ThrowIfNull(body, nameof(body));
        ArgumentNullException.ThrowIfNull(fault, nameof(fault));
        return Expression.TryFault(body, fault);
    }

    ///<summary>
    ///Creates a <see cref="TryExpression"/> with a finally block.
    ///</summary>
    ///<param name="body">The try body.</param>
    ///<param name="finally">The finally block.</param>
    ///<returns>A <see cref="TryExpression"/> with a finally block.</returns>
    public static TryExpression TryFinally(Expression body, Expression @finally)
    {
        ArgumentNullException.ThrowIfNull(body, nameof(body));
        ArgumentNullException.ThrowIfNull(@finally, nameof(@finally));
        return Expression.TryFinally(body, @finally);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> that represents a <c>as</c> type conversion (returns null on failure).
    ///</summary>
    ///<param name="operand">The expression to convert.</param>
    ///<param name="type">The target type.</param>
    ///<returns>A <see cref="UnaryExpression"/> representing <c>operand as type</c>.</returns>
    public static UnaryExpression TypeAs(Expression operand, Type type)
    {
        ArgumentNullException.ThrowIfNull(operand, nameof(operand));
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        return Expression.TypeAs(operand, type);
    }

    ///<summary>
    ///Creates a <see cref="TypeBinaryExpression"/> that compares the exact runtime type.
    ///</summary>
    ///<param name="expression">The expression to test.</param>
    ///<param name="type">The type to compare against.</param>
    ///<returns>A <see cref="TypeBinaryExpression"/> representing an exact type equality check.</returns>
    public static TypeBinaryExpression TypeEqual(Expression expression, Type type)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        return Expression.TypeEqual(expression, type);
    }

    ///<summary>
    ///Creates a <see cref="TypeBinaryExpression"/> that represents an <c>is</c> type check.
    ///</summary>
    ///<param name="expression">The expression to test.</param>
    ///<param name="type">The type to check against.</param>
    ///<returns>A <see cref="TypeBinaryExpression"/> representing <c>expression is type</c>.</returns>
    public static TypeBinaryExpression TypeIs(Expression expression, Type type)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        return Expression.TypeIs(expression, type);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> that represents a unary plus.
    ///</summary>
    ///<param name="operand">The operand.</param>
    ///<returns>A <see cref="UnaryExpression"/> representing <c>+operand</c>.</returns>
    public static UnaryExpression UnaryPlus(Expression operand)
    {
        ArgumentNullException.ThrowIfNull(operand, nameof(operand));
        return Expression.UnaryPlus(operand);
    }

    ///<summary>
    ///Creates a <see cref="UnaryExpression"/> that represents an explicit unboxing.
    ///</summary>
    ///<param name="operand">The object expression to unbox.</param>
    ///<param name="type">The target value type.</param>
    ///<returns>A <see cref="UnaryExpression"/> representing the unbox operation.</returns>
    public static UnaryExpression Unbox(Expression operand, Type type)
    {
        ArgumentNullException.ThrowIfNull(operand, nameof(operand));
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        return Expression.Unbox(operand, type);
    }

    ///<summary>
    ///Creates a <see cref="ParameterExpression"/> representing a local variable.
    ///</summary>
    ///<param name="type">The type of the variable.</param>
    ///<param name="name">An optional name for the variable.</param>
    ///<returns>A <see cref="ParameterExpression"/> representing a variable.</returns>
    public static ParameterExpression Variable(Type type, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        return Expression.Variable(type, name);
    }
    #endregion
}
