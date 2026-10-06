namespace Catharsis.Physics;

///<summary>
///Provides other useful physical, thermodynamic, and astronomical data. All distances are in SI base units (metres)
///unless stated otherwise.
///</summary>
public static class UsefulData
{
    #region Constants
    ///<summary>
    ///Absolute zero expressed in Celsius: −273.15 °C.
    ///</summary>
    public const double AbsoluteZeroCelsius = -273.15;
    ///<summary>
    ///Density of dry air: 1.29 kg/m³.
    ///</summary>
    public const double DensityOfAir = 1.29;
    ///<summary>
    ///Mean Earth–Moon distance: 384 × 10³ km = 3.84 × 10⁸ m.
    ///</summary>
    public const double EarthMoonDistance = 3.84e8;
    public const double EarthSunDistance = 1.496e11;
        ///<summary>
///Joule equivalent of 1 calorie: 4.186 J.
///</summary>
    public const double JoulePerCalorie = 4.186;
    ///<summary>
    ///Speed of sound in air at 20 °C: 343 m/s.
    ///</summary>
    public const double SpeedOfSoundInAir = 343.0;
    public const double StandardGravity = 9.80;
    #endregion

    ///<summary>
    ///Provides physical data for Earth.
    ///</summary>
    public static class Earth
    {
        #region Constants
        ///<summary>
        ///Mass of Earth: 5.98 × 10²⁴ kg.
        ///</summary>
        public const double Mass = 5.98e24;
        ///<summary>
        ///Mean radius of Earth: 6.38 × 10³ km = 6.38 × 10⁶ m.
        ///</summary>
        public const double MeanRadius = 6.38e6;
        #endregion
    }

        ///<summary>
///Provides physical data for the Moon.
///</summary>
    public static class Moon
    {
        #region Constants
        ///<summary>
        ///Mass of the Moon: 7.35 × 10²² kg.
        ///</summary>
        public const double Mass = 7.35e22;
        ///<summary>
        ///Mean radius of the Moon: 1.74 × 10³ km = 1.74 × 10⁶ m.
        ///</summary>
        public const double MeanRadius = 1.74e6;
        #endregion
    }

        ///<summary>
///Provides physical data for the Sun.
///</summary>
    public static class Sun
    {
        #region Constants
        ///<summary>
        ///Mass of the Sun: 1.99 × 10³⁰ kg.
        ///</summary>
        public const double Mass = 1.99e30;
        ///<summary>
        ///Mean radius of the Sun: 6.96 × 10⁵ km = 6.96 × 10⁸ m.
        ///</summary>
        public const double MeanRadius = 6.96e8;
        #endregion
    }
}
