namespace Catharsis.Units;

///<summary>
///Provides unit conversion equivalents organised by measurement category. Each constant represents the multiplicative
///factor to convert from the named unit to the unit indicated by the suffix (e.g., <see cref="Length.MileToKilometer"/>
///= 1.609 means 1 mile = 1.609 km). Source: Brooks/Cole, Cengage Learning — Unit Conversions (Equivalents).
///</summary>
public static class UnitConversions
{
    ///<summary>
    ///Length conversion factors.
    ///</summary>
    public static class Length
    {
        #region Constants
        ///<summary>
        ///1 angstrom (Å) = 10⁻¹⁰ m = 0.1 nm.
        ///</summary>
        public const double AngstromToMeter = 1e-10;
        ///<summary>
        ///1 cm = 0.3937 in.
        ///</summary>
        public const double CentimeterToInch = 0.3937;
        ///<summary>
        ///1 fermi = 1 femtometer (fm) = 10⁻¹⁵ m.
        ///</summary>
        public const double FemtometerToMeter = 1e-15;
        ///<summary>
        ///1 ft = 30.48 cm.
        ///</summary>
        public const double FootToCentimeter = 30.48;
        ///<summary>
        ///1 in. = 2.54 cm.
        ///</summary>
        public const double InchToCentimeter = 2.54;
        ///<summary>
        ///1 km = 0.6214 mi.
        ///</summary>
        public const double KilometerToMile = 0.6214;
        ///<summary>
        ///1 light-year (ly) = 9.461 × 10¹⁵ m.
        ///</summary>
        public const double LightYearToMeter = 9.461e15;
        ///<summary>
        ///1 m = 3.281 ft.
        ///</summary>
        public const double MeterToFoot = 3.281;
        ///<summary>
        ///1 m = 39.37 in.
        ///</summary>
        public const double MeterToInch = 39.37;
        ///<summary>
        ///1 mi = 5280 ft.
        ///</summary>
        public const double MileToFoot = 5280.0;
        ///<summary>
        ///1 mi = 1.609 km.
        ///</summary>
        public const double MileToKilometer = 1.609;
        ///<summary>
        ///1 nautical mile (U.S.) = 6076 ft.
        ///</summary>
        public const double NauticalMileToFoot = 6076.0;
        ///<summary>
        ///1 nautical mile (U.S.) = 1.852 km.
        ///</summary>
        public const double NauticalMileToKilometer = 1.852;
        ///<summary>
        ///1 nautical mile (U.S.) = 1.151 mi.
        ///</summary>
        public const double NauticalMileToMile = 1.151;
        ///<summary>
        ///1 parsec = 3.26 ly.
        ///</summary>
        public const double ParsecToLightYear = 3.26;
        ///<summary>
        ///1 parsec = 3.09 × 10¹⁶ m.
        ///</summary>
        public const double ParsecToMeter = 3.09e16;
        #endregion
    }

    ///<summary>
    ///Volume conversion factors.
    ///</summary>
    public static class Volume
    {
        #region Constants
        ///<summary>
        ///1 pint (British) = 568 mL.
        ///</summary>
        public const double BritishPintToMilliliter = 568.0;
        ///<summary>
        ///1 pint (British) = 1.20 pints (U.S.).
        ///</summary>
        public const double BritishPintToUSPint = 1.20;
        ///<summary>
        ///1 m³ = 35.31 ft³.
        ///</summary>
        public const double CubicMeterToCubicFoot = 35.31;
        ///<summary>
        ///1 liter (L) = 1000 cm³.
        ///</summary>
        public const double LiterToCubicCentimeter = 1000.0;
        ///<summary>
        ///1 liter (L) = 61.02 in.³.
        ///</summary>
        public const double LiterToCubicInch = 61.02;
        ///<summary>
        ///1 liter (L) = 1.0 × 10⁻³ m³.
        ///</summary>
        public const double LiterToCubicMeter = 1.0e-3;
        ///<summary>
        ///1 liter (L) = 1000 mL.
        ///</summary>
        public const double LiterToMilliliter = 1000.0;
        ///<summary>
        ///1 liter (L) = 1.057 qt (U.S.).
        ///</summary>
        public const double LiterToUSQuart = 1.057;
        ///<summary>
        ///1 gal (U.S.) = 0.8327 gal (British).
        ///</summary>
        public const double USGallonToBritishGallon = 0.8327;
        ///<summary>
        ///1 gal (U.S.) = 231 in.³.
        ///</summary>
        public const double USGallonToCubicInch = 231.0;
        ///<summary>
        ///1 gal (U.S.) = 3.785 L.
        ///</summary>
        public const double USGallonToLiter = 3.785;
        ///<summary>
        ///1 gal (U.S.) = 4 qt (U.S.).
        ///</summary>
        public const double USGallonToUSQuart = 4.0;
        ///<summary>
        ///1 qt (U.S.) = 946 mL.
        ///</summary>
        public const double USQuartToMilliliter = 946.0;
        ///<summary>
        ///1 qt (U.S.) = 2 pints (U.S.).
        ///</summary>
        public const double USQuartToUSPint = 2.0;
        #endregion
    }

    ///<summary>
    ///Speed conversion factors.
    ///</summary>
    public static class Speed
    {
        #region Constants
        ///<summary>
        ///1 ft/s = 0.305 m/s.
        ///</summary>
        public const double FeetPerSecondToMetersPerSecond = 0.305;
        ///<summary>
        ///1 ft/s = 0.682 mi/h.
        ///</summary>
        public const double FeetPerSecondToMilesPerHour = 0.682;
        ///<summary>
        ///1 km/h = 0.278 m/s.
        ///</summary>
        public const double KilometersPerHourToMetersPerSecond = 0.278;
        ///<summary>
        ///1 km/h = 0.621 mi/h.
        ///</summary>
        public const double KilometersPerHourToMilesPerHour = 0.621;
        ///<summary>
        ///1 knot = 0.5144 m/s.
        ///</summary>
        public const double KnotToMetersPerSecond = 0.5144;
        ///<summary>
        ///1 knot = 1.151 mi/h.
        ///</summary>
        public const double KnotToMilesPerHour = 1.151;
        ///<summary>
        ///1 m/s = 3.281 ft/s.
        ///</summary>
        public const double MetersPerSecondToFeetPerSecond = 3.281;
        ///<summary>
        ///1 m/s = 3.600 km/h.
        ///</summary>
        public const double MetersPerSecondToKilometersPerHour = 3.600;
        ///<summary>
        ///1 m/s = 2.237 mi/h.
        ///</summary>
        public const double MetersPerSecondToMilesPerHour = 2.237;
        ///<summary>
        ///1 mi/h = 1.467 ft/s.
        ///</summary>
        public const double MilesPerHourToFeetPerSecond = 1.467;
        ///<summary>
        ///1 mi/h = 1.609 km/h.
        ///</summary>
        public const double MilesPerHourToKilometersPerHour = 1.609;
        ///<summary>
        ///1 mi/h = 0.447 m/s.
        ///</summary>
        public const double MilesPerHourToMetersPerSecond = 0.447;
        #endregion
    }

    ///<summary>
    ///Angle conversion factors.
    ///</summary>
    public static class Angle
    {
        #region Constants
        ///<summary>
        ///1° = 0.01745 rad.
        ///</summary>
        public const double DegreeToRadian = 0.01745;
        ///<summary>
        ///1 radian (rad) = 57.30° = 57°18'.
        ///</summary>
        public const double RadianToDegree = 57.30;
        ///<summary>
        ///1 rev/min (rpm) = 0.1047 rad/s.
        ///</summary>
        public const double RevolutionsPerMinuteToRadiansPerSecond = 0.1047;
        #endregion
    }

    ///<summary>
    ///Time conversion factors.
    ///</summary>
    public static class Time
    {
        #region Constants
        ///<summary>
        ///1 day = 8.64 × 10⁴ s.
        ///</summary>
        public const double DayToSecond = 8.64e4;
        ///<summary>
        ///1 year = 3.156 × 10⁷ s.
        ///</summary>
        public const double YearToSecond = 3.156e7;
        #endregion
    }

    ///<summary>
    ///Mass conversion factors.
    ///</summary>
    public static class Mass
    {
        #region Constants
        ///<summary>
        ///1 atomic mass unit (u) = 1.6605 × 10⁻²⁷ kg.
        ///</summary>
        public const double AtomicMassUnitToKilogram = 1.6605e-27;
        ///<summary>
        ///1 kg = 0.0685 slug. [1 kg has a weight of 2.20 lb where g = 9.80 m/s².]
        ///</summary>
        public const double KilogramToSlug = 0.0685;
        #endregion
    }

    ///<summary>
    ///Force conversion factors.
    ///</summary>
    public static class Force
    {
        #region Constants
        ///<summary>
        ///1 N = 10⁵ dyne.
        ///</summary>
        public const double NewtonToDyne = 1e5;
        ///<summary>
        ///1 N = 0.225 lb.
        ///</summary>
        public const double NewtonToPoundForce = 0.225;
        ///<summary>
        ///1 lb = 4.45 N.
        ///</summary>
        public const double PoundForceToNewton = 4.45;
        #endregion
    }

    ///<summary>
    ///Energy and work conversion factors.
    ///</summary>
    public static class Energy
    {
        #region Constants
        ///<summary>
        ///1 eV = 1.602 × 10⁻¹⁹ J.
        ///</summary>
        public const double ElectronvoltToJoule = 1.602e-19;
        ///<summary>
        ///1 ft·lb = 1.29 × 10⁻³ Btu.
        ///</summary>
        public const double FootPoundToBtu = 1.29e-3;
        ///<summary>
        ///1 ft·lb = 1.36 J.
        ///</summary>
        public const double FootPoundToJoule = 1.36;
        ///<summary>
        ///1 ft·lb = 3.24 × 10⁻⁴ kcal.
        ///</summary>
        public const double FootPoundToKilocalorie = 3.24e-4;
        ///<summary>
        ///1 J = 10⁷ ergs.
        ///</summary>
        public const double JouleToErg = 1e7;
        ///<summary>
        ///1 J = 0.738 ft·lb.
        ///</summary>
        public const double JouleToFootPound = 0.738;
        ///<summary>
        ///1 kcal = 3.97 Btu.
        ///</summary>
        public const double KilocalorieToBtu = 3.97;
        ///<summary>
        ///1 kcal = 4.186 × 10³ J.
        ///</summary>
        public const double KilocalorieToJoule = 4.186e3;
        ///<summary>
        ///1 kWh = 3.60 × 10⁶ J.
        ///</summary>
        public const double KilowattHourToJoule = 3.60e6;
        ///<summary>
        ///1 kWh = 860 kcal.
        ///</summary>
        public const double KilowattHourToKilocalorie = 860.0;
        #endregion
    }

    ///<summary>
    ///Power conversion factors.
    ///</summary>
    public static class Power
    {
        #region Constants
        ///<summary>
        ///1 hp = 550 ft·lb/s.
        ///</summary>
        public const double HorsepowerToFootPoundPerSecond = 550.0;
        ///<summary>
        ///1 hp = 746 W.
        ///</summary>
        public const double HorsepowerToWatt = 746.0;
        ///<summary>
        ///1 W = 3.42 Btu/h.
        ///</summary>
        public const double WattToBtuPerHour = 3.42;
        ///<summary>
        ///1 W = 1 J/s = 0.738 ft·lb/s.
        ///</summary>
        public const double WattToFootPoundPerSecond = 0.738;
        #endregion
    }

    ///<summary>
    ///Pressure conversion factors.
    ///</summary>
    public static class Pressure
    {
        #region Constants
        ///<summary>
        ///1 atm = 1.013 bar.
        ///</summary>
        public const double AtmosphereToBar = 1.013;
        ///<summary>
        ///1 atm = 1.013 × 10⁵ N/m² (Pa).
        ///</summary>
        public const double AtmosphereToPascal = 1.013e5;
        ///<summary>
        ///1 atm = 14.7 lb/in.²
        ///</summary>
        public const double AtmosphereToPsi = 14.7;
        ///<summary>
        ///1 atm = 760 torr.
        ///</summary>
        public const double AtmosphereToTorr = 760.0;
        ///<summary>
        ///1 Pa = 1 N/m² = 1.45 × 10⁻⁴ lb/in.²
        ///</summary>
        public const double PascalToPsi = 1.45e-4;
        ///<summary>
        ///1 lb/in.² = 6.90 × 10³ N/m² (Pa).
        ///</summary>
        public const double PsiToPascal = 6.90e3;
        #endregion
    }
}
