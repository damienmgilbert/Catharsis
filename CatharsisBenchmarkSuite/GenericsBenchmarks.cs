using BenchmarkDotNet.Attributes;
using Catharsis.Generics;
using System.Reflection;

namespace CatharsisBenchmarkSuite;

///<summary>
///Measures <see cref="ExpressionMapper{TSource, TDest}"/>'s compiled mapping against a hand-written mapper (the
///ideal) and a reflection-based one (the naive alternative).
///</summary>
[MemoryDiagnoser]
public class ExpressionMapperBenchmarks
{
    const int CallCount = 10_000;
    static readonly Person Source = new() { Name = "Ada", Age = 36, City = "London" };
    static readonly Func<Person, PersonDto> Compiled = new ExpressionMapper<Person, PersonDto>().Build();
    static readonly PropertyInfo[] SourceProperties = typeof(Person).GetProperties();
    static readonly PropertyInfo[] DestinationProperties = typeof(PersonDto).GetProperties();

    [Benchmark(Baseline = true)]
    public int HandWritten()
    {
        int sum = 0;

        for(int i = 0; i < CallCount; i++)
        {
            PersonDto dto = new() { Name = Source.Name, Age = Source.Age, City = Source.City };
            sum += dto.Age;
        }

        return sum;
    }

    [Benchmark]
    public int ExpressionMapper_Compiled()
    {
        int sum = 0;

        for(int i = 0; i < CallCount; i++)
        {
            sum += Compiled(Source).Age;
        }

        return sum;
    }

    [Benchmark]
    public int Reflection()
    {
        int sum = 0;

        for(int i = 0; i < CallCount; i++)
        {
            PersonDto dto = new();

            foreach(PropertyInfo destination in DestinationProperties)
            {
                PropertyInfo? match = Array.Find(SourceProperties, p => p.Name == destination.Name);
                destination.SetValue(dto, match?.GetValue(Source));
            }

            sum += dto.Age;
        }

        return sum;
    }

    sealed class Person
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string City { get; set; } = "";
    }

    sealed class PersonDto
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string City { get; set; } = "";
    }
}

///<summary>
///Measures <see cref="ExpressionCache"/>'s compiled property getter against reading the same property through
///<see cref="PropertyInfo.GetValue(object?)"/>.
///</summary>
[MemoryDiagnoser]
public class ExpressionCacheBenchmarks
{
    const int CallCount = 10_000;
    static readonly Sample Instance = new() { Number = 5 };
    static readonly PropertyInfo Property = typeof(Sample).GetProperty(nameof(Sample.Number))!;
    readonly ExpressionCache _cache = new();

    [Benchmark(Baseline = true)]
    public int PropertyInfo_GetValue()
    {
        int sum = 0;

        for(int i = 0; i < CallCount; i++)
        {
            sum += (int)Property.GetValue(Instance)!;
        }

        return sum;
    }

    [Benchmark]
    public int ExpressionCache_Getter()
    {
        Func<Sample, int> getter = _cache.GetGetter<Sample, int>(nameof(Sample.Number));
        int sum = 0;

        for(int i = 0; i < CallCount; i++)
        {
            sum += getter(Instance);
        }

        return sum;
    }

    sealed class Sample
    {
        public int Number { get; set; }
    }
}

///<summary>
///Measures <see cref="EnumMap{TEnum, TValue}"/>'s dense-array lookup against a <see cref="Dictionary{TKey, TValue}"/>
///keyed by the same enum.
///</summary>
[MemoryDiagnoser]
public class EnumMapBenchmarks
{
    const int CallCount = 10_000;
    readonly Dictionary<Level, int> _dictionary = new() { [Level.Low] = 1, [Level.Medium] = 2, [Level.High] = 3 };
    readonly EnumMap<Level, int> _map = new(static level => (int)level + 1);

    [Benchmark(Baseline = true)]
    public int Dictionary_Lookup()
    {
        int sum = 0;

        for(int i = 0; i < CallCount; i++)
        {
            sum += _dictionary[(Level)(i % 3)];
        }

        return sum;
    }

    [Benchmark]
    public int EnumMap_Lookup()
    {
        int sum = 0;

        for(int i = 0; i < CallCount; i++)
        {
            sum += _map[(Level)(i % 3)];
        }

        return sum;
    }

    enum Level { Low, Medium, High }
}
