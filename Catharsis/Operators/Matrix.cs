namespace Catharsis.Operators;

///<summary>
///An immutable dense matrix of <see cref="double"/> values stored row-major in a single array. Addition, subtraction
///and multiplication are operators, and elements are read through a <c>[row, column]</c> indexer.
///</summary>
public sealed class Matrix : IEquatable<Matrix>
{
    #region Fields
    private readonly double[] _values;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new zero-filled matrix.
    ///</summary>
    ///<param name="rows">The row count. Must be positive.</param>
    ///<param name="columns">The column count. Must be positive.</param>
    ///<exception cref="ArgumentOutOfRangeException">A dimension is not positive.</exception>
    public Matrix(int rows, int columns)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rows);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columns);

        Rows = rows;
        Columns = columns;
        _values = new double[checked(rows * columns)];
    }

    ///<summary>
    ///Initializes a new matrix from row-major values.
    ///</summary>
    ///<param name="rows">The row count. Must be positive.</param>
    ///<param name="columns">The column count. Must be positive.</param>
    ///<param name="values">Exactly <c>rows * columns</c> values in row-major order. They are copied.</param>
    ///<exception cref="ArgumentException"><paramref name="values"/> has the wrong length.</exception>
    public Matrix(int rows, int columns, ReadOnlySpan<double> values) : this(rows, columns)
    {
        if(values.Length != _values.Length)
        {
            throw new ArgumentException($"Expected {_values.Length} values but got {values.Length}.", nameof(values));
        }

        values.CopyTo(_values);
    }
    #endregion

    #region Operators
    ///<summary>
    ///Subtracts one matrix from another of the same shape.
    ///</summary>
    ///<exception cref="ArgumentException">The shapes differ.</exception>
    public static Matrix operator -(Matrix left, Matrix right)
    {
        return Combine(left, right, static(a, b) => a - b);
    }

    ///<summary>
    ///Determines whether two matrices differ.
    ///</summary>
    public static bool operator !=(Matrix? left, Matrix? right)
    {
        return !(left == right);
    }

    ///<summary>
    ///Scales every element.
    ///</summary>
    public static Matrix operator *(Matrix matrix, double scalar)
    {
        ArgumentNullException.ThrowIfNull(matrix);

        Matrix result = new(matrix.Rows, matrix.Columns);

        for(int i = 0; i < matrix._values.Length; i++)
        {
            result._values[i] = matrix._values[i] * scalar;
        }

        return result;
    }
    ///<summary>
    ///Multiplies two matrices.
    ///</summary>
    ///<exception cref="ArgumentException">The left column count differs from the right row count.</exception>
    public static Matrix operator *(Matrix left, Matrix right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        if(left.Columns != right.Rows)
        {
            throw new ArgumentException($"Cannot multiply {left.Rows}x{left.Columns} by {right.Rows}x{right.Columns}.");
        }

        Matrix result = new(left.Rows, right.Columns);

        for(int r = 0; r < left.Rows; r++)
        {
            ReadOnlySpan<double> leftRow = left.Row(r);
            Span<double> resultRow = result._values.AsSpan(r * result.Columns, result.Columns);

            for(int k = 0; k < left.Columns; k++)
            {
                double factor = leftRow[k];
                ReadOnlySpan<double> rightRow = right.Row(k);

                for(int c = 0; c < resultRow.Length; c++)
                {
                    resultRow[c] += factor * rightRow[c];
                }
            }
        }

        return result;
    }
    ///<summary>
    ///Adds two matrices of the same shape.
    ///</summary>
    ///<exception cref="ArgumentException">The shapes differ.</exception>
    public static Matrix operator +(Matrix left, Matrix right)
    {
        return Combine(left, right, static(a, b) => a + b);
    }

    ///<summary>
    ///Determines whether two matrices are equal.
    ///</summary>
    public static bool operator ==(Matrix? left, Matrix? right)
    {
        return left is null ? right is null : left.Equals(right);
    }
    #endregion

    #region Indexers
    ///<summary>
    ///Gets the element at <paramref name="row"/>, <paramref name="column"/>.
    ///</summary>
    ///<exception cref="ArgumentOutOfRangeException">An index is out of range.</exception>
    public double this[int row, int column]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(row);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, Rows);
            ArgumentOutOfRangeException.ThrowIfNegative(column);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, Columns);

            return _values[(row * Columns) + column];
        }
    }
    #endregion

    #region Private methods
    private static Matrix Combine(Matrix left, Matrix right, Func<double, double, double> op)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        if(left.Rows != right.Rows || left.Columns != right.Columns)
        {
            throw new ArgumentException($"Shape mismatch: {left.Rows}x{left.Columns} vs {right.Rows}x{right.Columns}.");
        }

        Matrix result = new(left.Rows, left.Columns);

        for(int i = 0; i < left._values.Length; i++)
        {
            result._values[i] = op(left._values[i], right._values[i]);
        }

        return result;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Returns a read-only view of every element in row-major order, without copying.
    ///</summary>
    public ReadOnlySpan<double> AsSpan() => _values;

    ///<summary>
    ///Copies every element, in row-major order, into <paramref name="destination"/>.
    ///</summary>
    ///<exception cref="ArgumentException"><paramref name="destination"/> is shorter than <c>Rows * Columns</c>.</exception>
    public void CopyTo(Span<double> destination) => _values.CopyTo(destination);

    ///<inheritdoc/>
    public bool Equals(Matrix? other) => other is not null && Rows == other.Rows && Columns == other.Columns && _values.AsSpan().SequenceEqual(other._values);

    ///<inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as Matrix);

    ///<inheritdoc/>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(Rows);
        hash.Add(Columns);

        foreach(double value in _values)
        {
            hash.Add(value);
        }

        return hash.ToHashCode();
    }

        ///<summary>
///Creates an identity matrix.
///</summary>
    ///<param name="size">The row and column count. Must be positive.</param>
    public static Matrix Identity(int size)
    {
        Matrix result = new(size, size);

        for(int i = 0; i < size; i++)
        {
            result._values[(i * size) + i] = 1;
        }

        return result;
    }

    ///<summary>
    ///Returns a read-only view of one row without copying.
    ///</summary>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="row"/> is out of range.</exception>
    public ReadOnlySpan<double> Row(int row)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, Rows);

        return _values.AsSpan(row * Columns, Columns);
    }

    ///<summary>
    ///Returns the transpose of this matrix.
    ///</summary>
    public Matrix Transpose()
    {
        Matrix result = new(Columns, Rows);

        for(int r = 0; r < Rows; r++)
        {
            for(int c = 0; c < Columns; c++)
            {
                result._values[(c * Rows) + r] = _values[(r * Columns) + c];
            }
        }

        return result;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the column count.
    ///</summary>
    public int Columns { get; }

        ///<summary>
///Gets the row count.
///</summary>
    public int Rows { get; }
    #endregion
}
