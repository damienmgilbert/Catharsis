using System.Text;
using BenchmarkDotNet.Attributes;
using Catharsis.Buffers;

namespace CatharsisBenchmarkSuite;

///<summary>
///Compares <see cref="PooledBuffer{T}"/> (an <see cref="System.Buffers.ArrayPool{T}"/>-backed
///<see cref="System.Buffers.IBufferWriter{T}"/>) against a plain <see cref="List{T}"/> for an append-heavy
///workload.
///</summary>
[MemoryDiagnoser]
public class PooledBufferBenchmarks
{
    const int ItemCount = 10_000;

    [Benchmark(Baseline = true)]
    public int List_Add()
    {
        List<int> list = [];

        for(int i = 0; i < ItemCount; i++)
        {
            list.Add(i);
        }

        return list.Count;
    }

    [Benchmark]
    public int PooledBuffer_GetSpanAndAdvance()
    {
        using PooledBuffer<int> buffer = new();

        for(int i = 0; i < ItemCount; i++)
        {
            Span<int> span = buffer.GetSpan(1);
            span[0] = i;
            buffer.Advance(1);
        }

        return buffer.WrittenCount;
    }
}

///<summary>
///Compares <see cref="PooledStringBuilder"/> against <see cref="StringBuilder"/> for a mixed string/value append
///workload.
///</summary>
[MemoryDiagnoser]
public class PooledStringBuilderBenchmarks
{
    const int AppendCount = 1_000;

    [Benchmark(Baseline = true)]
    public string StringBuilder_Append()
    {
        StringBuilder builder = new();

        for(int i = 0; i < AppendCount; i++)
        {
            builder.Append("item-");
            builder.Append(i);
            builder.Append(';');
        }

        return builder.ToString();
    }

    [Benchmark]
    public string PooledStringBuilder_Append()
    {
        using PooledStringBuilder builder = new();

        for(int i = 0; i < AppendCount; i++)
        {
            builder.Append("item-");
            builder.Append(i);
            builder.Append(';');
        }

        return builder.ToString();
    }
}
