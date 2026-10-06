namespace Catharsis.Geometry;

///<summary>
///Provides static geometric formulas for common 2D and 3D shapes. All angle parameters are expressed in radians unless
///stated otherwise. Source: Brooks/Cole, Cengage Learning — Formulas from Geometry.
///</summary>
public static class GeometryFormulas
{
    ///<summary>
    ///Formulas for a general triangle with sides a, b, c and height h opposite base b.
    ///</summary>
    public static class Triangle
    {
        #region Public methods
        ///<summary>
        ///Computes the area: Area = (1/2)·b·h.
        ///</summary>
        ///<param name="b">The base length.</param>
        ///<param name="h">The perpendicular height.</param>
        public static double Area(double b, double h) => 0.5 * b * h;

        ///<summary>
        ///Computes the height from side a: h = a·sin(θ).
        ///</summary>
        ///<param name="a">The side adjacent to angle θ.</param>
        ///<param name="theta">The angle (in radians) between side a and the base.</param>
        public static double Height(double a, double theta) => a * Math.Sin(theta);

        ///<summary>
        ///Computes side c via the Law of Cosines: c² = a² + b² − 2ab·cos(θ), where θ is the angle opposite side c.
        ///</summary>
        ///<param name="a">Length of side a.</param>
        ///<param name="b">Length of side b.</param>
        ///<param name="theta">Angle (in radians) between sides a and b (opposite side c).</param>
        public static double SideC(double a, double b, double theta) => Math.Sqrt(a * a + b * b - 2.0 * a * b * Math.Cos(theta));
        #endregion
    }

    ///<summary>
    ///Formulas for a right triangle using the Pythagorean Theorem. The right angle is between legs a and b; c is the
    ///hypotenuse.
    ///</summary>
    public static class RightTriangle
    {
        #region Public methods
        ///<summary>
        ///Computes the hypotenuse: c = √(a² + b²).
        ///</summary>
        ///<param name="a">Length of one leg.</param>
        ///<param name="b">Length of the other leg.</param>
        public static double Hypotenuse(double a, double b) => Math.Sqrt(a * a + b * b);
        #endregion
    }

    ///<summary>
    ///Formulas for an equilateral triangle with side length s.
    ///</summary>
    public static class EquilateralTriangle
    {
        #region Public methods
        ///<summary>
        ///Computes the area: Area = (√3·s²) / 4.
        ///</summary>
        ///<param name="s">The side length.</param>
        public static double Area(double s) => Math.Sqrt(3.0) * s * s / 4.0;

        ///<summary>
        ///Computes the height: h = (√3·s) / 2.
        ///</summary>
        ///<param name="s">The side length.</param>
        public static double Height(double s) => Math.Sqrt(3.0) * s / 2.0;
        #endregion
    }

    ///<summary>
    ///Formulas for a parallelogram with base b and perpendicular height h.
    ///</summary>
    public static class Parallelogram
    {
        #region Public methods
        ///<summary>
        ///Computes the area: Area = b·h.
        ///</summary>
        ///<param name="b">The base length.</param>
        ///<param name="h">The perpendicular height.</param>
        public static double Area(double b, double h) => b * h;
        #endregion
    }

    ///<summary>
    ///Formulas for a trapezoid with parallel sides a and b, and height h.
    ///</summary>
    public static class Trapezoid
    {
        #region Public methods
        ///<summary>
        ///Computes the area: Area = (h/2)·(a + b).
        ///</summary>
        ///<param name="h">The perpendicular height between the parallel sides.</param>
        ///<param name="a">The length of one parallel side.</param>
        ///<param name="b">The length of the other parallel side.</param>
        public static double Area(double h, double a, double b) => (h / 2.0) * (a + b);
        #endregion
    }

    ///<summary>
    ///Formulas for a circle with radius r.
    ///</summary>
    public static class Circle
    {
        #region Public methods
        ///<summary>
        ///Computes the area: Area = π·r².
        ///</summary>
        ///<param name="r">The radius.</param>
        public static double Area(double r) => Math.PI * r * r;

        ///<summary>
        ///Computes the circumference: C = 2π·r.
        ///</summary>
        ///<param name="r">The radius.</param>
        public static double Circumference(double r) => 2.0 * Math.PI * r;
        #endregion
    }

    ///<summary>
    ///Formulas for a sector of a circle. θ must be expressed in radians.
    ///</summary>
    public static class SectorOfCircle
    {
        #region Public methods
        ///<summary>
        ///Computes the arc length: s = r·θ.
        ///</summary>
        ///<param name="r">The radius.</param>
        ///<param name="theta">The central angle in radians.</param>
        public static double ArcLength(double r, double theta) => r * theta;

        ///<summary>
        ///Computes the sector area: Area = θ·r² / 2.
        ///</summary>
        ///<param name="theta">The central angle in radians.</param>
        ///<param name="r">The radius.</param>
        public static double Area(double theta, double r) => theta * r * r / 2.0;
        #endregion
    }

    ///<summary>
    ///Formulas for a circular ring (annulus). p = average radius = (R + r) / 2; w = width of ring = R − r.
    ///</summary>
    public static class CircularRing
    {
        #region Public methods
        ///<summary>
        ///Computes the area from outer radius R and inner radius r: Area = π(R² − r²).
        ///</summary>
        ///<param name="outerRadius">Outer radius R.</param>
        ///<param name="innerRadius">Inner radius r.</param>
        public static double Area(double outerRadius, double innerRadius) => Math.PI * (outerRadius * outerRadius - innerRadius * innerRadius);

        ///<summary>
        ///Computes the area from average radius p and width w: Area = 2π·p·w.
        ///</summary>
        ///<param name="p">The average radius (R + r) / 2.</param>
        ///<param name="w">The width of the ring R − r.</param>
        public static double AreaFromAverageRadius(double p, double w) => 2.0 * Math.PI * p * w;
        #endregion
    }

    ///<summary>
    ///Formulas for a sector of a circular ring. p = average radius, w = width of ring; θ must be in radians.
    ///</summary>
    public static class SectorOfCircularRing
    {
        #region Public methods
        ///<summary>
        ///Computes the area: Area = θ·p·w.
        ///</summary>
        ///<param name="theta">The central angle in radians.</param>
        ///<param name="p">The average radius.</param>
        ///<param name="w">The width of the ring.</param>
        public static double Area(double theta, double p, double w) => theta * p * w;
        #endregion
    }

    ///<summary>
    ///Formulas for an ellipse with semi-axes a and b.
    ///</summary>
    public static class Ellipse
    {
        #region Public methods
        ///<summary>
        ///Computes the area: Area = π·a·b.
        ///</summary>
        ///<param name="a">The semi-major axis.</param>
        ///<param name="b">The semi-minor axis.</param>
        public static double Area(double a, double b) => Math.PI * a * b;

        ///<summary>
        ///Approximates the circumference: C ≈ 2π·√((a² + b²) / 2). This is the standard approximation; exact
        ///computation requires an elliptic integral.
        ///</summary>
        ///<param name="a">The semi-major axis.</param>
        ///<param name="b">The semi-minor axis.</param>
        public static double Circumference(double a, double b) => 2.0 * Math.PI * Math.Sqrt((a * a + b * b) / 2.0);
        #endregion
    }

    ///<summary>
    ///Formulas for a cone where A is the area of the base and h is the height.
    ///</summary>
    public static class Cone
    {
        #region Public methods
        ///<summary>
        ///Computes the volume: Volume = A·h / 3.
        ///</summary>
        ///<param name="baseArea">Area A of the base.</param>
        ///<param name="h">The perpendicular height.</param>
        public static double Volume(double baseArea, double h) => baseArea * h / 3.0;
        #endregion
    }

    ///<summary>
    ///Formulas for a right circular cone with base radius r and height h.
    ///</summary>
    public static class RightCircularCone
    {
        #region Public methods
        ///<summary>
        ///Computes the lateral surface area: LSA = π·r·√(r² + h²).
        ///</summary>
        ///<param name="r">The base radius.</param>
        ///<param name="h">The height.</param>
        public static double LateralSurfaceArea(double r, double h) => Math.PI * r * Math.Sqrt(r * r + h * h);

        ///<summary>
        ///Computes the volume: Volume = π·r²·h / 3.
        ///</summary>
        ///<param name="r">The base radius.</param>
        ///<param name="h">The height.</param>
        public static double Volume(double r, double h) => Math.PI * r * r * h / 3.0;
        #endregion
    }

    ///<summary>
    ///Formulas for a frustum (truncated cone) of a right circular cone. r = top radius, R = bottom radius, h = height,
    ///s = slant height.
    ///</summary>
    public static class FrustumOfRightCircularCone
    {
        #region Public methods
        ///<summary>
        ///Computes the lateral surface area: LSA = π·s·(R + r), where s is the slant height.
        ///</summary>
        ///<param name="s">The slant height.</param>
        ///<param name="R">The bottom radius.</param>
        ///<param name="r">The top radius.</param>
        public static double LateralSurfaceArea(double s, double R, double r) => Math.PI * s * (R + r);

        ///<summary>
        ///Computes the volume: Volume = π(r² + rR + R²)·h / 3.
        ///</summary>
        ///<param name="r">The top radius.</param>
        ///<param name="R">The bottom radius.</param>
        ///<param name="h">The height.</param>
        public static double Volume(double r, double R, double h) => Math.PI * (r * r + r * R + R * R) * h / 3.0;
        #endregion
    }

    ///<summary>
    ///Formulas for a right circular cylinder with radius r and height h.
    ///</summary>
    public static class RightCircularCylinder
    {
        #region Public methods
        ///<summary>
        ///Computes the lateral surface area: LSA = 2π·r·h.
        ///</summary>
        ///<param name="r">The radius.</param>
        ///<param name="h">The height.</param>
        public static double LateralSurfaceArea(double r, double h) => 2.0 * Math.PI * r * h;

        ///<summary>
        ///Computes the volume: Volume = π·r²·h.
        ///</summary>
        ///<param name="r">The radius.</param>
        ///<param name="h">The height.</param>
        public static double Volume(double r, double h) => Math.PI * r * r * h;
        #endregion
    }

    ///<summary>
    ///Formulas for a sphere with radius r.
    ///</summary>
    public static class Sphere
    {
        #region Public methods
        ///<summary>
        ///Computes the surface area: SA = 4·π·r².
        ///</summary>
        ///<param name="r">The radius.</param>
        public static double SurfaceArea(double r) => 4.0 * Math.PI * r * r;

        ///<summary>
        ///Computes the volume: Volume = (4/3)·π·r³.
        ///</summary>
        ///<param name="r">The radius.</param>
        public static double Volume(double r) => (4.0 / 3.0) * Math.PI * r * r * r;
        #endregion
    }

    ///<summary>
    ///Formulas for a wedge. A = area of upper face, B = area of base, θ = angle between the upper face and the base.
    ///</summary>
    public static class Wedge
    {
        #region Public methods
        ///<summary>
        ///Computes the upper face area from the base: A = B·sec(θ) = B / cos(θ).
        ///</summary>
        ///<param name="baseArea">Area B of the base.</param>
        ///<param name="theta">The angle in radians between the upper face and the base.</param>
        public static double UpperFaceArea(double baseArea, double theta) => baseArea / Math.Cos(theta);
        #endregion
    }
}
