using System.Runtime.CompilerServices;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace minlightcsfs;

public static class SpatialIndex
{
    public static int maxLevels_k => 44;
    public static int maxItems_k => 8;

    public static node construct(
        double[] bound,
        FSharpList<Triangle> items,
        int level)
    {
        if ((ListModule.Length(items) <= maxItems_k ? 0 : level < maxLevels_k - 1 ? 1 : 0) == 0)
            return new node(bound, nArray.NewItems(ArrayModule.OfList(items)));
        var q1 = Operators.Ref(0);
        FSharpFunc<int, SpatialIndex_t> fsharpFunc1 = new makeSubcell(bound, items, level, q1);
        var numArray = bound;
        var length = 8;
        var fsharpFunc2 = fsharpFunc1;
        var tArray = length >= 0
            ? new SpatialIndex_t[length]
            : throw new ArgumentException(string.Format("{0}\n{1} = {2}", new object[3]
            {
                LanguagePrimitives.ErrorStrings.InputMustBeNonNegativeString,
                "count",
                length
            }), "count");
        var bound1 = numArray;
        for (var func = 0; func < tArray.Length; ++func) tArray[func] = fsharpFunc2.Invoke(func);
        return new node(bound1, nArray.NewSubCells(tArray));
    }

    public static SpatialIndex_t create(
        Vector3f.vT eyePosition,
        FSharpList<Triangle> items)
    {
        var vTArray = ListModule.Fold(encompass._instance, new Vector3f.vT[2]
        {
            eyePosition,
            eyePosition
        }, items);
        var f1 = Vector3f.vFold(maxSize._instance, Vector3f.op_Minus(vTArray[1], vTArray[0]));
        var numArray = new double[2][]
        {
            Vector3f.vToArray(vTArray[0]),
            Vector3f.vToArray(Vector3f.vZip(bound._instance, vTArray[1],
                Vector3f.op_Plus(vTArray[0], Vector3f.op_Mul(Vector3f.vOne, f1))))
        };
        return SpatialIndex_t.NewNode(construct(ArrayModule.Append(numArray[0], numArray[1]), items, 0));
    }

    public static FSharpOption<Tuple<Triangle, Vector3f.vT>> intersection(
        SpatialIndex_t octree,
        Vector3f.vT rayOrigin,
        Vector3f.vT rayDirection,
        FSharpOption<Vector3f.vT> _param3,
        FSharpOption<Triangle> lastHit)
    {
        var fsharpOption1 = _param3;
        var start = fsharpOption1 == null ? rayOrigin : fsharpOption1.Value;
        var t = octree;
        if (t is SpatialIndex_t._Empty)
            return null;
        var node = (SpatialIndex_t.Node)t;
        if (node.item.subparts is nArray.Items)
        {
            var array = ((nArray.Items)node.item.subparts).item;
            var bound = node.item.bound;
            var tuple = ArrayModule.Fold(new findNearest(rayOrigin, rayDirection, lastHit, bound),
                new Tuple<FSharpOption<Triangle>, Vector3f.vT, double>(null, Vector3f.vZero,
                    Operators.Infinity), ArrayModule.Reverse(array));
            if (tuple.Item1 == null)
                return null;
            var fsharpOption2 = tuple.Item1;
            var vT = tuple.Item2;
            return FSharpOption<Tuple<Triangle, Vector3f.vT>>.Some(
                new Tuple<Triangle, Vector3f.vT>(fsharpOption2.Value, vT));
        }

        var subCells = ((nArray.SubCells)node.item.subparts).item;
        var bound1 = node.item.bound;
        FSharpFunc<int, int> fsharpFunc = new bit(start, bound1);
        var num = fsharpFunc.Invoke(0) | fsharpFunc.Invoke(1) | fsharpFunc.Invoke(2);
        return FSharpFunc<int, Vector3f.vT>.InvokeFast(new walk(rayOrigin, rayDirection, lastHit, subCells, bound1),
            num, start);
    }

    public abstract class nArray
    {
        internal nArray()
        {
        }

        public bool IsSubCells => this is SubCells;
        public bool IsItems => this is Items;
        public int Tag => this is Items ? 1 : 0;

        public static nArray NewSubCells(SpatialIndex_t[] item)
        {
            return new SubCells(item);
        }

        public static nArray NewItems(Triangle[] item)
        {
            return new Items(item);
        }

        public static class Tags
        {
            public const int SubCells = 0;
            public const int Items = 1;
        }

        public class SubCells : nArray
        {
            internal readonly SpatialIndex_t[] item;

            internal SubCells(SpatialIndex_t[] item)
            {
                this.item = item;
            }

            public SpatialIndex_t[] Item => item;
        }

        public class Items : nArray
        {
            internal readonly Triangle[] item;

            internal Items(Triangle[] item)
            {
                this.item = item;
            }

            public Triangle[] Item => item;
        }

        internal class SubCellsDebugTypeProxy
        {
            internal SubCells _obj;

            public SubCellsDebugTypeProxy(SubCells obj)
            {
                _obj = obj;
            }

            public SpatialIndex_t[] Item => _obj.item;
        }

        [SpecialName]
        internal class ItemsDebugTypeProxy
        {
            internal Items _obj;

            public ItemsDebugTypeProxy(Items obj)
            {
                _obj = obj;
            }

            public Triangle[] Item => _obj.item;
        }
    }

    public sealed class node
    {
        internal double[] bound;
        internal nArray subparts;

        public node(double[] bound, nArray subparts)
        {
            this.bound = bound;
            this.subparts = subparts;
        }

        public override string ToString()
        {
            return ExtraTopLevelOperators
                .PrintFormatToString(
                    new PrintfFormat<FSharpFunc<node, string>, Unit, string, string, node>("%+A")).Invoke(this);
        }
    }

    public abstract class SpatialIndex_t
    {
        internal static readonly SpatialIndex_t _unique_Empty = new _Empty();

        internal SpatialIndex_t()
        {
        }

        public bool IsNode => this is Node;
        public static SpatialIndex_t Empty => _unique_Empty;
        public bool IsEmpty => this is _Empty;
        public int Tag => this is _Empty ? 1 : 0;

        public static SpatialIndex_t NewNode(node item)
        {
            return new Node(item);
        }

        internal object __DebugDisplay()
        {
            return ExtraTopLevelOperators
                .PrintFormatToString(
                    new PrintfFormat<FSharpFunc<SpatialIndex_t, string>, Unit, string, string, string>("%+0.8A"))
                .Invoke(this);
        }

        public override string ToString()
        {
            return ExtraTopLevelOperators
                .PrintFormatToString(
                    new PrintfFormat<FSharpFunc<SpatialIndex_t, string>, Unit, string, string, SpatialIndex_t>("%+A"))
                .Invoke(this);
        }

        public static class Tags
        {
            public const int Node = 0;
            public const int Empty = 1;
        }

        public class Node : SpatialIndex_t
        {
            internal readonly node item;

            internal Node(node item)
            {
                this.item = item;
            }

            public node Item => item;
        }

        internal class _Empty : SpatialIndex_t
        {
        }

        internal class NodeDebugTypeProxy
        {
            internal Node _obj;

            public NodeDebugTypeProxy(Node obj)
            {
                _obj = obj;
            }

            public node Item => _obj.item;
        }

        internal class _EmptyDebugTypeProxy
        {
            internal _Empty _obj;

            public _EmptyDebugTypeProxy(_Empty obj)
            {
                _obj = obj;
            }
        }
    }

    internal sealed class subBound : FSharpFunc<int, double>
    {
        public double[] bound;
        public int subcellIndex;

        internal subBound(double[] bound, int subcellIndex)
        {
            this.bound = bound;
            this.subcellIndex = subcellIndex;
        }

        public override double Invoke(int i)
        {
            var index = i % 3;
            return (((subcellIndex >> index) & 1) ^ (i / 3)) != 0 ? (bound[index] + bound[index + 3]) * 0.5 : bound[i];
        }
    }

    internal sealed class isOverlap : FSharpFunc<Triangle, bool>
    {
        public double[] subBound;

        internal isOverlap(double[] subBound)
        {
            this.subBound = subBound;
        }

        public override bool Invoke(Triangle item)
        {
            var bound = item.bound;
            var cell = Operators.Ref(1);
            for (var index = 0; index < 1 + 5; ++index)
            {
                var tuple = new Tuple<int, int>(index / 3, index % 3);
                var i = tuple.Item2;
                var num = tuple.Item1;
                Operators.op_ColonEquals(cell,
                    Operators.op_Dereference(cell) & ((bound[num ^ 1][i] < subBound[index] ? 0 : 1) ^ num));
            }

            return Operators.op_Dereference(cell) == 1;
        }
    }

    internal sealed class makeSubcell : FSharpFunc<int, SpatialIndex_t>
    {
        public double[] bound;
        public FSharpList<Triangle> items;
        public int level;
        public FSharpRef<int> q1;

        internal makeSubcell(
            double[] bound,
            FSharpList<Triangle> items,
            int level,
            FSharpRef<int> q1)
        {
            this.bound = bound;
            this.items = items;
            this.level = level;
            this.q1 = q1;
        }

        public override SpatialIndex_t Invoke(int subcellIndex)
        {
            var length = 6;
            FSharpFunc<int, double> fsharpFunc = new subBound(bound, subcellIndex);
            var numArray1 = length >= 0
                ? new double[length]
                : throw new ArgumentException(string.Format("{0}\n{1} = {2}", new object[3]
                {
                    LanguagePrimitives.ErrorStrings.InputMustBeNonNegativeString,
                    "count",
                    length
                }), "count");
            for (var func = 0; func < numArray1.Length; ++func) numArray1[func] = fsharpFunc.Invoke(func);
            var numArray2 = numArray1;
            var fsharpList = ListModule.Filter(new isOverlap(numArray2), items);
            Operators.op_ColonEquals(q1,
                Operators.op_Dereference(q1) +
                (ListModule.Length(fsharpList) != ListModule.Length(items) ? 0 : 1));
            var flag = numArray2[3] - numArray2[0] < Triangle.tolerance_k * 4.0;
            return ListModule.Length(fsharpList) > 0
                ? SpatialIndex_t.NewNode(construct(numArray2, fsharpList,
                    (Operators.op_Dereference(q1) <= 1 ? flag ? 1 : 0 : 1) == 0 ? level + 1 : maxLevels_k))
                : SpatialIndex_t.Empty;
        }
    }

    internal sealed class encompass_min : OptimizedClosures.FSharpFunc<double, double, double>
    {
        internal static readonly encompass_min _instance = new();

        public override double Invoke(double e1, double e2)
        {
            return Math.Min(e1, e2);
        }
    }

    internal sealed class encompass_max : OptimizedClosures.FSharpFunc<double, double, double>
    {
        internal static readonly encompass_max _instance = new();

        public override double Invoke(double e1, double e2)
        {
            return Math.Max(e1, e2);
        }
    }

    internal sealed class encompass : OptimizedClosures.FSharpFunc<Vector3f.vT[], Triangle, Vector3f.vT[]>
    {
        internal static readonly encompass _instance = new();

        public override Vector3f.vT[] Invoke(Vector3f.vT[] rb, Triangle item)
        {
            var bound = item.bound;
            return new Vector3f.vT[2]
            {
                Vector3f.vZip(encompass_min._instance, rb[0], bound[0]),
                Vector3f.vZip(encompass_max._instance, rb[1], bound[1])
            };
        }
    }

    internal sealed class maxSize : OptimizedClosures.FSharpFunc<double, double, double>
    {
        internal static readonly maxSize _instance = new();

        public override double Invoke(double e1, double e2)
        {
            return Math.Max(e1, e2);
        }
    }

    internal sealed class bound : OptimizedClosures.FSharpFunc<double, double, double>
    {
        internal static readonly bound _instance = new();

        public override double Invoke(double e1, double e2)
        {
            return Math.Max(e1, e2);
        }
    }

    internal sealed class bit : FSharpFunc<int, int>
    {
        public double[] bound;
        public Vector3f.vT start;

        internal bit(Vector3f.vT start, double[] bound)
        {
            this.start = start;
            this.bound = bound;
        }

        public override int Invoke(int i)
        {
            return start[i] >= (bound[i] + bound[i + 3]) * 0.5 ? 1 << i : 0;
        }
    }

    internal sealed class findNext :
        FSharpFunc<Tuple<double, int, int>, Tuple<double, int, int>>
    {
        public double[] bound;
        public Vector3f.vT rayDirection;
        public Vector3f.vT rayOrigin;
        public int subCell;

        internal findNext(
            Vector3f.vT rayOrigin,
            Vector3f.vT rayDirection,
            double[] bound,
            int subCell)
        {
            this.rayOrigin = rayOrigin;
            this.rayDirection = rayDirection;
            this.bound = bound;
            this.subCell = subCell;
        }

        public override Tuple<double, int, int> Invoke(Tuple<double, int, int> tupledArg)
        {
            var num1 = tupledArg.Item1;
            var num2 = tupledArg.Item2;
            var i = tupledArg.Item3;
            var num3 = (subCell >> i) & 1;
            var num4 = ((((rayDirection[i] >= 0.0 ? 0 : 1) ^ num3) == 0
                ? (bound[i] + bound[i + 3]) * 0.5
                : bound[i + num3 * 3]) - rayOrigin[i]) / rayDirection[i];
            return num4 <= num1
                ? new Tuple<double, int, int>(num4, i, i - 1)
                : new Tuple<double, int, int>(num1, num2, i - 1);
        }
    }

    internal sealed class walk : OptimizedClosures.FSharpFunc<int, Vector3f.vT,
        FSharpOption<Tuple<Triangle, Vector3f.vT>>>
    {
        public double[] bound;
        public FSharpOption<Triangle> lastHit;
        public Vector3f.vT rayDirection;
        public Vector3f.vT rayOrigin;
        public SpatialIndex_t[] subCells;

        internal walk(
            Vector3f.vT rayOrigin,
            Vector3f.vT rayDirection,
            FSharpOption<Triangle> lastHit,
            SpatialIndex_t[] subCells,
            double[] bound)
        {
            this.rayOrigin = rayOrigin;
            this.rayDirection = rayDirection;
            this.lastHit = lastHit;
            this.subCells = subCells;
            this.bound = bound;
        }

        public override FSharpOption<Tuple<Triangle, Vector3f.vT>> Invoke(
            int subCell,
            Vector3f.vT cellPosition)
        {
            FSharpOption<Tuple<Triangle, Vector3f.vT>> fsharpOption;
            while (true)
            {
                fsharpOption = intersection(subCells[subCell], rayOrigin, rayDirection,
                    FSharpOption<Vector3f.vT>.Some(cellPosition), lastHit);
                if (fsharpOption == null)
                {
                    FSharpFunc<Tuple<double, int, int>, Tuple<double, int, int>> fsharpFunc =
                        new findNext(rayOrigin, rayDirection, bound, subCell);
                    var tuple = fsharpFunc.Invoke(
                        fsharpFunc.Invoke(fsharpFunc.Invoke(new Tuple<double, int, int>(double.MaxValue, 2, 2))));
                    var f1 = tuple.Item1;
                    var i = tuple.Item2;
                    if (((rayDirection[i] >= 0.0 ? 0 : 1) ^ ((subCell >> i) & 1)) != 1)
                    {
                        var num1 = subCell ^ (1 << i);
                        var vT = Vector3f.op_Plus(rayOrigin, Vector3f.op_Mul(rayDirection, f1));
                        var num2 = num1;
                        cellPosition = vT;
                        subCell = num2;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    goto label_4;
                }
            }

            return null;
            label_4:
            return fsharpOption;
        }
    }

    internal sealed class findNearest : OptimizedClosures.FSharpFunc<
        Tuple<FSharpOption<Triangle>, Vector3f.vT, double>, Triangle,
        Tuple<FSharpOption<Triangle>, Vector3f.vT, double>>
    {
        public double[] bound;
        public FSharpOption<Triangle> lastHit;
        public Vector3f.vT rayDirection;
        public Vector3f.vT rayOrigin;

        internal findNearest(
            Vector3f.vT rayOrigin,
            Vector3f.vT rayDirection,
            FSharpOption<Triangle> lastHit,
            double[] bound)
        {
            this.rayOrigin = rayOrigin;
            this.rayDirection = rayDirection;
            this.lastHit = lastHit;
            this.bound = bound;
        }

        public override Tuple<FSharpOption<Triangle>, Vector3f.vT, double> Invoke(
            Tuple<FSharpOption<Triangle>, Vector3f.vT, double> nearest,
            Triangle item)
        {
            var lastHit = this.lastHit;
            if (lastHit != null)
            {
                var fsharpOption = lastHit;
                if (LanguagePrimitives.HashCompare.GenericEqualityIntrinsic(fsharpOption.Value, item)) return nearest;
            }

            var num = nearest.Item3;
            var fsharpOption1 = item.intersection(rayOrigin, rayDirection);
            if (fsharpOption1 != null)
            {
                var fsharpOption2 = fsharpOption1;
                if (fsharpOption2.Value < num)
                {
                    var f1 = fsharpOption2.Value;
                    var vT = Vector3f.op_Plus(rayOrigin, Vector3f.op_Mul(rayDirection, f1));
                    var toleranceK = Triangle.tolerance_k;
                    return (((((bound[0] - vT.x <= toleranceK ? vT.x - bound[3] > toleranceK ? 1 : 0 : 1) == 0
                        ? bound[1] - vT.y > toleranceK ? 1 : 0
                        : 1) == 0
                        ? vT.y - bound[4] > toleranceK ? 1 : 0
                        : 1) == 0
                        ? bound[2] - vT.z > toleranceK ? 1 : 0
                        : 1) == 0
                        ? vT.z - bound[5] > toleranceK ? 1 : 0
                        : 1) != 0
                        ? nearest
                        : new Tuple<FSharpOption<Triangle>, Vector3f.vT, double>(
                            FSharpOption<Triangle>.Some(item), vT, f1);
                }
            }

            return nearest;
        }
    }
}