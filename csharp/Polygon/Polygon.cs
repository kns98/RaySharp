﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace minlightcsfs.PolygonTriangulation;

using Vertex = Vector2;

/// <summary>
///     Represent a collection of closed polygons.
/// </summary>
[DebuggerDisplay("{Debug}")]
public partial class Polygon
{
    /// <summary>
    ///     The current chain of vertices in the polygon. May contain multiple chains.
    /// </summary>
    private readonly VertexChain[] chain;

    /// <summary>
    ///     the start index in <see cref="chain" /> per sub polygon
    /// </summary>
    private readonly List<int> polygonStartIndices;

    /// <summary>
    ///     Gets the vertex coordinates
    /// </summary>
    private readonly Vertex[] vertexCoordinates;

    /// <summary>
    ///     Translate the vertex id to the index in the chain. For collisions look at
    ///     <see cref="VertexChain.SameVertexChain" />
    /// </summary>
    private readonly int[] vertexToChain;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Polygon" /> class.
    /// </summary>
    /// <param name="vertexCoordinates">the vertex coordinates</param>
    /// <param name="chain">the next vertex chain</param>
    /// <param name="vertexToChain">the translation from vertex id to chain index.</param>
    /// <param name="polygonStartIndices">the start of the subpolygones</param>
    private Polygon(Vertex[] vertexCoordinates, VertexChain[] chain, int[] vertexToChain,
        IEnumerable<int> polygonStartIndices)
    {
        this.chain = chain;
        this.vertexToChain = vertexToChain;
        this.vertexCoordinates = vertexCoordinates;
        this.polygonStartIndices = polygonStartIndices.ToList();
    }

    /// <summary>
    ///     Gets the debug string.
    /// </summary>
    public string Debug => string.Join(" || ", SubPolygonIds.Select(x => string.Join(" ", SubPolygonVertices(x))));

    /// <summary>
    ///     Gets the vertex coordinates
    /// </summary>
    public IReadOnlyList<Vertex> Vertices => Array.AsReadOnly(vertexCoordinates);

    /// <summary>
    ///     Gets the ids of all available sub polygons. Use <see cref="SubPolygonVertices" /> to enemerate the other polygons
    /// </summary>
    public IEnumerable<int> SubPolygonIds =>
        Enumerable.Range(0, polygonStartIndices.Count).Where(x => polygonStartIndices[x] >= 0);

    /// <summary>
    ///     Gets id/prev/next info per vertex sorted by vertex id.
    /// </summary>
    public IEnumerable<IPolygonVertexInfo> OrderedVertices
    {
        get
        {
            var workOrder = Enumerable.Range(0, chain.Length).OrderBy(x => chain[x].VertexId);
            return workOrder.Select(x => new VertexInfo(x, chain));
        }
    }

    /// <summary>
    ///     Create a polygon builder
    /// </summary>
    /// <param name="vertices">the vertices of the polygon</param>
    /// <returns>A builder to define vertex order and sub-polygons</returns>
    public static IPolygonBuilder Build(Vertex[] vertices)
    {
        return new PolygonBuilder(vertices);
    }

    /// <summary>
    ///     Split the polygon along tuples of two vertex indices.
    /// </summary>
    /// <param name="polygon">the polygon to split. It's modified.</param>
    /// <param name="splits">the splits as tuples of vertex ids</param>
    /// <param name="triangleCollector">the collector for simple triangles</param>
    /// <returns>A polygon that cotains only monotones.</returns>
    public static Polygon Split(Polygon polygon, IEnumerable<Tuple<int, int>> splits,
        ITriangleCollector triangleCollector)
    {
        var splitter = new PolygonSplitter(polygon, splits, triangleCollector);
        return splitter.Execute();
    }

    /// <summary>
    ///     create a polygon from multiple polygon lines
    /// </summary>
    /// <param name="vertexCoordinates">the vertex coordinates</param>
    /// <param name="lines">The polygon lines. Each vertex Id must be unique inside a polygon line.</param>
    /// <param name="fusionVertices">vertices that are used in more than one subpolygon</param>
    /// <returns>a polygon</returns>
    [SuppressMessage("Major Code Smell", "S1117:Local variables should not shadow class fields",
        Justification = "Reusing a fieldname in a static method seems fine")]
    public static Polygon FromPolygonLines(Vertex[] vertexCoordinates, IReadOnlyCollection<int>[] lines,
        ICollection<int> fusionVertices)
    {
        var vertexChain = new VertexChain[lines.Sum(x => x.Count)];
        var id = 0;
        var first = id;
        var vertexToChain = Enumerable.Repeat(-1, vertexCoordinates.Length).ToArray();
        var subPolygones = new List<int>();
        foreach (var line in lines)
        {
            var polygonId = subPolygones.Count;
            subPolygones.Add(id);
            foreach (var vertexId in line)
            {
                vertexChain[id].VertexId = vertexId;
                vertexChain[id].SubPolygonId = polygonId;
                vertexChain[id].SameVertexChain = vertexToChain[vertexId];
                vertexToChain[vertexId] = id;
                SetNext(vertexChain, id, id == vertexChain.Length - 1 ? first : id + 1);
                id++;
            }

            SetNext(vertexChain, id - 1, first);
            first = id;
        }

        var polygon = new Polygon(vertexCoordinates, vertexChain, vertexToChain, subPolygones);
        polygon.FusionVerticesIntoChain(fusionVertices);
        return polygon;
    }

    /// <summary>
    ///     Create a polygon with vertex id's and next chain. Can contain holes.
    /// </summary>
    /// <param name="vertexCoordinates">the coordinates</param>
    /// <param name="vertexIds">the vertex ids</param>
    /// <param name="nextIndices">the next index in vertexIds. Must be same length as vertexIds</param>
    /// <param name="polygonIds">The polygon ids.</param>
    /// <param name="fusionVertices">Vertices that are used in more than one subpolygon. Can be null.</param>
    /// <returns>a polygon</returns>
    [SuppressMessage("Major Code Smell", "S1117:Local variables should not shadow class fields",
        Justification = "Reusing a fieldname in a static method seems fine")]
    public static Polygon FromVertexList(Vertex[] vertexCoordinates, IEnumerable<int> vertexIds,
        IEnumerable<int> nextIndices, IEnumerable<int> polygonIds, IReadOnlyList<int> fusionVertices)
    {
        var vertexIdCollection = vertexIds as IReadOnlyCollection<int> ?? vertexIds.ToArray();
        var polygonIdCollection = polygonIds as IList<int> ?? polygonIds.ToArray();
        var vertexToChain = Enumerable.Repeat(-1, vertexCoordinates.Length).ToArray();
        var vertexChain = new VertexChain[vertexIdCollection.Count];
        var polygonStartIndex = new List<int>();
        var i = 0;
        foreach (var (vertexId, nextId) in vertexIdCollection.Zip(nextIndices, Tuple.Create))
        {
            vertexChain[i].VertexId = vertexId;
            vertexChain[i].SameVertexChain = vertexToChain[vertexId];
            vertexChain[i].SubPolygonId = polygonIdCollection[i];
            SetNext(vertexChain, i, nextId);
            if (polygonIdCollection[i] >= polygonStartIndex.Count) polygonStartIndex.Add(i);
            vertexToChain[vertexId] = i;
            i++;
        }

        var polygon = new Polygon(vertexCoordinates, vertexChain, vertexToChain, polygonStartIndex);
        polygon.FusionVerticesIntoChain(fusionVertices);
        return polygon;
    }

    /// <summary>
    ///     Gets the vertex ids of a subpolygon
    /// </summary>
    /// <param name="subPolygonId">the sub polygon id</param>
    /// <returns>vertex Ids</returns>
    public IEnumerable<int> SubPolygonVertices(int subPolygonId)
    {
        return new NextChainEnumerable(polygonStartIndices[subPolygonId], chain)
            .Select(x => chain[x].VertexId);
    }

    /// <summary>
    ///     Gets an enumerator that starts at startVertex and loops the whole polygon once.
    /// </summary>
    /// <param name="startVertex">The first vertex id</param>
    /// <param name="subPolygonId">the id of the subpolygon to traverse</param>
    /// <returns>An Enumerable/Enumerator</returns>
    public IEnumerable<int> IndicesStartingAt(int startVertex, int subPolygonId)
    {
        var startId = vertexToChain[startVertex];
        while (chain[startId].SubPolygonId != subPolygonId)
        {
            startId = chain[startId].SameVertexChain;
            if (startId < 0)
                throw new InvalidOperationException($"Vertex {startVertex} is not part of polygon {subPolygonId}");
        }

        return new NextChainEnumerable(startId, chain).Select(x => chain[x].VertexId);
    }

    /// <summary>
    ///     Connect two chain elements.
    /// </summary>
    /// <param name="chain">The chain.</param>
    /// <param name="current">The current.</param>
    /// <param name="next">The next.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetNext(VertexChain[] chain, int current, int next)
    {
        chain[current].SetNext(current, next, ref chain[next]);
    }

    /// <summary>
    ///     Calculates an angle that grows counter clockwise from 0 to 4
    /// </summary>
    /// <param name="vertex">the center point</param>
    /// <param name="peer">the point around the center</param>
    /// <returns>a float representing the angle</returns>
    private static float DiamondAngle(ref Vertex vertex, ref Vertex peer)
    {
#if UNITY_EDITOR || UNITY_STANDALONE
            var dx = peer.x - vertex.x;
            var dy = peer.y - vertex.y;
#else
        var dx = peer.X - vertex.X;
        var dy = peer.Y - vertex.Y;
#endif
        if (dy >= 0)
            return dx >= 0 ? 0 + dy / (dx + dy) : 1 - dx / (-dx + dy);
        return dx < 0 ? 2 - dy / (-dx - dy) : 3 + dx / (dx - dy);
    }

    /// <summary>
    ///     Iterate all fusion points and join the chain
    /// </summary>
    private void FusionVerticesIntoChain(IEnumerable<int> fusionVertices)
    {
        if (fusionVertices == null) return;
        foreach (var fusionVertexId in fusionVertices)
        {
            var jobList = CreateVertexFusionJobs(fusionVertexId);
            var first = true;
            var newSubPolygons = new List<int>();
            foreach (var (prev, next, samePolygon) in jobList)
            {
                if (chain[prev].Next == next) continue;
                if (first)
                {
                    polygonStartIndices[chain[prev].SubPolygonId] = prev;
                    first = false;
                }
                else if (!samePolygon)
                {
                    polygonStartIndices[chain[prev].SubPolygonId] = -1;
                    PolygonSplitter.FillPolygonId(chain, prev, chain[next].SubPolygonId);
                }
                else
                {
                    newSubPolygons.Add(prev);
                }

                SetNext(chain, prev, next);
            }

            // During the split, the chain may contain loops, so FillPolygonId might hang. Update the polygon id in the end.
            foreach (var polygonStart in newSubPolygons)
            {
                PolygonSplitter.FillPolygonId(chain, polygonStart, polygonStartIndices.Count);
                polygonStartIndices.Add(polygonStart);
            }
        }
    }

    /// <summary>
    ///     Creates a list of jobs to reorder all edges of a vertex.
    /// </summary>
    /// <param name="fusionVertexId">the central vertex id</param>
    /// <returns>tuples of prev/next and polygon split</returns>
    /// <remarks>
    ///     After all jobs are completed, the polygon will leave the vertex with the next counter-clock-wise edge, that has
    ///     reached the vertex.
    ///     Hence there are no implicit crossings.
    ///     As the execution of the job manipulates the chain, it's required to collect all data before.
    /// </remarks>
    private (int prev, int next, bool samePolygon)[] CreateVertexFusionJobs(int fusionVertexId)
    {
        var vertex = vertexCoordinates[fusionVertexId];
        var vertexInstances = new List<(int chain, bool outgoing)>(8);
        for (var chainId = vertexToChain[fusionVertexId]; chainId >= 0; chainId = chain[chainId].SameVertexChain)
        {
            vertexInstances.Add((chainId, true));
            vertexInstances.Add((chainId, false));
        }

        var sortedByAngle = vertexInstances
            .OrderBy(x =>
            {
                var peerId = x.outgoing ? chain[x.chain].Next : chain[x.chain].Prev;
                ref var peer = ref vertexCoordinates[chain[peerId].VertexId];
                return 4.0f - DiamondAngle(ref vertex, ref peer);
            })
            .ToArray();
        var jobList = new (int prev, int next, bool samePolygon)[sortedByAngle.Length / 2];
        if (sortedByAngle.Length == 0) return jobList;
        var start = sortedByAngle[0].outgoing ? 0 : 1;
        for (var i = 0; i < jobList.Length; i++)
        {
            var outgoing = i * 2 + start;
            var incoming = outgoing + 1 == sortedByAngle.Length ? 0 : outgoing + 1;
            var prev = chain[sortedByAngle[incoming].chain].Prev;
            var startOfEdge = sortedByAngle[outgoing].chain;
            jobList[i] = (prev, startOfEdge, chain[prev].SubPolygonId == chain[startOfEdge].SubPolygonId);
        }

        return jobList;
    }
}