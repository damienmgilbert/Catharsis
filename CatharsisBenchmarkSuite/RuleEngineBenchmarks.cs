using BenchmarkDotNet.Attributes;
using Catharsis.Diagnostics;
using Catharsis.Events;
using Catharsis.Linq;
using Catharsis.RuleEngine;

namespace CatharsisBenchmarkSuite;

///<summary>
///Measures the overhead <see cref="ManagedRuleEngine{T}"/> adds over the raw <see cref="RuleSet{T}"/>/
///<see cref="RuleEvaluator"/> evaluation it wraps — the cost of the registry lookup, metrics recording, and
///per-match event publishing.
///</summary>
[MemoryDiagnoser]
public class ManagedRuleEngineBenchmarks
{
    const int CallCount = 10_000;
    static readonly RuleSet<int> Rules = CreateRuleSet();
    static readonly ManagedRuleEngine<int> Engine = CreateEngine();

    static RuleSet<int> CreateRuleSet()
    {
        RuleSet<int> set = [];
        set.Add("IsPositive", static x => x > 0);
        set.Add("IsEven", static x => x % 2 == 0);
        return set;
    }

    static ManagedRuleEngine<int> CreateEngine()
    {
        ManagedRuleEngine<int> engine = new(new EventBus(), new MetricsRecorder("benchmark.rule-engine"));
        engine.RegisterRuleSet("standard", Rules);
        return engine;
    }

    [Benchmark(Baseline = true)]
    public int DirectRuleSetEvaluation()
    {
        int matches = 0;

        for (int i = 0; i < CallCount; i++)
        {
            if (new[] { i }.Evaluate(Rules).Single().HasMatch)
            {
                matches++;
            }
        }

        return matches;
    }

    [Benchmark]
    public async Task<int> ManagedRuleEngine_EvaluateAsync()
    {
        int matches = 0;

        for (int i = 0; i < CallCount; i++)
        {
            IReadOnlyDictionary<string, RuleContext<int>> results = await Engine.EvaluateAsync(i);

            if (results["standard"].HasMatch)
            {
                matches++;
            }
        }

        return matches;
    }
}
