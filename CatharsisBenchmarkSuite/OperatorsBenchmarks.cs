using BenchmarkDotNet.Attributes;
using Catharsis.Operators;

namespace CatharsisBenchmarkSuite;

///<summary>
///Measures the cost of <see cref="Money"/>'s currency-checked arithmetic against summing plain
///<see cref="decimal"/> values, to show what the safety check adds.
///</summary>
[MemoryDiagnoser]
public class MoneyBenchmarks
{
    const int CallCount = 10_000;

    [Benchmark(Baseline = true)]
    public decimal Decimal_Sum()
    {
        decimal sum = 0m;

        for(int i = 0; i < CallCount; i++)
        {
            sum += i * 0.01m;
        }

        return sum;
    }

    [Benchmark]
    public Money Money_Sum()
    {
        Money sum = new(0m, "USD");
        Money step = new(0.01m, "USD");

        for(int i = 0; i < CallCount; i++)
        {
            sum += step * i;
        }

        return sum;
    }
}

///<summary>
///Measures <see cref="Matrix"/> multiplication against a straightforward multidimensional-array implementation.
///</summary>
[MemoryDiagnoser]
public class MatrixBenchmarks
{
    const int Size = 64;
    readonly double[,] _array = new double[Size, Size];
    Matrix _matrix = new(Size, Size);

    [GlobalSetup]
    public void Setup()
    {
        double[] values = new double[Size * Size];

        for(int i = 0; i < values.Length; i++)
        {
            values[i] = i % 7;
            _array[i / Size, i % Size] = values[i];
        }

        _matrix = new Matrix(Size, Size, values);
    }

    [Benchmark(Baseline = true)]
    public double[,] MultidimensionalArray_Multiply()
    {
        double[,] result = new double[Size, Size];

        for(int r = 0; r < Size; r++)
        {
            for(int c = 0; c < Size; c++)
            {
                double sum = 0;

                for(int k = 0; k < Size; k++)
                {
                    sum += _array[r, k] * _array[k, c];
                }

                result[r, c] = sum;
            }
        }

        return result;
    }

    [Benchmark]
    public Matrix Matrix_Multiply() => _matrix * _matrix;
}
