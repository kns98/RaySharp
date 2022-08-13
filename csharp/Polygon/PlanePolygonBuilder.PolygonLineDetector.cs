using System.Numerics;

namespace minlightcsfs.PolygonTriangulation;

using Vertex = Vector2;

/// <summary>
///     Test interface for the polygon line detector (join edges to polygon lines)
/// </summary>
public interface IPolygonLineDetector
{
    /// <summary>
    ///     Gets the closed polygons
    /// </summary>
    IEnumerable<IReadOnlyCollection<int>> ClosedPolygons { get; }

    /// <summary>
    ///     Gets the unclosed polygons
    /// </summary>
    IEnumerable<IReadOnlyCollection<int>> UnclosedPolygons { get; }

    /// <summary>
    ///     Add multiple edges
    /// </summary>
    /// <param name="edges">pairs of vertex ids</param>
    void JoinEdgesToPolygones(IEnumerable<int> edges);

    /// <summary>
    ///     Try to close unclosed polygons by connecting close vertices.
    /// </summary>
    /// <param name="vertices">the vertices</param>
    /// <param name="maxDistance">the maximum distance between vertices</param>
    void TryClusteringUnclosedEnds(Vertex[] vertices, float maxDistance);
}

/// <summary>
///     subclass container
/// </summary>
public partial class PlanePolygonBuilder
{
    /// <summary>
    ///     Detect closed polygon lines
    /// </summary>
    private class PolygonLineDetector : IPolygonLineDetector
    {
        /// <summary>
        ///     Closed polygon lines
        /// </summary>
        private readonly List<PolygonLine> closedPolygones;

        /// <summary>
        ///     Segments with fusion point that need delay
        /// </summary>
        private readonly List<(int, int)> fusionDelayedSegments;

        /// <summary>
        ///     The fusion vertices
        /// </summary>
        private readonly ICollection<int> fusionVertices;

        /// <summary>
        ///     Mapping from start/end of polygon line to the line instance.
        /// </summary>
        private readonly Dictionary<int, PolygonLine> openPolygones;

        /// <summary>
        ///     Unclosed polygon lines
        /// </summary>
        private readonly List<PolygonLine> unclosedPolygones;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PolygonLineDetector" /> class.
        /// </summary>
        /// <param name="fusionVertices">Vertices that are used by more than two edges</param>
        public PolygonLineDetector(ICollection<int> fusionVertices)
        {
            openPolygones = new Dictionary<int, PolygonLine>();
            closedPolygones = new List<PolygonLine>();
            unclosedPolygones = new List<PolygonLine>();
            this.fusionVertices = fusionVertices;
            fusionDelayedSegments = fusionVertices.Any() ? new List<(int, int)>() : null;
        }

        /// <summary>
        ///     Gets the closed polygones.
        /// </summary>
        public IReadOnlyList<PolygonLine> Lines => closedPolygones;

        /// <inheritdoc />
        public IEnumerable<IReadOnlyCollection<int>> ClosedPolygons => closedPolygones.Select(x => x.ToIndexes());

        /// <inheritdoc />
        public IEnumerable<IReadOnlyCollection<int>> UnclosedPolygons => unclosedPolygones.Select(x => x.ToIndexes());

        /// <summary>
        ///     find continous combination of edges
        /// </summary>
        /// <param name="edges">triangle data, the relevant edges are from vertex0 to vertex1. vertex2 is ignored</param>
        public void JoinEdgesToPolygones(IEnumerable<int> edges)
        {
            var iterator = edges.GetEnumerator();
            while (iterator.MoveNext())
            {
                var start = iterator.Current;
                iterator.MoveNext();
                var end = iterator.Current;
                if (start == end) continue;
                AddEdge(start, end);
            }

            if (fusionDelayedSegments?.Count > 0)
                foreach (var (start, end) in fusionDelayedSegments)
                    AddEdge(start, end);
            unclosedPolygones.AddRange(openPolygones
                .Where(x => x.Key == x.Value.StartKey)
                .Select(x => x.Value));
        }

        /// <inheritdoc />
        public void TryClusteringUnclosedEnds(Vertex[] vertices, float maxDistance)
        {
            bool vertexFound;
            do
            {
                vertexFound = false;
                foreach (var vertexId in openPolygones.Keys)
                {
                    var closestPeer = openPolygones.Keys
                        .Where(x => x != vertexId)
                        .OrderBy(x => Distance(vertices, vertexId, x))
                        .First();
                    if (Distance(vertices, vertexId, closestPeer) < maxDistance)
                    {
                        JoinClusteredVertex(vertexId, closestPeer);
                        vertexFound = true;
                        break;
                    }
                }
            } while (vertexFound);

            unclosedPolygones.Clear();
            unclosedPolygones.AddRange(openPolygones
                .Where(x => x.Key == x.Value.StartKey)
                .Select(x => x.Value));
        }

        /// <summary>
        ///     Calculate the distance between two points
        /// </summary>
        /// <param name="vertices">the vertices</param>
        /// <param name="vertexId">the first point</param>
        /// <param name="peer">the second point</param>
        /// <returns>the sum of the x and y distance</returns>
        private static float Distance(Vertex[] vertices, int vertexId, int peer)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
                return Math.Abs(vertices[vertexId].x - vertices[peer].x) + Math.Abs(vertices[vertexId].y - vertices[peer].y);
#else
            return Math.Abs(vertices[vertexId].X - vertices[peer].X) +
                   Math.Abs(vertices[vertexId].Y - vertices[peer].Y);
#endif
        }

        /// <summary>
        ///     Join two vertices as they are close together.
        /// </summary>
        /// <param name="vertexId">the vertex id to drop</param>
        /// <param name="closestPeer">the closest existing peer, that will act as replacement</param>
        private void JoinClusteredVertex(int vertexId, int closestPeer)
        {
            var vertexSegment = openPolygones[vertexId];
            openPolygones.Remove(vertexId);
            var peerIsLineStart = vertexSegment.RemoveVertex(vertexId);
            var peerSegment = openPolygones[closestPeer];
            openPolygones.Remove(closestPeer);
            var vertexReplacement = peerIsLineStart ? vertexSegment.StartKey : vertexSegment.EndKey;
            var joinedKey = peerSegment.Join(vertexSegment, vertexReplacement, closestPeer);
            if (joinedKey < 0)
            {
                closedPolygones.Add(vertexSegment);
            }
            else
            {
                openPolygones.Remove(peerIsLineStart ? vertexSegment.EndKey : vertexSegment.StartKey);
                openPolygones.Add(joinedKey, peerSegment);
            }
        }

        /// <summary>
        ///     Add a new edge to the polygon line. Either join two polygon lines, creates a new or adds the edge to the
        ///     neighboring line
        /// </summary>
        /// <param name="start">the vertex id of the edge start</param>
        /// <param name="end">the vertex id of the edge end</param>
        private void AddEdge(int start, int end)
        {
            var startFits = openPolygones.TryGetValue(start, out var firstSegment);
            if (startFits) openPolygones.Remove(start);
            var endFits = openPolygones.TryGetValue(end, out var lastSegment);
            if (endFits) openPolygones.Remove(end);
            if (!startFits && !endFits)
            {
                var segment = new PolygonLine(start, end);
                openPolygones.Add(start, segment);
                openPolygones.Add(end, segment);
            }
            else if (startFits && endFits)
            {
                var remainingKeyOfOther = firstSegment.Join(lastSegment, start, end);
                if (remainingKeyOfOther < 0)
                    closedPolygones.Add(firstSegment);
                else
                    openPolygones[remainingKeyOfOther] = firstSegment;
            }
            else if (startFits)
            {
                if (start == firstSegment.EndKey || !fusionVertices.Contains(start))
                {
                    firstSegment.AddMatchingStart(start, end);
                    openPolygones[end] = firstSegment;
                }
                else
                {
                    fusionDelayedSegments.Add((start, end));
                    openPolygones[start] = firstSegment;
                }
            }
            else
            {
                if (end == lastSegment.StartKey || !fusionVertices.Contains(end))
                {
                    lastSegment.AddMatchingEnd(start, end);
                    openPolygones[start] = lastSegment;
                }
                else
                {
                    fusionDelayedSegments.Add((start, end));
                    openPolygones[end] = lastSegment;
                }
            }
        }
    }
}