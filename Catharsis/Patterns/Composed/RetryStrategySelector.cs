using Catharsis.Generics;
using Catharsis.Scheduling;

namespace Catharsis.Patterns.Composed;

///<summary>
///The strategy-selection half of the strategy pattern for retry back-off: it owns one ///<see
///cref="JitteredDelayCalculator"/> per <see cref="JitterStrategy"/> (kept in an ///<see cref="EnumMap{TEnum,
///TValue}"/>) and chooses among them, either by explicit request or by recommending one from how many clients are
///likely to retry at once.
///</summary>
///<param name="baseDelay">The delay for the first attempt, before jitter.</param>
///<param name="maxDelay">The cap on any delay.</param>
///<param name="multiplier">The exponential back-off multiplier. Defaults to 2.0.</param>
///<param name="random">The random source for jitter, or <c>null</c> for <see cref="Random.Shared"/>.</param>
///<exception cref="ArgumentOutOfRangeException">
public sealed class RetryStrategySelector(TimeSpan baseDelay, TimeSpan maxDelay, double multiplier = 2.0, Random? random = null)
{
    #region Fields
    private readonly EnumMap<JitterStrategy, JitteredDelayCalculator> _calculators = new(strategy => new JitteredDelayCalculator(baseDelay, maxDelay, multiplier, strategy, random));
    #endregion

    #region Public methods
    ///<summary>
    ///Gets the calculator for the strategy <see cref="Recommend"/> chooses for <paramref name="concurrentClients"/>.
    ///</summary>
    ///<param name="concurrentClients">How many clients may retry at the same time. Must be positive.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="concurrentClients"/> is not positive.</exception>
    public JitteredDelayCalculator ForClients(int concurrentClients) => Select(Recommend(concurrentClients));

    ///<summary>
    ///Recommends a strategy from the number of clients that may retry together. A lone client needs no jitter; a few
    ///benefit from <see cref="JitterStrategy.Equal"/> (which keeps a minimum wait); many need ///<see
    ///cref="JitterStrategy.Full"/> to spread out the most.
    ///</summary>
    ///<param name="concurrentClients">How many clients may retry at the same time. Must be positive.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="concurrentClients"/> is not positive.</exception>
    public static JitterStrategy Recommend(int concurrentClients)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(concurrentClients);

        return concurrentClients switch
        {
            1 => JitterStrategy.None,
            <= 10 => JitterStrategy.Equal,
            _ => JitterStrategy.Full
        };
    }

        ///<summary>
///Gets the calculator for an explicitly chosen strategy.
///</summary>
    ///<param name="strategy">The jitter strategy.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="strategy"/> is not a defined value.</exception>
    public JitteredDelayCalculator Select(JitterStrategy strategy) => _calculators[strategy];
    #endregion
}
