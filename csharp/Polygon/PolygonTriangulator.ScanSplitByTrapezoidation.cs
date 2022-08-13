namespace minlightcsfs.PolygonTriangulation;

/// <summary>
///     subclass container for triangulator
/// </summary>
public partial class PolygonTriangulator
{
    /// <summary>
    ///     Traverse the polygon, build trapezoids and collect possible splits
    /// </summary>
    private class ScanSplitByTrapezoidation : IPolygonSplitSink
    {
        private readonly Trapezoidation activeEdges;
        private readonly Polygon polygon;
        private readonly List<Tuple<int, int>> splits;

        private ScanSplitByTrapezoidation(Polygon polygon)
        {
            splits = new List<Tuple<int, int>>();
            this.polygon = polygon;
            activeEdges = new Trapezoidation(this.polygon.Vertices, this);
        }

        /// <inheritdoc />
        void IPolygonSplitSink.SplitPolygon(int leftVertex, int rightVertex)
        {
            splits.Add(Tuple.Create(leftVertex, rightVertex));
        }

        /// <summary>
        ///     Build the splits for the polygon
        /// </summary>
        /// <param name="polygon">the polygon</param>
        /// <returns>the splits</returns>
        public static IEnumerable<Tuple<int, int>> BuildSplits(Polygon polygon)
        {
            var splitter = new ScanSplitByTrapezoidation(polygon);
            splitter.BuildSplits(-1);
            return splitter.splits;
        }

        /// <summary>
        ///     Traverse the polygon and build all splits
        /// </summary>
        /// <param name="stepCount">number of steps during debugging. Use -1 for all</param>
        public void BuildSplits(int stepCount)
        {
            foreach (var group in polygon.OrderedVertices.GroupBy(x => x.Id))
            {
                var actions = group.ToArray();
                if (actions.Length > 1) actions = actions.OrderBy(x => x.Action).ToArray();
                foreach (var info in actions)
                {
                    if (stepCount >= 0)
                    {
                        stepCount -= 1;
                        if (stepCount < 0) return;
                    }

                    switch (info.Action)
                    {
                        case VertexAction.ClosingCusp:
                            activeEdges.HandleClosingCusp(info);
                            break;
                        case VertexAction.Transition:
                            activeEdges.HandleTransition(info);
                            break;
                        case VertexAction.OpeningCusp:
                            activeEdges.HandleOpeningCusp(info);
                            break;
                        default:
                            throw new InvalidOperationException($"Unkown action {info.Action}");
                    }
                }
            }
        }

        /// <summary>
        ///     Run n steps and return the edges after that step
        /// </summary>
        /// <param name="polygon">the polygon</param>
        /// <param name="depth">the number of steps to run</param>
        /// <returns>The edges sorted from High to Low</returns>
        internal static IEnumerable<string> GetEdgesAfterPartialTrapezoidation(Polygon polygon, int depth)
        {
            var splitter = new ScanSplitByTrapezoidation(polygon);
            splitter.BuildSplits(depth);
            return splitter.activeEdges.Edges.Reverse().Select(x => x.ToString());
        }
    }
}