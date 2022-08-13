﻿using System.Numerics;

namespace PolygonTriangulation;

using Vertex = Vector2;

/// <summary>
///     Resulting plane mesh with coordinates and triangles
/// </summary>
public interface ITriangulatedPlanePolygon
{
    /// <summary>
    ///     Gets the 3D vertices
    /// </summary>
    Vector3[] Vertices { get; }

    /// <summary>
    ///     Gets the 2D vertices of the plane points
    /// </summary>
    IReadOnlyList<Vertex> Vertices2D { get; }

    /// <summary>
    ///     Gets the triangles with vertex offset
    /// </summary>
    int[] Triangles { get; }
}

/// <summary>
///     subclass container
/// </summary>
public partial class PlanePolygonBuilder
{
    /// <summary>
    ///     Result for the plane mesh
    /// </summary>
    private class TriangulatedPlanePolygon : ITriangulatedPlanePolygon
    {
        public TriangulatedPlanePolygon(Vector3[] vertices, IReadOnlyList<Vertex> vertices2D, int[] triangles)
        {
            Vertices = vertices;
            Triangles = triangles;
            Vertices2D = vertices2D;
        }

        /// <inheritdoc />
        public Vector3[] Vertices { get; }

        /// <inheritdoc />
        public int[] Triangles { get; }

        /// <inheritdoc />
        public IReadOnlyList<Vertex> Vertices2D { get; }
    }
}