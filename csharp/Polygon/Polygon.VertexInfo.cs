using System.Diagnostics;

namespace PolygonTriangulation;

/// <summary>
///     The action necessary for the vertex transition.
///     Ordering is important, because for the same vertex id, we need to process closing before transition before opening
/// </summary>
public enum VertexAction
{
    /// <summary>
    ///     Prev and next are left of the vertex. => This is a closing cusp.
    /// </summary>
    ClosingCusp,

    /// <summary>
    ///     Transition from one vertex to the net. No cusp.
    /// </summary>
    Transition,

    /// <summary>
    ///     Prev and next are right of the vertex. => This is an opening cusp.
    /// </summary>
    OpeningCusp
}

/// <summary>
///     Information about an element in the vertex chain of a polygon.
/// </summary>
public interface IPolygonVertexInfo
{
    /// <summary>
    ///     Gets the action necessary to process the triple
    /// </summary>
    VertexAction Action { get; }

    /// <summary>
    ///     Gets the id of the current vertex
    /// </summary>
    int Id { get; }

    /// <summary>
    ///     Gets the id of the next vertex
    /// </summary>
    int NextVertexId { get; }

    /// <summary>
    ///     Gets the id of the previous vertex
    /// </summary>
    int PrevVertexId { get; }

    /// <summary>
    ///     Gets a unique identifier for overlaying vertexes
    /// </summary>
    int Unique { get; }

    /// <summary>
    ///     Gets the <see cref="Unique" /> for the next vertex
    /// </summary>
    int NextUnique { get; }

    /// <summary>
    ///     Gets the <see cref="Unique" /> for the prev vertex
    /// </summary>
    int PrevUnique { get; }
}

/// <summary>
///     subclass container for polygon
/// </summary>
public partial class Polygon
{
    /// <summary>
    ///     Information about an element in the vertex chain.
    /// </summary>
    [DebuggerDisplay("{Prev}>{Id}>{Next}")]
    private class VertexInfo : IPolygonVertexInfo
    {
        private readonly VertexChain[] chain;

        public VertexInfo(int element, VertexChain[] chain)
        {
            this.Unique = element;
            this.chain = chain;
            var id = Id;
            var prev = PrevVertexId;
            var next = NextVertexId;
            if (prev < id && next < id)
                Action = VertexAction.ClosingCusp;
            else if (prev > id && next > id)
                Action = VertexAction.OpeningCusp;
            else
                Action = VertexAction.Transition;
        }

        /// <inheritdoc />
        public VertexAction Action { get; }

        /// <inheritdoc />
        public int Id => chain[Unique].VertexId;

        /// <inheritdoc />
        public int NextVertexId => chain[chain[Unique].Next].VertexId;

        /// <inheritdoc />
        public int PrevVertexId => chain[chain[Unique].Prev].VertexId;

        /// <inheritdoc />
        public int Unique { get; }

        /// <inheritdoc />
        public int NextUnique => chain[Unique].Next;

        /// <inheritdoc />
        public int PrevUnique => chain[Unique].Prev;
    }
}