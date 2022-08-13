using System.Collections;

namespace minlightcsfs.PolygonTriangulation;

/// <summary>
///     subclass container for polygon
/// </summary>
public partial class Polygon
{
    /// <summary>
    ///     An enumerable that creates a <see cref="NextChainEnumerator" />
    /// </summary>
    private class NextChainEnumerable : IEnumerable<int>
    {
        private readonly IReadOnlyList<VertexChain> chain;
        private readonly int start;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NextChainEnumerable" /> class.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="chain">The chain.</param>
        public NextChainEnumerable(int start, IReadOnlyList<VertexChain> chain)
        {
            this.start = start;
            this.chain = chain;
        }

        /// <inheritdoc />
        public IEnumerator<int> GetEnumerator()
        {
            return new NextChainEnumerator(start, chain);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Internal enumerator
        /// </summary>
        private sealed class NextChainEnumerator : IEnumerator<int>
        {
            private readonly IReadOnlyList<VertexChain> chain;
            private readonly int start;
#if DEBUG
            private int maxIteratorCount;
#endif
            private bool reset;

            /// <summary>
            ///     Initializes a new instance of the <see cref="NextChainEnumerator" /> class.
            /// </summary>
            /// <param name="start">The start.</param>
            /// <param name="chain">The chain.</param>
            public NextChainEnumerator(int start, IReadOnlyList<VertexChain> chain)
            {
                this.start = start;
                this.chain = chain;
                reset = true;
#if DEBUG
                maxIteratorCount = chain.Count;
#endif
            }

            /// <inheritdoc />
            public int Current { get; private set; }

            /// <inheritdoc />
            object IEnumerator.Current => Current;

            /// <inheritdoc />
            public void Dispose()
            {
                Current = -1;
            }

            /// <inheritdoc />
            public bool MoveNext()
            {
                if (reset)
                {
                    reset = false;
                    Current = start;
                }
                else
                {
                    Current = chain[Current].Next;
                    if (Current == start) return false;
#if DEBUG
                    if (--maxIteratorCount < 0) throw new InvalidOperationException("Chain is damaged");
#endif
                }

                return true;
            }

            /// <inheritdoc />
            public void Reset()
            {
                reset = true;
            }
        }
    }
}