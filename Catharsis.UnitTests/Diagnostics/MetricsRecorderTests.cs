using Catharsis.Diagnostics;
using System.Diagnostics.Metrics;

namespace Catharsis.UnitTests.Diagnostics;

///<summary>
///Unit tests for the <see cref="MetricsRecorder"/> class.
///</summary>
[TestClass]
public class MetricsRecorderTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullMeterName_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new MetricsRecorder(null!)); }

    [TestMethod]
    public void Constructor_WhitespaceMeterName_Throws() { Assert.ThrowsExactly<ArgumentException>(static () => new MetricsRecorder("   ")); }

    #endregion

    #region CreateCounter / CreateHistogram / CreateUpDownCounter

    [TestMethod]
    public void CreateCounter_ReturnsCounterWithExpectedName()
    {
        using MetricsRecorder recorder = new($"test-meter-{Guid.NewGuid():N}");
        Counter<int> counter = recorder.CreateCounter<int>("requests");

        Assert.AreEqual("requests", counter.Name);
    }

    [TestMethod]
    public void CreateHistogram_ReturnsHistogramWithExpectedName()
    {
        using MetricsRecorder recorder = new($"test-meter-{Guid.NewGuid():N}");
        Histogram<double> histogram = recorder.CreateHistogram<double>("latency", "ms");

        Assert.AreEqual("latency", histogram.Name);
        Assert.AreEqual("ms", histogram.Unit);
    }

    [TestMethod]
    public void CreateUpDownCounter_ReturnsUpDownCounterWithExpectedName()
    {
        using MetricsRecorder recorder = new($"test-meter-{Guid.NewGuid():N}");
        UpDownCounter<int> counter = recorder.CreateUpDownCounter<int>("active-connections");

        Assert.AreEqual("active-connections", counter.Name);
    }

    [TestMethod]
    public void CreateCounter_AfterDispose_Throws()
    {
        MetricsRecorder recorder = new($"test-meter-{Guid.NewGuid():N}");
        recorder.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => recorder.CreateCounter<int>("requests"));
    }

    #endregion

    #region Dispose

    [TestMethod]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        MetricsRecorder recorder = new($"test-meter-{Guid.NewGuid():N}");
        recorder.Dispose();
        recorder.Dispose();
    }

    #endregion

    #region Instrument wiring

    [TestMethod]
    public void CreateCounter_IncrementedValue_IsObservedByMeterListener()
    {
        string meterName = $"test-meter-{Guid.NewGuid():N}";
        using MetricsRecorder recorder = new(meterName);
        Counter<int> counter = recorder.CreateCounter<int>("requests");

        List<int> observed = [];
        using MeterListener listener = new();
        listener.InstrumentPublished = (instrument, meterListener) =>
        {
            if((instrument.Meter.Name == meterName) && (instrument.Name == "requests"))
            {
                meterListener.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<int>((_, measurement, _, _) => observed.Add(measurement));
        listener.Start();

        counter.Add(5);

        CollectionAssert.Contains(observed, 5);
    }

    #endregion
}
