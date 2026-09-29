using Catharsis.Operators;

namespace Catharsis.UnitTests.Operators;

///<summary>
///Unit tests for <see cref="Vector2D"/> and <see cref="Vector3D"/>.
///</summary>
[TestClass]
public class VectorTests
{
    #region Vector2D

    [TestMethod]
    public void Vector2D_AddSubtractNegate_Componentwise()
    {
        Vector2D a = new(1, 2);
        Vector2D b = new(3, 5);
        Assert.AreEqual(new Vector2D(4, 7), a + b);
        Assert.AreEqual(new Vector2D(2, 3), b - a);
        Assert.AreEqual(new Vector2D(-1, -2), -a);
    }

    [TestMethod]
    public void Vector2D_ScalarAndDotProduct()
    {
        Vector2D a = new(1, 2);
        Assert.AreEqual(new Vector2D(2, 4), a * 2);
        Assert.AreEqual(new Vector2D(2, 4), 2 * a);
        Assert.AreEqual(new Vector2D(0.5, 1), a / 2);
        Assert.AreEqual(11d, a * new Vector2D(3, 4));
    }

    [TestMethod]
    public void Vector2D_LengthNormalizeAndZeroNormalize()
    {
        Assert.AreEqual(5d, new Vector2D(3, 4).Length);
        Assert.AreEqual(1d, new Vector2D(3, 4).Normalize().Length, 1e-12);
        Assert.AreEqual(Vector2D.Zero, Vector2D.Zero.Normalize());
    }

    [TestMethod]
    public void Vector2D_RotateQuarterTurn_SwapsAxes()
    {
        Vector2D rotated = new Vector2D(1, 0).Rotate(Math.PI / 2);
        Assert.AreEqual(0d, rotated.X, 1e-12);
        Assert.AreEqual(1d, rotated.Y, 1e-12);
    }

    [TestMethod]
    public void Vector2D_Cross_ReturnsSignedArea() { Assert.AreEqual(1d, new Vector2D(1, 0).Cross(new Vector2D(0, 1))); }

    [TestMethod]
    public void Vector2D_Indexer_ReadsAxes()
    {
        Vector2D a = new(7, 8);
        Assert.AreEqual(7d, a[0]);
        Assert.AreEqual(8d, a[1]);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => a[2]);
    }

    [TestMethod]
    public void Vector2D_ImplicitFromTuple_Works()
    {
        Vector2D v = (1d, 2d);
        Assert.AreEqual(new Vector2D(1, 2), v);
    }

    #endregion

    #region Vector3D

    [TestMethod]
    public void Vector3D_AddSubtractNegate_Componentwise()
    {
        Vector3D a = new(1, 2, 3);
        Vector3D b = new(4, 5, 6);
        Assert.AreEqual(new Vector3D(5, 7, 9), a + b);
        Assert.AreEqual(new Vector3D(3, 3, 3), b - a);
        Assert.AreEqual(new Vector3D(-1, -2, -3), -a);
    }

    [TestMethod]
    public void Vector3D_DotAndCross()
    {
        Vector3D x = new(1, 0, 0);
        Vector3D y = new(0, 1, 0);
        Assert.AreEqual(0d, x * y);
        Assert.AreEqual(new Vector3D(0, 0, 1), x ^ y);
        Assert.AreEqual(new Vector3D(0, 0, -1), y ^ x);
    }

    [TestMethod]
    public void Vector3D_LengthAndNormalize()
    {
        Assert.AreEqual(3d, new Vector3D(1, 2, 2).Length);
        Assert.AreEqual(Vector3D.Zero, Vector3D.Zero.Normalize());
    }

    [TestMethod]
    public void Vector3D_Indexer_ReadsAxes()
    {
        Vector3D a = new(1, 2, 3);
        Assert.AreEqual(3d, a[2]);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => a[3]);
    }

    [TestMethod]
    public void Vector3D_ImplicitFromVector2D_ZeroZ()
    {
        Vector3D v = new Vector2D(1, 2);
        Assert.AreEqual(new Vector3D(1, 2, 0), v);
    }

    #endregion
}
