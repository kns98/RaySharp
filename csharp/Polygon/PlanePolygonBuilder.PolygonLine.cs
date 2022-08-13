using System.Diagnostics;

namespace minlightcsfs.PolygonTriangulation;

/// <summary>
///     subclass container
/// </summary>
public partial class PlanePolygonBuilder
{
    /// <summary>
    ///     A polygon, defined by connected edges.
    /// </summary>
    [DebuggerDisplay("{Debug}")]
    private class PolygonLine
    {
        private readonly List<int> vertexIds;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PolygonLine" /> class. The line starts with two vertices.
        /// </summary>
        /// <param name="start">the first vertex</param>
        /// <param name="end">the second vertex</param>
        public PolygonLine(int start, int end)
        {
            StartKey = start;
            EndKey = end;
            vertexIds = new List<int> { start, end };
        }

        /// <summary>
        ///     Gets the first vertex id in the polygon
        /// </summary>
        public int StartKey { get; private set; }

        /// <summary>
        ///     Gets the last vertex id in the polygon
        /// </summary>
        public int EndKey { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the edge direction was inconsistend at any time
        /// </summary>
        public bool Dirty { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the polygon is closed
        /// </summary>
        public bool Closed { get; private set; }

        /// <summary>
        ///     Gets a debug string
        /// </summary>
        public string Debug =>
            $"{(Closed ? "*" : string.Empty)}, {(Dirty ? "#" : string.Empty)}, {string.Join(" ", vertexIds)}";

        /// <summary>
        ///     Gets the vertex ids in order
        /// </summary>
        /// <returns>the vertex ids</returns>
        public IReadOnlyCollection<int> ToIndexes()
        {
            return vertexIds;
        }

        /// <summary>
        ///     The start value of the added edge matches either the end or start of this polygon
        /// </summary>
        /// <param name="edgeStart">the start of the added edge</param>
        /// <param name="value">the other value of the added edgeegment</param>
        public void AddMatchingStart(int edgeStart, int value)
        {
            if (edgeStart == EndKey)
            {
                AddSingleVertex(value, true);
            }
            else
            {
                Dirty = true;
                AddSingleVertex(value, false);
            }
        }

        /// <summary>
        ///     The end value of the added edge matches either the end or start of this polygon
        /// </summary>
        /// <param name="value">the start value of the added edge</param>
        /// <param name="edgeEnd">the matching end</param>
        public void AddMatchingEnd(int value, int edgeEnd)
        {
            if (edgeEnd == StartKey)
            {
                AddSingleVertex(value, false);
            }
            else
            {
                Dirty = true;
                AddSingleVertex(value, true);
            }
        }

        /// <summary>
        ///     Join two polygones by an edge.
        /// </summary>
        /// <param name="other">the other polygone</param>
        /// <param name="edgeStart">the start of the edge that joines</param>
        /// <param name="edgeEnd">the end of the edge that joines</param>
        /// <returns>-1: the polygone is closed. Otherwise the start/end key that was changed</returns>
        public int Join(PolygonLine other, int edgeStart, int edgeEnd)
        {
            return JoinAndClose(other)
                   ?? JoinSameDirection(other, edgeStart, edgeEnd)
                   ?? JoinWithEdgeInInverseDirection(other, edgeStart, edgeEnd)
                   ?? JoinReversingOtherPolygon(other, edgeStart, edgeEnd)
                   ?? throw new InvalidOperationException(
                       $"Can't join s:{edgeStart} e:{edgeEnd}, ts: {StartKey} te: {EndKey}, os: {other.StartKey} oe: {other.EndKey}");
        }

        /// <summary>
        ///     Remove a vertex that is either start or end
        /// </summary>
        /// <param name="vertexId">the id of the vertex</param>
        /// <returns>true if start was modified, false if end was modified</returns>
        public bool RemoveVertex(int vertexId)
        {
            if (vertexId == StartKey)
            {
                vertexIds.RemoveAt(0);
                StartKey = vertexIds[0];
                return true;
            }

            if (vertexId == EndKey)
            {
                vertexIds.RemoveAt(vertexIds.Count - 1);
                EndKey = vertexIds[vertexIds.Count - 1];
                return false;
            }

            throw new InvalidOperationException("Can't remove a vertex in the middle of the polygon line");
        }

        /// <summary>
        ///     Compare the edge points to two corresponding keys.
        /// </summary>
        /// <param name="edgeStart">the edge start</param>
        /// <param name="edgeEnd">the edge end</param>
        /// <param name="keyStart">the key that's compared to edgeStart</param>
        /// <param name="keyEnd">the key that's compared to edgeEnd</param>
        /// <returns>true if edgeStart matches key1 and EdgeEnd matches key2</returns>
        private static bool CompareEdgeToKeys(int edgeStart, int edgeEnd, int keyStart, int keyEnd)
        {
            return edgeStart == keyStart && edgeEnd == keyEnd;
        }

        /// <summary>
        ///     Compare the edge points to two corresponding keys or the the swapped keys
        /// </summary>
        /// <param name="edgeStart">the edge start</param>
        /// <param name="edgeEnd">the edge end</param>
        /// <param name="key1">the key for edgeStart</param>
        /// <param name="key2">the key for edgeEnd</param>
        /// <returns>true if edgeStart matches key1 and EdgeEnd matches key2</returns>
        private static bool CompareEdgeToKeysOrSwappedKeys(int edgeStart, int edgeEnd, int key1, int key2)
        {
            return CompareEdgeToKeys(edgeStart, edgeEnd, key1, key2) ||
                   CompareEdgeToKeys(edgeStart, edgeEnd, key2, key1);
        }

        /// <summary>
        ///     Add a single value at the end or the start. Adjust the according key.
        /// </summary>
        /// <param name="value">the value to add</param>
        /// <param name="append">true for end, false for start</param>
        private void AddSingleVertex(int value, bool append)
        {
            if (append)
            {
                vertexIds.Add(value);
                EndKey = value;
            }
            else
            {
                vertexIds.Insert(0, value);
                StartKey = value;
            }
        }

        /// <summary>
        ///     Append the other vertices after our data and adjust the end
        /// </summary>
        /// <param name="otherVertices">the other vertices</param>
        /// <param name="newEnd">our new effective end</param>
        /// <returns>the new end</returns>
        private int AppendRange(IEnumerable<int> otherVertices, int newEnd)
        {
            vertexIds.AddRange(otherVertices);
            EndKey = newEnd;
            return newEnd;
        }

        /// <summary>
        ///     Insert the other vertices before our data and adjust the start
        /// </summary>
        /// <param name="otherVertices">the other vertices</param>
        /// <param name="newStart">our new effective start</param>
        /// <returns>the new start</returns>
        private int InsertRange(IEnumerable<int> otherVertices, int newStart)
        {
            vertexIds.InsertRange(0, otherVertices);
            StartKey = newStart;
            return newStart;
        }

        /// <summary>
        ///     Join and close the polygon if this and other is the same instance
        /// </summary>
        /// <param name="other">the other polygon line</param>
        /// <returns>-1 if the polygon was joined, null else</returns>
        private int? JoinAndClose(PolygonLine other)
        {
            if (ReferenceEquals(this, other))
            {
                Closed = true;
                return -1;
            }

            return null;
        }

        /// <summary>
        ///     The connecting edge fits one start and one end. Join with consistent direction.
        /// </summary>
        /// <param name="other">the other polygon line</param>
        /// <param name="edgeStart">the start of the joining edge</param>
        /// <param name="edgeEnd">the end of the joining edge</param>
        /// <returns>The start/end key that was changed or null if it doesn't fit</returns>
        private int? JoinSameDirection(PolygonLine other, int edgeStart, int edgeEnd)
        {
            if (CompareEdgeToKeys(edgeStart, edgeEnd, EndKey, other.StartKey))
                return AppendRange(other.vertexIds, other.EndKey);
            if (CompareEdgeToKeys(edgeStart, edgeEnd, other.EndKey, StartKey))
                return InsertRange(other.vertexIds, other.StartKey);
            return null;
        }

        /// <summary>
        ///     this and other has the same direction, but the edge direction is reversed
        /// </summary>
        /// <param name="other">the other polygon line</param>
        /// <param name="edgeStart">the start of the joining edge</param>
        /// <param name="edgeEnd">the end of the joining edge</param>
        /// <returns>The start/end key that was changed or null if it doesn't fit</returns>
        private int? JoinWithEdgeInInverseDirection(PolygonLine other, int edgeStart, int edgeEnd)
        {
            if (CompareEdgeToKeys(edgeStart, edgeEnd, other.StartKey, EndKey))
            {
                Dirty = true;
                return AppendRange(other.vertexIds, other.EndKey);
            }

            if (CompareEdgeToKeys(edgeStart, edgeEnd, StartKey, other.EndKey))
            {
                Dirty = true;
                return InsertRange(other.vertexIds, other.StartKey);
            }

            return null;
        }

        /// <summary>
        ///     new edge connects at both start or both end points, reverse the other segment and join
        /// </summary>
        /// <param name="other">the other polygon line</param>
        /// <param name="edgeStart">the start of the joining edge</param>
        /// <param name="edgeEnd">the end of the joining edge</param>
        /// <returns>The start/end key that was changed or null if it doesn't fit</returns>
        private int? JoinReversingOtherPolygon(PolygonLine other, int edgeStart, int edgeEnd)
        {
            var reversedOther = new List<int>(other.vertexIds);
            reversedOther.Reverse();
            if (CompareEdgeToKeysOrSwappedKeys(edgeStart, edgeEnd, StartKey, other.StartKey))
            {
                Dirty = true;
                return InsertRange(reversedOther, other.EndKey);
            }

            if (CompareEdgeToKeysOrSwappedKeys(edgeStart, edgeEnd, EndKey, other.EndKey))
            {
                Dirty = true;
                return AppendRange(reversedOther, other.StartKey);
            }

            return null;
        }
    }
}