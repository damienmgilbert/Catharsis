using System.ComponentModel;

namespace Catharsis.ComponentModel;

/// <summary>
/// A <see cref="BindingList{T}"/> that supports sorting and searching
/// through the <see cref="IBindingList"/> interface.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
public class SortableBindingList<T> : BindingList<T>
{
    private bool _isSorted;
    private PropertyDescriptor? _sortProperty;
    private ListSortDirection _sortDirection;

    /// <summary>
    /// Initializes a new instance of <see cref="SortableBindingList{T}"/> with an empty list.
    /// </summary>
    public SortableBindingList() { }

    /// <summary>
    /// Initializes a new instance of <see cref="SortableBindingList{T}"/>
    /// wrapping the specified list.
    /// </summary>
    /// <param name="list">The list to wrap.</param>
    public SortableBindingList(IList<T> list) : base(list) { }

    /// <inheritdoc />
    protected override bool SupportsSortingCore => true;

    /// <inheritdoc />
    protected override bool SupportsSearchingCore => true;

    /// <inheritdoc />
    protected override bool IsSortedCore => _isSorted;

    /// <inheritdoc />
    protected override PropertyDescriptor? SortPropertyCore => _sortProperty;

    /// <inheritdoc />
    protected override ListSortDirection SortDirectionCore => _sortDirection;

    /// <inheritdoc />
    protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
    {
        _sortProperty = prop;
        _sortDirection = direction;
        _isSorted = true;

        if (Items is List<T> items)
        {
            items.Sort((x, y) =>
            {
                var xValue = prop.GetValue(x);
                var yValue = prop.GetValue(y);
                int result = CompareValues(xValue, yValue);
                return direction == ListSortDirection.Descending ? -result : result;
            });
        }

        OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
    }

    /// <inheritdoc />
    protected override void RemoveSortCore()
    {
        _isSorted = false;
        _sortProperty = null;
        OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
    }

    /// <inheritdoc />
    protected override int FindCore(PropertyDescriptor prop, object? key)
    {
        for (int i = 0; i < Count; i++)
        {
            var value = prop.GetValue(Items[i]);

            if (Equals(value, key))
                return i;
        }

        return -1;
    }

    private static int CompareValues(object? x, object? y)
    {
        if (x is null && y is null) return 0;
        if (x is null) return -1;
        if (y is null) return 1;

        if (x is IComparable comparable)
            return comparable.CompareTo(y);

        return string.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal);
    }
}
