using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Catharsis.Generators
{
    /// <summary>
    /// An immutable list of <see cref="GeneratorDiagnostic"/> that compares by content, so a pipeline stage that carries
    /// it stays cacheable (an <see cref="ImmutableArray{T}"/> compares by reference).
    /// </summary>
    internal readonly struct EquatableArray(ImmutableArray<GeneratorDiagnostic> items) : System.IEquatable<EquatableArray>, IEnumerable<GeneratorDiagnostic>
    {
        public static readonly EquatableArray Empty = new EquatableArray(ImmutableArray<GeneratorDiagnostic>.Empty);

        private readonly ImmutableArray<GeneratorDiagnostic> _items = items;

        public bool Equals(EquatableArray other) => Items.SequenceEqual(other.Items);

        public override bool Equals(object? obj) => obj is EquatableArray other && Equals(other);

        public override int GetHashCode()
        {
            int hash = 17;

            foreach (GeneratorDiagnostic item in Items)
            {
                hash = (hash * 31) + item.GetHashCode();
            }

            return hash;
        }

        public IEnumerator<GeneratorDiagnostic> GetEnumerator() => ((IEnumerable<GeneratorDiagnostic>)Items).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private ImmutableArray<GeneratorDiagnostic> Items => _items.IsDefault ? ImmutableArray<GeneratorDiagnostic>.Empty : _items;
    }
}
