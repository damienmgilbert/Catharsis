using Catharsis.Diagnostics;
using Catharsis.Networking;
using System.Diagnostics.Metrics;

namespace Catharsis.UnitTests.Networking;

///<summary>
///Unit tests for the <see cref="HttpClientMetricsListener"/> class.
///</summary>
[TestClass]
public class HttpClientMetricsListenerTests
{
    #region Constructor

    [TestMethod]
    public void Constructor_NullRecorder_Throws() { Assert.ThrowsExactly<ArgumentNullException>(static () => new HttpClientMetricsListener(null!)); }

    #endregion

    #region Measurement forwarding

    [TestMethod]
    public void CounterMeasurement_OnHttpClientMeter_IsForwardedToRecorderAsHistogram()
    {
        using Meter httpMeter = new("System.Net.Http");
        Counter<long> requestCounter = httpMeter.CreateCounter<long>("http.client.request.count");

        using MetricsRecorder recorder = new($"test-recorder-{Guid.NewGuid():N}");
        using HttpClientMetricsListener listener = new(recorder);

        List<double> observed = [];
        using MeterListener verificationListener = new();
        verificationListener.InstrumentPublished = (instrument, meterListener) =>
        {
            if ((instrument.Meter.Name == recorder.Name) && (instrument.Name == "http.client.request.count"))
            {
                meterListener.EnableMeasurementEvents(instrument);
            }
        };
        verificationListener.SetMeasurementEventCallback<double>((_, measurement, _, _) => observed.Add(measurement));
        verificationListener.Start();

        requestCounter.Add(3);

        CollectionAssert.Contains(observed, 3.0);
    }

    [TestMethod]
    public void HistogramMeasurement_OnHttpClientMeter_IsForwardedToRecorder()
    {
        using Meter httpMeter = new("System.Net.Http");
        Histogram<double> durationHistogram = httpMeter.CreateHistogram<double>("http.client.request.duration");

        using MetricsRecorder recorder = new($"test-recorder-{Guid.NewGuid():N}");
        using HttpClientMetricsListener listener = new(recorder);

        List<double> observed = [];
        using MeterListener verificationListener = new();
        verificationListener.InstrumentPublished = (instrument, meterListener) =>
        {
            if ((instrument.Meter.Name == recorder.Name) && (instrument.Name == "http.client.request.duration"))
            {
                meterListener.EnableMeasurementEvents(instrument);
            }
        };
        verificationListener.SetMeasurementEventCallback<double>((_, measurement, _, _) => observed.Add(measurement));
        verificationListener.Start();

        durationHistogram.Record(123.5);

        CollectionAssert.Contains(observed, 123.5);
    }

    [TestMethod]
    public void Measurement_OnUnrelatedMeter_IsNotForwarded()
    {
        using Meter unrelatedMeter = new($"unrelated-{Guid.NewGuid():N}");
        Counter<long> counter = unrelatedMeter.CreateCounter<long>("some.counter");

        using MetricsRecorder recorder = new($"test-recorder-{Guid.NewGuid():N}");
        using HttpClientMetricsListener listener = new(recorder);

        List<double> observed = [];
        using MeterListener verificationListener = new();
        verificationListener.InstrumentPublished = (instrument, meterListener) =>
        {
            if (instrument.Meter.Name == recorder.Name)
            {
                meterListener.EnableMeasurementEvents(instrument);
            }
        };
        verificationListener.SetMeasurementEventCallback<double>((_, measurement, _, _) => observed.Add(measurement));
        verificationListener.Start();

        counter.Add(1);

        Assert.IsEmpty(observed);
    }

    #endregion

    #region Dispose

    [TestMethod]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        using MetricsRecorder recorder = new($"test-recorder-{Guid.NewGuid():N}");
        HttpClientMetricsListener listener = new(recorder);

        listener.Dispose();
        listener.Dispose();
    }

    #endregion
}
