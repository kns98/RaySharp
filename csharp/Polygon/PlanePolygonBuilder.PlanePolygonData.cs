﻿using System.Numerics;

namespace minlightcsfs.PolygonTriangulation;

/// <summary>
///     The polygon result after combining all edges
/// </summary>
public interface IPlanePolygon
{
    /// <summary>
    ///     Gets the 3D vertices.
    /// </summary>
    Vector3[] Vertices { get; }

    /// <summary>
    ///     Gets the polygon. It contains the 2D vertices.
    /// </summary>
    Polygon Polygon { get; }
}

/// <summary>
///     subclass container
/// </summary>
public partial class PlanePolygonBuilder
{
    /// <summary>
    ///     Result storage for polygon data
    /// </summary>
    private class PlanePolygonData : IPlanePolygon
    {
        public PlanePolygonData(Vector3[] vertices3D, Polygon polygon)
        {
            Vertices = vertices3D;
            Polygon = polygon;
        }

        /// <inheritdoc />
        public Vector3[] Vertices { get; }

        /// <inheritdoc />
        public Polygon Polygon { get; }
    }
}