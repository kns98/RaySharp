using System.Numerics;
using System.Text;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace minlightcsfs;

public static class Vector3f
{
    public const double fmax = 1.7976931348623157E+308;
    public const double fmin = -1.7976931348623157E+308;
    public const double fzero = 0.0;
    public const double fone = 1.0;
    internal static readonly vT vZero = new(0.0, 0.0, 0.0);
    internal static readonly vT vOne = new(1.0, 1.0, 1.0);
    internal static readonly vT vMaximum = new(double.MaxValue, double.MaxValue, double.MaxValue);
    internal static readonly vT vOneX = new(1.0, 0.0, 0.0);
    internal static readonly vT vOneY = new(0.0, 1.0, 0.0);
    internal static readonly vT vOneZ = new(0.0, 0.0, 1.0);

    internal static double[] vToArray(vT v)
    {
        var _arr = new double[3];
        _arr[0] = v[0];
        _arr[1] = v[1];
        _arr[2] = v[2];
        return _arr;
    }

    public static vT vCreate(double x, double y, double z)
    {
        return new vT(x, y, z);
    }

    public static vT op_Plus(vT v0, vT v1)
    {
        return new vT(v0.V + v1.V);
    }

    public static vT op_Plus(vT v0, vT v1, vT v2)
    {
        return new vT(v0.V + v1.V + v2.V);
    }

    public static vT op_Minus(vT v0, vT v1)
    {
        return new vT(v0.V - v1.V);
    }

    public static vT op_Mul(vT v0, vT v1)
    {
        return new vT(v0.V * v1.V);
    }

    public static vT op_Mul(vT v0, vT v1, double f)
    {
        return new vT(v0.V * v1.V * f);
    }

    public static vT op_Mul(vT v0, double f1)
    {
        return new vT(v0.V * f1);
    }

    public static vT op_Div(vT v0, double f1)
    {
        var f2 = 1.0 / f1;
        return new vT(v0.V * f2);
    }

    public static double vFold(FSharpFunc<double, FSharpFunc<double, double>> f, vT v)
    {
        return FSharpFunc<double, double>.InvokeFast(f, FSharpFunc<double, double>.InvokeFast(f, v.x, v.y), v.z);
    }

    public static vT vZip(FSharpFunc<double, FSharpFunc<double, double>> f, vT v0, vT v1)
    {
        return new vT(FSharpFunc<double, double>.InvokeFast(f, v0.x, v1.x),
            FSharpFunc<double, double>.InvokeFast(f, v0.y, v1.y),
            FSharpFunc<double, double>.InvokeFast(f, v0.z, v1.z));
    }

    public static double vDot(vT v0, vT v1)
    {
        return v0.x * v1.x + v0.y * v1.y + v0.z * v1.z;
    }

    public static double vLength(vT v)
    {
        return v.length;
    }

    public static vT vClamp(vT lower, vT upper, vT v)
    {
        return vZip(Min.__instance, upper, vZip(Max.__instance, lower, v));
    }

    public static vT vNeg(vT v)
    {
        return new vT(-v.x, -v.y, -v.z);
    }

    public static vT vUnitize(vT v)
    {
        if (vLength(v) != 0.0) return op_Div(v, vLength(v));
        return vZero;
    }

    public static vT vCross(vT v0, vT v1)
    {
        return new vT(v0.y * v1.z - v0.z * v1.y, v0.z * v1.x - v0.x * v1.z, v0.x * v1.y - v0.y * v1.x);
    }

    public static vT vScaleFrame(vT[] frame, vT scale)
    {
        return op_Plus(
            op_Mul(frame[0], scale.x),
            op_Mul(frame[1], scale.y),
            op_Mul(frame[2], scale.z));
    }

    public static vT vRead(TextReader inBuf)
    {
        var sb = new StringBuilder();
        while ((ushort)inBuf.Peek() != 40)
            if (inBuf.Read() == -1)
                throw new EndOfStreamException();
        while ((ushort)inBuf.Peek() != 41) sb.Append((char)inBuf.Read());
        if ((ushort)inBuf.Peek() == 41)
        {
            var num = inBuf.Read();
        }

        var array = sb.ToString().Trim('(', ')').Trim().Split(' ');
        FSharpFunc<bool, bool> _instance = Not._instance;
        FSharpFunc<string, bool> _instance2 = IsNullOrEmpty._instance;
        var s = ArrayModule.Filter(new f1f2(_instance, _instance2), array);
        var tuple = new Tuple<double, double, double>(double.Parse(s[0]), double.Parse(s[1]), double.Parse(s[2]));
        var z = tuple.Item3;
        var y = tuple.Item2;
        var x = tuple.Item1;
        return new vT(x, y, z);
    }

    public struct vT
    {
        public Vector<double> v;
        public readonly double length;

        public vT(Vector<double> v)
        {
            this.v = v;
            length = Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
        }

        public vT(double v1, double v2, double v3)
        {
            v = new Vector<double>(new[] { v1, v2, v3, 0 });
            length = Math.Sqrt(v1 * v1 + v2 * v2 + v3 * v3);
        }

        public double x => V[0];
        public double y => V[1];
        public double z => V[2];

        public Vector<double> V
        {
            get => v;
            set => v = value;
        }

        public bool isZero()
        {
            return V[0] == 0 && V[1] == 0 && V[2] == 0;
        }

        public double this[int i] => V[i];
    }
}

internal sealed class Min : OptimizedClosures.FSharpFunc<double, double, double>
{
    internal static readonly Min __instance = new();

    public override double Invoke(double e1, double e2)
    {
        return Math.Min(e1, e2);
    }
}

internal sealed class Max : OptimizedClosures.FSharpFunc<double, double, double>
{
    internal static readonly Max __instance = new();

    public override double Invoke(double e1, double e2)
    {
        return Math.Max(e1, e2);
    }
}

internal sealed class Not : FSharpFunc<bool, bool>
{
    internal static readonly Not _instance = new();

    public override bool Invoke(bool value)
    {
        return !value;
    }
}

internal sealed class IsNullOrEmpty : FSharpFunc<string, bool>
{
    internal static readonly IsNullOrEmpty _instance = new();

    public override bool Invoke(string arg00)
    {
        return string.IsNullOrEmpty(arg00);
    }
}

internal sealed class f1f2 : FSharpFunc<string, bool>
{
    public FSharpFunc<string, bool> func1;
    public FSharpFunc<bool, bool> func2;

    internal f1f2(FSharpFunc<bool, bool> func2, FSharpFunc<string, bool> func1)
    {
        this.func2 = func2;
        this.func1 = func1;
    }

    public override bool Invoke(string x)
    {
        return func2.Invoke(func1.Invoke(x));
    }
}