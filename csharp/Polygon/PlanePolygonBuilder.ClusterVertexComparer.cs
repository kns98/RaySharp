using System.Numerics;

namespace minlightcsfs.PolygonTriangulation;


using Vertex = Vector2;


/// <summary>
///     subclass container
/// </summary>
public partial class PlanePolygonBuilder
{
    /// <summary>
    ///     Compare two vertices, very close vertices are considered equal.
    /// </summary>
    private class ClusterVertexComparer : IComparer<Vertex>
    {
        /// <inheritdoc />
        public int Compare(Vertex x, Vertex y)
        {
            var xdist = Math.Abs(x.X - y.X);
            if (xdist < Epsilon)
            {
                var ydist = Math.Abs(x.Y - y.Y);
                if (ydist < Epsilon) return 0;
                var xCompare = x.X.CompareTo(y.X);
                if (xCompare != 0) return xCompare;
                if (x.Y < y.Y)
                    return -1;
                return 1;
            }

            if (x.X < y.X)
                return -1;
            return 1;
        }
    }
}