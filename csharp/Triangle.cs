using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace minlightcsfs;

public class Triangle
{
    internal Vector3f.vT edge1;
    internal Vector3f.vT edge2;
    internal Vector3f.vT emitivity_m;
    internal Vector3f.vT expand_minus;
    internal Vector3f.vT expand_plus;
    internal Vector3f.vT reflectivity_m;
    internal Vector3f.vT[] vertexs_m;

    public Triangle(TextReader inBuffer_i)
    {
        var triangle = this;
        var list = FSharpList<Vector3f.vT>.Cons(Vector3f.vRead(inBuffer_i),
            FSharpList<Vector3f.vT>.Cons(Vector3f.vRead(inBuffer_i),
                FSharpList<Vector3f.vT>.Cons(Vector3f.vRead(inBuffer_i),
                    FSharpList<Vector3f.vT>.Cons(Vector3f.vRead(inBuffer_i),
                        FSharpList<Vector3f.vT>.Cons(Vector3f.vRead(inBuffer_i), FSharpList<Vector3f.vT>.Empty)))));
        vertexs_m = new Vector3f.vT[3]
        {
            ListModule.Get(list, 0),
            ListModule.Get(list, 1),
            ListModule.Get(list, 2)
        };
        reflectivity_m = Vector3f.vClamp(Vector3f.vZero, Vector3f.vOne, ListModule.Get(list, 3));
        emitivity_m = Vector3f.vClamp(Vector3f.vZero, Vector3f.vMaximum, ListModule.Get(list, 4));
        edge1 = Vector3f.op_Minus(vertexs_m[1], vertexs_m[0]);
        edge2 = Vector3f.op_Minus(vertexs_m[2], vertexs_m[0]);
        expand_minus = Vector3f.vZip(
            new c4068(this),
            Vector3f.vOne, min_min(vertexs_m));
        expand_plus = Vector3f.vZip(
            new c02D1(this),
            Vector3f.vOne, max_max(vertexs_m));
    }

    public static double tolerance_k => 1d / 1024d;

    public Vector3f.vT[] bound => new Vector3f.vT[2]
    {
        expand_minus,
        expand_plus
    };

    public Vector3f.vT normal =>
        Vector3f.vUnitize(Vector3f.vCross(tangent, Vector3f.op_Minus(vertexs_m[2], vertexs_m[1])));

    public Vector3f.vT tangent => Vector3f.vUnitize(Vector3f.op_Minus(vertexs_m[1], vertexs_m[0]));

    public double area
    {
        get
        {
            var vT = Vector3f.vCross(edge1, Vector3f.op_Minus(vertexs_m[2], vertexs_m[1]));
            return Math.Sqrt(Vector3f.vDot(vT, vT)) * 0.5;
        }
    }

    public Vector3f.vT reflectivity => reflectivity_m;
    public Vector3f.vT emitivity => emitivity_m;

    public FSharpOption<double> intersection(
        Vector3f.vT rayOrigin,
        Vector3f.vT rayDirection)
    {
        var v1_1 = Vector3f.vCross(rayDirection, edge2);
        var num1 = Vector3f.vDot(edge1, v1_1);
        var num2 = 1E-06;
        if ((num1 <= -num2 ? 0 : num1 < num2 ? 1 : 0) != 0)
            return null;
        var num3 = 1.0 / num1;
        var v0 = Vector3f.op_Minus(rayOrigin, vertexs_m[0]);
        var num4 = Vector3f.vDot(v0, v1_1) * num3;
        if ((num4 >= 0.0 ? num4 > 1.0 ? 1 : 0 : 1) != 0)
            return null;
        var v1_2 = Vector3f.vCross(v0, edge1);
        var num5 = Vector3f.vDot(rayDirection, v1_2) * num3;
        if ((num5 >= 0.0 ? num4 + num5 > 1.0 ? 1 : 0 : 1) != 0)
            return null;
        var num6 = Vector3f.vDot(edge2, v1_2) * num3;
        return num6 >= 0.0 ? FSharpOption<double>.Some(num6) : null;
    }

    public Vector3f.vT samplePoint(Random random)
    {
        FSharpFunc<Unit, double> fsharpFunc = new rand(random);
        var tuple = new Tuple<double, double>(Math.Sqrt(fsharpFunc.Invoke(null)), fsharpFunc.Invoke(null));
        var num1 = tuple.Item1;
        var num2 = tuple.Item2;
        return Vector3f.vScaleFrame(new Vector3f.vT[3]
        {
            vertexs_m[0],
            edge1,
            edge2
        }, Vector3f.vCreate(1.0, 1.0 - num1, (1.0 - num2) * num1));
    }

    internal double tol_minus(double a, double b)
    {
        return b - (Math.Abs(b) + a) * tolerance_k;
    }

    internal double tol_plus(double a, double b)
    {
        return b + (Math.Abs(b) + a) * tolerance_k;
    }

    internal Vector3f.vT vZip(Func<double, double, double> f, Vector3f.vT v1, Vector3f.vT v2)
    {
        return new Vector3f.vT(f(v1.x, v2.x), f(v1.y, v2.y), f(v1.z, v2.z));
    }

    internal Vector3f.vT min_min(Vector3f.vT[] v)
    {
        return vZip(Math.Min, v[0], vZip(Math.Min, v[1], v[2]));
    }

    internal Vector3f.vT max_max(Vector3f.vT[] v)
    {
        return vZip(Math.Max, v[0], vZip(Math.Max, v[1], v[2]));
    }
}

internal sealed class c4068 : OptimizedClosures.FSharpFunc<double, double, double>
{
    public Triangle t;

    internal c4068(Triangle _param1)
    {
        t = _param1;
    }

    public override double Invoke(double a, double b)
    {
        return t.tol_minus(a, b);
    }
}

internal sealed class c02D1 : OptimizedClosures.FSharpFunc<double, double, double>
{
    public Triangle t;

    internal c02D1(Triangle _param1)
    {
        t = _param1;
    }

    public override double Invoke(double a, double b)
    {
        return t.tol_plus(a, b);
    }
}

internal sealed class rand : FSharpFunc<Unit, double>
{
    public Random random;

    internal rand(Random random)
    {
        this.random = random;
    }

    public override double Invoke(Unit unitVar0)
    {
        return random.NextDouble();
    }
}