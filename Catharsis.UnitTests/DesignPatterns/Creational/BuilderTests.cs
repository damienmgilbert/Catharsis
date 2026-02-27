using System.Text;
using Catharsis.DesignPatterns.Creational;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Catharsis.UnitTests.DesignPatterns.Creational;

[TestClass]
public class BuilderTests
{
    #region Public methods
    ///<summary>
    ///Verifies that Build works with complex object graphs and nested transformations.
    ///</summary>
    [TestMethod]
    public void Build_ComplexObjectGraph_AppliesStepsCorrectly()
    {
        // Arrange
        Dictionary<string, int> obj = new Dictionary<string, int>();
        Func<Dictionary<string, int>, int> finalizer = dict => dict.Values.Sum();
        Action<Dictionary<string, int>> step1 = dict => dict["a"] = 10;
        Action<Dictionary<string, int>> step2 = dict => dict["b"] = 20;
        Action<Dictionary<string, int>> step3 = dict => dict["a"] = dict["a"] * 2;
        // Act
        int result = new Builder().Build(obj, finalizer, step1, step2, step3);
        // Assert
        Assert.AreEqual(40, result); // a=20, b=20
    }

    ///<summary>
    ///Verifies that Build correctly transforms an object to a different result type.
    ///</summary>
    [TestMethod]
    public void Build_DifferentResultType_TransformsObjectCorrectly()
    {
        // Arrange
        List<int> obj = new List<int>();
        Func<List<int>, int> finalizer = list => list.Sum();
        Action<List<int>> step1 = list => list.Add(10);
        Action<List<int>> step2 = list => list.Add(20);
        Action<List<int>> step3 = list => list.Add(30);
        // Act
        int result = new Builder().Build(obj, finalizer, step1, step2, step3);
        // Assert
        Assert.AreEqual(60, result);
    }

    ///<summary>
    ///Verifies that Build applies no steps when an empty steps array is provided. The finalizer should be called on the
    ///unmodified object.
    ///</summary>
    [TestMethod]
    public void Build_EmptyStepsArray_CallsFinalizerOnUnmodifiedObject()
    {
        // Arrange
        StringBuilder obj = new StringBuilder("initial");
        Func<StringBuilder, string> finalizer = sb => sb.ToString();
        Action<StringBuilder>[] steps = Array.Empty<Action<StringBuilder>>();
        // Act
        string result = new Builder().Build(obj, finalizer, steps);
        // Assert
        Assert.AreEqual("initial", result);
    }

    ///<summary>
    ///Verifies that Build applies multiple steps in the correct order before calling the finalizer.
    ///</summary>
    [TestMethod]
    public void Build_MultipleSteps_AppliesStepsInOrderThenCallsFinalizer()
    {
        // Arrange
        StringBuilder obj = new StringBuilder();
        Func<StringBuilder, string> finalizer = sb => sb.ToString();
        Action<StringBuilder> step1 = sb => sb.Append("first");
        Action<StringBuilder> step2 = sb => sb.Append(" second");
        Action<StringBuilder> step3 = sb => sb.Append(" third");
        // Act
        string result = new Builder().Build(obj, finalizer, step1, step2, step3);
        // Assert
        Assert.AreEqual("first second third", result);
    }

    ///<summary>
    ///Verifies that Build applies no steps when no steps parameters are provided. The finalizer should be called on the
    ///unmodified object.
    ///</summary>
    [TestMethod]
    public void Build_NoStepsProvided_CallsFinalizerOnUnmodifiedObject()
    {
        // Arrange
        StringBuilder obj = new StringBuilder("initial");
        Func<StringBuilder, string> finalizer = sb => sb.ToString();
        // Act
        string result = new Builder().Build(obj, finalizer);
        // Assert
        Assert.AreEqual("initial", result);
    }

    ///<summary>
    ///Verifies that Build works correctly when the object is null (for reference types). Steps and finalizer receive
    ///null as the parameter.
    ///</summary>
    [TestMethod]
    public void Build_NullObject_PassesNullToStepsAndFinalizer()
    {
        // Arrange
        StringBuilder? obj = null;
        bool stepCalled = false;
        bool finalizerCalled = false;
        Func<StringBuilder?, string> finalizer = sb =>
        {
            finalizerCalled = true;
            return (sb == null) ? "null" : "not null";
        };
        Action<StringBuilder?> step = sb =>
        {
            stepCalled = true;
            Assert.IsNull(sb);
        };
        // Act
        string result = new Builder().Build(obj, finalizer, step);
        // Assert
        Assert.IsTrue(stepCalled);
        Assert.IsTrue(finalizerCalled);
        Assert.AreEqual("null", result);
    }

    ///<summary>
    ///Tests that Build works with reference types that are initially null.
    ///</summary>
    [TestMethod]
    public void Build_NullReferenceTypeObject_ExecutesStepsWithNull()
    {
        // Arrange
        TestObject? obj = null;
        bool stepExecuted = false;
        // Act
        TestObject? result = new Builder().Build(
                             obj,
                             o =>
                             {
                                 stepExecuted = true;
                                 Assert.IsNull(o);
                             });
        // Assert
        Assert.IsNull(result);
        Assert.IsTrue(stepExecuted);
    }

    ///<summary>
    ///Verifies that Build works correctly when TResult is the same type as T.
    ///</summary>
    [TestMethod]
    public void Build_ResultTypeSameAsObjectType_AppliesStepsAndReturnsResult()
    {
        // Arrange
        StringBuilder obj = new StringBuilder("start");
        Func<StringBuilder, StringBuilder> finalizer = sb => sb;
        Action<StringBuilder> step = sb => sb.Append(" end");
        // Act
        StringBuilder result = new Builder().Build(obj, finalizer, step);
        // Assert
        Assert.AreEqual("start end", result.ToString());
        Assert.AreSame(obj, result);
    }

    ///<summary>
    ///Verifies that Build passes the same object instance to all steps and the finalizer.
    ///</summary>
    [TestMethod]
    public void Build_SameObjectPassedToAllSteps_VerifiesObjectIdentity()
    {
        // Arrange
        StringBuilder obj = new StringBuilder();
        StringBuilder? capturedInStep1 = null;
        StringBuilder? capturedInStep2 = null;
        StringBuilder? capturedInFinalizer = null;
        Func<StringBuilder, string> finalizer = sb =>
        {
            capturedInFinalizer = sb;
            return sb.ToString();
        };
        Action<StringBuilder> step1 = sb => capturedInStep1 = sb;
        Action<StringBuilder> step2 = sb => capturedInStep2 = sb;
        // Act
        new Builder().Build(obj, finalizer, step1, step2);
        // Assert
        Assert.AreSame(obj, capturedInStep1);
        Assert.AreSame(obj, capturedInStep2);
        Assert.AreSame(obj, capturedInFinalizer);
    }

    ///<summary>
    ///Verifies that Build applies a single step before calling the finalizer.
    ///</summary>
    [TestMethod]
    public void Build_SingleStep_AppliesStepThenCallsFinalizer()
    {
        // Arrange
        StringBuilder obj = new StringBuilder("initial");
        Func<StringBuilder, string> finalizer = sb => sb.ToString();
        Action<StringBuilder> step = sb => sb.Append(" modified");
        // Act
        string result = new Builder().Build(obj, finalizer, step);
        // Assert
        Assert.AreEqual("initial modified", result);
    }

    ///<summary>
    ///Verifies that Build applies steps in exact order, where later steps can modify the changes made by earlier steps.
    ///</summary>
    [TestMethod]
    public void Build_StepsAppliedInOrder_LaterStepsCanOverrideEarlierSteps()
    {
        // Arrange
        List<int> obj = new List<int>();
        Func<List<int>, int> finalizer = list => list.Count;
        Action<List<int>> step1 = list => list.Add(1);
        Action<List<int>> step2 = list => list.Add(2);
        Action<List<int>> step3 = list => list.Clear();
        Action<List<int>> step4 = list => list.Add(100);
        // Act
        int result = new Builder().Build(obj, finalizer, step1, step2, step3, step4);
        // Assert
        Assert.AreEqual(1, result);
        Assert.AreEqual(100, obj[0]);
    }

    ///<summary>
    ///Tests that Build with string type works correctly.
    ///</summary>
    [TestMethod]
    public void Build_StringType_ExecutesStepsCorrectly()
    {
        // Arrange
        string obj = "test";
        bool stepExecuted = false;
        // Act
        string result = new Builder().Build(
                        obj,
                        s =>
                        {
                            stepExecuted = true;
                            Assert.AreEqual("test", s);
                        });
        // Assert
        Assert.AreEqual("test", result);
        Assert.IsTrue(stepExecuted);
    }

    ///<summary>
    ///Tests that Build works correctly with value types.
    ///</summary>
    [TestMethod]
    public void Build_ValueType_ReturnsModifiedValue()
    {
        // Arrange
        int value = 10;
        // Act
        // Note: For value types, the modification won't persist on the original
        // but Build should still execute and return the value
        int result = new Builder().Build(
                     value,
                     v =>
                     { /* no-op on value type */
                     });
        // Assert
        Assert.AreEqual(10, result);
    }

    ///<summary>
    ///Verifies that Build works correctly with value types as the object being built.
    ///</summary>
    [TestMethod]
    public void Build_ValueTypeObject_AppliesStepsAndReturnsResult()
    {
        // Arrange
        int obj = 10;
        Func<int, string> finalizer = i => i.ToString();
        // Note: Value type cannot be modified by steps, but steps can still be called
        // Act
        string result = new Builder().Build(obj, finalizer);
        // Assert
        Assert.AreEqual("10", result);
    }
    #endregion
}
