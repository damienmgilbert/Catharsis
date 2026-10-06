using BenchmarkDotNet.Attributes;
using Catharsis.Collections;

namespace CatharsisBenchmarkSuite;

///<summary>
///Compares <see cref="Deque{T}"/> and <see cref="CircularBuffer{T}"/> against the closest BCL equivalents for the
///workloads they target: bounded double-ended churn, and fixed-capacity overwrite-on-full buffering.
///</summary>
[MemoryDiagnoser]
public class CollectionsBenchmarks
{
    const int OperationCount = 10_000;
    const int WindowSize = 100;

    [Benchmark(Baseline = true)]
    public int Queue_EnqueueDequeueWindow()
    {
        Queue<int> queue = new();

        for (int i = 0; i < OperationCount; i++)
        {
            queue.Enqueue(i);

            if (queue.Count > WindowSize)
            {
                queue.Dequeue();
            }
        }

        return queue.Count;
    }

    [Benchmark]
    public int Deque_AddLastRemoveFirstWindow()
    {
        Deque<int> deque = new();

        for (int i = 0; i < OperationCount; i++)
        {
            deque.AddLast(i);

            if (deque.Count > WindowSize)
            {
                deque.RemoveFirst();
            }
        }

        return deque.Count;
    }

    [Benchmark]
    public int CircularBuffer_AddOverwriting()
    {
        CircularBuffer<int> buffer = new(WindowSize);

        for (int i = 0; i < OperationCount; i++)
        {
            buffer.Add(i);
        }

        return buffer.Count;
    }
}
