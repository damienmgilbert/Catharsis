using Catharsis.Operators;

namespace Catharsis.UnitTests.Operators;

///<summary>
///Unit tests for the <see cref="Matrix"/> class.
///</summary>
[TestClass]
public class MatrixTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NonPositiveDimension_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new Matrix(0, 1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new Matrix(1, 0));
    }

    [TestMethod]
    public void Constructor_WrongValueCount_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => new Matrix(2, 2, new double[] { 1, 2, 3 })); }

    [TestMethod]
    public void Constructor_CopiesValues()
    {
        double[] source = [1, 2, 3, 4];
        Matrix matrix = new(2, 2, source);
        source[0] = 99;
        Assert.AreEqual(1d, matrix[0, 0]);
    }

    #endregion

    #region Indexer

    [TestMethod]
    public void Indexer_ReadsRowMajor()
    {
        Matrix matrix = new(2, 3, new double[] { 1, 2, 3, 4, 5, 6 });
        Assert.AreEqual(6d, matrix[1, 2]);
        Assert.AreEqual(4d, matrix[1, 0]);
    }

    [TestMethod]
    public void Indexer_OutOfRange_Throws()
    {
        Matrix matrix = new(2, 2);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => matrix[2, 0]);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => matrix[0, -1]);
    }

    #endregion

    #region Operators

    [TestMethod]
    public void AddSubtract_SameShape_Elementwise()
    {
        Matrix a = new(1, 2, new double[] { 1, 2 });
        Matrix b = new(1, 2, new double[] { 3, 5 });
        Assert.AreEqual(new Matrix(1, 2, new double[] { 4, 7 }), a + b);
        Assert.AreEqual(new Matrix(1, 2, new double[] { 2, 3 }), b - a);
    }

    [TestMethod]
    public void Add_ShapeMismatch_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => new Matrix(1, 2) + new Matrix(2, 1)); }

    [TestMethod]
    public void Multiply_ByScalar_ScalesElements() { Assert.AreEqual(new Matrix(1, 2, new double[] { 2, 4 }), new Matrix(1, 2, new double[] { 1, 2 }) * 2); }

    [TestMethod]
    public void Multiply_Matrices_ComputesProduct()
    {
        Matrix a = new(2, 3, new double[] { 1, 2, 3, 4, 5, 6 });
        Matrix b = new(3, 2, new double[] { 7, 8, 9, 10, 11, 12 });
        Assert.AreEqual(new Matrix(2, 2, new double[] { 58, 64, 139, 154 }), a * b);
    }

    [TestMethod]
    public void Multiply_IncompatibleShapes_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => new Matrix(2, 3) * new Matrix(2, 3)); }

    [TestMethod]
    public void Multiply_ByIdentity_ReturnsEqualMatrix()
    {
        Matrix a = new(2, 2, new double[] { 1, 2, 3, 4 });
        Assert.AreEqual(a, a * Matrix.Identity(2));
    }

    [TestMethod]
    public void Equality_NullHandling()
    {
        Matrix? none = null;
        Assert.IsTrue(none == null);
        Assert.IsTrue(new Matrix(1, 1) != null);
    }

    #endregion

    #region Transpose / Row

    [TestMethod]
    public void Transpose_SwapsRowsAndColumns()
    {
        Matrix transposed = new Matrix(2, 3, new double[] { 1, 2, 3, 4, 5, 6 }).Transpose();
        Assert.AreEqual(new Matrix(3, 2, new double[] { 1, 4, 2, 5, 3, 6 }), transposed);
    }

    [TestMethod]
    public void Row_ReturnsViewOfRow()
    {
        Matrix matrix = new(2, 2, new double[] { 1, 2, 3, 4 });
        Assert.IsTrue(matrix.Row(1).SequenceEqual(new double[] { 3, 4 }));
    }

    [TestMethod]
    public void Row_OutOfRange_Throws()
    {
        Matrix matrix = new(2, 2);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => matrix.Row(2).ToArray());
    }

    #endregion
}
