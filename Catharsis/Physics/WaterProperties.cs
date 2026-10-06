namespace Catharsis.Physics;

///<summary>
///Provides physical properties of water.
///</summary>
public static class WaterProperties
{
    #region Constants
    ///<summary>
    ///Density of water at 4 °C: 1.000 kg/m³ = 1000 kg/m³.
    ///</summary>
    public const double Density = 1000.0;
    ///<summary>
    ///Heat of fusion at 0 °C: 333 kJ/kg = 333 000 J/kg.
    ///</summary>
    public const double HeatOfFusion = 333_000.0;
    ///<summary>
    ///Heat of fusion at 0 °C in kcal/kg: 80 kcal/kg.
    ///</summary>
    public const double HeatOfFusionKcal = 80.0;
    ///<summary>
    ///Heat of vaporization at 100 °C: 2260 kJ/kg = 2 260 000 J/kg.
    ///</summary>
    public const double HeatOfVaporization = 2_260_000.0;
    ///<summary>
    ///Heat of vaporization at 100 °C in kcal/kg: 539 kcal/kg.
    ///</summary>
    public const double HeatOfVaporizationKcal = 539.0;
    ///<summary>
    ///Index of refraction of water: 1.33.
    ///</summary>
    public const double IndexOfRefraction = 1.33;
    ///<summary>
    ///Specific heat at 15 °C: 4186 J/(kg·C°) = 1.00 kcal/(kg·C°).
    ///</summary>
    public const double SpecificHeat = 4186.0;
    #endregion
}
