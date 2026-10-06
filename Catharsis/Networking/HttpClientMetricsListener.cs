using Catharsis.Diagnostics;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace Catharsis.Networking;

///<summary>
///Taps the BCL's own built-in <c>"System.Net.Http"</c> <see cref="Meter"/> (populated automatically by every
///<see cref="HttpClient"/> since .NET 8 with instruments such as <c>http.client.request.duration</c>) and
///republishes every observed measurement as a histogram through an injected <see cref="MetricsRecorder"/>. Reads an
///existing, always-on Meter — no exporter or additional instrumentation at the call site is required.
///</summary>
///<param name="recorder">The recorder whose <see cref="Meter"/> republished measurements are written into.</param>
///<exception cref="ArgumentNullException"><paramref name="recorder"/> is <c>null</c>.</exception>
public sealed class HttpClientMetricsListener : IDisposable
{
    #region Fields
    const string HttpClientMeterName = "System.Net.Http";

    readonly MetricsRecorder _recorder;
    readonly ConcurrentDictionary<string, Histogram<double>> _histograms = new();
    readonly MeterListener _listener;
    bool _disposed;
    #endregion

    #region Constructors
    public HttpClientMetricsListener(MetricsRecorder recorder)
    {
        ArgumentNullException.ThrowIfNull(recorder);
        _recorder = recorder;

        _listener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == HttpClientMeterName)
                {
                    listener.EnableMeasurementEvents(instrument, this);
                }
            }
        };

        _listener.SetMeasurementEventCallback<double>(static (instrument, measurement, _, state) => ((HttpClientMetricsListener)state!).Record(instrument.Name, measurement));
        _listener.SetMeasurementEventCallback<long>(static (instrument, measurement, _, state) => ((HttpClientMetricsListener)state!).Record(instrument.Name, measurement));
        _listener.SetMeasurementEventCallback<int>(static (instrument, measurement, _, state) => ((HttpClientMetricsListener)state!).Record(instrument.Name, measurement));
        _listener.Start();
    }
    #endregion

    #region Private methods
    void Record(string instrumentName, double value) => _histograms.GetOrAdd(instrumentName, name => _recorder.CreateHistogram<double>(name)).Record(value);
    #endregion

    #region Public methods
    ///<summary>
    ///Stops listening to the <c>"System.Net.Http"</c> meter. Does not dispose the injected <see cref="MetricsRecorder"/>.
    ///</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _listener.Dispose();
    }
    #endregion
}
