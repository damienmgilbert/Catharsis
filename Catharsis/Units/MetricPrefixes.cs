namespace Catharsis.Units;

///<summary>
///Provides the complete catalogue of Metric (SI) prefixes ordered from largest to smallest.
///</summary>
public static class MetricPrefixes
{
    #region Public properties
    ///<summary>
    ///Gets all 20 Metric (SI) prefixes ordered from largest (yotta, 10²⁴) to smallest (yocto, 10⁻²⁴).
    ///</summary>
    public static IReadOnlyList<MetricPrefix> All
    {
        get;
    } =[ new("yotta", "Y", 24), new("zeta", "Z", 21), new("exa", "E", 18), new("peta", "P", 15), new("tera", "T", 12), new("giga", "G", 9), new("mega", "M", 6), new("kilo", "k", 3), new("hecto", "h", 2), new("deka", "da", 1), new("deci", "d", -1), new("centi", "c", -2), new("milli", "m", -3), new(
                                                                                                                                                                                                                                                                                                      "micro",
                                                                                                                                                                                                                                                                                                      "μ",
                                                                                                                                                                                                                                                                                                      -6), new(
                                                                                                                                                                                                                                                                                                           "nano",
                                                                                                                                                                                                                                                                                                           "n",
                                                                                                                                                                                                                                                                                                           -9), new(
                                                                                                                                                                                                                                                                                                                "pico",
                                                                                                                                                                                                                                                                                                                "p",
                                                                                                                                                                                                                                                                                                                -12), new(
                                                                                                                                                                                                                                                                                                                      "femto",
                                                                                                                                                                                                                                                                                                                      "f",
                                                                                                                                                                                                                                                                                                                      -15), new(
                                                                                                                                                                                                                                                                                                                            "atto",
                                                                                                                                                                                                                                                                                                                            "a",
                                                                                                                                                                                                                                                                                                                            -18), new(
                                                                                                                                                                                                                                                                                                                                  "zepto",
                                                                                                                                                                                                                                                                                                                                  "z",
                                                                                                                                                                                                                                                                                                                                  -21), new(
                                                                                                                                                                                                                                                                                                                                        "yocto",
                                                                                                                                                                                                                                                                                                                                        "y",
                                                                                                                                                                                                                                                                                                                                        -24), ];
    #endregion
}
