using System.Numerics;

namespace minlightcsfs.PolygonTriangulation;

using Vertex = Vector2;

/// <summary>
///     Build a polygon
/// </summary>
public interface IPolygonBuilder
{
    /// <summary>
    ///     Add a single vertex id
    /// </summary>
    /// <param name="vertexId">the index in the vertices array of the builder</param>
    /// <returns>the same builder instance for call chains</returns>
    IPolygonBuilder Add(int vertexId);

    /// <summary>
    ///     Add multiple vertex ids
    /// </summary>
    /// <param name="vertices">the indices in the vertices array of the builder</param>
    /// <returns>the same builder instance for call chains</returns>
    IPolygonBuilder AddVertices(IEnumerable<int> vertices);

    /// <summary>
    ///     Close the current polygon. Next vertices are considered a new polygon line i.e a hole or a non-intersecting polygon
    /// </summary>
    /// <returns>the same builder instance for call chains</returns>
    IPolygonBuilder ClosePartialPolygon();

    /// <summary>
    ///     Close the polygon. Do not use the builder after closing it.
    /// </summary>
    /// <param name="fusionVertices">vertices that are used in more than one subpolygon</param>
    /// <returns>a polygon</returns>
    Polygon Close(params int[] fusionVertices);

    /// <summary>
    ///     Create one polygon that includes all vertices in the builder.
    /// </summary>
    /// <returns>a polygon</returns>
    Polygon Auto();
}

/// <summary>
///     subclass container for polygon
/// </summary>
public partial class Polygon
{
    /// <summary>
    ///     Build a new polygon
    /// </summary>
    private class PolygonBuilder : IPolygonBuilder
    {
        private readonly List<int> nextIndices;
        private readonly List<int> polygonIds;
        private readonly List<int> vertexIds;
        private readonly Vertex[] vertices;
        private int first;
        private int polygonId;

        public PolygonBuilder(Vertex[] vertices)
        {
            first = 0;
            this.vertices = vertices;
            vertexIds = new List<int>();
            nextIndices = new List<int>();
            polygonIds = new List<int>();
            polygonId = 0;
        }

        public Polygon Auto()
        {
            return FromVertexList(
                vertices,
                Enumerable.Range(0, vertices.Length),
                Enumerable.Range(1, vertices.Length - 1).Concat(Enumerable.Range(0, 1)),
                Enumerable.Repeat(0, vertices.Length),
                null);
        }

        public IPolygonBuilder Add(int vertexId)
        {
            nextIndices.Add(nextIndices.Count + 1);
            vertexIds.Add(vertexId);
            polygonIds.Add(polygonId);
            return this;
        }

        public IPolygonBuilder AddVertices(IEnumerable<int> vertices)
        {
            foreach (var vertex in vertices)
            {
                nextIndices.Add(nextIndices.Count + 1);
                vertexIds.Add(vertex);
                polygonIds.Add(polygonId);
            }

            return this;
        }

        public IPolygonBuilder ClosePartialPolygon()
        {
            if (vertexIds.Count > first)
            {
                nextIndices[nextIndices.Count - 1] = first;
                polygonId++;
                first = vertexIds.Count;
            }

            return this;
        }

        public Polygon Close(params int[] fusionVertices)
        {
            ClosePartialPolygon();
            return FromVertexList(vertices, vertexIds, nextIndices, polygonIds, fusionVertices);
        }
    }
}