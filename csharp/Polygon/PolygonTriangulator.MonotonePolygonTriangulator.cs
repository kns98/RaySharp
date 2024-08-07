﻿using System.Numerics;

namespace minlightcsfs.PolygonTriangulation;

using Vertex = Vector2;

/// <summary>
///     subclass container for triangulator
/// </summary>
public partial class PolygonTriangulator
{
    /// <summary>
    ///     Class to triangluate a monotone polygon
    /// </summary>
    private class MonotonePolygonTriangulator
    {
        private readonly Polygon polygon;
        private readonly int subPolygonId;
        private readonly Stack<int> vertexStack;
        private readonly IReadOnlyList<Vertex> vertices;
        private int current;
        private IEnumerator<int> iterator;
        private int second;
        private int third;

        public MonotonePolygonTriangulator(Polygon polygon, int subPolygonId)
        {
            this.polygon = polygon;
            this.subPolygonId = subPolygonId;
            vertices = polygon.Vertices;
            vertexStack = new Stack<int>();
        }

        /// <summary>
        ///     traverse the polygon and add triangles to the collector
        /// </summary>
        /// <param name="collector">collector for resulting triangles</param>
        public void Build(ITriangleCollector collector)
        {
            var start = FindStartOfMonotonePolygon();
            if (start >= 0)
            {
                TriangulateMonotonePolygon(start, collector);
            }
            else
            {
                var triangleVertices = polygon.SubPolygonVertices(subPolygonId).ToArray();
                collector.AddTriangle(triangleVertices[0], triangleVertices[1], triangleVertices[2]);
            }
        }

        /// <summary>
        ///     Create triangles for a monotone polygon
        /// </summary>
        /// <param name="startPoint">the first point (clockwise) of the long edge.</param>
        /// <param name="result">the collector for resulting triangles</param>
        private void TriangulateMonotonePolygon(int startPoint, ITriangleCollector result)
        {
            PullFirstTriangle(startPoint);
            while (true)
                if (IsConvexCorner())
                {
                    result.AddTriangle(current, third, second);
                    if (!PopOrPullNextVertex()) return;
                }
                else
                {
                    PushAndPullNextVertex();
                }
        }

        /// <summary>
        ///     Gets the first three points from the triangle
        /// </summary>
        /// <param name="startPoint">the start vertex, aka the target vertex of the long edge</param>
        private void PullFirstTriangle(int startPoint)
        {
            iterator = polygon.IndicesStartingAt(startPoint, subPolygonId).GetEnumerator();
            iterator.MoveNext();
            third = iterator.Current;
            iterator.MoveNext();
            second = iterator.Current;
            iterator.MoveNext();
            current = iterator.Current;
        }

        /// <summary>
        ///     Current triangle is not valid, push the third point, shift down and pull the next vertex from the polygon
        /// </summary>
        private void PushAndPullNextVertex()
        {
            vertexStack.Push(third);
            third = second;
            second = current;
            if (!iterator.MoveNext()) throw new InvalidOperationException("Triangle is incomplete");
            current = iterator.Current;
        }

        /// <summary>
        ///     either pop the last vertex from the stack or get the next vertex from the polygon
        /// </summary>
        /// <returns>true if there is one more vertex</returns>
        private bool PopOrPullNextVertex()
        {
            if (vertexStack.Count > 0)
            {
                second = third;
                third = vertexStack.Pop();
            }
            else if (!iterator.MoveNext())
            {
                return false;
            }
            else
            {
                second = current;
                current = iterator.Current;
            }

            return true;
        }

        /// <summary>
        ///     Test if the current three vertices form a clockwise triangle.
        /// </summary>
        /// <returns>true if the triangle is valid</returns>
        private bool IsConvexCorner()
        {
            var v0 = vertices[current];
            var v1 = vertices[second];
            var v2 = vertices[third];
#if UNITY_EDITOR || UNITY_STANDALONE
                var cross = ((v2.x - v0.x) * (v1.y - v0.y)) - ((v2.y - v0.y) * (v1.x - v0.x));
#else
            var cross = (v2.X - v0.X) * (v1.Y - v0.Y) - (v2.Y - v0.Y) * (v1.X - v0.X);
#endif
            return cross < 0;
        }

        /// <summary>
        ///     Find the point in the polygon that starts at the monotone side
        /// </summary>
        /// <returns>highest/lowest point in the polygon, depending if itss left hand or right hand. -1 if its a triangle.</returns>
        private int FindStartOfMonotonePolygon()
        {
            var startLookupIterator = polygon.SubPolygonVertices(subPolygonId).GetEnumerator();
            startLookupIterator.MoveNext();
            var first = startLookupIterator.Current;
            var posmax = first;
            var posmin = first;
            var movedNext = startLookupIterator.MoveNext();
            var posmaxNext = startLookupIterator.Current;
            var count = 1;
            while (movedNext)
            {
                var index = startLookupIterator.Current;
                movedNext = startLookupIterator.MoveNext();
                if (index > posmax)
                {
                    posmax = index;
                    posmaxNext = movedNext ? startLookupIterator.Current : first;
                }

                if (index < posmin) posmin = index;
                count++;
            }

            if (count == 3) return -1;
            if (posmin == posmaxNext)
                // LHS is a single segment and it's next in the chain
                return posmaxNext;
            return posmax;
        }
    }
}