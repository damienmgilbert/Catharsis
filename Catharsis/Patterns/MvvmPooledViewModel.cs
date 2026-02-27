using System.Buffers;
using Catharsis.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Catharsis.Patterns;

/// <summary>
/// Demonstrates a memory-efficient MVVM ViewModel that uses pooled buffers
/// for data display, combining <see cref="ObservablePooledBuffer{T}"/> with
/// async command patterns.
/// </summary>
public class MvvmPooledViewModel : BufferViewModelBase
{
    private readonly ObservablePooledBuffer<byte> _dataBuffer = new();
    private string _displayText = string.Empty;

    /// <summary>Gets or sets the display text describing the buffer state.</summary>
    public string DisplayText
    {
        get => _displayText;
        set => SetProperty(ref _displayText, value);
    }

    /// <summary>Gets the data buffer for direct binding.</summary>
    public ObservablePooledBuffer<byte> DataBuffer => _dataBuffer;

    /// <summary>Gets the command to load sample data.</summary>
    public IAsyncRelayCommand LoadSampleDataCommand => _loadSampleDataCommand ??= new AsyncRelayCommand(LoadSampleDataAsync);
    private AsyncRelayCommand? _loadSampleDataCommand;

    /// <summary>Gets the command to clear all data.</summary>
    public IRelayCommand ClearAllDataCommand => _clearAllDataCommand ??= new RelayCommand(ClearAllData);
    private RelayCommand? _clearAllDataCommand;

    private async Task LoadSampleDataAsync()
    {
        await LoadAsync();
    }

    private void ClearAllData()
    {
        _dataBuffer.Clear();
        DisplayText = string.Empty;
        ClearData();
    }

    /// <inheritdoc />
    protected override Task LoadCoreAsync(CancellationToken cancellationToken)
    {
        // Simulate loading data into pooled buffer
        byte[] sampleData = new byte[1024];
        Random.Shared.NextBytes(sampleData);

        _dataBuffer.Write(sampleData);
        DataSize = _dataBuffer.Count;
        DisplayText = $"Loaded {_dataBuffer.Count} bytes into pooled buffer (capacity: {_dataBuffer.Capacity})";

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _dataBuffer.Dispose();

        base.Dispose(disposing);
    }
}
