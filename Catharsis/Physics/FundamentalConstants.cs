namespace Catharsis.Physics;

/// <summary>
/// Provides fundamental physical constants with both approximate and current best values.
/// Source: CODATA 12/03 — Peter J. Mohr and Barry N. Taylor, National Institute of Standards and Technology.
/// Numbers in parentheses in the source indicate one-standard-deviation experimental uncertainties in final digits.
/// Values without parentheses are exact (i.e., defined quantities).
/// </summary>
public static class FundamentalConstants
{
    #region Speed of Light

    /// <summary>Speed of light in vacuum (approximate): 3.00 × 10⁸ m/s. Symbol: c.</summary>
    public const double SpeedOfLight = 3.00e8;

    /// <summary>Speed of light in vacuum (current best value): 2.99792458 × 10⁸ m/s. Symbol: c.</summary>
    public const double SpeedOfLightBest = 2.99792458e8;

    #endregion

    #region Gravitational Constant

    /// <summary>Gravitational constant (approximate): 6.67 × 10⁻¹¹ N·m²/kg². Symbol: G.</summary>
    public const double GravitationalConstant = 6.67e-11;

    /// <summary>Gravitational constant (current best value): 6.6742 × 10⁻¹¹ N·m²/kg². Symbol: G.</summary>
    public const double GravitationalConstantBest = 6.6742e-11;

    #endregion

    #region Avogadro's Number

    /// <summary>Avogadro's number (approximate): 6.02 × 10²³ mol⁻¹. Symbol: Nₐ.</summary>
    public const double AvogadrosNumber = 6.02e23;

    /// <summary>Avogadro's number (current best value): 6.0221415 × 10²³ mol⁻¹. Symbol: Nₐ.</summary>
    public const double AvogadrosNumberBest = 6.0221415e23;

    #endregion

    #region Gas Constant

    /// <summary>
    /// Gas constant (approximate): 8.314 J/mol·K = 1.99 cal/mol·K = 0.0821 L·atm/mol·K. Symbol: R.
    /// </summary>
    public const double GasConstant = 8.314;

    /// <summary>Gas constant (current best value): 8.314472 J/mol·K. Symbol: R.</summary>
    public const double GasConstantBest = 8.314472;

    #endregion

    #region Boltzmann Constant

    /// <summary>Boltzmann's constant (approximate): 1.38 × 10⁻²³ J/K. Symbol: k.</summary>
    public const double BoltzmannConstant = 1.38e-23;

    /// <summary>Boltzmann's constant (current best value): 1.3806505 × 10⁻²³ J/K. Symbol: k.</summary>
    public const double BoltzmannConstantBest = 1.3806505e-23;

    #endregion

    #region Charge on Electron

    /// <summary>Elementary charge on electron (approximate): 1.60 × 10⁻¹⁹ C. Symbol: e.</summary>
    public const double ElementaryCharge = 1.60e-19;

    /// <summary>Elementary charge on electron (current best value): 1.60217653 × 10⁻¹⁹ C. Symbol: e.</summary>
    public const double ElementaryChargeBest = 1.60217653e-19;

    #endregion

    #region Stefan–Boltzmann Constant

    /// <summary>Stefan–Boltzmann constant (approximate): 5.67 × 10⁻⁸ W/m²·K⁴. Symbol: σ.</summary>
    public const double StefanBoltzmannConstant = 5.67e-8;

    /// <summary>Stefan–Boltzmann constant (current best value): 5.670400 × 10⁻⁸ W/m²·K⁴. Symbol: σ.</summary>
    public const double StefanBoltzmannConstantBest = 5.6704e-8;

    #endregion

    #region Permittivity of Free Space

    /// <summary>
    /// Permittivity of free space (approximate): 8.85 × 10⁻¹² C²/N·m². Symbol: ε₀ = 1/(c²μ₀).
    /// </summary>
    public const double PermittivityOfFreeSpace = 8.85e-12;

    /// <summary>
    /// Permittivity of free space (current best value): 8.854187817 × 10⁻¹² C²/N·m². Symbol: ε₀.
    /// </summary>
    public const double PermittivityOfFreeSpaceBest = 8.854187817e-12;

    #endregion

    #region Permeability of Free Space

    /// <summary>
    /// Permeability of free space (approximate): 4π × 10⁻⁷ T·m/A ≈ 1.2566 × 10⁻⁶ T·m/A. Symbol: μ₀.
    /// Computed as 4π × 10⁻⁷.
    /// </summary>
    public static readonly double PermeabilityOfFreeSpace = 4.0 * Math.PI * 1e-7;

    /// <summary>
    /// Permeability of free space (current best value): 1.2566370614 × 10⁻⁶ T·m/A. Symbol: μ₀.
    /// </summary>
    public const double PermeabilityOfFreeSpaceBest = 1.2566370614e-6;

    #endregion

    #region Planck's Constant

    /// <summary>Planck's constant (approximate): 6.63 × 10⁻³⁴ J·s. Symbol: h.</summary>
    public const double PlanckConstant = 6.63e-34;

    /// <summary>Planck's constant (current best value): 6.6260693 × 10⁻³⁴ J·s. Symbol: h.</summary>
    public const double PlanckConstantBest = 6.6260693e-34;

    #endregion

    #region Particle Rest Masses

    /// <summary>
    /// Electron rest mass (approximate): 9.11 × 10⁻³¹ kg = 0.000549 u = 0.511 MeV/c². Symbol: mₑ.
    /// </summary>
    public const double ElectronRestMass = 9.11e-31;

    /// <summary>Electron rest mass (current best value): 9.1093826 × 10⁻³¹ kg. Symbol: mₑ.</summary>
    public const double ElectronRestMassBest = 9.1093826e-31;

    /// <summary>Electron rest mass in atomic mass units (best value): 5.4857990945 × 10⁻⁴ u.</summary>
    public const double ElectronRestMassInU = 5.4857990945e-4;

    /// <summary>
    /// Proton rest mass (approximate): 1.6726 × 10⁻²⁷ kg = 1.00728 u = 938.3 MeV/c². Symbol: mₚ.
    /// </summary>
    public const double ProtonRestMass = 1.6726e-27;

    /// <summary>Proton rest mass (current best value): 1.67262171 × 10⁻²⁷ kg. Symbol: mₚ.</summary>
    public const double ProtonRestMassBest = 1.67262171e-27;

    /// <summary>Proton rest mass in atomic mass units (best value): 1.00727646688 u.</summary>
    public const double ProtonRestMassInU = 1.00727646688;

    /// <summary>
    /// Neutron rest mass (approximate): 1.6749 × 10⁻²⁷ kg = 1.008665 u = 939.6 MeV/c². Symbol: mₙ.
    /// </summary>
    public const double NeutronRestMass = 1.6749e-27;

    /// <summary>Neutron rest mass (current best value): 1.67492728 × 10⁻²⁷ kg. Symbol: mₙ.</summary>
    public const double NeutronRestMassBest = 1.67492728e-27;

    /// <summary>Neutron rest mass in atomic mass units (best value): 1.00866491560 u.</summary>
    public const double NeutronRestMassInU = 1.00866491560;

    #endregion

    #region Atomic Mass Unit

    /// <summary>Atomic mass unit (approximate): 1.6605 × 10⁻²⁷ kg = 931.5 MeV/c².</summary>
    public const double AtomicMassUnit = 1.6605e-27;

    /// <summary>Atomic mass unit (current best value): 1.66053886 × 10⁻²⁷ kg.</summary>
    public const double AtomicMassUnitBest = 1.66053886e-27;

    /// <summary>Atomic mass unit energy equivalent (best value): 931.494043 MeV/c².</summary>
    public const double AtomicMassUnitMeV = 931.494043;

    #endregion
}
