using System.Diagnostics.Metrics;

namespace Catharsis.Diagnostics;

///<summary>
///A thin factory over a single, disposal-scoped <see cref="Meter"/>, for creating <see cref="Counter{T}"/>, ///<see
///cref="Histogram{T}"/>, and <see cref="UpDownCounter{T}"/> instruments. This is a BCL primitive only: it does not
///configure or require any exporter (Prometheus, OpenTelemetry, etc.) — instruments created here simply do nothing
///unless something else in the process is listening via <see cref="MeterListener"/> or an OpenTelemetry SDK.
///</summary>
///<param name="meterName">The name of the underlying <see cref="Meter"/>, conventionally the owning component's name.</param>
///<param name="version">An optional version string for the meter.</param>
///<exception cref="ArgumentException"><paramref name="meterName"/> is <c>null</c>, empty, or whitespace.</exception>
public sealed class MetricsRecorder(string meterName, string? version = null) : IDisposable
{
    #region Fields
    private bool _disposed;
    private readonly Meter _meter = new(ValidateName(meterName), version);
    #endregion

    #region Private methods
    private static string ValidateName(string meterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(meterName);
        return meterName;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Creates a monotonically increasing counter instrument.
    ///</summary>
    ///<typeparam name="T">The instrument's numeric value type.</typeparam>
    ///<param name="name">The instrument's name.</param>
    ///<param name="unit">An optional unit of measurement.</param>
    ///<param name="description">An optional human-readable description.</param>
    ///<returns>A new <see cref="Counter{T}"/> owned by this recorder's <see cref="Meter"/>.</returns>
    public Counter<T> CreateCounter<T>(string name, string? unit = null, string? description = null) where T : struct
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _meter.CreateCounter<T>(name, unit, description);
    }

    ///<summary>
    ///Creates a histogram instrument for recording a distribution of values.
    ///</summary>
    ///<typeparam name="T">The instrument's numeric value type.</typeparam>
    ///<param name="name">The instrument's name.</param>
    ///<param name="unit">An optional unit of measurement.</param>
    ///<param name="description">An optional human-readable description.</param>
    ///<returns>A new <see cref="Histogram{T}"/> owned by this recorder's <see cref="Meter"/>.</returns>
    public Histogram<T> CreateHistogram<T>(string name, string? unit = null, string? description = null) where T : struct
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _meter.CreateHistogram<T>(name, unit, description);
    }

    ///<summary>
    ///Creates a counter instrument that can both increase and decrease.
    ///</summary>
    ///<typeparam name="T">The instrument's numeric value type.</typeparam>
    ///<param name="name">The instrument's name.</param>
    ///<param name="unit">An optional unit of measurement.</param>
    ///<param name="description">An optional human-readable description.</param>
    ///<returns>A new <see cref="UpDownCounter{T}"/> owned by this recorder's <see cref="Meter"/>.</returns>
    public UpDownCounter<T> CreateUpDownCounter<T>(string name, string? unit = null, string? description = null) where T : struct
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _meter.CreateUpDownCounter<T>(name, unit, description);
    }

    ///<summary>
    ///Disposes the underlying <see cref="Meter"/>, which stops publishing every instrument created from it.
    ///</summary>
    public void Dispose()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;
        _meter.Dispose();
    }
    #endregion

    #region Public properties
    ///<summary>
    ///The name of the underlying <see cref="Meter"/>.
    ///</summary>
    public string Name => _meter.Name;
    #endregion
}
