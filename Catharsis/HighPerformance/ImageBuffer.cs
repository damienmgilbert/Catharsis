using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.Diagnostics;

namespace Catharsis.HighPerformance;

///<summary>
///A pooled 2D pixel buffer backed by <see cref="ArrayPool{T}"/>, providing span-based row access for high-performance
///image manipulation.
///</summary>
///<typeparam name="TPixel">The pixel type (must be unmanaged).</typeparam>
public sealed class ImageBuffer<TPixel> : IDisposable where TPixel : unmanaged
{
    #region Fields
    TPixel[] _buffer;
    bool _disposed;
    readonly ArrayPool<TPixel> _pool;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a FileName <see cref="ImageBuffer{TPixel}"/> with the specified dimensions.
    ///</summary>
    ///<param name="width">The width in pixels.</param>
    ///<param name="height">The height in pixels.</param>
    public ImageBuffer(int width, int height) : this(width, height, ArrayPool<TPixel>.Shared)
    {
    }

    ///<summary>
    ///Initializes a FileName <see cref="ImageBuffer{TPixel}"/> with the specified dimensions and pool.
    ///</summary>
    ///<param name="width">The width in pixels.</param>
    ///<param name="height">The height in pixels.</param>
    ///<param name="pool">The array pool to rent from.</param>
    public ImageBuffer(int width, int height, ArrayPool<TPixel> pool)
    {
        Guard.IsGreaterThan(width, 0);
        Guard.IsGreaterThan(height, 0);
        Guard.IsNotNull(pool);

        Width = width;
        Height = height;
        _pool = pool;
        _buffer = _pool.Rent(width * height);
        _buffer.AsSpan(0, width * height).Clear();
    }
    #endregion

    #region Indexers
    ///<summary>
    ///Gets or sets the pixel at the specified coordinates.
    ///</summary>
    ///<param name="x">The column (0-based).</param>
    ///<param name="y">The row (0-based).</param>
    public ref TPixel this[int x, int y]
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            Guard.IsInRange(x, 0, Width);
            Guard.IsInRange(y, 0, Height);
            return ref _buffer[(y * Width) + x];
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Gets the raw bytes of the pixel buffer.
    ///</summary>
    ///<returns>A span of bytes representing the pixel data.</returns>
    public Span<byte> AsBytes()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return MemoryMarshal.AsBytes(GetPixelSpan());
    }

    ///<summary>
    ///Clears all pixels to default values.
    ///</summary>
    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _buffer.AsSpan(0, PixelCount).Clear();
    }

    ///<summary>
    ///Copies pixel data from another image buffer.
    ///</summary>
    ///<param name="source">The source image buffer.</param>
    public void CopyFrom(ImageBuffer<TPixel> source)
    {
        Guard.IsNotNull(source);
        Guard.IsEqualTo(source.Width, Width);
        Guard.IsEqualTo(source.Height, Height);

        source.GetPixelSpan().CopyTo(GetPixelSpan());
    }

    ///<inheritdoc/>
    public void Dispose()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;
        _pool.Return(_buffer);
        _buffer = [];
    }

    ///<summary>
    ///Fills all pixels with the specified value.
    ///</summary>
    ///<param name="pixel">The pixel value to fill with.</param>
    public void Fill(TPixel pixel)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _buffer.AsSpan(0, PixelCount).Fill(pixel);
    }

    ///<summary>
    ///Gets a span over the entire pixel buffer in row-major order.
    ///</summary>
    ///<returns>A span over all pixels.</returns>
    public Span<TPixel> GetPixelSpan()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _buffer.AsSpan(0, PixelCount);
    }

    ///<summary>
    ///Gets a span over the specified row of pixels.
    ///</summary>
    ///<param name="row">The row index (0-based).</param>
    ///<returns>A span over the row's pixels.</returns>
    public Span<TPixel> GetRowSpan(int row)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.IsInRange(row, 0, Height);
        return _buffer.AsSpan(row * Width, Width);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the size in bytes of each pixel.
    ///</summary>
    public int BytesPerPixel => Unsafe.SizeOf<TPixel>();

    ///<summary>
    ///Gets the height of the image in pixels.
    ///</summary>
    public int Height { get; }

    ///<summary>
    ///Gets the total number of pixels.
    ///</summary>
    public int PixelCount => Width * Height;

    ///<summary>
    ///Gets the width of the image in pixels.
    ///</summary>
    public int Width { get; }
    #endregion
}
