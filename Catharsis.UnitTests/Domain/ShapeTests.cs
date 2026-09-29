using Catharsis.Domain;

namespace Catharsis.UnitTests.Domain;

///<summary>
///Unit tests for <see cref="Shape"/> and its subclasses.
///</summary>
[TestClass]
public class ShapeTests
{
    #region Measurements

    [TestMethod]
    public void Circle_MeasuresFromRadius()
    {
        Circle circle = new(2);

        Assert.AreEqual(Math.PI * 4, circle.Area, 1e-12);
        Assert.AreEqual(Math.PI * 4, circle.Perimeter, 1e-12);
    }

    [TestMethod]
    public void Rectangle_MeasuresFromSides()
    {
        Rectangle rectangle = new(3, 4);

        Assert.AreEqual(12d, rectangle.Area);
        Assert.AreEqual(14d, rectangle.Perimeter);
    }

    [TestMethod]
    public void Square_IsARectangleWithEqualSides()
    {
        Rectangle square = new Square(5);

        Assert.AreEqual(25d, square.Area);
        Assert.AreEqual(20d, square.Perimeter);
        Assert.AreEqual(5d, ((Square)square).Side);
        Assert.AreEqual("Square", square.Name);
    }

    [TestMethod]
    public void Triangle_UsesHeronsFormula()
    {
        Triangle triangle = new(3, 4, 5);

        Assert.AreEqual(6d, triangle.Area, 1e-12);
        Assert.AreEqual(12d, triangle.Perimeter);
    }

    #endregion

    #region Validation

    [TestMethod]
    [DataRow(0d)]
    [DataRow(-1d)]
    [DataRow(double.NaN)]
    [DataRow(double.PositiveInfinity)]
    public void Constructors_RejectNonPositiveOrNonFiniteDimensions(double bad)
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Circle(bad));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Rectangle(bad, 1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Rectangle(1, bad));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Square(bad));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Triangle(bad, 1, 1));
    }

    [TestMethod]
    public void Triangle_ImpossibleSides_Throw()
    {
        Assert.ThrowsExactly<ArgumentException>(static () => new Triangle(1, 2, 3));
        Assert.ThrowsExactly<ArgumentException>(static () => new Triangle(1, 1, 10));
    }

    #endregion

    #region Polymorphism

    [TestMethod]
    public void ShapesOrderByArea()
    {
        Shape[] shapes = [new Square(5), new Circle(1), new Triangle(3, 4, 5), new Rectangle(1, 2)];

        CollectionAssert.AreEqual(new[] { "Rectangle", "Circle", "Triangle", "Square" }, shapes.Order().Select(static s => s.Name).ToArray());
    }

    [TestMethod]
    public void Operators_CompareByArea()
    {
        Shape small = new Square(1);
        Shape large = new Square(2);

        Assert.IsTrue(small < large);
        Assert.IsTrue(small <= large);
        Assert.IsTrue(large > small);
        Assert.IsTrue(large >= new Square(2));
        Assert.IsFalse(small > large);
    }

    [TestMethod]
    public void Operators_NullSortsFirst()
    {
        Shape? none = null;
        Shape some = new Square(1);

        Assert.IsTrue(none < some);
        Assert.IsTrue(some > none);
        Assert.IsTrue(none >= null);
        Assert.AreEqual(1, some.CompareTo(null));
    }

    [TestMethod]
    public void ToString_DescribesNameAreaAndPerimeter() { Assert.AreEqual("Rectangle (area 12, perimeter 14)", new Rectangle(3, 4).ToString()); }

    [TestMethod]
    public void TotalArea_SumsAcrossMixedShapes()
    {
        IEnumerable<Shape> shapes = [new Square(2), new Rectangle(1, 3), new Triangle(3, 4, 5)];

        Assert.AreEqual(13d, shapes.Sum(static s => s.Area), 1e-12);
    }

    #endregion
}
