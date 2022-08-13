using System.Numerics;

namespace PolygonTriangulation;

/// <summary>
///     Collect the edges for a plane polygon
/// </summary>
public interface IPlanePolygonEdgeCollector
{
    /// <summary>
    ///     Add an edge
    /// </summary>
    /// <param name="p0">start point</param>
    /// <param name="p1">end point</param>
    void AddEdge(Vector3 p0, Vector3 p1);

    /// <summary>
    ///     Dump the collected edges
    /// </summary>
    /// <returns>dump the collected edges for debug</returns>
    string Dump();
}

/// <summary>
///     Build a list of triangles from polygon edges
/// </summary>
public partial class PlanePolygonBuilder : IPlanePolygonEdgeCollector
{
    private const float Epsilon = 1.1E-5f;
    private readonly EdgesToPolygonBuilder edgesToPolygon;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PlanePolygonBuilder" /> class.
    /// </summary>
    /// <param name="plane">The plane to rotate the 3D point into 2D.</param>
    public PlanePolygonBuilder(Plane plane)
    {
#if UNITY_EDITOR || UNITY_STANDALONE
            var rotation = Quaternion.FromToRotation(plane.normal, new Vector3(0, 0, -1));
#else
        var rotation = IdendityQuaternion;
        if (plane.Normal != Vector3.UnitZ) throw new NotImplementedException("rotation setup is not implemented");
#endif
        edgesToPolygon = new EdgesToPolygonBuilder(rotation);
    }

    /// <summary>
    ///     Gets the 3D to 2D rotation
    /// </summary>
    public Quaternion Rotation => edgesToPolygon.Rotation;

#if UNITY_EDITOR || UNITY_STANDALONE
        private static Quaternion IdendityQuaternion => Quaternion.identity;
#else
    private static Quaternion IdendityQuaternion => Quaternion.Identity;
#endif

    /// <inheritdoc />
    public void AddEdge(Vector3 p0, Vector3 p1)
    {
        edgesToPolygon.AddEdge(p0, p1);
    }

    /// <inheritdoc />
    public string Dump()
    {
        return edgesToPolygon.Dump();
    }

    /// <summary>
    ///     Build the plane triangles
    /// </summary>
    /// <returns>The triangles, the 2D vertices and the 3D vertices</returns>
    public ITriangulatedPlanePolygon Build()
    {
        IPlanePolygon polygonResult = null;
        try
        {
            polygonResult = edgesToPolygon.BuildPolygon();
            var triangulator = new PolygonTriangulator(polygonResult.Polygon);
            var triangles = triangulator.BuildTriangles();
            return new TriangulatedPlanePolygon(polygonResult.Vertices, polygonResult.Polygon.Vertices, triangles);
        }
        catch (Exception e)
        {
            throw new TriangulationException(polygonResult?.Polygon, edgesToPolygon.Dump(), e);
        }
    }

    /// <summary>
    ///     Create a edges to polygon builder for unit testing
    /// </summary>
    /// <returns>the polygon builder</returns>
    internal static IEdgesToPolygonBuilder CreatePolygonBuilder()
    {
        return new EdgesToPolygonBuilder(IdendityQuaternion);
    }

    /// <summary>
    ///     Create a polygon from edges detector. The edge is defined by the vertex ids
    /// </summary>
    /// <param name="fusionVertices">The fusion vertices.</param>
    /// <returns>the detector</returns>
    internal static IPolygonLineDetector CreatePolygonLineDetector(params int[] fusionVertices)
    {
        return new PolygonLineDetector(fusionVertices);
    }
}